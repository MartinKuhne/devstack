export type GitProviderType = 'github' | 'gitea';

export interface GitProviderConfig {
  type: GitProviderType;
  baseUrl: string;
  token: string;
  owner: string;
  repo: string;
}

export interface PullRequestData {
  number: number;
  title: string;
  body?: string;
  state: 'open' | 'closed' | 'merged';
  head: {
    ref: string;
    sha: string;
    repo: string;
  };
  base: {
    ref: string;
    sha: string;
  };
  user: {
    login: string;
  };
  created_at: string;
  updated_at: string;
  merged_at?: string;
  mergeable?: boolean | null;
  draft?: boolean;
}

export interface PullRequestComment {
  id: number;
  body: string;
  user: {
    login: string;
  };
  created_at: string;
  updated_at: string;
  path?: string;
  line?: number;
}

export interface PullRequestReview {
  id: number;
  state: 'APPROVED' | 'CHANGES_REQUESTED' | 'COMMENTED' | 'DISMISSED' | 'PENDING';
  body?: string;
  user: {
    login: string;
  };
  submitted_at?: string;
}

export interface CreatePullRequestInput {
  repoUrl: string;
  headBranch: string;
  baseBranch: string;
  title: string;
  body?: string;
  draft?: boolean;
}

export interface GetPullRequestInput {
  repoUrl: string;
  prNumber: number;
}

export interface ListCommentsInput {
  repoUrl: string;
  prNumber: number;
}

export interface ApprovePullRequestInput {
  repoUrl: string;
  prNumber: number;
  message?: string;
}

export interface MergePullRequestInput {
  repoUrl: string;
  prNumber: number;
  mergeMethod?: 'merge' | 'squash' | 'rebase';
  commitTitle?: string;
  commitMessage?: string;
}

export interface PullRequestResult {
  ok: boolean;
  data?: PullRequestData;
  error?: string;
}

export interface CommentsResult {
  ok: boolean;
  comments?: PullRequestComment[];
  error?: string;
}

export interface ReviewResult {
  ok: boolean;
  review?: PullRequestReview;
  error?: string;
}

export interface MergeResult {
  ok: boolean;
  merged?: boolean;
  message?: string;
  error?: string;
}

export abstract class GitProvider {
  abstract type: GitProviderType;
  
  abstract createPullRequest(input: CreatePullRequestInput): Promise<PullRequestResult>;
  abstract getPullRequest(input: GetPullRequestInput): Promise<PullRequestResult>;
  abstract listComments(input: ListCommentsInput): Promise<CommentsResult>;
  abstract approvePullRequest(input: ApprovePullRequestInput): Promise<ReviewResult>;
  abstract mergePullRequest(input: MergePullRequestInput): Promise<MergeResult>;
  
  protected async fetchJson<T>(url: string, options: RequestInit = {}): Promise<T> {
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${this.getToken()}`,
        ...options.headers,
      },
    });
    
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }
    
    return response.json() as Promise<T>;
  }
  
  protected abstract getToken(): string;
  protected abstract getBaseUrl(): string;
  
  protected parseRepoUrl(repoUrl: string): { owner: string; repo: string } {
    const normalizedUrl = repoUrl.replace(/\.git$/, '');
    const parts = normalizedUrl.split('/').filter(Boolean);
    
    if (parts.length < 2) {
      throw new Error(`Invalid repository URL: ${repoUrl}`);
    }
    
    const repo = parts[parts.length - 1];
    const owner = parts[parts.length - 2];
    
    return { owner, repo };
  }
}

export class GitHubProvider extends GitProvider {
  readonly type: GitProviderType = 'github';
  
  constructor(private config: { token: string; baseUrl?: string }) {
    super();
  }
  
  protected getToken(): string {
    return this.config.token;
  }
  
  protected getBaseUrl(): string {
    return this.config.baseUrl || 'https://api.github.com';
  }
  
  async createPullRequest(input: CreatePullRequestInput): Promise<PullRequestResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls`;
      
      const data = await this.fetchJson<PullRequestData>(url, {
        method: 'POST',
        body: JSON.stringify({
          title: input.title,
          body: input.body,
          head: input.headBranch,
          base: input.baseBranch,
          draft: input.draft || false,
        }),
      });
      
      return {
        ok: true,
        data,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to create pull request: ${errorMessage}`,
      };
    }
  }
  
  async getPullRequest(input: GetPullRequestInput): Promise<PullRequestResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}`;
      
      const data = await this.fetchJson<PullRequestData>(url);
      
      return {
        ok: true,
        data,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to get pull request: ${errorMessage}`,
      };
    }
  }
  
  async listComments(input: ListCommentsInput): Promise<CommentsResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}/comments`;
      
      const comments = await this.fetchJson<PullRequestComment[]>(url);
      
      return {
        ok: true,
        comments,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to list comments: ${errorMessage}`,
      };
    }
  }
  
  async approvePullRequest(input: ApprovePullRequestInput): Promise<ReviewResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}/reviews`;
      
      const review = await this.fetchJson<PullRequestReview>(url, {
        method: 'POST',
        body: JSON.stringify({
          event: 'APPROVE',
          body: input.message,
        }),
      });
      
      return {
        ok: true,
        review,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to approve pull request: ${errorMessage}`,
      };
    }
  }
  
  async mergePullRequest(input: MergePullRequestInput): Promise<MergeResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}/merge`;
      
      const data = await this.fetchJson<{ merged: boolean; message: string }>(url, {
        method: 'PUT',
        body: JSON.stringify({
          merge_method: input.mergeMethod || 'merge',
          commit_title: input.commitTitle,
          commit_message: input.commitMessage,
        }),
      });
      
      return {
        ok: true,
        merged: data.merged,
        message: data.message,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to merge pull request: ${errorMessage}`,
      };
    }
  }
}

export class GiteaProvider extends GitProvider {
  readonly type: GitProviderType = 'gitea';
  
  constructor(private config: { token: string; baseUrl: string }) {
    super();
  }
  
  protected getToken(): string {
    return this.config.token;
  }
  
  protected getBaseUrl(): string {
    return this.config.baseUrl;
  }
  
  async createPullRequest(input: CreatePullRequestInput): Promise<PullRequestResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls`;
      
      const data = await this.fetchJson<PullRequestData>(url, {
        method: 'POST',
        body: JSON.stringify({
          title: input.title,
          body: input.body,
          head: input.headBranch,
          base: input.baseBranch,
        }),
      });
      
      return {
        ok: true,
        data,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to create pull request: ${errorMessage}`,
      };
    }
  }
  
  async getPullRequest(input: GetPullRequestInput): Promise<PullRequestResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}`;
      
      const data = await this.fetchJson<PullRequestData>(url);
      
      return {
        ok: true,
        data,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to get pull request: ${errorMessage}`,
      };
    }
  }
  
  async listComments(input: ListCommentsInput): Promise<CommentsResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/issues/${input.prNumber}/comments`;
      
      const comments = await this.fetchJson<PullRequestComment[]>(url);
      
      return {
        ok: true,
        comments,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to list comments: ${errorMessage}`,
      };
    }
  }
  
  async approvePullRequest(input: ApprovePullRequestInput): Promise<ReviewResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}/reviews`;
      
      const review = await this.fetchJson<PullRequestReview>(url, {
        method: 'POST',
        body: JSON.stringify({
          event: 'approve',
          content: input.message,
        }),
      });
      
      return {
        ok: true,
        review,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to approve pull request: ${errorMessage}`,
      };
    }
  }
  
  async mergePullRequest(input: MergePullRequestInput): Promise<MergeResult> {
    try {
      const { owner, repo } = this.parseRepoUrl(input.repoUrl);
      const url = `${this.getBaseUrl()}/repos/${owner}/${repo}/pulls/${input.prNumber}/merge`;
      
      const data = await this.fetchJson<{ merged: boolean; message: string }>(url, {
        method: 'POST',
        body: JSON.stringify({
          Do: (input.mergeMethod || 'merge').toUpperCase(),
          merge_title_field: input.commitTitle,
          merge_message: input.commitMessage,
        }),
      });
      
      return {
        ok: true,
        merged: data.merged,
        message: data.message,
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        ok: false,
        error: `Failed to merge pull request: ${errorMessage}`,
      };
    }
  }
}

export function detectProviderType(repoUrl: string): GitProviderType {
  const url = new URL(repoUrl.replace(/\.git$/, ''));
  const hostname = url.hostname.toLowerCase();
  
  if (hostname.includes('github.com') || hostname.includes('github')) {
    return 'github';
  }
  
  if (hostname.includes('gitea') || hostname.includes('gitea.com')) {
    return 'gitea';
  }
  
  return 'gitea';
}

export function createProvider(config: {
  type?: GitProviderType;
  repoUrl: string;
  token: string;
  baseUrl?: string;
}): GitProvider {
  const providerType = config.type || detectProviderType(config.repoUrl);
  
  if (providerType === 'github') {
    return new GitHubProvider({
      token: config.token,
      baseUrl: config.baseUrl,
    });
  }
  
  return new GiteaProvider({
    token: config.token,
    baseUrl: config.baseUrl || 'https://gitea.com',
  });
}
