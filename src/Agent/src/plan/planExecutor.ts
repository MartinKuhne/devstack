import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { DevStackGraphQLClient, ProjectDto, DeliverableDto } from '../graphql/client.js';
import { GitService, normalizeGithubUrl, validateWorktreeDirectory } from '../git/gitService.js';
import { OpenCodeAgentEngine } from '../opencode/opencodeEngine.js';
import { logger } from '../logger.js';

export interface PlanDiscoveryResult {
  worktree: string;
  repoUrl: string;
  project: ProjectDto;
  planDeliverables: DeliverableDto[];
}

/**
 * [AG-151] Summary returned by plan execution batch.
 */
export interface PlanRunSummary {
  processedIds: string[];
  failedDeliverables: Record<string, string>;
  succeededCount: number;
  failedCount: number;
}

/**
 * Resolves project and deliverables for --show-plan and --run-plan commands.
 */
export async function discoverPlanContext(
  graphqlClient: DevStackGraphQLClient,
  repositoryRootOverride?: string,
  opencodeSdkWorktree?: string
): Promise<PlanDiscoveryResult> {
  let worktree = '';

  // [AG-210] Explicit --repositoryRoot
  if (repositoryRootOverride) {
    worktree = validateWorktreeDirectory(repositoryRootOverride);
  } else if (opencodeSdkWorktree && opencodeSdkWorktree.trim()) {
    // [AG-212] OpenCode SDK worktree
    worktree = validateWorktreeDirectory(opencodeSdkWorktree);
  } else {
    // [AG-215] Fallback: use current directory if git repo, or throw
    worktree = process.cwd();
  }

  const gitService = new GitService(worktree);

  let rawRemoteUrl = '';
  try {
    rawRemoteUrl = await gitService.getOriginRemoteUrl();
  } catch (err: unknown) {
    const error = err as Error;
    // [AG-217] Missing remote / not git repo error handling
    process.stderr.write(`error: ${error.message}\n`);
    process.exit(2);
  }

  const { normalizedUrl } = normalizeGithubUrl(rawRemoteUrl);

  // [AG-124] DevStack project lookup by remote URL
  const project = await graphqlClient.findProjectByRepository(normalizedUrl);
  if (!project) {
    process.stderr.write(
      `error: No DevStack project registered for repository URL '${rawRemoteUrl}' (normalized: '${normalizedUrl}').\n`
    );
    process.exit(2);
  }

  // Filter deliverables with STATUS = PLAN
  const planDeliverables = (project.deliverables || []).filter(
    (d) => d.status.toUpperCase() === 'PLAN'
  );

  return {
    worktree,
    repoUrl: rawRemoteUrl,
    project,
    planDeliverables,
  };
}

/**
 * [AG-120 - AG-123] Handles --show-plan command execution.
 */
export async function executeShowPlan(
  graphqlClient: DevStackGraphQLClient,
  repositoryRootOverride?: string,
  opencodeWorktree?: string
): Promise<void> {
  try {
    const discovery = await discoverPlanContext(graphqlClient, repositoryRootOverride, opencodeWorktree);
    const { planDeliverables } = discovery;

    if (planDeliverables.length === 0) {
      console.log('  (none)');
      process.exit(0);
    }

    // [AG-123] Tabular report output
    console.log('  TYPE         ID                                     STATUS  TITLE');
    for (const d of planDeliverables) {
      const typePadded = d.type.padEnd(12);
      const idPadded = d.id.padEnd(38);
      const statusPadded = d.status.padEnd(7);
      console.log(`  ${typePadded} ${idPadded} ${statusPadded} ${d.title}`);
    }

    process.exit(0);
  } catch (err: unknown) {
    const error = err as Error;
    process.stderr.write(`error: ${error.message}\n`);
    process.exit(2);
  }
}

/**
 * [AG-140 - AG-152] Handles --run-plan command execution.
 */
export async function executeRunPlan(
  graphqlClient: DevStackGraphQLClient,
  engine: OpenCodeAgentEngine,
  planPromptPath: string,
  repositoryRootOverride?: string,
  opencodeWorktree?: string,
  userModelProvider?: string,
  userModelName?: string
): Promise<PlanRunSummary> {
  let discovery: PlanDiscoveryResult;
  try {
    // [AG-140] Discovery (shared with --show-plan)
    discovery = await discoverPlanContext(graphqlClient, repositoryRootOverride, opencodeWorktree);
  } catch (err: unknown) {
    const error = err as Error;
    process.stderr.write(`error: ${error.message}\n`);
    process.exit(2);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  // [AG-141 - AG-142] Template resolution
  let resolvedPromptPath = planPromptPath;
  if (!path.isAbsolute(resolvedPromptPath)) {
    const agentBaseDir = path.dirname(fileURLToPath(import.meta.url));
    resolvedPromptPath = path.resolve(agentBaseDir, '..', '..', planPromptPath);
    if (!fs.existsSync(resolvedPromptPath)) {
      resolvedPromptPath = path.resolve(process.cwd(), planPromptPath);
    }
  }

  // [AG-143] File existence check
  if (!fs.existsSync(resolvedPromptPath)) {
    process.stderr.write(`error: Plan prompt template file '${resolvedPromptPath}' does not exist.\n`);
    process.exit(2);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  const templateContent = fs.readFileSync(resolvedPromptPath, 'utf-8');

  // [AG-144] {{DeliverableId}} placeholder check
  if (!templateContent.includes('{{DeliverableId}}')) {
    process.stderr.write(
      `error: Plan prompt template '${resolvedPromptPath}' does not contain required placeholder '{{DeliverableId}}'.\n`
    );
    process.exit(2);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  const { planDeliverables } = discovery;
  if (planDeliverables.length === 0) {
    console.log('Plan summary: 0 succeeded, 0 failed.');
    process.exit(0);
    return { processedIds: [], failedDeliverables: {}, succeededCount: 0, failedCount: 0 };
  }

  const processedIds: string[] = [];
  const failedDeliverables: Record<string, string> = {};
  let succeeded = 0;
  let failed = 0;

  // [AG-145] Per-deliverable execution in order returned
  for (const d of planDeliverables) {
    processedIds.push(d.id);

    // [AG-146] Header output
    console.log(`→ Planning ${d.title} (${d.id})`);
    console.log(`  type: ${d.type}`);
    console.log(`  status: ${d.status}`);

    const promptText = templateContent.replace(/\{\{DeliverableId\}\}/g, d.id);

    try {
      // [AG-147] Session title: Plan: <title>
      const sessionId = await engine.runPrompt({
        prompt: promptText,
        title: `Plan: ${d.title}`,
        modelProvider: userModelProvider,
        modelName: userModelName,
      });

      succeeded++;
      // [AG-149] Success indicator
      console.log(`✓ Done. sessionId=${sessionId}`);
    } catch (err: unknown) {
      // [AG-148] Exception handling (one bad deliverable does not sink batch)
      failed++;
      const error = err as Error;
      failedDeliverables[d.id] = error.message;
      logger.error(error, `Planning deliverable ${d.id} failed`);
      process.stderr.write(`error: planning ${d.id} failed: ${error.message}\n`);
    }
  }

  const summary: PlanRunSummary = {
    processedIds,
    failedDeliverables,
    succeededCount: succeeded,
    failedCount: failed,
  };

  // [AG-152] Final summary output and exit code
  console.log(`Plan summary: ${succeeded} succeeded, ${failed} failed.`);
  if (failed > 0) {
    process.exit(3);
  } else {
    process.exit(0);
  }

  return summary;
}
