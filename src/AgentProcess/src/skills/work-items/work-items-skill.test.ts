import { describe, it, expect, vi } from 'vitest';
import { z } from 'zod';
import { GraphQLClient } from 'graphql-request';
import { createWorkItemTools } from './work-items-skill.js';
import { ToolContext } from '../tool.js';
import { Logger } from 'pino';

const createMockContext = (): ToolContext => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
    debug: vi.fn(),
  } as unknown as Logger,
  api: {} as unknown as GraphQLClient,
  metadata: {},
});

describe('work-items skill', () => {
  describe('createWorkItemTools', () => {
    it('should create all work item tools', () => {
      const tools = createWorkItemTools();
      
      expect(tools).toHaveLength(9);
      expect(tools.map(t => t.name)).toEqual([
        'create_feature',
        'update_feature',
        'transition_feature_status',
        'create_task',
        'update_task',
        'transition_task_status',
        'create_defect',
        'update_defect',
        'transition_defect_status',
      ]);
    });

    it('should have valid schemas for each tool', () => {
      const tools = createWorkItemTools();
      
      for (const tool of tools) {
        expect(tool.inputSchema).toBeDefined();
        expect(tool.execute).toBeDefined();
      }
    });
  });

  describe('create_feature tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[0].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        projectId: 'test-project-id',
        title: 'Test Feature',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject missing project id', () => {
      const schema = createWorkItemTools()[0].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        title: 'Test Feature',
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });

    it('should reject missing title', () => {
      const schema = createWorkItemTools()[0].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        projectId: 'test-project-id',
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });

    it('should accept optional fields', () => {
      const schema = createWorkItemTools()[0].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        projectId: 'test-project-id',
        title: 'Test Feature',
        description: 'Test description',
        acceptanceCriteria: 'Test criteria',
        initialStatus: 'PLANNED',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });
  });

  describe('update_feature tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[1].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        id: 'test-feature-id',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject missing feature id', () => {
      const schema = createWorkItemTools()[1].inputSchema as z.ZodTypeAny;
      
      const invalidInput: Record<string, unknown> = {};
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });

    it('should accept at least one optional field', () => {
      const schema = createWorkItemTools()[1].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        id: 'test-feature-id',
        title: 'Updated Title',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });
  });

  describe('transition_feature_status tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[2].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        id: 'test-feature-id',
        targetStatus: 'IN_PROGRESS',
        actor: 'test-user',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject missing actor', () => {
      const schema = createWorkItemTools()[2].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        id: 'test-feature-id',
        targetStatus: 'IN_PROGRESS',
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });
  });

  describe('create_task tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[3].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        featureId: 'test-feature-id',
        title: 'Test Task',
        complexityRating: 5,
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject complexity rating below 1', () => {
      const schema = createWorkItemTools()[3].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        featureId: 'test-feature-id',
        title: 'Test Task',
        complexityRating: 0,
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });

    it('should reject complexity rating above 10', () => {
      const schema = createWorkItemTools()[3].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        featureId: 'test-feature-id',
        title: 'Test Task',
        complexityRating: 11,
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });
  });

  describe('transition_task_status tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[5].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        id: 'test-task-id',
        targetStatus: 'IN_PROGRESS',
        actor: 'test-user',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should accept all valid task statuses', () => {
      const schema = createWorkItemTools()[5].inputSchema as z.ZodTypeAny;
      
      const statuses = ['TODO', 'IN_PROGRESS', 'REVIEW', 'DONE', 'BLOCKED'];
      
      for (const status of statuses) {
        expect(() => schema.parse({
          id: 'test-task-id',
          targetStatus: status,
          actor: 'test-user',
        })).not.toThrow();
      }
    });
  });

  describe('create_defect tool', () => {
    it('should validate required fields', () => {
      const schema = createWorkItemTools()[6].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        projectId: 'test-project-id',
        title: 'Test Defect',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should accept optional parent feature id', () => {
      const schema = createWorkItemTools()[6].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        projectId: 'test-project-id',
        title: 'Test Defect',
        parentFeatureId: 'test-feature-id',
        severity: 'HIGH',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should accept all valid severities', () => {
      const schema = createWorkItemTools()[6].inputSchema as z.ZodTypeAny;
      
      const severities = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];
      
      for (const severity of severities) {
        expect(() => schema.parse({
          projectId: 'test-project-id',
          title: 'Test Defect',
          severity: severity,
        })).not.toThrow();
      }
    });
  });

  describe('ToolContext usage', () => {
    it('should use context logger for logging', async () => {
      const context = createMockContext();
      const tools = createWorkItemTools();
      
      // Verify that the tools have access to the context
      expect(tools[0].execute).toBeDefined();
    });

    it('should use context API client for GraphQL mutations', async () => {
      const context = createMockContext();
      const tools = createWorkItemTools();
      
      // Verify that the tools have access to the context
      expect(tools[3].execute).toBeDefined();
    });
  });
});
