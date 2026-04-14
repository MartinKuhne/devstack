import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

vi.mock('../observability/logger.js', () => ({
  logger: {
    info: vi.fn(),
    error: vi.fn(),
    warn: vi.fn(),
    debug: vi.fn(),
  },
}));

import { WorkflowRegistry } from './registry.js';
import { WorkflowDefinition, WorkflowResult, WorkflowContext } from './types.js';
import { z } from 'zod';

describe('WorkflowRegistry', () => {
  let registry: WorkflowRegistry;

  beforeEach(() => {
    registry = new WorkflowRegistry();
  });

  it('should register a workflow', () => {
    const definition: WorkflowDefinition<{ id: string }, { result: string }> = {
      name: 'test-workflow',
      inputSchema: z.object({ id: z.string() }),
      maxRetries: 3,
      timeout: 30000,
      run: async () => ({ ok: true, output: { result: 'success' }, events: [] }),
    };

    registry.registerWorkflow(definition);
    
    expect(registry.hasWorkflow('test-workflow')).toBe(true);
    expect(registry.listWorkflows()).toContain('test-workflow');
  });

  it('should get a registered workflow', () => {
    const definition: WorkflowDefinition<{ id: string }, { result: string }> = {
      name: 'test-workflow',
      inputSchema: z.object({ id: z.string() }),
      maxRetries: 3,
      timeout: 30000,
      run: async () => ({ ok: true, output: { result: 'success' }, events: [] }),
    };

    registry.registerWorkflow(definition);
    
    const retrieved = registry.getWorkflow('test-workflow');
    expect(retrieved.name).toBe('test-workflow');
  });

  it('should throw when getting non-existent workflow', () => {
    expect(() => registry.getWorkflow('non-existent')).toThrow('Workflow not found: non-existent');
  });

  it('should list all registered workflows', () => {
    const workflow1: WorkflowDefinition<{ id: string }, { result: string }> = {
      name: 'workflow-1',
      inputSchema: z.object({ id: z.string() }),
      maxRetries: 3,
      timeout: 30000,
      run: async () => ({ ok: true, output: { result: 'success' }, events: [] }),
    };

    const workflow2: WorkflowDefinition<{ id: string }, { result: string }> = {
      name: 'workflow-2',
      inputSchema: z.object({ id: z.string() }),
      maxRetries: 3,
      timeout: 30000,
      run: async () => ({ ok: true, output: { result: 'success' }, events: [] }),
    };

    registry.registerWorkflow(workflow1);
    registry.registerWorkflow(workflow2);
    
    const workflows = registry.listWorkflows();
    expect(workflows).toHaveLength(2);
    expect(workflows).toContain('workflow-1');
    expect(workflows).toContain('workflow-2');
  });
});
