import express from 'express';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { logger } from '../observability/logger.js';
import type { Server } from 'node:http';

const config = loadConfig();

let redisConnection: Redis | null = null;
let healthServer: express.Express | null = null;
let server: Server | null = null;

export function initializeHealthEndpoints(port = 3000): void {
  redisConnection = new Redis(config.REDIS_URL);
  healthServer = express();

  healthServer.get('/health/live', (_req, res) => {
    res.json({ status: 'ok', timestamp: new Date().toISOString() });
  });

  healthServer.get('/health/ready', async (_req, res) => {
    try {
      if (!redisConnection) {
        res.status(503).json({ status: 'not ready', reason: 'Redis not initialized' });
        return;
      }

      await redisConnection.ping();
      res.json({ status: 'ok', timestamp: new Date().toISOString(), redis: 'connected' });
    } catch (error) {
      logger.error(error, 'Health check failed');
      res.status(503).json({ status: 'not ready', reason: 'Redis connection failed' });
    }
  });

  server = healthServer.listen(port, () => {
    logger.info({ port }, 'Health endpoints started');
  });
}

export async function shutdownHealthEndpoints(): Promise<void> {
  if (server) {
    await new Promise((resolve) => server?.close(resolve));
    server = null;
  }

  if (redisConnection) {
    await redisConnection.quit();
    redisConnection = null;
  }

  healthServer = null;
}

export function isHealthy(): boolean {
  return redisConnection !== null && healthServer !== null;
}
