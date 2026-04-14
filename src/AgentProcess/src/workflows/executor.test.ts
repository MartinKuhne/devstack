import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { executeWorkflow } from './executor.js';
import { workflowRegistry } from './registry.js';
import { WorkflowResult, WorkflowFailureResult } from './types.js';
import { z } from 'zod';

vi.mock('../api/graphql-client.js', () => ({
  createGraphQLClient: () => ({
    request: vi.fn(),
  }),
}));

vi.mock('../observability/logger.js', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
    warn: vi.fn(),
    debug: vi.fn(),
  },
}));

vi.mock('../observability/telemetry.js', () => ({
  getTracer: () => ({
    startActiveSpan: async (name: string, fn: any) => {
      const span = {
        setStatus: vi.fn(),
        end: vi.fn(),
      };
      return await fn(span);
    },
  }),
  workflowMetrics: {
    workflowDurationHistogram: {
      record: vi.fn(),
    },
    workflowRunCounter: {
      add: vi.fn(),
    },
    workflowFailuresCounter: {
      add: vi.fn(),
    },
  },
}));

describe('executeWorkflow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should execute a successful workflow', async () => {
    const workflowName = 'test-success-workflow';
    const input = { id: '123', name: 'test' };

    const mockResult: WorkflowResult<{ success: boolean }> = {
      ok: true as const,
      output: { success: true },
      events: [],
    };

    const workflowDefinition = {
      name: workflowName,
      inputSchema: z.object({
        id: z.string(),
        name: z.string(),
      }),
      maxRetries: 3,
      timeout: 30000,
      run: async (): Promise<WorkflowResult<{ success: boolean }>> => mockResult,
    };

    workflowRegistry.registerWorkflow(workflowDefinition);

    const result = await executeWorkflow(workflowName, 'job-123', input);

    expect(result.ok).toBe(true);
    expect(result.output).toEqual({ success: true });
  });

  it('should validate input against schema', async () => {
    const workflowName = 'test-validation-workflow';
    const input = { id: 123 };

    const workflowDefinition = {
      name: workflowName,
      inputSchema: z.object({
        id: z.string(),
        name: z.string(),
      }),
      maxRetries: 3,
      timeout: 30000,
      run: async (): Promise<WorkflowResult<{ success: boolean }>> => ({
        ok: true as const,
        output: { success: true },
        events: [],
      }),
    };

    workflowRegistry.registerWorkflow(workflowDefinition);

    await expect(executeWorkflow(workflowName, 'job-123', input)).rejects.toThrow();
  });

  it('should handle workflow failure', async () => {
    const workflowName = 'test-failure-workflow';
    const input = { id: '123' };

    const workflowDefinition = {
      name: workflowName,
      inputSchema: z.object({
        id: z.string(),
      }),
      maxRetries: 3,
      timeout: 30000,
      run: async (): Promise<WorkflowFailureResult> => ({
        ok: false as const,
        error: {
          code: 'BUSINESS_ERROR',
          message: 'Something went wrong',
          retryable: false,
        },
        events: [],
      }),
    };

    workflowRegistry.registerWorkflow(workflowDefinition);

    await expect(executeWorkflow(workflowName, 'job-123', input)).rejects.toThrow('Something went wrong');
  });

  it('should throw error for non-existent workflow', async () => {
    const input = { id: '123' };

    await expect(executeWorkflow('non-existent-workflow', 'job-123', input)).rejects.toThrow(
      'Workflow not found: non-existent-workflow'
    );
  });
});
