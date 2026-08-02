import { logger } from './logger.js';
import { parseCliArgs } from './cli/args.js';
import { printError, exitProcess, EXIT_CODE_SUCCESS, EXIT_CODE_ERROR } from './cli/output.js';
import { DevStackGraphQLClient } from './graphql/client.js';
import { executeListProjects, executeGetProject } from './graphql/projectOperations.js';
import { executeShowPlan, executeRunPlan } from './plan/planExecutor.js';
import { OpenCodeAgentEngine } from './opencode/opencodeEngine.js';
import { GitService } from './git/gitService.js';
import { GitHubService } from './github/githubService.js';

export { logger, DevStackGraphQLClient, GitService, GitHubService, OpenCodeAgentEngine };

/**
 * Main entry point for DevStack Agent.
 */
export async function main(): Promise<void> {
  const args = process.argv.slice(2);
  const options = parseCliArgs(args);

  const graphqlEndpoint = options.graphqlEndpoint;
  const opencodeBaseUrl = process.env.OPENCODE_BASE_URL || 'http://localhost:4096';

  logger.info(`DevStack Agent Starting...`);
  logger.info(`  OpenCode Base URL: ${opencodeBaseUrl}`);
  logger.info(`  GraphQL Endpoint:  ${graphqlEndpoint}`);

  const graphqlClient = new DevStackGraphQLClient({ endpoint: graphqlEndpoint });
  const engine = new OpenCodeAgentEngine({ baseUrl: opencodeBaseUrl });

  // [AG-010] Mode short-circuiting

  // 1. --list-projects
  if (options.listProjects) {
    await executeListProjects(graphqlClient, options.listProjectsCount);
    return;
  }

  // 1b. --list-providers
  if (options.listProviders) {
    await engine.listProviders();
    return;
  }

  // 2. --get-project
  if (options.getProjectUuid) {
    await executeGetProject(graphqlClient, options.getProjectUuid);
    return;
  }

  // [AG-212] OpenCode SDK Project.GetCurrent() worktree
  let opencodeWorktree: string | undefined = undefined;
  if (!options.repositoryRoot) {
    try {
      const projRes = await engine.getClient().project.current();
      const projData = projRes.data as { worktree?: string } | undefined;
      opencodeWorktree = projData?.worktree;
      if (!opencodeWorktree || !opencodeWorktree.trim()) {
        // [AG-213] Empty worktree warning
        logger.warn(`OpenCode SDK at ${opencodeBaseUrl} reported an empty worktree.`);
      }
    } catch {
      logger.debug('Failed to get current project from OpenCode SDK');
    }
  }

  // 3. --show-plan
  if (options.showPlan) {
    await executeShowPlan(graphqlClient, options.repositoryRoot, opencodeWorktree);
    return;
  }

  // 4. --run-plan
  if (options.runPlan) {
    await executeRunPlan({
      graphqlClient,
      engine,
      planPromptPath: options.planPrompt || 'prompts/plan.prompt',
      repositoryRootOverride: options.repositoryRoot,
      opencodeWorktree,
      userModelProvider: options.modelProvider,
      userModelName: options.modelName
    });
    return;
  }

  // 5. Default OpenCode prompt flow [AG-011]
  await engine.runPrompt({
    prompt: options.prompt,
    modelProvider: options.modelProvider,
    modelName: options.modelName,
  });

  exitProcess(EXIT_CODE_SUCCESS);
}

if (process.env.NODE_ENV !== 'test') {
  main().catch((err: unknown) => {
    // [AG-012] Top-level unhandled exception handler
    const error = err as Error;
    logger.error(error, 'Fatal unhandled exception in DevStack Agent');
    printError(error.message || String(err));
    exitProcess(EXIT_CODE_ERROR);
  });
}
