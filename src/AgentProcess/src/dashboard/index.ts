import express from 'express';
import { Queue } from 'bullmq';
import { Redis } from 'ioredis';
import { loadConfig } from '../config.js';
import { logger } from '../observability/logger.js';
import { queues } from '../queues/queues.js';

const config = loadConfig();
const app = express();
const PORT = process.env.DASHBOARD_PORT || 3001;

// Middleware
app.use(express.json());

// Basic auth middleware for production
const auth = (req: express.Request, res: express.Response, next: express.NextFunction) => {
  if (config.NODE_ENV === 'production') {
    const authHeader = req.headers.authorization;
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({ error: 'Unauthorized' });
    }
    
    const token = authHeader.substring(7);
    if (token !== config.DASHBOARD_TOKEN) {
      return res.status(403).json({ error: 'Forbidden' });
    }
  }
  next();
};

// Apply auth in production
if (config.NODE_ENV === 'production') {
  app.use(auth);
}

// Routes
app.get('/', (req, res) => {
  res.json({
    service: 'DevStack Agent Process Dashboard',
    version: '0.1.0',
    endpoints: {
      queues: '/api/queues',
      queue: '/api/queues/:name',
      stats: '/api/stats',
      health: '/health'
    }
  });
});

app.get('/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

app.get('/api/queues', async (req, res) => {
  try {
    const queuePromises = Object.entries(queues).map(async ([name, queue]) => {
      const [waiting, active, completed, failed] = await Promise.all([
        queue.getWaitingCount(),
        queue.getActiveCount(),
        queue.getCompletedCount(),
        queue.getFailedCount()
      ]);
      
      return {
        name,
        waiting,
        active,
        completed,
        failed
      };
    });
    
    const queueStats = await Promise.all(queuePromises);
    res.json({ queues: queueStats });
  } catch (error) {
    logger.error({ error }, 'Failed to get queue stats');
    res.status(500).json({ error: 'Internal server error' });
  }
});

app.get('/api/queues/:name', async (req, res) => {
  try {
    const queueName = req.params.name;
    const queue = queues[queueName as keyof typeof queues];
    
    if (!queue) {
      return res.status(404).json({ error: 'Queue not found' });
    }
    
    const [waiting, active, completed, failed] = await Promise.all([
      queue.getWaitingCount(),
      queue.getActiveCount(),
      queue.getCompletedCount(),
      queue.getFailedCount()
    ]);
    
    // Get sample jobs from each state
    const [waitingJobs, activeJobs, completedJobs, failedJobs] = await Promise.all([
      queue.getWaiting(0, 10),
      queue.getActive(0, 10),
      queue.getCompleted(0, 10),
      queue.getFailed(0, 10)
    ]);
    
    res.json({
      name: queueName,
      counts: { waiting, active, completed, failed },
      jobs: {
        waiting: waitingJobs,
        active: activeJobs,
        completed: completedJobs,
        failed: failedJobs
      }
    });
  } catch (error) {
    logger.error({ error: error, queueName: req.params.name }, 'Failed to get queue details');
    res.status(500).json({ error: 'Internal server error' });
  }
});

app.get('/api/stats', async (req, res) => {
  try {
    const redis = new Redis(config.REDIS_URL);
    const info = await redis.info();
    await redis.quit();
    
    res.json({
      redis: info,
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    logger.error({ error }, 'Failed to get Redis stats');
    res.status(500).json({ error: 'Internal server error' });
  }
});

// Start server
const server = app.listen(PORT, () => {
  logger.info({ port: PORT, env: config.NODE_ENV }, `Dashboard server started`);
});

export default server;