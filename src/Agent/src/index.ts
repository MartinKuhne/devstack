import { logger } from './logger.js';
import { DevStackGraphQLClient } from './graphql/client.js';
import { GitService } from './git/gitService.js';
import { GitHubService } from './github/githubService.js';
import { OpenCodeService } from './opencode/opencodeService.js';

export { logger, DevStackGraphQLClient, GitService, GitHubService, OpenCodeService };

/**
 * Entry point for DevStack Agent.
 */
export async function main(): Promise<void> {
  logger.info('Starting DevStack Agent...');

  const graphqlClient = new DevStackGraphQLClient({
    endpoint: process.env.GRAPHQL_ENDPOINT || 'http://localhost:8087/graphql',
  });

  const gitService = new GitService();
  const githubService = new GitHubService();
  const opencodeService = new OpenCodeService();

  logger.info(
    {
      graphql: graphqlClient !== undefined,
      git: gitService !== undefined,
      github: githubService !== undefined,
      opencode: opencodeService !== undefined,
    },
    'DevStack Agent services initialized successfully.'
  );
}

if (process.env.NODE_ENV !== 'test') {
  main().catch((err) => {
    logger.error(err, 'Unhandled error in DevStack Agent');
    process.exit(1);
  });
}
