import { describe, it, expect } from 'vitest';
import { logger } from './logger.js';
import { DevStackGraphQLClient } from './graphql/client.js';
import { GitService } from './git/gitService.js';
import { GitHubService } from './github/githubService.js';
import { OpenCodeAgentEngine } from './opencode/opencodeEngine.js';

describe('DevStack Agent Module', () => {
  it('should initialize pino logger', () => {
    expect(logger).toBeDefined();
    expect(typeof logger.info).toBe('function');
  });

  it('should instantiate DevStackGraphQLClient', () => {
    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    expect(client.getClient()).toBeDefined();
  });

  it('should instantiate GitService', () => {
    const gitService = new GitService();
    expect(gitService).toBeDefined();
    expect(typeof gitService.getOriginRemoteUrl).toBe('function');
  });

  it('should instantiate GitHubService', () => {
    const githubService = new GitHubService();
    expect(githubService).toBeDefined();
  });

  it('should instantiate OpenCodeAgentEngine', () => {
    const engine = new OpenCodeAgentEngine();
    expect(engine.getClient()).toBeDefined();
  });
});
