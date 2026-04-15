import { z } from 'zod';

const configSchema = z.object({
  NODE_ENV: z.string().default('development'),
  GRAPHQL_API_URL: z.string().url(), // eslint-disable-line @typescript-eslint/no-deprecated
  GRAPHQL_API_TOKEN: z.string().optional(),
  REDIS_URL: z.string().refine(
    (val) => /^redis\+sentinel?:\/\//.test(val) || z.string().url().safeParse(val).success,
    { message: 'REDIS_URL must be a valid Redis URL (redis://, rediss://, redis+sentinel://, or redis+sentinel+tls://)' }
  ),
  GITHUB_TOKEN: z.string().optional(),
  LOG_LEVEL: z.string().default('info'),
  WORKER_CONCURRENCY: z.string().default('2').transform((val) => Number.parseInt(val, 10)),
  MAX_RETRIES: z.string().default('3').transform((val) => Number.parseInt(val, 10)),
  OTLP_ENDPOINT: z.string().url().optional(), // eslint-disable-line @typescript-eslint/no-deprecated
  ENABLE_SCHEDULER: z.string().default('false').transform((val) => val === 'true'),
  ENABLE_WORKERS: z.string().default('true').transform((val) => val === 'true'),
  SCHEDULER_INTERVAL: z.string().default('30000').transform((val) => Number.parseInt(val, 10)),
  GRACEFUL_SHUTDOWN_TIMEOUT_MS: z.string().default('30000').transform((val) => Number.parseInt(val, 10)),
  DRY_RUN: z.string().default('false').transform((val) => val === 'true'),
  PROJECT_ID: z.string().optional(),
  DASHBOARD_TOKEN: z.string().optional(),
});

export type Config = z.infer<typeof configSchema>;

export function loadConfig(): Config {
  const result = configSchema.safeParse(process.env);

  if (!result.success) {
    const errors = result.error.issues.map((issue) => ({
      field: issue.path.join('.'),
      message: issue.message,
    }));

    console.error('Configuration validation failed:');
    for (const error of errors) {
      console.error(`  - ${error.field}: ${error.message}`);
    }

    throw new Error('Invalid configuration. See errors above.');
  }

  return result.data;
}
