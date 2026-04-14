import { z } from 'zod';
import { WorkflowDefinition, WorkflowContext, WorkflowResult, WorkflowFailureResult, WorkflowEvent } from './types.js';
import { renderPrompt } from '../prompts/loader.js';
import { createModel, type ModelConfig } from '../llm/model.js';
import type { BaseChatModel } from '@langchain/core/language_models/chat_models';
import { createGraphQLClient } from '../api/graphql-client.js';
import { logger } from '../observability/logger.js';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import path from 'node:path';
import os from 'node:os';

const execFileAsync = promisify(execFile);

const DevLeadWorkflowInputSchema = z.object({
  featureId: z.string().min(1, 'Feature ID is required'),
});

const DevLeadWorkflowOutputSchema = z.object({
  action: z.enum(['branch_created', 'pr_created', 'rework_requested', 'feature_completed', 'waiting_for_tasks']),
  branchName: z.string().optional(),
  prNumber: z.number().optional(),
  tasksCreated: z.array(z.string()).optional(),
  summary: z.string(),
  nextSteps: z.array(z.string()),
});

export type DevLeadWorkflowInput = z.infer<typeof DevLeadWorkflowInputSchema>;
export type DevLeadWorkflowOutput = z.infer<typeof DevLeadWorkflowOutputSchema>;

function createDevLeadModel(): BaseChatModel {
  const modelConfig: ModelConfig = {
    modelId: process.env.DEFAULT_MODEL_ID || 'default',
    provider: (process.env.DEFAULT_MODEL_PROVIDER as 'openai' | 'anthropic' | 'ollama' | 'openai-compatible') || 'openai',
    modelName: process.env.DEFAULT_MODEL_NAME || 'gpt-4',
    apiUrl: process.env.DEFAULT_MODEL_URL || 'https://api.openai.com/v1',
    apiKey: process.env.DEFAULT_MODEL_API_KEY,
    maxTokens: process.env.DEFAULT_MODEL_MAX_TOKENS ? Number.parseInt(process.env.DEFAULT_MODEL_MAX_TOKENS, 10) : undefined,
    temperature: process.env.DEFAULT_MODEL_TEMPERATURE ? Number.parseFloat(process.env.DEFAULT_MODEL_TEMPERATURE) : 0.2,
  };
  return createModel(modelConfig);
}

const GET_FEATURE_WITH_TASKS_QUERY = `
  query GetFeatureWithTasks($id: ID!) {
    feature(id: $id) {
      id
      title
      description
      acceptanceCriteria
      status
      branchName
      pullRequestNumber
      project {
        id
        name
        memory
        architecture
        codingStandards
        gitRepositoryUrl
      }
      tasks {
        id
        title
        status
        complexity
      }
    }
  }
`;

const UPDATE_FEATURE_MUTATION = `
  mutation UpdateFeature($id: ID!, $title: String, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String, $branchName: String, $pullRequestNumber: Int) {
    updateFeature(id: $id, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions, branchName: $branchName, pullRequestNumber: $pullRequestNumber) {
      id
      title
      status
      branchName
      pullRequestNumber
      updatedAt
    }
  }
`;

const TRANSITION_FEATURE_STATUS_MUTATION = `
  mutation TransitionFeatureStatus($id: ID!, $targetStatus: FeatureStatus!, $actor: String!) {
    transitionFeatureStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      id
      title
      status
    }
  }
`;

const CREATE_TASK_MUTATION = `
  mutation CreateTask($featureId: ID!, $title: String!, $deliverable: String, $acceptanceCriteria: String, $risks: String, $result: String, $requiredFollowUps: String, $complexityRating: Int!) {
    createTask(featureId: $featureId, title: $title, deliverable: $deliverable, acceptanceCriteria: $acceptanceCriteria, risks: $risks, result: $result, requiredFollowUps: $requiredFollowUps, complexityRating: $complexityRating) {
      id
      title
      status
    }
  }
`;

async function fetchFeature(api: ReturnType<typeof createGraphQLClient>, featureId: string): Promise<unknown> {
  try {
    const result = await api.request(GET_FEATURE_WITH_TASKS_QUERY, { id: featureId });
    return result;
  } catch (error) {
    logger.error({ featureId, error }, 'Failed to fetch feature');
    throw error;
  }
}

async function updateFeature(
  api: ReturnType<typeof createGraphQLClient>,
  featureId: string,
  updates: {
    branchName?: string;
    pullRequestNumber?: number;
  }
): Promise<void> {
  try {
    await api.request(UPDATE_FEATURE_MUTATION, {
      id: featureId,
      ...updates,
    });
  } catch (error) {
    logger.error({ featureId, updates, error }, 'Failed to update feature');
    throw error;
  }
}

async function transitionFeatureStatus(
  api: ReturnType<typeof createGraphQLClient>,
  featureId: string,
  targetStatus: 'DRAFT' | 'PLANNED' | 'IN_PROGRESS' | 'IN_REVIEW' | 'COMPLETED' | 'REJECTED',
  actor: string
): Promise<void> {
  try {
    await api.request(TRANSITION_FEATURE_STATUS_MUTATION, {
      id: featureId,
      targetStatus,
      actor,
    });
  } catch (error) {
    logger.error({ featureId, targetStatus, actor, error }, 'Failed to transition feature status');
    throw error;
  }
}

async function createTask(
  api: ReturnType<typeof createGraphQLClient>,
  featureId: string,
  task: {
    title: string;
    deliverable: string;
    acceptanceCriteria: string;
    risks: string;
    complexityRating: number;
    requiredFollowUps?: string;
  }
): Promise<string> {
  try {
    const result = await api.request(CREATE_TASK_MUTATION, {
      featureId,
      title: task.title,
      deliverable: task.deliverable,
      acceptanceCriteria: task.acceptanceCriteria,
      risks: task.risks,
      complexityRating: task.complexityRating,
      requiredFollowUps: task.requiredFollowUps || '',
    });
    
    return result.createTask.id;
  } catch (error) {
    logger.error({ featureId, task, error }, 'Failed to create task');
    throw error;
  }
}

async function createGitBranch(repoUrl: string, branchName: string, targetDir: string): Promise<boolean> {
  try {
    await execFileAsync('git', ['clone', repoUrl, targetDir]);
    await execFileAsync('git', ['checkout', '-b', branchName], { cwd: targetDir });
    return true;
  } catch (error) {
    logger.error({ repoUrl, branchName, targetDir, error }, 'Failed to create git branch');
    return false;
  }
}

export const devLeadWorkflow: WorkflowDefinition<DevLeadWorkflowInput, DevLeadWorkflowOutput> = {
  name: 'devlead',
  inputSchema: DevLeadWorkflowInputSchema,
  maxRetries: 2,
  timeout: 300000, // 5 minutes
  
  async run(ctx): Promise<WorkflowResult<DevLeadWorkflowOutput> | WorkflowFailureResult> {
    const events: WorkflowEvent[] = [];
    const model = createDevLeadModel();
    
    try {
      events.push({
        type: 'devlead_started',
        data: { featureId: ctx.input.featureId, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info({ featureId: ctx.input.featureId }, 'Starting devlead workflow');
      
      const featureData = await fetchFeature(ctx.api, ctx.input.featureId);
      const feature = featureData as { 
        feature: { 
          id: string;
          title: string;
          description: string;
          acceptanceCriteria: string;
          status: string;
          branchName: string | null;
          pullRequestNumber: number | null;
          project: {
            id: string;
            name: string;
            memory: string;
            architecture: string;
            codingStandards: string;
            gitRepositoryUrl: string;
          };
          tasks: Array<{ id: string; title: string; status: string; complexity: number }>;
        } 
      };
      
      if (!feature?.feature) {
        return {
          ok: false,
          error: {
            code: 'FEATURE_NOT_FOUND',
            message: `Feature not found: ${ctx.input.featureId}`,
            retryable: false,
          },
          events,
        };
      }
      
      const featureInfo = feature.feature;
      const projectInfo = featureInfo.project;
      const tasks = featureInfo.tasks || [];
      
      const allTasksDone = tasks.length > 0 && tasks.every(t => t.status === 'DONE' || t.status === 'REVIEW');
      const hasPendingTasks = tasks.some(t => t.status === 'TODO' || t.status === 'IN_PROGRESS');
      
      if (hasPendingTasks) {
        events.push({
          type: 'devlead_waiting',
          data: { pendingTaskCount: tasks.filter(t => t.status === 'TODO' || t.status === 'IN_PROGRESS').length },
          timestamp: new Date().toISOString(),
        });
        
        return {
          ok: true,
          output: {
            action: 'waiting_for_tasks',
            summary: `Waiting for ${tasks.filter(t => t.status === 'TODO' || t.status === 'IN_PROGRESS').length} pending tasks to complete`,
            nextSteps: ['Monitor task progress', 'Run devlead workflow again when all tasks are done'],
          },
          events,
        };
      }
      
      const branchName = featureInfo.branchName || `feature/${ctx.input.featureId}-${Date.now()}`;
      
      if (!featureInfo.branchName) {
        const workspaceDir = path.join(os.tmpdir(), `devstack-devlead-${Date.now()}`);
        const branchCreated = await createGitBranch(projectInfo.gitRepositoryUrl, branchName, workspaceDir);
        
        if (!branchCreated) {
          return {
            ok: false,
            error: {
              code: 'BRANCH_CREATION_FAILED',
              message: 'Failed to create feature branch',
              retryable: true,
            },
            events,
          };
        }
        
        await updateFeature(ctx.api, ctx.input.featureId, { branchName });
        await transitionFeatureStatus(ctx.api, ctx.input.featureId, 'IN_PROGRESS', 'devlead-workflow');
        
        events.push({
          type: 'branch_created',
          data: { branchName },
          timestamp: new Date().toISOString(),
        });
        
        return {
          ok: true,
          output: {
            action: 'branch_created',
            branchName,
            summary: `Created feature branch: ${branchName}`,
            nextSteps: ['Tasks can now be implemented on this branch'],
          },
          events,
        };
      }
      
      if (allTasksDone && !featureInfo.pullRequestNumber) {
        const promptInput = {
          projectMemory: projectInfo.memory || 'No project memory available',
          architecture: projectInfo.architecture || 'No architecture documentation available',
          codingStandards: projectInfo.codingStandards || 'No coding standards available',
          featureTitle: featureInfo.title,
          featureDescription: featureInfo.description,
          acceptanceCriteria: featureInfo.acceptanceCriteria,
          taskList: tasks.map(t => `${t.id}: ${t.title} (${t.status})`).join('\n'),
        };
        
        ctx.logger.info('Rendering devlead prompt for PR creation');
        const promptText = renderPrompt('devlead', promptInput);
        
        events.push({
          type: 'prompt_rendered',
          data: { promptLength: promptText.length },
          timestamp: new Date().toISOString(),
        });
        
        ctx.logger.info('Calling LLM for PR orchestration');
        const response = await model.invoke(promptText);
        
        const rawOutput = typeof response.content === 'string' 
          ? response.content 
          : JSON.stringify(response.content || '');
        
        const prOutput = JSON.parse(rawOutput) as {
          action: 'branch_created' | 'pr_created' | 'rework_requested' | 'feature_completed' | 'waiting_for_tasks';
          branchName?: string;
          prNumber?: number;
          tasksCreated?: string[];
          summary: string;
          nextSteps: string[];
        };
        
        if (prOutput.action === 'pr_created' && prOutput.prNumber) {
          await updateFeature(ctx.api, ctx.input.featureId, { pullRequestNumber: prOutput.prNumber });
          await transitionFeatureStatus(ctx.api, ctx.input.featureId, 'IN_REVIEW', 'devlead-workflow');
          
          events.push({
            type: 'pr_created',
            data: { prNumber: prOutput.prNumber },
            timestamp: new Date().toISOString(),
          });
        }
        
        return {
          ok: true,
          output: prOutput,
          events,
        };
      }
      
      events.push({
        type: 'devlead_completed',
        data: { action: 'no_action_needed' },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: true,
        output: {
          action: 'feature_completed',
          summary: 'Feature is already in review or completed',
          nextSteps: ['Monitor PR status'],
        },
        events,
      };
    } catch (error) {
      ctx.logger.error({ error }, 'DevLead workflow failed');
      
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      
      events.push({
        type: 'devlead_failed',
        data: { 
          error: errorMessage,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: false,
        error: {
          code: 'DEVLEAD_WORKFLOW_ERROR',
          message: errorMessage,
          retryable: true,
          details: error,
        },
        events,
      };
    }
  },
};
