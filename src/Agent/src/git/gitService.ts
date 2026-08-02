import { simpleGit, SimpleGit } from 'simple-git';
import fs from 'fs';
import path from 'path';
import { logger } from '../logger.js';

export interface GitHubRepoRef {
  owner: string;
  name: string;
  normalizedUrl: string;
  rawUrl: string;
}

/**
 * [AG-240] Normalizes SSH and HTTPS GitHub URLs into canonical form for project matching.
 */
export function normalizeGithubUrl(rawUrl: string): GitHubRepoRef {
  const trimmed = rawUrl.trim();
  let owner = '';
  let name = '';

  // SSH pattern: git@github.com:owner/name[.git]
  const sshMatch = trimmed.match(/^git@github\.com:([^/]+)\/([^/]+?)(?:\.git)?$/i);
  if (sshMatch) {
    owner = sshMatch[1];
    name = sshMatch[2];
  } else {
    // HTTPS pattern: https://github.com/owner/name[.git]
    const httpMatch = trimmed.match(/^https?:\/\/github\.com\/([^/]+)\/([^/]+?)(?:\.git)?$/i);
    if (httpMatch) {
      owner = httpMatch[1];
      name = httpMatch[2];
    }
  }

  if (owner && name) {
    return {
      owner,
      name,
      normalizedUrl: `https://github.com/${owner}/${name}.git`,
      rawUrl: trimmed,
    };
  }

  // Fallback normalization if not matching standard GitHub format
  const cleanUrl = trimmed.endsWith('.git') ? trimmed : `${trimmed}.git`;
  return {
    owner: '',
    name: '',
    normalizedUrl: cleanUrl,
    rawUrl: trimmed,
  };
}

/**
 * Git service wrapper around simple-git.
 */
export class GitService {
  private git: SimpleGit;
  private baseDir: string;

  constructor(baseDir: string = process.cwd()) {
    this.baseDir = path.resolve(baseDir);
    this.git = simpleGit(this.baseDir);
    logger.debug({ baseDir: this.baseDir }, 'Initialized GitService');
  }

  public getBaseDir(): string {
    return this.baseDir;
  }

  /**
   * [AG-216] Gets remote origin URL from worktree.
   */
  public async getOriginRemoteUrl(): Promise<string> {
    const isRepo = await this.git.checkIsRepo();
    if (!isRepo) {
      throw new Error(`Directory '${this.baseDir}' is not a git repository.`);
    }

    const remotes = await this.git.getRemotes(true);
    const origin = remotes.find((r) => r.name === 'origin');
    if (!origin || !origin.refs.fetch) {
      throw new Error(
        `Remote 'origin' is missing in repository '${this.baseDir}'. Run 'git remote add origin <url>' to configure it.`
      );
    }

    return origin.refs.fetch;
  }
}

/**
 * Validates worktree directory exists.
 */
export function validateWorktreeDirectory(worktreePath: string): string {
  const resolved = path.resolve(worktreePath);
  if (!fs.existsSync(resolved) || !fs.statSync(resolved).isDirectory()) {
    throw new Error(`Repository root path '${worktreePath}' does not exist or is not a directory.`);
  }
  return resolved;
}
