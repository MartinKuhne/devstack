import { z } from 'zod';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { ToolContext, ToolResult, ToolDefinition } from '../tool.js';

const execFileAsync = promisify(execFile);

const GIT_ALLOWLIST: Set<string> = new Set([
  'clone',
  'checkout',
  'checkout -b',
  'add',
  'commit',
  'status',
  'diff',
  'log',
  'push',
  'pull',
  'fetch',
  'branch',
  'merge',
  'rev-parse',
  'show',
  'remote',
  'remote add',
  'remote remove',
  'remote -v',
  'init',
]);

const DANGEROUS_GIT_COMMANDS: Set<string> = new Set([
  'filter-branch',
  'filter-repo',
  'push --force',
  'push -f',
  'reset --hard',
  'clean -fd',
  'gc --prune=all',
  'update-ref',
  'replace',
  'bisect',
]);

export interface GitResult {
  ok: boolean;
  output?: string;
  error?: string;
}

const GitCloneInputSchema = z.object({
  url: z.string().min(1, 'Repository URL is required'),
  branch: z.string().optional(),
  targetDir: z.string().min(1, 'Target directory is required'),
});

const GitCheckoutInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  branch: z.string().min(1, 'Branch name is required'),
});

const GitCheckoutNewBranchInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  branchName: z.string().min(1, 'Branch name is required'),
});

const GitAddInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  files: z.array(z.string()).optional(),
});

const GitCommitInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  message: z.string().min(1, 'Commit message is required'),
});

const GitStatusInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
});

const GitDiffInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  file: z.string().optional(),
});

const GitLogInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  n: z.number().int().positive().optional(),
});

const GitPushInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  remote: z.string().optional(),
  branch: z.string().optional(),
});

const GitPullInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  remote: z.string().optional(),
  branch: z.string().optional(),
});

const GitFetchInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  remote: z.string().optional(),
});

const GitBranchInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  name: z.string().optional(),
});

const GitOutputSchema = z.object({
  ok: z.boolean(),
  output: z.string().optional(),
  error: z.string().optional(),
});

function isGitCommandAllowed(command: string, args: string[]): boolean {
  const fullCommand = `${command} ${args.join(' ')}`.toLowerCase().trim();
  
  for (const dangerousCommand of DANGEROUS_GIT_COMMANDS) {
    if (fullCommand.includes(dangerousCommand.toLowerCase())) {
      return false;
    }
  }

  if (args.length === 0) {
    return false;
  }

  const firstArg = args[0].toLowerCase().trim();
  
  if (GIT_ALLOWLIST.has(firstArg)) {
    return true;
  }

  const twoArgCommand = args.length >= 2 ? `${firstArg} ${args[1].toLowerCase()}` : null;
  if (twoArgCommand && GIT_ALLOWLIST.has(twoArgCommand)) {
    return true;
  }

  return false;
}

async function executeGitCommand(
  dir: string,
  args: string[],
  context: ToolContext
): Promise<GitResult> {
  if (!isGitCommandAllowed('git', args)) {
    context.logger.warn({ args }, 'Git command blocked by allowlist');
    return {
      ok: false,
      error: `Git command '${args.join(' ')}' is not allowed`,
    };
  }

  context.logger.debug({ dir, args }, 'Executing git command');

  try {
    const { stdout, stderr } = await execFileAsync('git', args, {
      cwd: dir,
      env: { ...process.env },
    });

    const output = stdout || stderr;

    context.logger.debug({ dir, args, output: output?.substring(0, 200) }, 'Git command completed');

    return {
      ok: true,
      output: output?.trim(),
    };
  } catch (error) {
    context.logger.error({ dir, args, error }, 'Git command failed');

    if (error instanceof Error) {
      const execError = error as Error & { stdout?: string; stderr?: string; code?: number };
      const errorMessage = execError.stderr || execError.stdout || execError.message;

      return {
        ok: false,
        error: errorMessage,
      };
    }

    return {
      ok: false,
      error: 'Unknown error executing git command',
    };
  }
}

export async function gitCloneTool(
  input: z.infer<typeof GitCloneInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { url, branch, targetDir } = input;

  const args: string[] = ['clone'];
  if (branch) {
    args.push('--branch', branch);
  }
  args.push(url, targetDir);

  const result = await executeGitCommand(process.cwd(), args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitCheckoutTool(
  input: z.infer<typeof GitCheckoutInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, branch } = input;

  const result = await executeGitCommand(dir, ['checkout', branch], context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitCheckoutNewBranchTool(
  input: z.infer<typeof GitCheckoutNewBranchInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, branchName } = input;

  const result = await executeGitCommand(dir, ['checkout', '-b', branchName], context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitAddTool(
  input: z.infer<typeof GitAddInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, files } = input;

  const args = ['add'];
  if (files && files.length > 0) {
    args.push(...files);
  } else {
    args.push('.');
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitCommitTool(
  input: z.infer<typeof GitCommitInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, message } = input;

  const result = await executeGitCommand(dir, ['commit', '-m', message], context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitStatusTool(
  input: z.infer<typeof GitStatusInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir } = input;

  const result = await executeGitCommand(dir, ['status'], context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitDiffTool(
  input: z.infer<typeof GitDiffInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, file } = input;

  const args = ['diff'];
  if (file) {
    args.push(file);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitLogTool(
  input: z.infer<typeof GitLogInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, n } = input;

  const args = ['log', '--oneline'];
  if (n && n > 0) {
    args.push(`-n ${n}`);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitPushTool(
  input: z.infer<typeof GitPushInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, remote, branch } = input;

  const args = ['push'];
  if (remote) {
    args.push(remote);
  }
  if (branch) {
    args.push(branch);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitPullTool(
  input: z.infer<typeof GitPullInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, remote, branch } = input;

  const args = ['pull'];
  if (remote) {
    args.push(remote);
  }
  if (branch) {
    args.push(branch);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitFetchTool(
  input: z.infer<typeof GitFetchInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, remote } = input;

  const args = ['fetch'];
  if (remote) {
    args.push(remote);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export async function gitBranchTool(
  input: z.infer<typeof GitBranchInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof GitOutputSchema>>> {
  const { dir, name } = input;

  const args = ['branch'];
  if (name) {
    args.push(name);
  }

  const result = await executeGitCommand(dir, args, context);

  return {
    ok: true,
    output: result,
  };
}

export function createGitTools(): ToolDefinition<z.ZodTypeAny, unknown>[] {
  return [
    {
      name: 'git_clone',
      description:
        'Clone a git repository. Optionally specify a branch. Returns the cloned repository path.',
      inputSchema: GitCloneInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitCloneTool(input as z.infer<typeof GitCloneInputSchema>, context),
    },
    {
      name: 'git_checkout',
      description:
        'Switch to an existing branch. The branch must already exist in the repository.',
      inputSchema: GitCheckoutInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitCheckoutTool(input as z.infer<typeof GitCheckoutInputSchema>, context),
    },
    {
      name: 'git_checkout_new_branch',
      description:
        'Create and switch to a new branch. The branch will be created from the current HEAD.',
      inputSchema: GitCheckoutNewBranchInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) =>
        gitCheckoutNewBranchTool(input as z.infer<typeof GitCheckoutNewBranchInputSchema>, context),
    },
    {
      name: 'git_add',
      description:
        'Stage changes for commit. Can stage specific files or all changes (.).',
      inputSchema: GitAddInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitAddTool(input as z.infer<typeof GitAddInputSchema>, context),
    },
    {
      name: 'git_commit',
      description:
        'Commit staged changes with a message. The working directory must have staged changes.',
      inputSchema: GitCommitInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitCommitTool(input as z.infer<typeof GitCommitInputSchema>, context),
    },
    {
      name: 'git_status',
      description:
        'Show the working tree status. Returns information about modified, staged, and untracked files.',
      inputSchema: GitStatusInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitStatusTool(input as z.infer<typeof GitStatusInputSchema>, context),
    },
    {
      name: 'git_diff',
      description:
        'Show changes between commits, commit and working tree, etc. Optionally specify a file.',
      inputSchema: GitDiffInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitDiffTool(input as z.infer<typeof GitDiffInputSchema>, context),
    },
    {
      name: 'git_log',
      description:
        'Show commit logs. Optionally limit the number of commits with n parameter.',
      inputSchema: GitLogInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitLogTool(input as z.infer<typeof GitLogInputSchema>, context),
    },
    {
      name: 'git_push',
      description:
        'Push changes to a remote repository. Optionally specify remote and branch.',
      inputSchema: GitPushInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitPushTool(input as z.infer<typeof GitPushInputSchema>, context),
    },
    {
      name: 'git_pull',
      description:
        'Fetch and integrate changes from a remote repository. Optionally specify remote and branch.',
      inputSchema: GitPullInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitPullTool(input as z.infer<typeof GitPullInputSchema>, context),
    },
    {
      name: 'git_fetch',
      description:
        'Download objects and refs from another repository. Optionally specify remote.',
      inputSchema: GitFetchInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitFetchTool(input as z.infer<typeof GitFetchInputSchema>, context),
    },
    {
      name: 'git_branch',
      description:
        'List, create, or delete branches. Optionally specify a branch name.',
      inputSchema: GitBranchInputSchema,
      outputSchema: GitOutputSchema,
      execute: async (input, context) => gitBranchTool(input as z.infer<typeof GitBranchInputSchema>, context),
    },
  ];
}

export {
  GIT_ALLOWLIST,
  DANGEROUS_GIT_COMMANDS,
  isGitCommandAllowed,
  executeGitCommand,
};
