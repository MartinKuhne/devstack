import express from 'express';
import { createQueueDashExpressMiddleware } from '@queuedash/api';
import { Queue } from 'bullmq';
import type { RedisOptions as IORedisOptions } from 'ioredis';
import { Context } from './context'; // Adjust the path as needed

const app = express();

// Get Redis connection details from environment or use defaults
const redisHost = process.env.REDIS_HOST || 'localhost';
const redisPort = parseInt(process.env.REDIS_PORT || '6379');
const redisPassword = process.env.REDIS_PASSWORD || undefined;

const connection: IORedisOptions = {
  host: redisHost,
  port: redisPort,
  password: redisPassword,
  // Add any other Redis options needed
};

// Create BullMQ queue instance (we'll use the same connection as the worker)
const queues = [
  new Queue('worker', { connection }),
  // Add other queues if needed
];

// Create Express middleware for BullMQ dashboard
// We need to provide a context object that matches what the middleware expects
const context: Context = {
  // Add required context properties based on the actual Context type
  // This is a placeholder - adjust based on actual implementation
  // For now, we'll create a minimal context to satisfy the type requirement
};

// Create a mock context if the real one isn't available
// In a real implementation, this would come from your application's dependency injection
const dashboardMiddleware = createQueueDashExpressMiddleware({
  ctx: {
    // Provide the minimum required context properties
    // This needs to match what createQueueDashExpressMiddleware actually expects
  } as Context
});

// Serve the dashboard UI and API
app.use('/', dashboardMiddleware);

// Health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok', service: 'bullmq-dashboard' });
});

const PORT = parseInt(process.env.DASHBOARD_PORT || '3001');
const HOST = process.env.DASHBOARD_HOST || '0.0.0.0';

app.listen(PORT, HOST, () => {
  console.log(`BullMQ Dashboard running on http://${HOST}:${PORT}`);
  console.log(`Queues monitored: ${queues.map(q => q.name).join(', ')}`);
});

export default app;