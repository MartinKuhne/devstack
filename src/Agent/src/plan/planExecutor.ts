import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { DevStackGraphQLClient, ProjectDto, DeliverableDto } from '../graphql/client.js';
import { GitService, normalizeGithubUrl, validateWorktreeDirectory } from '../git/gitService.js';
import { GitHubService } from '../github/githubService.js';
import { OpenCodeAgentEngine } from '../opencode/opencodeEngine.js';
import { logger } from '../logger.js';
import { printMessage, printError, exitProcess, EXIT_CODE_SUCCESS, EXIT_CODE_ERROR, EXIT_CODE_PLAN_FAILED } from '../cli/output.js';

export interface PlanDiscoveryResult {
  worktree: string;
  repoUrl: string;
  project: ProjectDto;
  planDeliverables: DeliverableDto[];
}

export interface PlanRunSummary {
  processedIds: string[];
  failedDeliverables: Record<string, string>;
  succeededCount: number;
  failedCount: number;
}

export interface ExecuteRunPlanOptions {
  graphqlClient: DevStackGraphQLClient;
  engine: OpenCodeAgentEngine;
  planPromptPath: string;
  repositoryRootOverride?: string;
  opencodeWorktree?: string;
  userModelProvider?: string;
  userModelName?: string;
}

function resolveWorktree(repositoryRootOverride?: string, opencodeSdkWorktree?: string): string {
  if (repositoryRootOverride) {
    return validateWorktreeDirectory(repositoryRootOverride);
  }
  if (opencodeSdkWorktree && opencodeSdkWorktree.trim()) {
    return validateWorktreeDirectory(opencodeSdkWorktree);
  }
  return process.cwd();
}

async function resolveRepositoryUrl(gitService: GitService): Promise<string> {
  try {
    return await gitService.getOriginRemoteUrl();
  } catch (err: unknown) {
    printError((err as Error).message);
    return exitProcess(EXIT_CODE_ERROR);
  }
}

async function verifyRepository(rawRemoteUrl: string): Promise<string> {
  const parsedUrl = normalizeGithubUrl(rawRemoteUrl);
  if (parsedUrl.owner && parsedUrl.name) {
    const githubService = new GitHubService();
    const isVerified = await githubService.verifyRepository(parsedUrl.owner, parsedUrl.name);
    if (!isVerified) {
      logger.warn(`Failed to verify GitHub repository ${parsedUrl.owner}/${parsedUrl.name} via Octokit. Continuing with locally known details.`);
    }
  }
  return parsedUrl.normalizedUrl;
}

export async function discoverPlanContext(
  graphqlClient: DevStackGraphQLClient,
  repositoryRootOverride?: string,
  opencodeSdkWorktree?: string
): Promise<PlanDiscoveryResult> {
  const worktree = resolveWorktree(repositoryRootOverride, opencodeSdkWorktree);
  const gitService = new GitService(worktree);
  const rawRemoteUrl = await resolveRepositoryUrl(gitService);
  const normalizedUrl = await verifyRepository(rawRemoteUrl);

  const project = await graphqlClient.findProjectByRepository(normalizedUrl);
  if (!project) {
    printError(`No DevStack project registered for repository URL '${rawRemoteUrl}' (normalized: '${normalizedUrl}').`);
    exitProcess(EXIT_CODE_ERROR);
  }

  const planDeliverables = await graphqlClient.getPlanDeliverables(project.id);
  project.deliverables = planDeliverables;

  return { worktree, repoUrl: rawRemoteUrl, project, planDeliverables };
}

export async function executeShowPlan(
  graphqlClient: DevStackGraphQLClient,
  repositoryRootOverride?: string,
  opencodeWorktree?: string
): Promise<void> {
  try {
    const { planDeliverables } = await discoverPlanContext(graphqlClient, repositoryRootOverride, opencodeWorktree);

    if (planDeliverables.length === 0) {
      printMessage('  (none)');
      exitProcess(EXIT_CODE_SUCCESS);
    }

    printMessage('  TYPE         ID                                     STATUS  TITLE');
    for (const d of planDeliverables) {
      const typePadded = d.type.padEnd(12);
      const idPadded = d.id.padEnd(38);
      const statusPadded = d.status.padEnd(7);
      printMessage(`  ${typePadded} ${idPadded} ${statusPadded} ${d.title}`);
    }

    exitProcess(EXIT_CODE_SUCCESS);
  } catch (err: unknown) {
    printError((err as Error).message);
    exitProcess(EXIT_CODE_ERROR);
  }
}

function resolveTemplateContent(planPromptPath: string): string {
  let resolvedPromptPath = planPromptPath;
  if (!path.isAbsolute(resolvedPromptPath)) {
    const agentBaseDir = path.dirname(fileURLToPath(import.meta.url));
    resolvedPromptPath = path.resolve(agentBaseDir, '..', '..', planPromptPath);
    if (!fs.existsSync(resolvedPromptPath)) {
      resolvedPromptPath = path.resolve(process.cwd(), planPromptPath);
    }
  }

  if (!fs.existsSync(resolvedPromptPath)) {
    printError(`Plan prompt template file '${resolvedPromptPath}' does not exist.`);
    exitProcess(EXIT_CODE_ERROR);
    return '';
  }

  const content = fs.readFileSync(resolvedPromptPath, 'utf-8');
  if (!content.includes('{{DeliverableId}}')) {
    printError(`Plan prompt template '${resolvedPromptPath}' does not contain required placeholder '{{DeliverableId}}'.`);
    exitProcess(EXIT_CODE_ERROR);
    return '';
  }

  return content;
}

export async function executeRunPlan(options: ExecuteRunPlanOptions): Promise<PlanRunSummary> {
  const { graphqlClient, engine, planPromptPath, repositoryRootOverride, opencodeWorktree, userModelProvider, userModelName } = options;

  let discovery: PlanDiscoveryResult;
  try {
    discovery = await discoverPlanContext(graphqlClient, repositoryRootOverride, opencodeWorktree);
  } catch (err: unknown) {
    printError((err as Error).message);
    exitProcess(EXIT_CODE_ERROR);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  const templateContent = resolveTemplateContent(planPromptPath);
  if (!templateContent) {
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }
  const { planDeliverables } = discovery;

  if (planDeliverables.length === 0) {
    printMessage('Plan summary: 0 succeeded, 0 failed.');
    exitProcess(EXIT_CODE_SUCCESS);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  const processedIds: string[] = [];
  const failedDeliverables: Record<string, string> = {};
  let succeeded = 0;
  let failed = 0;

  for (const d of planDeliverables) {
    processedIds.push(d.id);
    printMessage(`→ Planning ${d.title} (${d.id})`);
    printMessage(`  type: ${d.type}`);
    printMessage(`  status: ${d.status}`);

    const promptText = templateContent.replace(/\{\{DeliverableId\}\}/g, d.id);

    try {
      const sessionId = await engine.runPrompt({
        prompt: promptText,
        title: `Plan: ${d.title}`,
        modelProvider: userModelProvider,
        modelName: userModelName,
      });
      succeeded++;
      printMessage(`✓ Done. sessionId=${sessionId}`);
    } catch (err: unknown) {
      failed++;
      const error = err as Error;
      failedDeliverables[d.id] = error.message;
      logger.error(error, `Planning deliverable ${d.id} failed`);
      printError(`planning ${d.id} failed: ${error.message}`);
    }
  }

  const summary: PlanRunSummary = { processedIds, failedDeliverables, succeededCount: succeeded, failedCount: failed };
  printMessage(`Plan summary: ${succeeded} succeeded, ${failed} failed.`);
  exitProcess(failed > 0 ? EXIT_CODE_PLAN_FAILED : EXIT_CODE_SUCCESS);
  return summary;
}
