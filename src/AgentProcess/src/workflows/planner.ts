import { z } from 'zod';
import { WorkflowDefinition, WorkflowContext, WorkflowResult, WorkflowFailureResult, WorkflowEvent } from './types.js';
import { renderPrompt } from '../prompts/loader.js';
import { createModel, type ModelConfig } from '../llm/model.js';
import type { BaseChatModel } from '@langchain/core/language_models/chat_models';
import { createGraphQLClient } from '../api/graphql-client.js';
import { logger } from '../observability/logger.js';

const PlannerWorkflowInputSchema = z.object({
  featureId: z.string().min(1, 'Feature ID is required'),
});

const PlannerWorkflowOutputSchema = z.object({
  tasksCreated: z.array(z.object({
    id: z.string(),
    title: z.string(),
  })),
  openQuestions: z.array(z.string()),
});

export type PlannerWorkflowInput = z.infer<typeof PlannerWorkflowInputSchema>;
export type PlannerWorkflowOutput = z.infer<typeof PlannerWorkflowOutputSchema>;

function createPlannerModel(): BaseChatModel {
  const modelConfig: ModelConfig = {
    modelId: process.env.DEFAULT_MODEL_ID || 'default',
    provider: (process.env.DEFAULT_MODEL_PROVIDER as 'openai' | 'anthropic' | 'ollama' | 'openai-compatible') || 'openai',
    modelName: process.env.DEFAULT_MODEL_NAME || 'gpt-4',
    apiUrl: process.env.DEFAULT_MODEL_URL || 'https://api.openai.com/v1',
    apiKey: process.env.DEFAULT_MODEL_API_KEY,
    maxTokens: process.env.DEFAULT_MODEL_MAX_TOKENS ? Number.parseInt(process.env.DEFAULT_MODEL_MAX_TOKENS, 10) : undefined,
    temperature: process.env.DEFAULT_MODEL_TEMPERATURE ? Number.parseFloat(process.env.DEFAULT_MODEL_TEMPERATURE) : 0,
  };
  return createModel(modelConfig);
}

const CREATE_TASK_MUTATION = `
  mutation CreateTask($featureId: ID!, $title: String!, $deliverable: String, $acceptanceCriteria: String, $risks: String, $result: String, $requiredFollowUps: String, $complexityRating: Int!) {
    createTask(featureId: $featureId, title: $title, deliverable: $deliverable, acceptanceCriteria: $acceptanceCriteria, risks: $risks, result: $result, requiredFollowUps: $requiredFollowUps, complexityRating: $complexityRating) {
      id
      title
      description
      status
      complexity
      featureId
      createdAt
    }
  }
`;

const UPDATE_FEATURE_MUTATION = `
  mutation UpdateFeature($id: ID!, $title: String, $description: String, $acceptanceCriteria: String, $plan: String, $securityImpact: String, $performanceImpact: String, $testPlan: String, $deploymentPlan: String, $openQuestions: String) {
    updateFeature(id: $id, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria, plan: $plan, securityImpact: $securityImpact, performanceImpact: $performanceImpact, testPlan: $testPlan, deploymentPlan: $deploymentPlan, openQuestions: $openQuestions) {
      id
      title
      description
      status
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

const GET_FEATURE_QUERY = `
  query GetFeature($id: ID!) {
    feature(id: $id) {
      id
      title
      description
      acceptanceCriteria
      status
      project {
        id
        name
        memory
        architecture
        codingStandards
      }
      defects {
        id
        title
        severity
        status
      }
    }
  }
`;

async function fetchFeature(api: ReturnType<typeof createGraphQLClient>, featureId: string): Promise<unknown> {
  try {
    const result = await api.request(GET_FEATURE_QUERY, { id: featureId });
    return result;
  } catch (error) {
    logger.error({ featureId, error }, 'Failed to fetch feature');
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
): Promise<{ id: string; title: string }> {
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
    
    const createdTask = result.createTask;
    return {
      id: createdTask.id,
      title: createdTask.title,
    };
  } catch (error) {
    logger.error({ featureId, task, error }, 'Failed to create task');
    throw error;
  }
}

async function updateFeature(
  api: ReturnType<typeof createGraphQLClient>,
  featureId: string,
  updates: {
    plan: string;
    testPlan: string;
    deploymentPlan: string;
    securityImpact: string;
    performanceImpact: string;
    openQuestions: string[];
  }
): Promise<void> {
  try {
    await api.request(UPDATE_FEATURE_MUTATION, {
      id: featureId,
      plan: updates.plan,
      testPlan: updates.testPlan,
      deploymentPlan: updates.deploymentPlan,
      securityImpact: updates.securityImpact,
      performanceImpact: updates.performanceImpact,
      openQuestions: updates.openQuestions.join('\n'),
    });
  } catch (error) {
    logger.error({ featureId, updates, error }, 'Failed to update feature');
    throw error;
  }
}

async function transitionFeatureStatus(
  api: ReturnType<typeof createGraphQLClient>,
  featureId: string,
  targetStatus: 'IN_PROGRESS' | 'IN_REVIEW',
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

export const plannerWorkflow: WorkflowDefinition<PlannerWorkflowInput, PlannerWorkflowOutput> = {
  name: 'planner',
  inputSchema: PlannerWorkflowInputSchema,
  maxRetries: 3,
  timeout: 300000, // 5 minutes
  
  async run(ctx): Promise<WorkflowResult<PlannerWorkflowOutput> | WorkflowFailureResult> {
    const events: WorkflowEvent[] = [];
    const model = createPlannerModel();
    
    try {
      events.push({
        type: 'planner_started',
        data: { featureId: ctx.input.featureId, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info({ featureId: ctx.input.featureId }, 'Starting planner workflow');
      
      const featureData = await fetchFeature(ctx.api, ctx.input.featureId);
      const feature = featureData as { feature: { 
        id: string;
        title: string;
        description: string;
        acceptanceCriteria: string;
        project: {
          memory: string;
          architecture: string;
          codingStandards: string;
        };
        defects: Array<{ title: string; severity: string; status: string }>;
      } };
      
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
      
      const defectSummaries = featureInfo.defects
        ?.filter(d => d.status !== 'COMPLETED')
        .map(d => `${d.title} (Severity: ${d.severity})`)
        .join('\n');
      
      const promptInput = {
        projectId: featureInfo.project.id,
        featureId: ctx.input.featureId,
        projectMemory: featureInfo.project.memory || 'No project memory available',
        architecture: featureInfo.project.architecture || 'No architecture documentation available',
        codingStandards: featureInfo.project.codingStandards || 'No coding standards available',
        defectSummaries,
        modelMaxComplexity: 8,
        featureTitle: featureInfo.title,
        featureDescription: featureInfo.description,
        acceptanceCriteria: featureInfo.acceptanceCriteria,
      };
      
      ctx.logger.info('Rendering planner prompt');
      const promptText = renderPrompt('planner', promptInput);
      
      events.push({
        type: 'prompt_rendered',
        data: { promptLength: promptText.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('Calling LLM for planning');
      const response = await model.invoke({
        messages: [
          {
            role: 'user',
            content: promptText,
          },
        ],
      });
      
      const rawOutput = typeof response.content === 'string' 
        ? response.content 
        : JSON.stringify(response.content);
      
      events.push({
        type: 'llm_response_received',
        data: { responseLength: rawOutput.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('Parsing LLM output');
      const planOutput = JSON.parse(rawOutput) as {
        plan: string;
        tasks: Array<{
          title: string;
          deliverable: string;
          acceptanceCriteria: string;
          risks: string;
          complexityRating: number;
          requiredFollowUps?: string;
        }>;
        openQuestions: string[];
        securityImpact: string;
        performanceImpact: string;
        testPlan: string;
        deploymentPlan: string;
      };
      
      ctx.logger.info({ taskCount: planOutput.tasks.length }, 'Creating tasks');
      const createdTasks: { id: string; title: string }[] = [];
      
      for (const task of planOutput.tasks) {
        const createdTask = await createTask(ctx.api, ctx.input.featureId, task);
        createdTasks.push(createdTask);
        ctx.logger.info({ taskId: createdTask.id, title: task.title }, 'Task created');
      }
      
      ctx.logger.info('Updating feature with plan');
      await updateFeature(ctx.api, ctx.input.featureId, {
        plan: planOutput.plan,
        testPlan: planOutput.testPlan,
        deploymentPlan: planOutput.deploymentPlan,
        securityImpact: planOutput.securityImpact,
        performanceImpact: planOutput.performanceImpact,
        openQuestions: planOutput.openQuestions,
      });
      
      const targetStatus = planOutput.openQuestions.length === 0 ? 'IN_PROGRESS' : 'IN_REVIEW';
      ctx.logger.info({ targetStatus }, 'Transitioning feature status');
      await transitionFeatureStatus(ctx.api, ctx.input.featureId, targetStatus, 'planner-workflow');
      
      events.push({
        type: 'planner_completed',
        data: { 
          tasksCreated: createdTasks.length,
          openQuestions: planOutput.openQuestions.length,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info(
        { tasksCreated: createdTasks.length, openQuestions: planOutput.openQuestions.length },
        'Planner workflow completed successfully'
      );
      
      return {
        ok: true,
        output: {
          tasksCreated: createdTasks,
          openQuestions: planOutput.openQuestions,
        },
        events,
      };
    } catch (error) {
      ctx.logger.error({ error }, 'Planner workflow failed');
      
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      
      events.push({
        type: 'planner_failed',
        data: { 
          error: errorMessage,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: false,
        error: {
          code: 'PLANNER_WORKFLOW_ERROR',
          message: errorMessage,
          retryable: true,
          details: error,
        },
        events,
      };
    }
  },
};
