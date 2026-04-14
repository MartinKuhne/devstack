import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

vi.mock('../config.js', () => ({
  loadConfig: vi.fn().mockReturnValue({
    graphqlApiUrl: 'http://test.local/graphql',
    redisUrl: 'redis://localhost:6379',
    LOG_LEVEL: 'info',
  }),
}));

vi.mock('../prompts/loader.js', () => ({
  renderPrompt: vi.fn().mockReturnValue('mocked prompt'),
}));

vi.mock('../llm/model.js', () => ({
  createModel: vi.fn().mockReturnValue({
    invoke: vi.fn().mockResolvedValue({
      content: JSON.stringify({
        filesModified: ['src/test.ts'],
        filesCreated: [],
        buildResult: 'SUCCESS',
        testResult: 'SUCCESS',
        summary: 'Task completed successfully',
      }),
    }),
  }),
}));

vi.mock('../api/graphql-client.js', () => ({
  createGraphQLClient: vi.fn(),
}));

vi.mock('node:child_process', () => ({
  execFile: vi.fn(),
  promisify: vi.fn(() => {
    return () => Promise.resolve({ stdout: '', stderr: '' });
  }),
}));

vi.mock('node:path', () => ({
  default: {
    join: vi.fn().mockImplementation((...args: string[]) => args.join('/')),
    dirname: vi.fn().mockImplementation((p: string) => p.split('/').slice(0, -1).join('/')),
    resolve: vi.fn().mockImplementation((...args: string[]) => args.join('/')),
    relative: vi.fn().mockImplementation((from: string, to: string) => to),
  },
}));

vi.mock('node:os', () => ({
  tmpdir: vi.fn().mockReturnValue('/tmp'),
}));

import { coderWorkflow } from './coder.js';
import type { CoderWorkflowInput } from './coder.js';
import type { WorkflowContext } from './types.js';
import { GraphQLClient } from 'graphql-request';
import { Logger } from 'pino';
import { Span } from '@opentelemetry/api';

describe('CoderWorkflow', () => {
  let mockApi: GraphQLClient;
  let mockLogger: Logger;
  let mockSpan: Span;
  let mockCancel: () => void;

  beforeEach(() => {
    mockApi = {
      request: vi.fn().mockResolvedValue({
        task: {
          id: 'task-123',
          title: 'Add user authentication',
          description: 'Implement JWT authentication',
          status: 'TODO',
          complexity: 5,
          feature: {
            id: 'feature-456',
            title: 'Authentication System',
            description: 'Add authentication to the app',
            acceptanceCriteria: 'Users can login and logout',
            project: {
              id: 'project-789',
              name: 'Test Project',
              memory: 'Test memory',
              architecture: 'Test architecture',
              codingStandards: 'Test standards',
              gitRepositoryUrl: 'https://github.com/test/repo.git',
            },
          },
        },
      }),
    } as unknown as GraphQLClient;

    mockLogger = {
      info: vi.fn(),
      error: vi.fn(),
      warn: vi.fn(),
      debug: vi.fn(),
    } as unknown as Logger;

    mockSpan = {
      setAttribute: vi.fn(),
      addEvent: vi.fn(),
      end: vi.fn(),
    } as unknown as Span;

    mockCancel = vi.fn();
  });

  const createContext = (input: CoderWorkflowInput): WorkflowContext<CoderWorkflowInput, unknown> => ({
    input,
    api: mockApi as unknown as GraphQLClient,
    logger: mockLogger as unknown as Logger,
    span: mockSpan as unknown as Span,
    attempt: 1,
    cancel: mockCancel,
  });

  it('should have correct workflow definition', () => {
    expect(coderWorkflow.name).toBe('coder');
    expect(coderWorkflow.maxRetries).toBe(2);
    expect(coderWorkflow.timeout).toBe(600000);
    expect(coderWorkflow.inputSchema).toBeDefined();
  });

  it('should validate input schema', async () => {
    const validInput: CoderWorkflowInput = { taskId: 'task-123' };
    const result = coderWorkflow.inputSchema.safeParse(validInput);
    expect(result.success).toBe(true);
  });

  it('should reject invalid input', async () => {
    const invalidInput = { taskId: '' };
    const result = coderWorkflow.inputSchema.safeParse(invalidInput);
    expect(result.success).toBe(false);
  });

  it('should handle task not found', async () => {
    (mockApi.request as ReturnType<typeof vi.fn>).mockResolvedValueOnce({ task: null });

    const input: CoderWorkflowInput = { taskId: 'nonexistent' };
    const ctx = createContext(input);

    const result = await coderWorkflow.run(ctx);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe('TASK_NOT_FOUND');
    }
  });

  it('should handle workflow errors gracefully', async () => {
    (mockApi.request as ReturnType<typeof vi.fn>).mockRejectedValueOnce(new Error('API error'));

    const input: CoderWorkflowInput = { taskId: 'task-123' };
    const ctx = createContext(input);

    const result = await coderWorkflow.run(ctx);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.retryable).toBe(true);
    }
  });

  it('should include events in result', async () => {
    const input: CoderWorkflowInput = { taskId: 'task-123' };
    const ctx = createContext(input);

    const result = await coderWorkflow.run(ctx);

    expect(result.events).toBeDefined();
    expect(result.events.length).toBeGreaterThan(0);
  });
});
