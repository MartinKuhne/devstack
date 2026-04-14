import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { plannerWorkflow, PlannerWorkflowInput } from './planner.js';
import { WorkflowContext, WorkflowResult, WorkflowFailureResult } from './types.js';

vi.mock('../prompts/loader.js', () => ({
  renderPrompt: vi.fn().mockReturnValue('Mocked prompt text'),
}));

vi.mock('../llm/model.js', () => ({
  createModel: vi.fn().mockReturnValue({
    invoke: vi.fn().mockResolvedValue({
      content: JSON.stringify({
        plan: 'Test plan',
        tasks: [
          {
            title: 'Task 1',
            deliverable: 'Deliverable 1',
            acceptanceCriteria: 'Criteria 1',
            risks: '',
            complexityRating: 5,
          },
        ],
        openQuestions: [],
        securityImpact: 'None',
        performanceImpact: 'None',
        testPlan: 'Run tests',
        deploymentPlan: 'Deploy normally',
      }),
    }),
  }),
  countTokens: vi.fn().mockResolvedValue(100),
}));

vi.mock('../api/graphql-client.js', () => ({
  createGraphQLClient: vi.fn(),
}));

vi.mock('../observability/logger.js', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
    debug: vi.fn(),
    warn: vi.fn(),
  },
}));

vi.mock('../config.js', () => ({
  loadConfig: vi.fn().mockReturnValue({
    GRAPHQL_API_URL: 'http://test.local',
    GRAPHQL_API_TOKEN: 'test-token',
    REDIS_URL: 'redis://localhost:6379',
    LOG_LEVEL: 'info',
    WORKER_CONCURRENCY: 2,
    MAX_RETRIES: 3,
    ENABLE_SCHEDULER: false,
    ENABLE_WORKERS: true,
    SCHEDULER_INTERVAL: 30000,
    GRACEFUL_SHUTDOWN_TIMEOUT_MS: 30000,
  }),
}));

describe('PlannerWorkflow', () => {
  let mockApiRequest: ReturnType<typeof vi.fn>;
  let mockSpan: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    vi.clearAllMocks();
    
    mockApiRequest = vi.fn();
    mockSpan = {
      setStatus: vi.fn(),
      end: vi.fn(),
    } as any;
    
    process.env.GRAPHQL_API_URL = 'http://test.local';
    process.env.REDIS_URL = 'redis://localhost:6379';
  });

  afterEach(() => {
    delete process.env.GRAPHQL_API_URL;
    delete process.env.REDIS_URL;
  });

  const createMockContext = (input: PlannerWorkflowInput): WorkflowContext<PlannerWorkflowInput, unknown> => ({
    input,
    api: { request: mockApiRequest } as any,
    logger: {
      info: vi.fn(),
      error: vi.fn(),
      debug: vi.fn(),
      warn: vi.fn(),
    } as any,
    span: mockSpan as any,
    attempt: 1,
    cancel: vi.fn(),
  });

  describe('inputSchema validation', () => {
    it('should validate valid input', () => {
      const validInput = { featureId: 'feature-123' };
      const result = plannerWorkflow.inputSchema.safeParse(validInput);
      expect(result.success).toBe(true);
    });

    it('should reject missing featureId', () => {
      const invalidInput = {} as PlannerWorkflowInput;
      const result = plannerWorkflow.inputSchema.safeParse(invalidInput);
      expect(result.success).toBe(false);
    });

    it('should reject empty featureId', () => {
      const invalidInput = { featureId: '' };
      const result = plannerWorkflow.inputSchema.safeParse(invalidInput);
      expect(result.success).toBe(false);
    });
  });

  describe('workflow metadata', () => {
    it('should have correct name', () => {
      expect(plannerWorkflow.name).toBe('planner');
    });

    it('should have correct maxRetries', () => {
      expect(plannerWorkflow.maxRetries).toBe(3);
    });

    it('should have correct timeout', () => {
      expect(plannerWorkflow.timeout).toBe(300000);
    });
  });

  describe('run method', () => {
    it('should return failure when feature not found', async () => {
      mockApiRequest.mockRejectedValueOnce(new Error('Feature not found'));

      const ctx = createMockContext({ featureId: 'non-existent-feature' });
      const result = await plannerWorkflow.run(ctx);

      expect(result.ok).toBe(false);
      if (!result.ok) {
        expect(result.error.code).toBe('PLANNER_WORKFLOW_ERROR');
      }
    });

    it('should handle LLM response and create tasks', async () => {
      const mockFeature = {
        feature: {
          id: 'feature-123',
          title: 'Test Feature',
          description: 'A test feature',
          acceptanceCriteria: 'Must pass tests',
          project: {
            id: 'project-1',
            memory: 'Project memory',
            architecture: 'Architecture docs',
            codingStandards: 'Coding standards',
          },
          defects: [],
        },
      };

      mockApiRequest
        .mockResolvedValueOnce(mockFeature)
        .mockResolvedValueOnce({ createTask: { id: 'task-1', title: 'Task 1' } })
        .mockResolvedValueOnce({ updateFeature: { id: 'feature-123' } })
        .mockResolvedValueOnce({ transitionFeatureStatus: { id: 'feature-123', status: 'IN_PROGRESS' } });

      const ctx = createMockContext({ featureId: 'feature-123' });
      const result = await plannerWorkflow.run(ctx);

      expect(result.ok).toBe(true);
      if (result.ok) {
        expect(result.output.tasksCreated).toHaveLength(1);
        expect(result.output.openQuestions).toHaveLength(0);
      }
    });

    it('should transition feature to IN_REVIEW when open questions exist', async () => {
      vi.mocked(await import('../llm/model.js')).createModel.mockReturnValueOnce({
        invoke: vi.fn().mockResolvedValue({
          content: JSON.stringify({
            plan: 'Test plan',
            tasks: [
              {
                title: 'Task 1',
                deliverable: 'Deliverable 1',
                acceptanceCriteria: 'Criteria 1',
                risks: '',
                complexityRating: 5,
              },
            ],
            openQuestions: ['Question about implementation'],
            securityImpact: 'None',
            performanceImpact: 'None',
            testPlan: 'Run tests',
            deploymentPlan: 'Deploy normally',
          }),
        }),
      } as never);

      const mockFeature = {
        feature: {
          id: 'feature-123',
          title: 'Test Feature',
          description: 'A test feature',
          acceptanceCriteria: 'Must pass tests',
          project: {
            id: 'project-1',
            memory: 'Project memory',
            architecture: 'Architecture docs',
            codingStandards: 'Coding standards',
          },
          defects: [],
        },
      };

      mockApiRequest
        .mockResolvedValueOnce(mockFeature)
        .mockResolvedValueOnce({ createTask: { id: 'task-1', title: 'Task 1' } })
        .mockResolvedValueOnce({ updateFeature: { id: 'feature-123' } })
        .mockResolvedValueOnce({ transitionFeatureStatus: { id: 'feature-123', status: 'IN_REVIEW' } });

      const ctx = createMockContext({ featureId: 'feature-123' });
      const result = await plannerWorkflow.run(ctx);

      expect(result.ok).toBe(true);
      if (result.ok) {
        expect(result.output.openQuestions).toHaveLength(1);
      }
    });

    it('should handle multiple tasks creation', async () => {
      vi.mocked(await import('../llm/model.js')).createModel.mockReturnValueOnce({
        invoke: vi.fn().mockResolvedValue({
          content: JSON.stringify({
            plan: 'Test plan',
            tasks: [
              {
                title: 'Task 1',
                deliverable: 'Deliverable 1',
                acceptanceCriteria: 'Criteria 1',
                risks: '',
                complexityRating: 3,
              },
              {
                title: 'Task 2',
                deliverable: 'Deliverable 2',
                acceptanceCriteria: 'Criteria 2',
                risks: '',
                complexityRating: 5,
              },
              {
                title: 'Task 3',
                deliverable: 'Deliverable 3',
                acceptanceCriteria: 'Criteria 3',
                risks: '',
                complexityRating: 7,
              },
            ],
            openQuestions: [],
            securityImpact: 'None',
            performanceImpact: 'None',
            testPlan: 'Run tests',
            deploymentPlan: 'Deploy normally',
          }),
        }),
      } as never);

      const mockFeature = {
        feature: {
          id: 'feature-123',
          title: 'Test Feature',
          description: 'A test feature',
          acceptanceCriteria: 'Must pass tests',
          project: {
            id: 'project-1',
            memory: 'Project memory',
            architecture: 'Architecture docs',
            codingStandards: 'Coding standards',
          },
          defects: [],
        },
      };

      mockApiRequest
        .mockResolvedValueOnce(mockFeature)
        .mockResolvedValueOnce({ createTask: { id: 'task-1', title: 'Task 1' } })
        .mockResolvedValueOnce({ createTask: { id: 'task-2', title: 'Task 2' } })
        .mockResolvedValueOnce({ createTask: { id: 'task-3', title: 'Task 3' } })
        .mockResolvedValueOnce({ updateFeature: { id: 'feature-123' } })
        .mockResolvedValueOnce({ transitionFeatureStatus: { id: 'feature-123', status: 'IN_PROGRESS' } });

      const ctx = createMockContext({ featureId: 'feature-123' });
      const result = await plannerWorkflow.run(ctx);

      expect(result.ok).toBe(true);
      if (result.ok) {
        expect(result.output.tasksCreated).toHaveLength(3);
      }
    });

    it('should handle errors gracefully', async () => {
      const mockFeature = {
        feature: {
          id: 'feature-123',
          title: 'Test Feature',
          description: 'A test feature',
          acceptanceCriteria: 'Must pass tests',
          project: {
            id: 'project-1',
            memory: 'Project memory',
            architecture: 'Architecture docs',
            codingStandards: 'Coding standards',
          },
          defects: [],
        },
      };

      mockApiRequest
        .mockResolvedValueOnce(mockFeature)
        .mockRejectedValueOnce(new Error('Task creation failed'));

      const ctx = createMockContext({ featureId: 'feature-123' });
      const result = await plannerWorkflow.run(ctx);

      expect(result.ok).toBe(false);
      if (!result.ok) {
        expect(result.error.retryable).toBe(true);
      }
    });
  });
});
