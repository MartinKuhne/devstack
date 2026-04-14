import { defineConfig } from 'vitest/config';

process.env.GRAPHQL_API_URL = 'http://localhost:5000/graphql';
process.env.REDIS_URL = 'redis://localhost:6379';

export default defineConfig({
  test: {
    include: ['src/**/*.test.ts', 'tests/e2e/**/*.test.ts'],
    environment: 'node',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
    },
  },
});
