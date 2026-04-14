import { Worker, Job } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';

const config = loadConfig();

export type JobProcessor<T = unknown> = (job: Job<T>) => Promise<unknown>;

export interface WorkerOptions {
  name: string;
  concurrency?: number;
}

export function createWorker<T = unknown>(
  queueName: string,
  processor: JobProcessor<T>,
  options: WorkerOptions = { name: queueName, concurrency: config.WORKER_CONCURRENCY }
): Worker<T> {
  const redisConnection = new Redis(config.REDIS_URL);

  const worker = new Worker<T>(
    queueName,
    async (job: Job<T>) => {
      console.log(`[${options.name}] Processing job ${job.id ?? 'unknown'}`);
      try {
        const result = await processor(job);
        console.log(`[${options.name}] Job ${job.id ?? 'unknown'} completed`);
        return result;
      } catch (error) {
        console.error(`[${options.name}] Job ${job.id ?? 'unknown'} failed:`, error);
        throw error;
      }
    },
    {
      connection: redisConnection,
      concurrency: options.concurrency,
    }
  );

  worker.on('completed', (job: Job) => {
    console.log(`[${options.name}] Job ${job.id ?? 'unknown'} completed successfully`);
  });

  worker.on('failed', (job: Job | undefined, error: Error) => {
    console.error(`[${options.name}] Job ${job?.id ?? 'unknown'} failed:`, error);
  });

  worker.on('error', (error: Error) => {
    console.error(`[${options.name}] Worker error:`, error);
  });

  return worker;
}
