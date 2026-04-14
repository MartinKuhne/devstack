import { Queue, Job, Worker } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { logger } from '../observability/logger.js';
import { createGraphQLClient } from '../api/graphql-client.js';

const config = loadConfig();
const redisConnection = new Redis(config.REDIS_URL);
const mockGraphQLClient = createGraphQLClient();

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
      // In a real implementation, this would call the create_defect mutation
      // For now, we'll log that we would create a defect
      logger.info(
        {
          workflowName: jobData.workflowName,
          failureReason: jobData.failureReason,
          originalJobId: jobData.originalJobId,
        },
        'Would create defect for significant failure (feature not yet implemented)'
      );
      
      // TODO: Implement actual defect creation when create_defect mutation is available
      // const defectId = await createDefect({
      //   title: `Permanent workflow failure: ${jobData.workflowName}`,
      //   description: `Workflow ${jobData.workflowName} failed permanently after ${jobData.attemptsMade} attempts. Last error: ${jobData.failureReason}`,
      //   severity: 'high',
      //   status: 'TODO',
      //   // Additional fields as needed
      // });
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