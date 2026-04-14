import { Redis } from 'ioredis';
import { GraphQLClient } from 'graphql-request';
import { loadConfig } from './config.js';
import { logger } from './observability/logger.js';
import { initializeTelemetry, shutdownTelemetry } from './observability/telemetry.js';
import { initializeHealthEndpoints, shutdownHealthEndpoints } from './health/health.js';
import { PollingScheduler } from './queues/polling-scheduler.js';

let scheduler: PollingScheduler | null = null;
let redisConnection: Redis | null = null;
let apiClient: GraphQLClient | null = null;

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

function main(): void {
  const config = loadConfig();

  initializeTelemetry();

  logger.info('Agent Process started');
  logger.info({
    graphqlApi: config.GRAPHQL_API_URL,
    redis: config.REDIS_URL,
    concurrency: config.WORKER_CONCURRENCY,
    maxRetries: config.MAX_RETRIES,
    schedulerEnabled: config.ENABLE_SCHEDULER,
  });

  initializeHealthEndpoints(3000);

  void startScheduler(config);

  process.on('SIGTERM', () => {
    logger.info('Received SIGTERM, shutting down...');
    void shutdownScheduler()
      .then(() => shutdownHealthEndpoints())
      .then(() => shutdownTelemetry())
      .finally(() => process.exit(0));
  });

  process.on('SIGINT', () => {
    logger.info('Received SIGINT, shutting down...');
    void shutdownScheduler()
      .then(() => shutdownHealthEndpoints())
      .then(() => shutdownTelemetry())
      .finally(() => process.exit(0));
  });
}

try {
  main();
} catch (error: unknown) {
  logger.error(error, 'Failed to start Agent Process');
  process.exit(1);
}
