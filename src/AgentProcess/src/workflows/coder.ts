import { z } from 'zod';
import { WorkflowDefinition, WorkflowContext, WorkflowResult, WorkflowFailureResult, WorkflowEvent } from './types.js';
import { renderPrompt } from '../prompts/loader.js';
import { createModel, type ModelConfig } from '../llm/model.js';
import type { BaseChatModel } from '@langchain/core/language_models/chat_models';
import { createGraphQLClient } from '../api/graphql-client.js';
import { logger } from '../observability/logger.js';
import { createGitTools } from '../skills/git/git-skill.js';
import { createFilesystemTools } from '../skills/filesystem/fs-skill.js';
import { createCommandTools } from '../skills/command/command-skill.js';
import { createWorkItemTools } from '../skills/work-items/work-items-skill.js';
import path from 'node:path';
import os from 'node:os';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const execFileAsync = promisify(execFile);

const CoderWorkflowInputSchema = z.object({
  taskId: z.string().min(1, 'Task ID is required'),
});

const CoderWorkflowOutputSchema = z.object({
  filesModified: z.array(z.string()),
  filesCreated: z.array(z.string()),
  buildResult: z.string(),
  testResult: z.string(),
  commitMessage: z.string().optional(),
  summary: z.string(),
  issues: z.array(z.string()).optional(),
});

export type CoderWorkflowInput = z.infer<typeof CoderWorkflowInputSchema>;
export type CoderWorkflowOutput = z.infer<typeof CoderWorkflowOutputSchema>;

function createCoderModel(): BaseChatModel {
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

const GET_TASK_QUERY = `
  query GetTask($id: ID!) {
    task(id: $id) {
      id
      title
      description
      status
      complexity
      feature {
        id
        title
        description
        acceptanceCriteria
        project {
          id
          name
          memory
          architecture
          codingStandards
          gitRepositoryUrl
        }
      }
    }
  }
`;

const UPDATE_TASK_MUTATION = `
  mutation UpdateTask($id: ID!, $title: String, $description: String, $status: TaskStatus, $complexity: Int, $result: String) {
    updateTask(id: $id, title: $title, description: $description, status: $status, complexity: $complexity, result: $result) {
      id
      title
      status
      result
      updatedAt
    }
  }
`;

const TRANSITION_TASK_STATUS_MUTATION = `
  mutation TransitionTaskStatus($id: ID!, $targetStatus: TaskStatus!, $actor: String!) {
    transitionTaskStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      id
      title
      status
    }
  }
`;

async function fetchTask(api: ReturnType<typeof createGraphQLClient>, taskId: string): Promise<unknown> {
  try {
    const result = await api.request(GET_TASK_QUERY, { id: taskId });
    return result;
  } catch (error) {
    logger.error({ taskId, error }, 'Failed to fetch task');
    throw error;
  }
}

async function updateTaskResult(
  api: ReturnType<typeof createGraphQLClient>,
  taskId: string,
  status: 'DONE' | 'FAILED' | 'REVIEW',
  result: string
): Promise<void> {
  try {
    await api.request(UPDATE_TASK_MUTATION, {
      id: taskId,
      status,
      result,
    });
  } catch (error) {
    logger.error({ taskId, status, result, error }, 'Failed to update task result');
    throw error;
  }
}

async function transitionTaskStatus(
  api: ReturnType<typeof createGraphQLClient>,
  taskId: string,
  targetStatus: 'DONE' | 'FAILED' | 'REVIEW',
  actor: string
): Promise<void> {
  try {
    await api.request(TRANSITION_TASK_STATUS_MUTATION, {
      id: taskId,
      targetStatus,
      actor,
    });
  } catch (error) {
    logger.error({ taskId, targetStatus, actor, error }, 'Failed to transition task status');
    throw error;
  }
}

async function cloneRepository(gitUrl: string, targetDir: string, context: ToolContext): Promise<boolean> {
  const args: string[] = ['clone', gitUrl, targetDir];
  
  try {
    await execFileAsync('git', args, {
      cwd: process.cwd(),
      env: { ...process.env },
    });
    
    context.logger.info({ targetDir }, 'Repository cloned successfully');
    return true;
  } catch (error) {
    context.logger.error({ gitUrl, targetDir, error }, 'Failed to clone repository');
    return false;
  }
}

interface ToolContext {
  logger: any;
}

export const coderWorkflow: WorkflowDefinition<CoderWorkflowInput, CoderWorkflowOutput> = {
  name: 'coder',
  inputSchema: CoderWorkflowInputSchema,
  maxRetries: 2,
  timeout: 600000, // 10 minutes
  
  async run(ctx): Promise<WorkflowResult<CoderWorkflowOutput> | WorkflowFailureResult> {
    const events: WorkflowEvent[] = [];
    const model = createCoderModel();
    
    try {
      events.push({
        type: 'coder_started',
        data: { taskId: ctx.input.taskId, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info({ taskId: ctx.input.taskId }, 'Starting coder workflow');
      
      const taskData = await fetchTask(ctx.api, ctx.input.taskId);
      const task = taskData as { 
        task: { 
          id: string;
          title: string;
          description: string;
          status: string;
          complexity: number;
          feature: {
            id: string;
            title: string;
            description: string;
            acceptanceCriteria: string;
            project: {
              id: string;
              name: string;
              memory: string;
              architecture: string;
              codingStandards: string;
              gitRepositoryUrl: string;
            };
          };
        } 
      };
      
      if (!task?.task) {
        return {
          ok: false,
          error: {
            code: 'TASK_NOT_FOUND',
            message: `Task not found: ${ctx.input.taskId}`,
            retryable: false,
          },
          events,
        };
      }
      
      const taskInfo = task.task;
      const featureInfo = taskInfo.feature;
      const projectInfo = featureInfo.project;
      
      const workspaceDir = path.join(os.tmpdir(), `devstack-coder-${Date.now()}`);
      
      ctx.logger.info({ workspaceDir }, 'Creating workspace');
      
      const cloneSuccess = await cloneRepository(projectInfo.gitRepositoryUrl, workspaceDir, { logger: ctx.logger });
      
      if (!cloneSuccess) {
        const errorMessage = 'Failed to clone repository';
        await updateTaskResult(ctx.api, ctx.input.taskId, 'FAILED', errorMessage);
        await transitionTaskStatus(ctx.api, ctx.input.taskId, 'FAILED', 'coder-workflow');
        
        return {
          ok: false,
          error: {
            code: 'CLONE_FAILED',
            message: errorMessage,
            retryable: true,
          },
          events,
        };
      }
      
      const promptInput = {
        projectMemory: projectInfo.memory || 'No project memory available',
        architecture: projectInfo.architecture || 'No architecture documentation available',
        codingStandards: projectInfo.codingStandards || 'No coding standards available',
        taskTitle: taskInfo.title,
        taskDescription: taskInfo.description || '',
        acceptanceCriteria: featureInfo.acceptanceCriteria || '',
        featureTitle: featureInfo.title,
        featureDescription: featureInfo.description,
      };
      
      ctx.logger.info('Rendering coder prompt');
      const promptText = renderPrompt('coder', promptInput);
      
      events.push({
        type: 'prompt_rendered',
        data: { promptLength: promptText.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('Calling LLM for code generation');
      const response = await model.invoke(promptText);
      
      const rawOutput = typeof response.content === 'string' 
        ? response.content 
        : JSON.stringify(response.content || '');
      
      events.push({
        type: 'llm_response_received',
        data: { responseLength: rawOutput.length, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('Parsing LLM output');
      const codeOutput = JSON.parse(rawOutput) as {
        filesModified: string[];
        filesCreated: string[];
        buildResult: string;
        testResult: string;
        commitMessage?: string;
        summary: string;
        issues?: string[];
      };
      
      ctx.logger.info({ filesModified: codeOutput.filesModified }, 'Running quality gates');
      
      let buildSuccess = false;
      let testSuccess = false;
      
      try {
        const buildResult = await execFileAsync('dotnet', ['build'], {
          cwd: workspaceDir,
          env: { ...process.env },
        });
        
        buildSuccess = true;
        ctx.logger.info('Build succeeded');
        codeOutput.buildResult = 'SUCCESS';
      } catch (error) {
        ctx.logger.warn({ error }, 'Build failed');
        codeOutput.buildResult = 'FAILURE';
      }
      
      if (buildSuccess) {
        try {
          const testResult = await execFileAsync('dotnet', ['test'], {
            cwd: workspaceDir,
            env: { ...process.env },
          });
          
          testSuccess = true;
          ctx.logger.info('Tests passed');
          codeOutput.testResult = 'SUCCESS';
        } catch (error) {
          ctx.logger.warn({ error }, 'Tests failed');
          codeOutput.testResult = 'FAILURE';
        }
      } else {
        codeOutput.testResult = 'SKIPPED';
      }
      
      if (buildSuccess && testSuccess && codeOutput.commitMessage) {
        try {
          await execFileAsync('git', ['add', '.'], {
            cwd: workspaceDir,
            env: { ...process.env },
          });
          
          await execFileAsync('git', ['commit', '-m', codeOutput.commitMessage], {
            cwd: workspaceDir,
            env: { ...process.env },
          });
          
          ctx.logger.info('Changes committed');
        } catch (error) {
          ctx.logger.warn({ error }, 'Failed to commit changes');
        }
      }
      
      const summary = codeOutput.summary || 'Task implementation completed';
      await updateTaskResult(ctx.api, ctx.input.taskId, testSuccess ? 'DONE' : 'FAILED', summary);
      await transitionTaskStatus(ctx.api, ctx.input.taskId, testSuccess ? 'DONE' : 'FAILED', 'coder-workflow');
      
      events.push({
        type: 'coder_completed',
        data: { 
          buildResult: codeOutput.buildResult,
          testResult: codeOutput.testResult,
          summary: summary,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info(
        { buildResult: codeOutput.buildResult, testResult: codeOutput.testResult },
        'Coder workflow completed'
      );
      
      return {
        ok: true,
        output: codeOutput,
        events,
      };
    } catch (error) {
      ctx.logger.error({ error }, 'Coder workflow failed');
      
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      
      events.push({
        type: 'coder_failed',
        data: { 
          error: errorMessage,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: false,
        error: {
          code: 'CODER_WORKFLOW_ERROR',
          message: errorMessage,
          retryable: true,
          details: error,
        },
        events,
      };
    }
  },
};
