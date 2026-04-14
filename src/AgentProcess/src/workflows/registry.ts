import { WorkflowDefinition } from './types.js';
import { logger } from '../observability/logger.js';
import { plannerWorkflow } from './planner.js';
import { coderWorkflow } from './coder.js';
import { devLeadWorkflow } from './devlead.js';
import { testerWorkflow } from './tester.js';

export class WorkflowRegistry {
  private workflows: Map<string, WorkflowDefinition<unknown, unknown>> = new Map();

  registerWorkflow<TInput, TOutput>(definition: WorkflowDefinition<TInput, TOutput>): void {
    logger.info(`Registering workflow: ${definition.name}`);
    this.workflows.set(definition.name, definition as WorkflowDefinition<unknown, unknown>);
  }

  getWorkflow(name: string): WorkflowDefinition<unknown, unknown> {
    const workflow = this.workflows.get(name);
    if (!workflow) {
      throw new Error(`Workflow not found: ${name}`);
    }
    return workflow;
  }

  hasWorkflow(name: string): boolean {
    return this.workflows.has(name);
  }

  listWorkflows(): string[] {
    return Array.from(this.workflows.keys());
  }
}

export const workflowRegistry = new WorkflowRegistry();

workflowRegistry.registerWorkflow(plannerWorkflow);
workflowRegistry.registerWorkflow(coderWorkflow);
workflowRegistry.registerWorkflow(devLeadWorkflow);
workflowRegistry.registerWorkflow(testerWorkflow);
