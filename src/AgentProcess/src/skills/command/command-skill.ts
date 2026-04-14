import { z } from 'zod';
import { spawn, SpawnOptionsWithoutStdio } from 'node:child_process';
import { ToolContext, ToolResult, ToolDefinition } from '../tool.js';

const COMMAND_ALLOWLIST: Record<string, { patterns: string[][]; description: string }> = {
  'dotnet': {
    patterns: [
      ['build'],
      ['build', '*'],
      ['test'],
      ['test', '*'],
      ['run'],
      ['restore'],
      ['publish'],
      ['publish', '*'],
      ['clean'],
      ['format'],
      ['format', '*'],
    ],
    description: 'Dotnet CLI for builds, tests, and package management',
  },
  'npm': {
    patterns: [
      ['run', 'build'],
      ['run', 'test'],
      ['run', 'lint'],
      ['run', 'typecheck'],
      ['run', 'dev'],
      ['run', 'start'],
      ['run', 'clean'],
      ['test'],
      ['ci'],
      ['install'],
    ],
    description: 'NPM package manager and scripts',
  },
  'pnpm': {
    patterns: [
      ['run', '*'],
      ['install'],
      ['install', '*'],
      ['test'],
      ['test', '*'],
      ['build'],
      ['build', '*'],
      ['run', 'build'],
      ['run', 'test'],
      ['run', 'lint'],
      ['run', 'typecheck'],
      ['run', 'dev'],
      ['run', 'start'],
      ['run', 'clean'],
    ],
    description: 'PNPM package manager and scripts',
  },
  'node': {
    patterns: [
      ['*'],
    ],
    description: 'Node.js runtime for executing TypeScript/JavaScript files',
  },
  'npx': {
    patterns: [
      ['*'],
    ],
    description: 'NPM package runner for tools',
  },
  'git': {
    patterns: [
      ['status'],
      ['diff'],
      ['diff', '*'],
      ['log'],
      ['log', '*'],
      ['add'],
      ['add', '*'],
      ['commit'],
      ['commit', '*'],
      ['push'],
      ['push', '*'],
      ['pull'],
      ['pull', '*'],
      ['fetch'],
      ['fetch', '*'],
      ['checkout'],
      ['checkout', '*'],
      ['branch'],
      ['branch', '*'],
      ['merge'],
      ['merge', '*'],
      ['rebase'],
      ['rebase', '*'],
      ['show'],
      ['show', '*'],
      ['rev-parse'],
      ['rev-parse', '*'],
      ['remote'],
      ['remote', '*'],
    ],
    description: 'Git version control commands',
  },
  'ls': {
    patterns: [
      ['*'],
    ],
    description: 'List directory contents',
  },
  'cat': {
    patterns: [
      ['*'],
    ],
    description: 'Display file contents',
  },
  'find': {
    patterns: [
      ['*'],
    ],
    description: 'Find files and directories',
  },
  'grep': {
    patterns: [
      ['*'],
    ],
    description: 'Search for patterns in files',
  },
  'dir': {
    patterns: [
      ['*'],
    ],
    description: 'List directory contents (Windows)',
  },
  'type': {
    patterns: [
      ['*'],
    ],
    description: 'Display file contents (Windows)',
  },
  'where': {
    patterns: [
      ['*'],
    ],
    description: 'Locate executable files (Windows)',
  },
};

const DANGEROUS_COMMANDS: string[] = [
  'rm',
  'rmdir',
  'del',
  'format',
  'mkfs',
  'dd',
  'fdisk',
  'parted',
  'shutdown',
  'reboot',
  'poweroff',
  'init',
  'killall',
  'pkill',
  'kill',
  'chmod',
  'chown',
  'chgrp',
  'sudo',
  'su',
  'passwd',
  'useradd',
  'userdel',
  'usermod',
  'groupadd',
  'groupdel',
  'groupmod',
];

const DANGEROUS_ARGUMENT_PATTERNS: RegExp[] = [
  /\/dev\/sd[a-z]/i,
  /\/dev\/sda/i,
  /\/etc\/passwd/i,
  /\/etc\/shadow/i,
  /\/proc\//i,
  /\/sys\//i,
  />\s*\/dev\//i,
  />>\s*\/dev\//i,
  /\|\|\s*rm/i,
  /&&\s*rm/i,
  /;\s*rm/i,
  /\|\s*rm/i,
  />\s*\//i,
  />>\s*\//i,
  />\s*\/mnt\//i,
  />\s*\/media\//i,
];

export interface CommandResult {
  exitCode: number;
  stdout: string;
  stderr: string;
  durationMs: number;
}

export interface CommandValidationError {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

const RunCommandInputSchema = z.object({
  command: z.string().min(1, 'Command is required'),
  args: z.array(z.string()).optional(),
  cwd: z.string().optional(),
  timeoutMs: z.number().int().positive().optional(),
});

const RunCommandOutputSchema = z.object({
  ok: z.boolean(),
  result: z.object({
    exitCode: z.number().int(),
    stdout: z.string(),
    stderr: z.string(),
    durationMs: z.number().int(),
  }).optional(),
  error: z.string().optional(),
});

function isCommandAllowed(command: string, args: string[] | null | undefined): boolean {
  const normalizedCommand = command?.toLowerCase().trim();
  
  if (!normalizedCommand) {
    return false;
  }
  
  for (const dangerousCommand of DANGEROUS_COMMANDS) {
    if (normalizedCommand === dangerousCommand.toLowerCase()) {
      return false;
    }
  }

  const commandEntry = COMMAND_ALLOWLIST[normalizedCommand];
  if (!commandEntry) {
    return false;
  }

  const allArgs = (args ?? []).map(arg => arg.toLowerCase());
  
  for (const pattern of commandEntry.patterns) {
    if (pattern.length === 1 && pattern[0] === '*') {
      return true;
    }

    const hasWildcard = pattern.includes('*');
    
    if (!hasWildcard && pattern.length !== allArgs.length) {
      continue;
    }

    if (pattern.length > allArgs.length) {
      continue;
    }

    let matches = true;
    for (let i = 0; i < pattern.length; i++) {
      const patternArg = pattern[i];
      const actualArg = allArgs[i];

      if (patternArg === '*') {
        return true;
      }

      if (patternArg.toLowerCase() !== actualArg) {
        matches = false;
        break;
      }
    }

    if (matches) {
      return true;
    }
  }

  return false;
}

function checkForDangerousPatterns(command: string, args: string[]): CommandValidationError | null {
  const normalizedCommand = command.toLowerCase().trim();
  const allArgs = args || [];
  const allArgsString = allArgs.join(' ').toLowerCase();
  const fullCommand = `${normalizedCommand} ${allArgsString}`.toLowerCase();

  for (const dangerousCommand of DANGEROUS_COMMANDS) {
    if (fullCommand.includes(dangerousCommand.toLowerCase())) {
      return {
        code: 'DANGEROUS_COMMAND_BLOCKED',
        message: `Command '${dangerousCommand}' is blocked for safety reasons`,
        details: { blockedCommand: dangerousCommand },
      };
    }
  }

  for (const pattern of DANGEROUS_ARGUMENT_PATTERNS) {
    if (pattern.test(fullCommand)) {
      return {
        code: 'DANGEROUS_PATTERN_BLOCKED',
        message: 'Command contains dangerous pattern',
        details: { pattern: pattern.toString() },
      };
    }
  }

  return null;
}

function validateCommand(command: string, args: string[]): { allowed: true } | { allowed: false; error: CommandValidationError } {
  if (!command || command.trim().length === 0) {
    return {
      allowed: false,
      error: {
        code: 'EMPTY_COMMAND',
        message: 'Command cannot be empty',
      },
    };
  }

  const dangerousCheck = checkForDangerousPatterns(command, args);
  if (dangerousCheck) {
    return {
      allowed: false,
      error: dangerousCheck,
    };
  }

  if (!isCommandAllowed(command, args)) {
    return {
      allowed: false,
      error: {
        code: 'COMMAND_NOT_ALLOWED',
        message: `Command '${command}' is not in the allowlist`,
        details: { command, args },
      },
    };
  }

  return { allowed: true };
}

function executeCommand(
  command: string,
  args: string[],
  options: { cwd?: string; timeoutMs?: number }
): Promise<CommandResult> {
  return new Promise((resolve, reject) => {
    const startTime = Date.now();

    const spawnOptions: SpawnOptionsWithoutStdio = {
      cwd: options.cwd,
      shell: false,
      env: { ...process.env },
    };

    const child = spawn(command, args, spawnOptions);
    let stdout = '';
    let stderr = '';

    const timeoutHandle = options.timeoutMs
      ? setTimeout(() => {
          child.kill('SIGTERM');
          resolve({
            exitCode: -1,
            stdout,
            stderr: `Command timed out after ${options.timeoutMs}ms`,
            durationMs: Date.now() - startTime,
          });
        }, options.timeoutMs)
      : undefined;

    child.stdout.on('data', (data: Buffer) => {
      const chunk = data.toString();
      stdout += chunk;
      console.log(chunk);
    });

    child.stderr.on('data', (data: Buffer) => {
      const chunk = data.toString();
      stderr += chunk;
      console.error(chunk);
    });

    child.on('error', (error: Error) => {
      if (timeoutHandle) clearTimeout(timeoutHandle);
      reject(error);
    });

    child.on('close', (code: number) => {
      if (timeoutHandle) clearTimeout(timeoutHandle);
      resolve({
        exitCode: code,
        stdout,
        stderr,
        durationMs: Date.now() - startTime,
      });
    });

    child.on('exit', (code: number) => {
      if (timeoutHandle) clearTimeout(timeoutHandle);
    });
  });
}

export async function runCommandTool(
  input: z.infer<typeof RunCommandInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof RunCommandOutputSchema>>> {
  const { command, args = [], cwd, timeoutMs } = input;

  context.logger.info({ command, args, cwd }, 'Executing command');

  const validation = validateCommand(command, args);
  if (!validation.allowed) {
    context.logger.warn({ error: validation.error }, 'Command blocked by allowlist');
    return {
      ok: true,
      output: {
        ok: false,
        error: validation.error.message,
      },
    };
  }

  try {
    const result = await executeCommand(command, args, { cwd, timeoutMs });

    context.logger.info(
      { command, exitCode: result.exitCode, durationMs: result.durationMs },
      'Command completed'
    );

    return {
      ok: true,
      output: {
        ok: true,
        result,
      },
    };
  } catch (error) {
    context.logger.error({ command, error }, 'Command execution failed');

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
        error: 'Unknown error executing command',
      },
    };
  }
}

export function createCommandTools(): ToolDefinition<z.ZodTypeAny, unknown>[] {
  return [
    {
      name: 'run_command',
      description:
        'Run a shell command with arguments. Only allowlisted commands can be executed. Dangerous commands like rm, chmod, sudo are always blocked. Use for build and test commands only.',
      inputSchema: RunCommandInputSchema,
      outputSchema: RunCommandOutputSchema,
      execute: async (input, context) =>
        runCommandTool(input as z.infer<typeof RunCommandInputSchema>, context),
    },
  ];
}

export {
  COMMAND_ALLOWLIST,
  DANGEROUS_COMMANDS,
  DANGEROUS_ARGUMENT_PATTERNS,
  validateCommand,
  isCommandAllowed,
  checkForDangerousPatterns,
  executeCommand,
};
