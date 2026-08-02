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

  /**
   * Verifies that a repository exists and is accessible.
   */
  public async verifyRepository(owner: string, name: string): Promise<boolean> {
    try {
      await this.octokit.rest.repos.get({ owner, repo: name });
      return true;
    } catch {
      return false;
    }
  }
}
