import express from 'express';

const app = express();

// Health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok', service: 'agent-process-dashboard' });
});

const PORT = parseInt(process.env.DASHBOARD_PORT || '3001');
const HOST = process.env.DASHBOARD_HOST || '0.0.0.0';

app.listen(PORT, HOST, () => {
  console.log(`Agent Process Dashboard running on http://${HOST}:${PORT}`);
});

export default app;