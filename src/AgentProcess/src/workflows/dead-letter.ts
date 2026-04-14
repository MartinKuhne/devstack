import { Queue, Job, Worker } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { logger } from '../observability/logger.js';
import { createGraphQLClient } from '../api/graphql-client.js';
import { createDefectTool } from '../skills/work-items/work-items-skill.js';
import { ToolContext } from '../skills/tool.js';

const config = loadConfig();
const redisConnection = new Redis(config.REDIS_URL);
const graphQLClient = createGraphQLClient();

const toolContext: ToolContext = {
  logger,
  api: graphQLClient,
};

// Dead letter queue for permanently failed jobs
export const deadLetterQueue = new Queue('deadLetter', {
  connection: redisConnection,
  defaultJobOptions: {
    removeOnComplete: {
      count: 100,
    },
    removeOnFail: {
      count: 0, // Keep failed jobs in dead letter queue for inspection
    },
  },
});

interface DeadLetterJobData {
  originalQueue: string;
  originalJobId: string;
  workflowName: string;
  input: unknown;
  failureReason: string;
  failedAt: string;
  attemptsMade: number;
}

/**
 * Processor for dead letter queue jobs
 * Logs structured error events and optionally creates defects for significant failures
 */
export class DeadLetterProcessor {
  private worker: Worker<DeadLetterJobData>;

  constructor() {
    this.worker = new Worker<DeadLetterJobData>(
      'deadLetter',
      async (job: Job<DeadLetterJobData>) => {
        const jobData = job.data;
        
        // Log structured error event with job metadata
        logger.error(
          {
            jobId: job.id,
            originalJobId: jobData.originalJobId,
            originalQueue: jobData.originalQueue,
            workflowName: jobData.workflowName,
            failureReason: jobData.failureReason,
            failedAt: jobData.failedAt,
            attemptsMade: jobData.attemptsMade,
            input: jobData.input,
          },
          'Permanently failed job moved to dead letter queue'
        );

        // Optionally create a defect for significant permanent failures
        await this.createDefectIfSignificant(jobData);
        
        // Mark job as processed in dead letter queue
        return true;
      },
      {
        connection: redisConnection,
      }
    );

    // Handle worker events
    this.worker.on('completed', (job: Job) => {
      logger.info({ jobId: job.id }, 'Dead letter job processed successfully');
    });

    this.worker.on('failed', (job: Job | undefined, error: Error) => {
      logger.error(
        { jobId: job?.id ?? 'unknown', error },
        'Dead letter job processing failed'
      );
    });
  }

  /**
   * Determines if a failure is significant enough to warrant creating a defect
   * For now, we'll create defects for all failures, but this could be refined
   * based on error type, workflow type, or other criteria
   */
  private async createDefectIfSignificant(jobData: DeadLetterJobData): Promise<void> {
    try {
      if (!config.PROJECT_ID) {
        logger.warn(
          { workflowName: jobData.workflowName },
          'PROJECT_ID not configured, skipping defect creation'
        );
        return;
      }

      const defectTitle = `Permanent workflow failure: ${jobData.workflowName}`;
      const defectDescription = `Workflow **${jobData.workflowName}** failed permanently after ${jobData.attemptsMade} attempts.\n\n**Failure Reason:** ${jobData.failureReason}\n\n**Original Job ID:** ${jobData.originalJobId}\n\n**Failed At:** ${jobData.failedAt}`;

      const result = await createDefectTool(
        {
          projectId: config.PROJECT_ID,
          title: defectTitle,
          description: defectDescription,
          severity: 'HIGH',
          initialStatus: 'PLANNED',
        },
        toolContext
      );

      if (result.ok && result.output.ok && result.output.id) {
        logger.info(
          {
            defectId: result.output.id,
            workflowName: jobData.workflowName,
            originalJobId: jobData.originalJobId,
          },
          'Created defect for significant failure'
        );
      } else if (result.ok === false) {
        const errorResult = result as { ok: false; error: { message: string } };
        logger.error(
          {
            error: errorResult.error.message,
            workflowName: jobData.workflowName,
            originalJobId: jobData.originalJobId,
          },
          'Failed to create defect for dead letter job'
        );
      } else if (result.ok === true && !result.output.ok) {
        logger.error(
          {
            error: result.output.error,
            workflowName: jobData.workflowName,
            originalJobId: jobData.originalJobId,
          },
          'Failed to create defect for dead letter job'
        );
      }
    } catch (error) {
      logger.error(
        { error, jobData: jobData.originalJobId },
        'Failed to create defect for dead letter job'
      );
    }
  }

  /**
   * Starts the dead letter queue processor
   */
  async start(): Promise<void> {
    // Worker starts automatically on construction
    logger.info('Dead letter processor started');
  }

  /**
   * Stops the dead letter queue processor
   */
  async stop(): Promise<void> {
    await this.worker.close();
    logger.info('Dead letter processor stopped');
  }
}

/**
 * Moves a job to the dead letter queue after retries are exhausted
 * @param originalQueue The queue where the job originally failed
 * @param job The failed job
 * @param error The error that caused the final failure
 */
export async function moveToDeadLetterQueue(
  originalQueue: string,
  job: Job,
  error: Error
): Promise<void> {
  try {
    const deadLetterJobData: DeadLetterJobData = {
      originalQueue,
      originalJobId: job.id ?? 'unknown',
      workflowName: job.data?.workflowName ?? 'unknown',
      input: job.data?.input ?? null,
      failureReason: error.message,
      failedAt: new Date().toISOString(),
      attemptsMade: job.attemptsMade ?? 0,
    };

    await deadLetterQueue.add('process-dead-letter-job', deadLetterJobData);
    
    logger.info(
      {
        jobId: job.id,
        originalJobId: job.id,
        originalQueue,
        workflowName: job.data.workflowName,
        attemptsMade: job.attemptsMade,
      },
      'Moved job to dead letter queue after exhausted retries'
    );
  } catch (moveError) {
    logger.error(
      { moveError, jobId: job.id, originalQueue },
      'Failed to move job to dead letter queue'
    );
    // Even if moving to DLQ fails, we still want to log the original error
    throw moveError;
  }
}