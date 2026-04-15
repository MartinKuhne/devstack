import { Worker, Job } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { executeWorkflow } from '../workflows/executor.js';
import { moveToDeadLetterQueue } from '../workflows/dead-letter.js';
import { logger } from '../observability/logger.js';

const config = loadConfig();

export type JobProcessor<T = unknown> = (job: Job<T>) => Promise<unknown>;

export interface WorkerOptions {
  name: string;
  concurrency?: number;
}

export { Worker };

export function createWorker<T extends { workflowName: string; input: unknown }>(
  queueName: string,
  options: WorkerOptions = { name: queueName, concurrency: config.WORKER_CONCURRENCY }
): Worker<T> {
  const redisConnection = new Redis(config.REDIS_URL, {
    maxRetriesPerRequest: null,
  });

  const worker = new Worker<T>(
    queueName,
    async (job: Job<T>) => {
      const jobData = job.data;
      const workflowName = jobData.workflowName;
      
      // Optional chaining is safe here as job.data might be undefined
      // but we know it's defined based on our job structure
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
        // Optional chaining is safe here as job.data might be undefined
        // but we know it's defined based on our job structure
        logger.error({ workflowName, jobId: job.id, error }, 'Workflow job failed');
        
        // Check if job has exhausted retries and move to dead letter queue
        if (job.attemptsMade >= (job.opts?.attempts ?? config.MAX_RETRIES)) {
          // Optional chaining is safe here as job.data might be undefined
          // but we know it's defined based on our job structure
          logger.warn(
            { 
              jobId: job.id, 
              workflowName, 
              attemptsMade: job.attemptsMade,
              maxAttempts: job.opts?.attempts ?? config.MAX_RETRIES
            },
            'Job has exhausted retries, moving to dead letter queue'
          );
          
          await moveToDeadLetterQueue(queueName, job, error as Error);
        }
        
        throw error;
      }
    },
    {
      connection: redisConnection,
      concurrency: options.concurrency,
    }
  );

  // Optional chaining is safe here as job might be undefined
  // but we know it's defined based on our event registration
  worker.on('completed', (job: Job) => {
    logger.info({ jobId: job.id, workflowName: job.data?.workflowName }, 'Job completed successfully');
  });

  // Optional chaining is safe here as job might be undefined
  // but we know it's defined based on our event registration
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

export async function stopWorker(worker: Worker): Promise<void> {
  await worker.close();
}

export async function stopWorkers(workers: Worker[]): Promise<void> {
  const closePromises = workers.map(async (worker) => {
    try {
      await worker.close();
      logger.info({ queueName: worker.name }, 'Worker closed');
    } catch (error) {
      logger.error({ queueName: worker.name, error }, 'Error closing worker');
    }
  });

  await Promise.all(closePromises);
  workers = [];
}