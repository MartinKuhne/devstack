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
import { promises as fs } from 'node:fs';

const execFileAsync = promisify(execFile);

const TesterWorkflowInputSchema = z.object({
  featureId: z.string().min(1, 'Feature ID is required'),
});

const TesterWorkflowOutputSchema = z.object({
  buildResult: z.enum(['PASS', 'FAIL']),
  testResult: z.enum(['PASS', 'FAIL', 'SKIPPED']),
  defectsCreated: z.array(z.string()),
  summary: z.string(),
  featureStatus: z.enum(['COMPLETED', 'REJECTED']),
});

export type TesterWorkflowInput = z.infer<typeof TesterWorkflowInputSchema>;
export type TesterWorkflowOutput = z.infer<typeof TesterWorkflowOutputSchema>;

function createTesterModel(): BaseChatModel {
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
        gitRepositoryUrl
      }
    }
  }
`;

const CREATE_DEFECT_MUTATION = `
  mutation CreateDefect($projectId: ID!, $title: String!, $description: String, $severity: Severity, $initialStatus: DefectStatus) {
    createDefect(projectId: $projectId, title: $title, description: $description, severity: $severity, initialStatus: $initialStatus) {
      id
      title
      severity
      status
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

async function fetchFeature(api: ReturnType<typeof createGraphQLClient>, featureId: string): Promise<unknown> {
  try {
    const result = await api.request(GET_FEATURE_QUERY, { id: featureId });
    return result;
  } catch (error) {
    logger.error({ featureId, error }, 'Failed to fetch feature');
    throw error;
  }
}

async function createDefect(
  api: ReturnType<typeof createGraphQLClient>,
  projectId: string,
  title: string,
  description: string,
  severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL'
): Promise<string> {
  try {
    const result = await api.request(CREATE_DEFECT_MUTATION, {
      projectId,
      title,
      description,
      severity,
      initialStatus: 'TODO',
    });
    
    return result.createDefect.id;
  } catch (error) {
    logger.error({ projectId, title, severity, error }, 'Failed to create defect');
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

async function cloneRepository(repoUrl: string, targetDir: string): Promise<boolean> {
  try {
    await execFileAsync('git', ['clone', repoUrl, targetDir]);
    return true;
  } catch (error) {
    logger.error({ repoUrl, targetDir, error }, 'Failed to clone repository');
    return false;
  }
}

async function runBuild(cwd: string): Promise<{ success: boolean; output: string }> {
  try {
    // Try dotnet build first
    try {
      const result = await execFileAsync('dotnet', ['build'], { cwd });
      return { success: true, output: result.stdout };
    } catch {
      // Try npm/pnpm build
      try {
        const result = await execFileAsync('npm', ['run', 'build'], { cwd });
        return { success: true, output: result.stdout };
      } catch {
        return { success: false, output: 'Build command not found or failed' };
      }
    }
  } catch (error: unknown) {
    const errorMessage = error instanceof Error ? error.message : 'Unknown error';
    return { success: false, output: errorMessage };
  }
}

async function runTests(cwd: string): Promise<{ success: boolean; output: string }> {
  try {
    // Try dotnet test first
    try {
      const result = await execFileAsync('dotnet', ['test'], { cwd });
      return { success: true, output: result.stdout };
    } catch {
      // Try npm/pnpm test
      try {
        const result = await execFileAsync('npm', ['run', 'test'], { cwd });
        return { success: true, output: result.stdout };
      } catch {
        return { success: false, output: 'Test command not found or failed' };
      }
    }
  } catch (error: unknown) {
    const errorMessage = error instanceof Error ? error.message : 'Unknown error';
    return { success: false, output: errorMessage };
  }
}

export const testerWorkflow: WorkflowDefinition<TesterWorkflowInput, TesterWorkflowOutput> = {
  name: 'tester',
  inputSchema: TesterWorkflowInputSchema,
  maxRetries: 2,
  timeout: 300000, // 5 minutes
  
  async run(ctx): Promise<WorkflowResult<TesterWorkflowOutput> | WorkflowFailureResult> {
    const events: WorkflowEvent[] = [];
    const model = createTesterModel();
    
    try {
      events.push({
        type: 'tester_started',
        data: { featureId: ctx.input.featureId, timestamp: new Date().toISOString() },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info({ featureId: ctx.input.featureId }, 'Starting tester workflow');
      
      const featureData = await fetchFeature(ctx.api, ctx.input.featureId);
      const feature = featureData as { 
        feature: { 
          id: string;
          title: string;
          description: string;
          acceptanceCriteria: string;
          status: string;
          project: {
            id: string;
            name: string;
            memory: string;
            architecture: string;
            codingStandards: string;
            gitRepositoryUrl: string;
          };
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
      
      const workspaceDir = path.join(os.tmpdir(), `devstack-tester-${Date.now()}`);
      
      ctx.logger.info({ workspaceDir }, 'Cloning repository for testing');
      
      const cloneSuccess = await cloneRepository(projectInfo.gitRepositoryUrl, workspaceDir);
      
      if (!cloneSuccess) {
        return {
          ok: false,
          error: {
            code: 'CLONE_FAILED',
            message: 'Failed to clone repository',
            retryable: true,
          },
          events,
        };
      }
      
      ctx.logger.info('Running build');
      const buildResult = await runBuild(workspaceDir);
      
      const defectsCreated: string[] = [];
      let featureStatus: 'COMPLETED' | 'REJECTED' = 'COMPLETED';
      
      if (!buildResult.success) {
        ctx.logger.warn('Build failed, creating defect');
        
        const defectId = await createDefect(
          ctx.api,
          projectInfo.id,
          `Build failure: ${featureInfo.title}`,
          `Build failed for feature "${featureInfo.title}".\n\nError output:\n${buildResult.output}`,
          'CRITICAL'
        );
        
        defectsCreated.push(defectId);
        featureStatus = 'REJECTED';
        
        await fs.rm(workspaceDir, { recursive: true, force: true });
        
        await transitionFeatureStatus(ctx.api, ctx.input.featureId, 'REJECTED', 'tester-workflow');
        
        events.push({
          type: 'build_failed',
          data: { defectId: defectId },
          timestamp: new Date().toISOString(),
        });
        
        return {
          ok: true,
          output: {
            buildResult: 'FAIL',
            testResult: 'SKIPPED',
            defectsCreated,
            summary: 'Build failed, defect created',
            featureStatus,
          },
          events,
        };
      }
      
      ctx.logger.info('Running tests');
      const testResult = await runTests(workspaceDir);
      
      if (!testResult.success) {
        ctx.logger.warn('Tests failed, creating defect');
        
        const defectId = await createDefect(
          ctx.api,
          projectInfo.id,
          `Test failure: ${featureInfo.title}`,
          `Tests failed for feature "${featureInfo.title}".\n\nTest output:\n${testResult.output}`,
          'HIGH'
        );
        
        defectsCreated.push(defectId);
        featureStatus = 'REJECTED';
        
        await fs.rm(workspaceDir, { recursive: true, force: true });
        
        await transitionFeatureStatus(ctx.api, ctx.input.featureId, 'REJECTED', 'tester-workflow');
        
        events.push({
          type: 'tests_failed',
          data: { defectId: defectId },
          timestamp: new Date().toISOString(),
        });
        
        return {
          ok: true,
          output: {
            buildResult: 'PASS',
            testResult: 'FAIL',
            defectsCreated,
            summary: 'Tests failed, defect created',
            featureStatus,
          },
          events,
        };
      }
      
      await fs.rm(workspaceDir, { recursive: true, force: true });
      
      await transitionFeatureStatus(ctx.api, ctx.input.featureId, 'COMPLETED', 'tester-workflow');
      
      events.push({
        type: 'tester_completed',
        data: { buildResult: 'PASS', testResult: 'PASS' },
        timestamp: new Date().toISOString(),
      });
      
      ctx.logger.info('All tests passed');
      
      return {
        ok: true,
        output: {
          buildResult: 'PASS',
          testResult: 'PASS',
          defectsCreated: [],
          summary: 'All builds and tests passed',
          featureStatus: 'COMPLETED',
        },
        events,
      };
    } catch (error) {
      ctx.logger.error({ error }, 'Tester workflow failed');
      
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      
      events.push({
        type: 'tester_failed',
        data: { 
          error: errorMessage,
          timestamp: new Date().toISOString() 
        },
        timestamp: new Date().toISOString(),
      });
      
      return {
        ok: false,
        error: {
          code: 'TESTER_WORKFLOW_ERROR',
          message: errorMessage,
          retryable: true,
          details: error,
        },
        events,
      };
    }
  },
};
