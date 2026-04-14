import { describe, it, expect, beforeEach, vi } from 'vitest';

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
        action: 'pr_created',
        prNumber: 123,
        summary: 'PR created successfully',
        nextSteps: ['Wait for review'],
      }),
    }),
  }),
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

import { devLeadWorkflow } from './devlead.js';
import type { DevLeadWorkflowInput } from './devlead.js';
import type { WorkflowContext } from './types.js';
import { GraphQLClient } from 'graphql-request';
import { Logger } from 'pino';
import { Span } from '@opentelemetry/api';

describe('DevLeadWorkflow', () => {
  let mockApi: GraphQLClient;
  let mockLogger: Logger;
  let mockSpan: Span;
  let mockCancel: () => void;

  beforeEach(() => {
    mockApi = {
      request: vi.fn().mockResolvedValue({
        feature: {
          id: 'feature-123',
          title: 'Add user authentication',
          description: 'Implement JWT authentication',
          acceptanceCriteria: 'Users can login and logout',
          status: 'PLANNED',
          branchName: null,
          pullRequestNumber: null,
          project: {
            id: 'project-456',
            name: 'Test Project',
            memory: 'Test memory',
            architecture: 'Test architecture',
            codingStandards: 'Test standards',
            gitRepositoryUrl: 'https://github.com/test/repo.git',
          },
          tasks: [
            { id: 'task-1', title: 'Setup auth', status: 'DONE', complexity: 3 },
            { id: 'task-2', title: 'Add login', status: 'DONE', complexity: 5 },
          ],
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

  const createContext = (input: DevLeadWorkflowInput): WorkflowContext<DevLeadWorkflowInput, unknown> => ({
    input,
    api: mockApi,
    logger: mockLogger,
    span: mockSpan,
    attempt: 1,
    cancel: mockCancel,
  });

  it('should have correct workflow definition', () => {
    expect(devLeadWorkflow.name).toBe('devlead');
    expect(devLeadWorkflow.maxRetries).toBe(2);
    expect(devLeadWorkflow.timeout).toBe(300000);
    expect(devLeadWorkflow.inputSchema).toBeDefined();
  });

  it('should validate input schema', async () => {
    const validInput: DevLeadWorkflowInput = { featureId: 'feature-123' };
    const result = devLeadWorkflow.inputSchema.safeParse(validInput);
    expect(result.success).toBe(true);
  });

  it('should reject invalid input', async () => {
    const invalidInput = { featureId: '' };
    const result = devLeadWorkflow.inputSchema.safeParse(invalidInput);
    expect(result.success).toBe(false);
  });

  it('should handle feature not found', async () => {
    (mockApi.request as ReturnType<typeof vi.fn>).mockResolvedValueOnce({ feature: null });

    const input: DevLeadWorkflowInput = { featureId: 'nonexistent' };
    const ctx = createContext(input);

    const result = await devLeadWorkflow.run(ctx);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe('FEATURE_NOT_FOUND');
    }
  });

  it('should return waiting_for_tasks when tasks are pending', async () => {
    (mockApi.request as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      feature: {
        id: 'feature-123',
        title: 'Test Feature',
        description: 'Test',
        acceptanceCriteria: 'Test criteria',
        status: 'PLANNED',
        branchName: null,
        pullRequestNumber: null,
        project: {
          id: 'project-456',
          name: 'Test Project',
          memory: 'Test memory',
          architecture: 'Test architecture',
          codingStandards: 'Test standards',
          gitRepositoryUrl: 'https://github.com/test/repo.git',
        },
        tasks: [
          { id: 'task-1', title: 'Task 1', status: 'TODO', complexity: 3 },
        ],
      },
    });

    const input: DevLeadWorkflowInput = { featureId: 'feature-123' };
    const ctx = createContext(input);

    const result = await devLeadWorkflow.run(ctx);

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.output.action).toBe('waiting_for_tasks');
    }
  });

  it('should include events in result', async () => {
    const input: DevLeadWorkflowInput = { featureId: 'feature-123' };
    const ctx = createContext(input);

    const result = await devLeadWorkflow.run(ctx);

    expect(result.events).toBeDefined();
    expect(result.events.length).toBeGreaterThan(0);
  });
});
