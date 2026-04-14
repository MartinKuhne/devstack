import { z } from 'zod';
import { promises as fsPromises, mkdirSync } from 'node:fs';
import path from 'node:path';
import { glob } from 'glob';
import { ToolContext, ToolResult, ToolDefinition } from '../tool.js';

const WORKSPACE_ENV = process.env.WORKSPACE_ROOT;
export const WORKSPACE_ROOT = WORKSPACE_ENV
  ? path.resolve(WORKSPACE_ENV)
  : path.resolve(process.cwd(), 'workspace');

export function ensureWorkspaceExists(): void {
  if (!WORKSPACE_ENV) {
    return;
  }
  try {
    mkdirSync(WORKSPACE_ROOT, { recursive: true });
  } catch (error) {
    console.error('Failed to create workspace directory:', error);
  }
}

export function isPathContained(filePath: string, baseDir: string): boolean {
  const resolvedPath = path.resolve(filePath);
  const resolvedBase = path.resolve(baseDir);

  const relative = path.relative(resolvedBase, resolvedPath);
  return !relative.startsWith('..') && !path.isAbsolute(relative);
}

export function validatePathContained(filePath: string, baseDir: string): string {
  const resolvedPath = path.resolve(filePath);
  const resolvedBase = path.resolve(baseDir);

  if (!isPathContained(resolvedPath, resolvedBase)) {
    throw new Error(
      `Path traversal detected: '${filePath}' is outside workspace '${baseDir}'`
    );
  }

  return resolvedPath;
}

const ReadFileInputSchema = z.object({
  path: z.string().min(1, 'Path is required'),
  maxLines: z.number().int().positive().optional(),
});

const WriteFileInputSchema = z.object({
  path: z.string().min(1, 'Path is required'),
  content: z.string(),
});

const ListFilesInputSchema = z.object({
  dir: z.string().min(1, 'Directory is required'),
  pattern: z.string().optional(),
});

const DeleteFileInputSchema = z.object({
  path: z.string().min(1, 'Path is required'),
});

const ReadFileOutputSchema = z.object({
  ok: z.boolean(),
  content: z.string().optional(),
  error: z.string().optional(),
  linesRead: z.number().int().optional(),
});

const WriteFileOutputSchema = z.object({
  ok: z.boolean(),
  error: z.string().optional(),
});

const ListFilesOutputSchema = z.object({
  ok: z.boolean(),
  files: z.array(z.string()).optional(),
  error: z.string().optional(),
});

const DeleteFileOutputSchema = z.object({
  ok: z.boolean(),
  error: z.string().optional(),
});

export async function readFileTool(
  input: z.infer<typeof ReadFileInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof ReadFileOutputSchema>>> {
  try {
    const resolvedPath = validatePathContained(input.path, WORKSPACE_ROOT);
    context.logger.debug({ path: resolvedPath }, 'Reading file');

    let content: string;
    let linesRead = 0;

    if (input.maxLines) {
      const allLines = await fsPromises.readFile(resolvedPath, 'utf-8');
      const lines = allLines.split('\n');
      const limitedLines = lines.slice(0, input.maxLines);
      content = limitedLines.join('\n');
      linesRead = limitedLines.length;
    } else {
      content = await fsPromises.readFile(resolvedPath, 'utf-8');
      linesRead = content.split('\n').length;
    }

    return {
      ok: true,
      output: {
        ok: true,
        content,
        linesRead,
      },
    };
  } catch (error) {
    context.logger.error({ path: input.path, error }, 'Failed to read file');

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
        error: 'Unknown error reading file',
      },
    };
  }
}

export async function writeFileTool(
  input: z.infer<typeof WriteFileInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof WriteFileOutputSchema>>> {
  try {
    const resolvedPath = validatePathContained(input.path, WORKSPACE_ROOT);
    const dir = path.dirname(resolvedPath);

    context.logger.debug({ path: resolvedPath, dir }, 'Writing file');

    await fsPromises.mkdir(dir, { recursive: true });
    await fsPromises.writeFile(resolvedPath, input.content, 'utf-8');

    return {
      ok: true,
      output: {
        ok: true,
      },
    };
  } catch (error) {
    context.logger.error({ path: input.path, error }, 'Failed to write file');

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
        error: 'Unknown error writing file',
      },
    };
  }
}

export async function listFilesTool(
  input: z.infer<typeof ListFilesInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof ListFilesOutputSchema>>> {
  try {
    const resolvedDir = validatePathContained(input.dir, WORKSPACE_ROOT);
    context.logger.debug({ dir: resolvedDir, pattern: input.pattern }, 'Listing files');

    const files: string[] = [];

    if (input.pattern) {
      const searchPattern = path.join(resolvedDir, input.pattern).replace(/\\/g, '/');
      const matches = await glob(searchPattern);

      for (const match of matches) {
        const normalizedMatch = path.resolve(match);
        if (isPathContained(normalizedMatch, WORKSPACE_ROOT)) {
          const relativePath = path.relative(WORKSPACE_ROOT, normalizedMatch);
          files.push(relativePath.replace(/\\/g, '/'));
        }
      }
    } else {
      const entries = await fsPromises.readdir(resolvedDir, { withFileTypes: true });
      for (const entry of entries) {
        files.push(entry.name);
      }
    }

    return {
      ok: true,
      output: {
        ok: true,
        files: files.sort(),
      },
    };
  } catch (error) {
    context.logger.error({ dir: input.dir, error }, 'Failed to list files');

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
        error: 'Unknown error listing files',
      },
    };
  }
}

export async function deleteFileTool(
  input: z.infer<typeof DeleteFileInputSchema>,
  context: ToolContext
): Promise<ToolResult<z.infer<typeof DeleteFileOutputSchema>>> {
  try {
    const resolvedPath = validatePathContained(input.path, WORKSPACE_ROOT);
    context.logger.debug({ path: resolvedPath }, 'Deleting file');

    await fsPromises.unlink(resolvedPath);

    return {
      ok: true,
      output: {
        ok: true,
      },
    };
  } catch (error) {
    context.logger.error({ path: input.path, error }, 'Failed to delete file');

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
        error: 'Unknown error deleting file',
      },
    };
  }
}

export function createFilesystemTools(): ToolDefinition<z.ZodTypeAny, unknown>[] {
  return [
    {
      name: 'read_file',
      description:
        'Read the content of a file. The path must be within the workspace directory. Optionally limit the number of lines returned.',
      inputSchema: ReadFileInputSchema,
      outputSchema: ReadFileOutputSchema,
      execute: async (input, context) => readFileTool(input as z.infer<typeof ReadFileInputSchema>, context),
    },
    {
      name: 'write_file',
      description:
        'Write content to a file. The path must be within the workspace directory. Creates parent directories if needed.',
      inputSchema: WriteFileInputSchema,
      outputSchema: WriteFileOutputSchema,
      execute: async (input, context) => writeFileTool(input as z.infer<typeof WriteFileInputSchema>, context),
    },
    {
      name: 'list_files',
      description:
        'List files in a directory. Optionally filter by a glob pattern. The directory must be within the workspace.',
      inputSchema: ListFilesInputSchema,
      outputSchema: ListFilesOutputSchema,
      execute: async (input, context) => listFilesTool(input as z.infer<typeof ListFilesInputSchema>, context),
    },
    {
      name: 'delete_file',
      description:
        'Delete a file. The path must be within the workspace directory.',
      inputSchema: DeleteFileInputSchema,
      outputSchema: DeleteFileOutputSchema,
      execute: async (input, context) => deleteFileTool(input as z.infer<typeof DeleteFileInputSchema>, context),
    },
  ];
}
