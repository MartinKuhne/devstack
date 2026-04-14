import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

vi.mock('ioredis', () => ({
  Redis: vi.fn().mockImplementation(() => ({
    ping: vi.fn().mockResolvedValue('PONG'),
    quit: vi.fn().mockResolvedValue(undefined),
  })),
}));

vi.mock('../config.js', () => ({
  loadConfig: () => ({
    GRAPHQL_API_URL: 'http://localhost:5000/graphql',
    REDIS_URL: 'redis://localhost:6379',
    LOG_LEVEL: 'info',
    WORKER_CONCURRENCY: 2,
    MAX_RETRIES: 3,
  }),
}));

vi.mock('../observability/logger.js', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
    warn: vi.fn(),
    debug: vi.fn(),
  },
}));

describe('Health', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(async () => {
    const { shutdownHealthEndpoints } = await import('./health.js');
    await shutdownHealthEndpoints();
  });

  it('should initialize health endpoints', async () => {
    const { initializeHealthEndpoints } = await import('./health.js');
    initializeHealthEndpoints(3456);

    const { isHealthy } = await import('./health.js');
    expect(isHealthy()).toBe(true);
  });
});
