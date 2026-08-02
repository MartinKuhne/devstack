import { Octokit } from '@octokit/rest';
import { logger } from '../logger.js';

export interface GitHubServiceOptions {
  auth?: string;
  baseUrl?: string;
}

/**
 * GitHub service wrapper around Octokit REST client.
 */
export class GitHubService {
  private octokit: Octokit;

  /**
   * Initializes GitHub service with options.
   */
  constructor(options: GitHubServiceOptions = {}) {
    this.octokit = new Octokit({
      auth: options.auth || process.env.GITHUB_TOKEN,
      baseUrl: options.baseUrl,
    });
    logger.debug('Initialized GitHubService');
  }

  /**
   * Gets authenticated user profile.
   */
  public async getAuthenticatedUser() {
    const { data } = await this.octokit.rest.users.getAuthenticated();
    return data;
  }
}
