import { Worker, Job } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { executeWorkflow } from '../workflows/executor.js';
import { logger } from '../observability/logger.js';

const config = loadConfig();

export type JobProcessor<T = unknown> = (job: Job<T>) => Promise<unknown>;

export interface WorkerOptions {
  name: string;
  concurrency?: number;
}

export function createWorker<T extends { workflowName: string; input: unknown }>(
  queueName: string,
  options: WorkerOptions = { name: queueName, concurrency: config.WORKER_CONCURRENCY }
): Worker<T> {
  const redisConnection = new Redis(config.REDIS_URL);

  const worker = new Worker<T>(
    queueName,
    async (job: Job<T>) => {
      const jobData = job.data;
      const workflowName = jobData.workflowName;
      
      logger.info({ workflowName, jobId: job.id }, 'Processing workflow job');
      
      try {
        const result = await executeWorkflow(
          workflowName,
          job.id ?? 'unknown',
          jobData.input
        );
        
        logger.info({ workflowName, jobId: job.id }, 'Workflow job completed successfully');
        return result;
      } catch (error) {
        logger.error({ workflowName, jobId: job.id, error }, 'Workflow job failed');
        throw error;
      }
    },
    {
      connection: redisConnection,
      concurrency: options.concurrency,
    }
  );

  worker.on('completed', (job: Job) => {
    logger.info({ jobId: job.id, workflowName: job.data?.workflowName }, 'Job completed successfully');
  });

  worker.on('failed', (job: Job | undefined, error: Error) => {
    logger.error(
      { jobId: job?.id ?? 'unknown', workflowName: job?.data?.workflowName, error },
      'Job failed'
    );
  });

  worker.on('error', (error: Error) => {
    logger.error({ error }, 'Worker error');
  });

  return worker;
}
