# BullMQ Dashboard

This package provides a development dashboard for monitoring BullMQ queues used by the Agent Process worker.

## Features

- Real-time queue monitoring
- Job inspection (waiting, active, completed, failed)
- Queue statistics and metrics
- Ability to retry or remove jobs
- Clean, responsive UI based on Shadcn UI

## Usage

### Development

To run the dashboard in development:

```bash
npm run dashboard
```

The dashboard will be available at http://localhost:3001 by default.

### Configuration

The dashboard can be configured via environment variables:

- `REDIS_HOST` - Redis server hostname (default: localhost)
- `REDIS_PORT` - Redis server port (default: 6379)
- `REDIS_PASSWORD` - Redis password (if required)
- `DASHBOARD_HOST` - Host to bind the dashboard server (default: 0.0.0.0)
- `DASHBOARD_PORT` - Port to run the dashboard on (default: 3001)

### Production Considerations

In production environments, the dashboard should either:
1. Be disabled (don't start the dashboard process)
2. Be protected behind authentication and access controls
3. Be run on an internal network only

The dashboard does not include any built-in authentication mechanisms, so it should not be exposed directly to the internet in production environments.

## Implementation Details

The dashboard uses:
- [@queuedash/api](https://www.npmjs.com/package/@queuedash/api) for the BullMQ adapter
- [@queuedash/ui](https://www.npmjs.com/package/@queuedash/ui) for the frontend UI
- Express.js for serving the dashboard

It connects to the same Redis instance used by the BullMQ queues in the Agent Process worker.

## Monitoring Capabilities

The dashboard provides visibility into:
- Worker queue (primary queue for agent workflows)
- Job states: waiting, active, completed, delayed, failed
- Job details: payload, progress, logs, timestamps
- Queue metrics: throughput, latency, job counts