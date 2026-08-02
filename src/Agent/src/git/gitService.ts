import { simpleGit, SimpleGit } from 'simple-git';
import { logger } from '../logger.js';

/**
 * Git service wrapper around simple-git.
 */
export class GitService {
  private git: SimpleGit;

  /**
   * Initializes GitService for a repository directory.
   */
  constructor(baseDir: string = process.cwd()) {
    this.git = simpleGit(baseDir);
    logger.debug({ baseDir }, 'Initialized GitService');
  }

  /**
   * Gets current repository status.
   */
  public async getStatus() {
    return await this.git.status();
  }

  /**
   * Gets current branch name.
   */
  public async getCurrentBranch(): Promise<string> {
    const status = await this.git.status();
    return status.current || '';
  }
}
