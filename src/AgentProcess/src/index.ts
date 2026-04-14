import { Redis } from 'ioredis';
import { GraphQLClient } from 'graphql-request';
import { loadConfig } from './config.js';
import { logger } from './observability/logger.js';
import { initializeTelemetry, shutdownTelemetry } from './observability/telemetry.js';
import { initializeHealthEndpoints, shutdownHealthEndpoints } from './health/health.js';
import { PollingScheduler } from './queues/polling-scheduler.js';
import { createWorker, Worker } from './queues/worker.js';

let scheduler: PollingScheduler | null = null;
let redisConnection: Redis | null = null;
let apiClient: GraphQLClient | null = null;
let workers: Worker[] = [];
let isShuttingDown = false;

const config = loadConfig();
const GRACEFUL_SHUTDOWN_TIMEOUT_MS = config.GRACEFUL_SHUTDOWN_TIMEOUT_MS;

async function startScheduler(config: ReturnType<typeof loadConfig>): Promise<void> {
  if (!config.ENABLE_SCHEDULER) {
    logger.info('Scheduler is disabled (ENABLE_SCHEDULER=false)');
    return;
  }

  redisConnection = new Redis(config.REDIS_URL);
  apiClient = new GraphQLClient(config.GRAPHQL_API_URL, {
    headers: config.GRAPHQL_API_TOKEN ? { Authorization: `Bearer ${config.GRAPHQL_API_TOKEN}` } : {},
  });

  scheduler = new PollingScheduler(redisConnection, apiClient, config.SCHEDULER_INTERVAL);
  await scheduler.start();
  logger.info({ intervalMs: config.SCHEDULER_INTERVAL }, 'Scheduler enabled');
}

function startWorkers(): void {
  const queueNames = ['planner', 'devlead', 'coder', 'tester', 'architect'];

  for (const queueName of queueNames) {
    const worker = createWorker(queueName, {
      name: queueName,
      concurrency: config.WORKER_CONCURRENCY,
    });
    workers.push(worker);
    logger.info({ queueName }, 'Worker started');
  }
}

async function shutdownScheduler(): Promise<void> {
  if (scheduler) {
    await scheduler.stop();
    scheduler = null;
  }

  if (redisConnection) {
    await redisConnection.quit();
    redisConnection = null;
  }

  apiClient = null;
}

async function shutdownWorkers(): Promise<void> {
  if (workers.length === 0) {
    logger.info('No workers to shut down');
    return;
  }

  logger.info({ workerCount: workers.length }, 'Closing workers');

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

async function waitForInFlightJobs(timeoutMs: number): Promise<void> {
  const startTime = Date.now();
  const checkIntervalMs = 1000;

  logger.info({ timeoutMs }, 'Waiting for in-flight jobs to complete');

  while (Date.now() - startTime < timeoutMs) {
    const runningWorkers = workers.filter((w) => w.isRunning());

    if (runningWorkers.length === 0) {
      logger.info('All workers have stopped processing');
      return;
    }

    logger.info({ runningWorkers: runningWorkers.map((w) => w.name) }, 'Waiting for workers to finish');
    await new Promise((resolve) => setTimeout(resolve, checkIntervalMs));
  }

  logger.warn({ elapsedMs: Date.now() - startTime }, 'Shutdown timeout reached, forcing shutdown');
}

async function gracefulShutdown(signal: string): Promise<void> {
  logger.info({ signal }, `Received ${signal}, initiating graceful shutdown...`);
  isShuttingDown = true;

  const shutdownSteps: { name: string; action: () => Promise<void> }[] = [
    { name: 'Stop scheduler', action: async () => { await shutdownScheduler(); } },
    { name: 'Wait for in-flight jobs', action: async () => { await waitForInFlightJobs(GRACEFUL_SHUTDOWN_TIMEOUT_MS); } },
    { name: 'Close workers', action: async () => { await shutdownWorkers(); } },
    { name: 'Close health endpoints', action: async () => { await shutdownHealthEndpoints(); } },
    { name: 'Shutdown telemetry', action: async () => { await shutdownTelemetry(); } },
  ];

  for (const { name, action } of shutdownSteps) {
    try {
      logger.info(`Shutting down: ${name}`);
      await action();
      logger.info(`Completed: ${name}`);
    } catch (error) {
      logger.error({ name, error }, `Error during shutdown: ${name}`);
    }
  }

  logger.info('Graceful shutdown completed');
  process.exit(0);
}

function setupShutdownHandlers(): void {
  let shutdownTimeout: NodeJS.Timeout | null = null;

  const handleSignal = (signal: string) => {
    if (isShuttingDown) {
      logger.warn({ signal }, 'Shutdown already in progress, forcing exit');
      if (shutdownTimeout) {
        clearTimeout(shutdownTimeout);
      }
      process.exit(1);
      return;
    }

    void gracefulShutdown(signal);

    shutdownTimeout = setTimeout(() => {
      logger.error('Forced shutdown due to timeout');
      process.exit(1);
    }, GRACEFUL_SHUTDOWN_TIMEOUT_MS + 5000);
  };

  process.on('SIGTERM', () => {
    handleSignal('SIGTERM');
  });
  process.on('SIGINT', () => {
    handleSignal('SIGINT');
  });

  process.on('uncaughtException', (error) => {
    logger.error(error, 'Uncaught exception');
    if (!isShuttingDown) {
      void gracefulShutdown('uncaughtException');
    }
  });

  process.on('unhandledRejection', (reason) => {
    logger.error({ reason }, 'Unhandled rejection');
    if (!isShuttingDown) {
      void gracefulShutdown('unhandledRejection');
    }
  });
}

function main(): void {
  initializeTelemetry();

  logger.info('Agent Process started');
  logger.info({
    graphqlApi: config.GRAPHQL_API_URL,
    redis: config.REDIS_URL,
    concurrency: config.WORKER_CONCURRENCY,
    maxRetries: config.MAX_RETRIES,
    schedulerEnabled: config.ENABLE_SCHEDULER,
    gracefulShutdownTimeoutMs: GRACEFUL_SHUTDOWN_TIMEOUT_MS,
  });

  initializeHealthEndpoints(3000);

  setupShutdownHandlers();

  void startScheduler(config);

  if (config.ENABLE_WORKERS) {
    startWorkers();
  } else {
    logger.info('Workers are disabled (ENABLE_WORKERS=false)');
  }
}

try {
  main();
} catch (error: unknown) {
  logger.error(error, 'Failed to start Agent Process');
  process.exit(1);
}
