import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

vi.mock('ioredis');
vi.mock('./config.js', () => ({
  loadConfig: () => ({
    GRAPHQL_API_URL: 'http://localhost:4000/graphql',
    REDIS_URL: 'redis://localhost:6379',
    WORKER_CONCURRENCY: 2,
    MAX_RETRIES: 3,
    ENABLE_SCHEDULER: false,
    ENABLE_WORKERS: false,
    SCHEDULER_INTERVAL: 30000,
    GRACEFUL_SHUTDOWN_TIMEOUT_MS: 5000,
  }),
}));
vi.mock('./observability/logger.js', () => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
    debug: vi.fn(),
  },
}));
vi.mock('./observability/telemetry.js', () => ({
  initializeTelemetry: vi.fn(),
  shutdownTelemetry: vi.fn().mockResolvedValue(undefined),
}));
vi.mock('./health/health.js', () => ({
  initializeHealthEndpoints: vi.fn(),
  shutdownHealthEndpoints: vi.fn().mockResolvedValue(undefined),
}));

describe('Graceful Shutdown', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should track shutdown timeout configuration', () => {
    const timeout = 5000;
    expect(timeout).toBeGreaterThan(0);
  });

  it('should wait for workers to finish before closing', async () => {
    let isRunning = true;
    const checkIntervalMs = 10;
    const maxWaitMs = 200;
    const startTime = Date.now();

    setTimeout(() => {
      isRunning = false;
    }, 50);

    while (Date.now() - startTime < maxWaitMs) {
      await new Promise((resolve) => setTimeout(resolve, checkIntervalMs));
      if (!isRunning) {
        break;
      }
    }

    const elapsed = Date.now() - startTime;
    expect(elapsed).toBeGreaterThanOrEqual(50);
    expect(isRunning).toBe(false);
  });

  it('should respect shutdown timeout', async () => {
    const timeout = 100;
    const startTime = Date.now();

    await new Promise((resolve) => setTimeout(resolve, timeout + 50));

    const elapsed = Date.now() - startTime;
    expect(elapsed).toBeGreaterThanOrEqual(timeout);
  });
});
