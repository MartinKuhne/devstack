import { z } from 'zod';
import { ToolContext, ToolResult, ToolDefinition } from '../tool.js';
import {
  createProvider,
  GitProvider,
  CreatePullRequestInput,
  GetPullRequestInput,
  ListCommentsInput,
  ApprovePullRequestInput,
  MergePullRequestInput,
} from './git-provider.js';

const CreatePullRequestInputSchema = z.object({
  repoUrl: z.string().url('Repository URL must be a valid URL'),
  headBranch: z.string().min(1, 'Head branch name is required'),
  baseBranch: z.string().min(1, 'Base branch name is required'),
  title: z.string().min(1, 'Title is required'),
  body: z.string().optional(),
  draft: z.boolean().optional(),
  gitProvider: z.enum(['github', 'gitea']).optional(),
  providerBaseUrl: z.string().url().optional(),
});

const GetPullRequestInputSchema = z.object({
  repoUrl: z.string().url('Repository URL must be a valid URL'),
  prNumber: z.number().int().positive('PR number must be a positive integer'),
  gitProvider: z.enum(['github', 'gitea']).optional(),
  providerBaseUrl: z.string().url().optional(),
});

const ListCommentsInputSchema = z.object({
  repoUrl: z.string().url('Repository URL must be a valid URL'),
  prNumber: z.number().int().positive('PR number must be a positive integer'),
  gitProvider: z.enum(['github', 'gitea']).optional(),
  providerBaseUrl: z.string().url().optional(),
});

const ApprovePullRequestInputSchema = z.object({
  repoUrl: z.string().url('Repository URL must be a valid URL'),
  prNumber: z.number().int().positive('PR number must be a positive integer'),
  message: z.string().optional(),
  gitProvider: z.enum(['github', 'gitea']).optional(),
  providerBaseUrl: z.string().url().optional(),
});

const MergePullRequestInputSchema = z.object({
  repoUrl: z.string().url('Repository URL must be a valid URL'),
  prNumber: z.number().int().positive('PR number must be a positive integer'),
  mergeMethod: z.enum(['merge', 'squash', 'rebase']).optional(),
  commitTitle: z.string().optional(),
  commitMessage: z.string().optional(),
  gitProvider: z.enum(['github', 'gitea']).optional(),
  providerBaseUrl: z.string().url().optional(),
});

const PullRequestOutputSchema = z.object({
  ok: z.boolean(),
  data: z.object({
    number: z.number(),
    title: z.string(),
    body: z.string().optional(),
    state: z.string(),
    head: z.object({
      ref: z.string(),
      sha: z.string(),
    }),
    base: z.object({
      ref: z.string(),
      sha: z.string(),
    }),
    created_at: z.string(),
    updated_at: z.string(),
    merged_at: z.string().optional(),
  }).optional(),
  error: z.string().optional(),
});

const CommentsOutputSchema = z.object({
  ok: z.boolean(),
  comments: z.array(z.object({
    id: z.number(),
    body: z.string(),
    user: z.object({
      login: z.string(),
    }),
    created_at: z.string(),
    updated_at: z.string(),
    path: z.string().optional(),
    line: z.number().optional(),
  })).optional(),
  error: z.string().optional(),
});

const ReviewOutputSchema = z.object({
  ok: z.boolean(),
  review: z.object({
    id: z.number(),
    state: z.string(),
    body: z.string().optional(),
    user: z.object({
      login: z.string(),
    }),
    submitted_at: z.string().optional(),
  }).optional(),
  error: z.string().optional(),
});

const MergeOutputSchema = z.object({
  ok: z.boolean(),
  merged: z.boolean().optional(),
  message: z.string().optional(),
  error: z.string().optional(),
});

function getProviderFromContext(context: ToolContext, repoUrl: string, providerType?: 'github' | 'gitea', baseUrl?: string): GitProvider {
  const token = (context.metadata?.gitToken as string) || (context.metadata?.githubToken as string);
  
  if (!token) {
    throw new Error('Git token not found in context metadata. Please ensure gitToken or githubToken is provided.');
  }
  
  return createProvider({
    type: providerType,
    repoUrl,
    token,
    baseUrl,
  });
}

export async function prCreateTool(
  input: z.infer<typeof CreatePullRequestInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof PullRequestOutputSchema>>> {
  try {
    context.logger.info({ repoUrl: input.repoUrl, headBranch: input.headBranch, baseBranch: input.baseBranch }, 'Creating pull request');
    
    const provider = getProviderFromContext(context, input.repoUrl, input.gitProvider, input.providerBaseUrl);
    
    const prInput: CreatePullRequestInput = {
      repoUrl: input.repoUrl,
      headBranch: input.headBranch,
      baseBranch: input.baseBranch,
      title: input.title,
      body: input.body,
      draft: input.draft,
    };
    
    const result = await provider.createPullRequest(prInput);
    
    context.logger.info({ ok: result.ok, error: result.error }, 'Pull request creation completed');
    
    return {
      ok: true,
      output: {
        ok: result.ok,
        data: result.data,
        error: result.error,
      },
    };
  } catch (error) {
    context.logger.error({ repoUrl: input.repoUrl, error }, 'Failed to create pull request');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error creating pull request',
      },
    };
  }
}

export async function prGetTool(
  input: z.infer<typeof GetPullRequestInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof PullRequestOutputSchema>>> {
  try {
    context.logger.info({ repoUrl: input.repoUrl, prNumber: input.prNumber }, 'Getting pull request');
    
    const provider = getProviderFromContext(context, input.repoUrl, input.gitProvider, input.providerBaseUrl);
    
    const prInput: GetPullRequestInput = {
      repoUrl: input.repoUrl,
      prNumber: input.prNumber,
    };
    
    const result = await provider.getPullRequest(prInput);
    
    context.logger.info({ ok: result.ok, error: result.error }, 'Pull request retrieval completed');
    
    return {
      ok: true,
      output: {
        ok: result.ok,
        data: result.data,
        error: result.error,
      },
    };
  } catch (error) {
    context.logger.error({ repoUrl: input.repoUrl, prNumber: input.prNumber, error }, 'Failed to get pull request');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error getting pull request',
      },
    };
  }
}

export async function prListCommentsTool(
  input: z.infer<typeof ListCommentsInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof CommentsOutputSchema>>> {
  try {
    context.logger.info({ repoUrl: input.repoUrl, prNumber: input.prNumber }, 'Listing pull request comments');
    
    const provider = getProviderFromContext(context, input.repoUrl, input.gitProvider, input.providerBaseUrl);
    
    const commentsInput: ListCommentsInput = {
      repoUrl: input.repoUrl,
      prNumber: input.prNumber,
    };
    
    const result = await provider.listComments(commentsInput);
    
    context.logger.info({ ok: result.ok, commentCount: result.comments?.length }, 'Comments listing completed');
    
    return {
      ok: true,
      output: {
        ok: result.ok,
        comments: result.comments,
        error: result.error,
      },
    };
  } catch (error) {
    context.logger.error({ repoUrl: input.repoUrl, prNumber: input.prNumber, error }, 'Failed to list comments');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error listing comments',
      },
    };
  }
}

export async function prApproveTool(
  input: z.infer<typeof ApprovePullRequestInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof ReviewOutputSchema>>> {
  try {
    context.logger.info({ repoUrl: input.repoUrl, prNumber: input.prNumber }, 'Approving pull request');
    
    const provider = getProviderFromContext(context, input.repoUrl, input.gitProvider, input.providerBaseUrl);
    
    const approveInput: ApprovePullRequestInput = {
      repoUrl: input.repoUrl,
      prNumber: input.prNumber,
      message: input.message,
    };
    
    const result = await provider.approvePullRequest(approveInput);
    
    context.logger.info({ ok: result.ok, error: result.error }, 'Pull request approval completed');
    
    return {
      ok: true,
      output: {
        ok: result.ok,
        review: result.review,
        error: result.error,
      },
    };
  } catch (error) {
    context.logger.error({ repoUrl: input.repoUrl, prNumber: input.prNumber, error }, 'Failed to approve pull request');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error approving pull request',
      },
    };
  }
}

export async function prMergeTool(
  input: z.infer<typeof MergePullRequestInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof MergeOutputSchema>>> {
  try {
    context.logger.info({ repoUrl: input.repoUrl, prNumber: input.prNumber, mergeMethod: input.mergeMethod }, 'Merging pull request');
    
    const provider = getProviderFromContext(context, input.repoUrl, input.gitProvider, input.providerBaseUrl);
    
    const mergeInput: MergePullRequestInput = {
      repoUrl: input.repoUrl,
      prNumber: input.prNumber,
      mergeMethod: input.mergeMethod,
      commitTitle: input.commitTitle,
      commitMessage: input.commitMessage,
    };
    
    const result = await provider.mergePullRequest(mergeInput);
    
    context.logger.info({ ok: result.ok, merged: result.merged, error: result.error }, 'Pull request merge completed');
    
    return {
      ok: true,
      output: {
        ok: result.ok,
        merged: result.merged,
        message: result.message,
        error: result.error,
      },
    };
  } catch (error) {
    context.logger.error({ repoUrl: input.repoUrl, prNumber: input.prNumber, error }, 'Failed to merge pull request');
    
    if (error instanceof Error) {
      return {
        ok: true,
        output: {
          ok: false,
          error: error.message,
        },
      };
    }
    
    return {
      ok: true,
      output: {
        ok: false,
        error: 'Unknown error merging pull request',
      },
    };
  }
}

export function createPullRequestTools(): ToolDefinition<z.ZodTypeAny, unknown>[] {
  return [
    {
      name: 'pr_create',
      description:
        'Create a new pull request on GitHub or Gitea. Requires repoUrl, headBranch, baseBranch, and title. The git token must be provided in the context metadata. Supports both GitHub and Gitea providers.',
      inputSchema: CreatePullRequestInputSchema,
      outputSchema: PullRequestOutputSchema,
      execute: async (input, context) => prCreateTool(input as z.infer<typeof CreatePullRequestInputSchema>, context),
    },
    {
      name: 'pr_get',
      description:
        'Get details of a specific pull request by number. Returns the PR state, branches, and metadata. Requires repoUrl and prNumber.',
      inputSchema: GetPullRequestInputSchema,
      outputSchema: PullRequestOutputSchema,
      execute: async (input, context) => prGetTool(input as z.infer<typeof GetPullRequestInputSchema>, context),
    },
    {
      name: 'pr_list_comments',
      description:
        'List all review comments on a pull request. Returns comments with author, body, and timestamps. Requires repoUrl and prNumber.',
      inputSchema: ListCommentsInputSchema,
      outputSchema: CommentsOutputSchema,
      execute: async (input, context) => prListCommentsTool(input as z.infer<typeof ListCommentsInputSchema>, context),
    },
    {
      name: 'pr_approve',
      description:
        'Approve a pull request. Submits an approving review. Requires repoUrl and prNumber. Optionally include a review message.',
      inputSchema: ApprovePullRequestInputSchema,
      outputSchema: ReviewOutputSchema,
      execute: async (input, context) => prApproveTool(input as z.infer<typeof ApprovePullRequestInputSchema>, context),
    },
    {
      name: 'pr_merge',
      description:
        'Merge a pull request. Supports merge, squash, and rebase strategies. Requires repoUrl and prNumber. Optionally specify merge method and commit message.',
      inputSchema: MergePullRequestInputSchema,
      outputSchema: MergeOutputSchema,
      execute: async (input, context) => prMergeTool(input as z.infer<typeof MergePullRequestInputSchema>, context),
    },
  ];
}

export {
  GitProvider,
  GitHubProvider,
  GiteaProvider,
  createProvider,
  detectProviderType,
} from './git-provider.js';
