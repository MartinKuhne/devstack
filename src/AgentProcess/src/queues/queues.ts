import { Queue, JobsOptions } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';

const config = loadConfig();

const redisConnection = new Redis(config.REDIS_URL);

const defaultJobOptions: JobsOptions = {
  attempts: config.MAX_RETRIES,
  backoff: {
    type: 'exponential',
    delay: 5000,
  },
  removeOnComplete: {
    count: 100,
  },
  removeOnFail: {
    count: 500,
  },
};

export const plannerQueue = new Queue('planner', {
  connection: redisConnection,
  defaultJobOptions,
});

export const devleadQueue = new Queue('devlead', {
  connection: redisConnection,
  defaultJobOptions,
});

export const coderQueue = new Queue('coder', {
  connection: redisConnection,
  defaultJobOptions,
});

export const testerQueue = new Queue('tester', {
  connection: redisConnection,
  defaultJobOptions,
});

export const architectQueue = new Queue('architect', {
  connection: redisConnection,
  defaultJobOptions,
});

export const queues = {
  planner: plannerQueue,
  devlead: devleadQueue,
  coder: coderQueue,
  tester: testerQueue,
  architect: architectQueue,
};

export interface WorkflowJobData {
  workflowName: string;
  input: unknown;
}

export async function enqueueWorkflowJob(
  queue: Queue,
  workflowName: string,
  input: unknown
): Promise<void> {
  await queue.add(workflowName, {
    workflowName,
    input,
  } as WorkflowJobData);
}

export async function closeQueals(): Promise<void> {
  await redisConnection.quit();
}
