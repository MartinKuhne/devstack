import { Queue } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';

const config = loadConfig();

const redisConnection = new Redis(config.REDIS_URL);

const defaultJobOptions = {
  attempts: config.MAX_RETRIES,
  backoff: {
    type: 'exponential' as const,
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

export async function closeQueals(): Promise<void> {
  await redisConnection.quit();
}
