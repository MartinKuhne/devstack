import { logger } from './logger.js';
import { parseCliArgs } from './cli/args.js';
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

  // 3. --show-plan
  if (options.showPlan) {
    await executeShowPlan(graphqlClient, options.repositoryRoot);
    return;
  }

  // 4. --run-plan
  if (options.runPlan) {
    await executeRunPlan(
      graphqlClient,
      engine,
      options.planPrompt || 'prompts/plan.prompt',
      options.repositoryRoot,
      undefined,
      options.modelProvider,
      options.modelName
    );
    return;
  }

  // 5. Default OpenCode prompt flow [AG-011]
  await engine.runPrompt({
    prompt: options.prompt,
    modelProvider: options.modelProvider,
    modelName: options.modelName,
  });

  process.exit(0);
}

if (process.env.NODE_ENV !== 'test') {
  main().catch((err: unknown) => {
    // [AG-012] Top-level unhandled exception handler
    const error = err as Error;
    logger.error(error, 'Fatal unhandled exception in DevStack Agent');
    process.stderr.write(`fatal error: ${error.message || String(err)}\n`);
    process.exit(1);
  });
}
