import { describe, it, expect, beforeEach, afterEach, vi, beforeAll, afterAll } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { createGraphQLClient, withTraceContext } from './graphql-client.js';
import { ApiClient } from './api-client.js';

vi.mock('../config.js', () => ({
  loadConfig: () => ({
    GRAPHQL_API_URL: 'http://localhost:5000/graphql',
    REDIS_URL: 'redis://localhost:6379',
    LOG_LEVEL: 'info',
    WORKER_CONCURRENCY: 2,
    MAX_RETRIES: 3,
  }),
}));

const server = setupServer(
  http.post('http://localhost:5000/graphql', async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;

    if (typeof body.query === 'string' && body.query.includes('dashboardSummary')) {
      return HttpResponse.json({
        data: {
          dashboardSummary: {
            totalProjects: 5,
            totalFeatures: 10,
            totalTasks: 25,
            activeWorkflows: 2,
            recentActivity: [],
          },
        },
      });
    }

    if (typeof body.query === 'string' && body.query.includes('createProject')) {
      const variables = body.variables as Record<string, string | undefined>;
      return HttpResponse.json({
        data: {
          createProject: {
            id: 'proj-1',
            name: variables.name ?? '',
            description: variables.description,
            status: 'active',
            createdAt: new Date().toISOString(),
          },
        },
      });
    }

    return HttpResponse.json({ data: {} });
  })
);

beforeAll(() => { server.listen({ onUnhandledRequest: 'error' }); });
afterEach(() => { server.resetHandlers(); });
afterAll(() => { server.close(); });

describe('GraphQL Client', () => {
  it('should create a client with auth header', () => {
    const client = createGraphQLClient();
    expect(client).toBeDefined();
  });

  it('should create a client with trace context', () => {
    const client = createGraphQLClient();
    const tracedClient = withTraceContext(client, '00-traceid-spanid-01');
    expect(tracedClient).toBeDefined();
  });
});

describe('API Client', () => {
  let apiClient: ApiClient;

  beforeEach(() => {
    apiClient = new ApiClient(createGraphQLClient());
  });

  it('should get dashboard summary', async () => {
    const summary = await apiClient.getDashboardSummary();
    expect(summary.totalProjects).toBe(5);
    expect(summary.totalFeatures).toBe(10);
  });

  it('should create a project', async () => {
    const project = await apiClient.createProject('Test Project', 'Test description');
    expect(project.id).toBe('proj-1');
    expect(project.name).toBe('Test Project');
    expect(project.status).toBe('active');
  });
});
