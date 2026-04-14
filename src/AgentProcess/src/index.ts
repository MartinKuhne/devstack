import { loadConfig } from './config.js';
import { logger } from './observability/logger.js';
import { initializeTelemetry, shutdownTelemetry } from './observability/telemetry.js';
import { initializeHealthEndpoints, shutdownHealthEndpoints } from './health/health.js';

function main(): void {
  const config = loadConfig();

  initializeTelemetry();

  logger.info('Agent Process started');
  logger.info({
    graphqlApi: config.GRAPHQL_API_URL,
    redis: config.REDIS_URL,
    concurrency: config.WORKER_CONCURRENCY,
    maxRetries: config.MAX_RETRIES,
  });

  initializeHealthEndpoints(3000);

  process.on('SIGTERM', () => {
    logger.info('Received SIGTERM, shutting down...');
    void shutdownHealthEndpoints()
      .then(() => shutdownTelemetry())
      .finally(() => process.exit(0));
  });

  process.on('SIGINT', () => {
    logger.info('Received SIGINT, shutting down...');
    void shutdownHealthEndpoints()
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
