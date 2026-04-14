import { z } from 'zod';
import { WorkflowDefinition, WorkflowContext, WorkflowResult, WorkflowFailureResult, WorkflowEvent } from './types.js';
import { renderPrompt } from '../prompts/loader.js';
import { createModel, type ModelConfig } from '../llm/model.js';
import type { BaseChatModel } from '@langchain/core/language_models/chat_models';
import { createGraphQLClient } from '../api/graphql-client.js';
import { logger } from '../observability/logger.js';
import { isDryRunMode, getDryRunArchitectResponse, logDryRunEvent } from './dry-run.js';

const ArchitectWorkflowInputSchema = z.object({
  projectId: z.string().min(1, 'Project ID is required'),
});

const ArchitectWorkflowOutputSchema = z.object({
  recommendationsCount: z.number(),
  featuresCreated: z.array(z.object({
    id: z.string(),
    title: z.string(),
  })),
  memoryUpdated: z.boolean(),
});

export type ArchitectWorkflowInput = z.infer<typeof ArchitectWorkflowInputSchema>;
export type ArchitectWorkflowOutput = z.infer<typeof ArchitectWorkflowOutputSchema>;

function createArchitectModel(): BaseChatModel {
  const modelConfig: ModelConfig = {
    modelId: process.env.DEFAULT_MODEL_ID || 'default',
    provider: (process.env.DEFAULT_MODEL_PROVIDER as 'openai' | 'anthropic' | 'ollama' | 'openai-compatible') || 'openai',
    modelName: process.env.DEFAULT_MODEL_NAME || 'gpt-4',
    apiUrl: process.env.DEFAULT_MODEL_URL || 'https://api.openai.com/v1',
    apiKey: process.env.DEFAULT_MODEL_API_KEY,
    maxTokens: process.env.DEFAULT_MODEL_MAX_TOKENS ? Number.parseInt(process.env.DEFAULT_MODEL_MAX_TOKENS, 10) : undefined,
    temperature: process.env.DEFAULT_MODEL_TEMPERATURE ? Number.parseFloat(process.env.DEFAULT_MODEL_TEMPERATURE) : 0.3,
  };
  return createModel(modelConfig);
}

const UPDATE_PROJECT_MEMORY_MUTATION = `
  mutation UpdateProject($id: ID!, $memory: String, $architecture: String, $codingStandards: String) {
    updateProject(id: $id, memory: $memory, architecture: $architecture, codingStandards: $codingStandards) {
      id
      name
      memory
      architecture
      codingStandards
    }
  }
`;

const CREATE_FEATURE_MUTATION = `
  mutation CreateFeature($projectId: ID!, $title: String!, $description: String!, $acceptanceCriteria: String) {
    createFeature(projectId: $projectId, title: $title, description: $description, acceptanceCriteria: $acceptanceCriteria) {
      id
      title
      description
      status
      createdAt
    }
  }
`;

const GET_PROJECT_QUERY = `
  query GetProject($id: ID!) {
    project(id: $id) {
      id
      name
      memory
      architecture
      codingStandards
    }
  }
`;

const GET_RECENT_FEATURES_QUERY = `
  query GetRecentFeatures($projectId: ID!, $limit: Int) {
    features(projectId: $projectId) {
      id
      title
      status
      createdAt
    }
  }
`;

const GET_RECENT_DEFECTS_QUERY = `
  query GetRecentDefects($projectId: ID!, $limit: Int) {
    defects(projectId: $projectId) {
      id
      title
      severity
      status
      createdAt
    }
  }
`;

async function fetchProject(api: ReturnType<typeof createGraphQLClient>, projectId: string): Promise<unknown> {
  try {
    const result = await api.request(GET_PROJECT_QUERY, { id: projectId });
    return result;
  } catch (error) {
    logger.error({ projectId, error }, 'Failed to fetch project');
    throw error;
  }
}

async function fetchRecentFeatures(api: ReturnType<typeof createGraphQLClient>, projectId: string, limit: number = 10): Promise<unknown> {
  try {
    const result = await api.request(GET_RECENT_FEATURES_QUERY, { projectId, limit });
    return result;
  } catch (error) {
    logger.error({ projectId, error }, 'Failed to fetch recent features');
    throw error;
  }
}

async function fetchRecentDefects(api: ReturnType<typeof createGraphQLClient>, projectId: string, limit: number = 10): Promise<unknown> {
  try {
    const result = await api.request(GET_RECENT_DEFECTS_QUERY, { projectId, limit });
    return result;
  } catch (error) {
    logger.error({ projectId, error }, 'Failed to fetch recent defects');
    throw error;
  }
}

async function updateProjectMemory(
  api: ReturnType<typeof createGraphQLClient>,
  projectId: string,
  memoryUpdate: string
): Promise<void> {
  try {
    const projectData = await fetchProject(api, projectId);
    const project = projectData as { project: { memory: string; architecture: string; codingStandards: string } };
    
    const updatedMemory = project.project.memory 
      ? `${project.project.memory}\n\n${memoryUpdate}`
      : memoryUpdate;

    await api.request(UPDATE_PROJECT_MEMORY_MUTATION, {
      id: projectId,
      memory: updatedMemory,
    });
  } catch (error) {
    logger.error({ projectId, memoryUpdate, error }, 'Failed to update project memory');
    throw error;
  }
}

async function createFeature(
  api: ReturnType<typeof createGraphQLClient>,
  projectId: string,
  feature: {
    title: string;
    description: string;
    acceptanceCriteria: string;
  }
): Promise<{ id: string; title: string }> {
  try {
    const result = await api.request(CREATE_FEATURE_MUTATION, {
      projectId,
      title: feature.title,
      description: feature.description,
      acceptanceCriteria: feature.acceptanceCriteria || '',
    });
    
    const createdFeature = result.createFeature;
    return {
      id: createdFeature.id,
      title: createdFeature.title,
    };
  } catch (error) {
    logger.error({ projectId, feature, error }, 'Failed to create feature');
    throw error;
  }
}

export const architectWorkflow: WorkflowDefinition<ArchitectWorkflowInput, ArchitectWorkflowOutput> = {
  name: 'architect',
  inputSchema: ArchitectWorkflowInputSchema,
  maxRetries: 3,
  timeout: 300000, // 5 minutes
  
  async run(ctx): Promise<WorkflowResult<ArchitectWorkflowOutput> | WorkflowFailureResult> {
    const events: WorkflowEvent[] = [];
    const model = createArchitectModel();
    
    try {
      events.push({
        type: 'architect_started',
        data: { projectId: ctx.input.projectId, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info({ projectId: ctx.input.projectId }, 'Starting architect workflow');
      
      const projectData = await fetchProject(ctx.api, ctx.input.projectId);
      const project = projectData as { project: { 
        id: string;
        name: string;
        memory: string;
        architecture: string;
        codingStandards: string;
      } };
      
      if (!project?.project) {
        return {
          ok: false,
          error: {
            code: 'PROJECT_NOT_FOUND',
            message: `Project not found: ${ctx.input.projectId}`,
            retryable: false,
          },
          events,
        };
      }
      
      const projectInfo = project.project;
      
      const featuresData = await fetchRecentFeatures(ctx.api, ctx.input.projectId, 10);
      const features = featuresData as { features: Array<{ id: string; title: string; status: string; createdAt: string }> };
      
      const defectsData = await fetchRecentDefects(ctx.api, ctx.input.projectId, 10);
      const defects = defectsData as { defects: Array<{ id: string; title: string; severity: string; status: string; createdAt: string }> };
      
      const recentFeatures = (features?.features || [])
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5)
        .map(f => ({
          id: f.id,
          title: f.title,
          status: f.status,
        }));
      
      const recentDefects = (defects?.defects || [])
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5)
        .map(d => ({
          id: d.id,
          title: d.title,
          severity: d.severity,
        }));
      
      const promptInput = {
        projectId: ctx.input.projectId,
        projectMemory: projectInfo.memory || 'No project memory available',
        architecture: projectInfo.architecture || 'No architecture documentation available',
        codingStandards: projectInfo.codingStandards || 'No coding standards available',
        recentFeatures,
        recentDefects,
      };
      
      ctx.logger.info('Rendering architect prompt');
      const promptText = renderPrompt('architect', promptInput);
      
      events.push({
        type: 'prompt_rendered',
        data: { promptLength: promptText.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      let rawOutput: string;
      
      if (isDryRunMode()) {
        logDryRunEvent('architect');
        rawOutput = getDryRunArchitectResponse(ctx.input.projectId);
      } else {
        ctx.logger.info('Calling LLM for architectural review');
        const response = await model.invoke(promptText);
        
        rawOutput = typeof response.content === 'string' 
          ? response.content 
          : JSON.stringify(response.content || '');
      }
      
      events.push({
        type: 'llm_response_received',
        data: { responseLength: rawOutput.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('Parsing LLM output');
      const planOutput = JSON.parse(rawOutput) as {
        recommendations: Array<{
          type: 'refactoring' | 'security' | 'observability' | 'performance';
          description: string;
          priority: 'high' | 'medium' | 'low';
        }>;
        memoryUpdate: string;
        suggestedFeatures: Array<{
          title: string;
          description: string;
        }>;
      };
      
      const createdFeatures: { id: string; title: string }[] = [];
      
      if (planOutput.suggestedFeatures && planOutput.suggestedFeatures.length > 0) {
        ctx.logger.info({ featureCount: planOutput.suggestedFeatures.length }, 'Creating suggested features');
        
        for (const feature of planOutput.suggestedFeatures) {
          const createdFeature = await createFeature(ctx.api, ctx.input.projectId, {
            title: feature.title,
            description: feature.description,
            acceptanceCriteria: 'Architecturally recommended feature',
          });
          createdFeatures.push(createdFeature);
          ctx.logger.info({ featureId: createdFeature.id, title: feature.title }, 'Feature created');
        }
      }
      
      if (planOutput.memoryUpdate && planOutput.memoryUpdate.trim().length > 0) {
        ctx.logger.info('Updating project memory');
        await updateProjectMemory(ctx.api, ctx.input.projectId, planOutput.memoryUpdate);
      }
      
      events.push({
        type: 'architect_completed',
        data: { 
          recommendationsCount: planOutput.recommendations?.length || 0,
          featuresCreated: createdFeatures.length,
          memoryUpdated: !!planOutput.memoryUpdate,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info(
        { 
          recommendationsCount: planOutput.recommendations?.length || 0,
          featuresCreated: createdFeatures.length,
        },
        'Architect workflow completed successfully'
      );
      
      return {
        ok: true,
        output: {
          recommendationsCount: planOutput.recommendations?.length || 0,
          featuresCreated: createdFeatures,
          memoryUpdated: !!planOutput.memoryUpdate,
        },
        events,
      };
    } catch (error) {
      ctx.logger.error({ error }, 'Architect workflow failed');
      
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      
      events.push({
        type: 'architect_failed',
        data: { 
          error: errorMessage,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: false,
        error: {
          code: 'ARCHITECT_WORKFLOW_ERROR',
          message: errorMessage,
          retryable: true,
          details: error,
        },
        events,
      };
    }
  },
};
