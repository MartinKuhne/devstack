import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { promises as fs } from 'node:fs';
import path from 'node:path';
import { tmpdir } from 'node:os';
import {
  isPathContained,
  validatePathContained,
  readFileTool,
  writeFileTool,
  listFilesTool,
  deleteFileTool,
} from './fs-skill.js';

const workspaceForTests = path.join(tmpdir(), 'devstack-fs-test');

const mockContext = {
  logger: {
    debug: vi.fn(),
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
  },
  api: {},
};

describe('Filesystem Skill - Path Containment', () => {
  beforeEach(async () => {
    await fs.mkdir(workspaceForTests, { recursive: true });
  });

  afterEach(async () => {
    await fs.rm(workspaceForTests, { recursive: true, force: true });
  });

  describe('isPathContained', () => {
    it('should return true for path inside workspace', () => {
      const workspace = '/workspace';
      const filePath = '/workspace/src/file.txt';
      expect(isPathContained(filePath, workspace)).toBe(true);
    });

    it('should return true for path equal to workspace', () => {
      const workspace = '/workspace';
      const filePath = '/workspace';
      expect(isPathContained(filePath, workspace)).toBe(true);
    });

    it('should return false for path traversal attempt', () => {
      const workspace = '/workspace';
      const filePath = '/workspace/../etc/passwd';
      expect(isPathContained(filePath, workspace)).toBe(false);
    });

    it('should return false for path outside workspace', () => {
      const workspace = '/workspace';
      const filePath = '/etc/passwd';
      expect(isPathContained(filePath, workspace)).toBe(false);
    });

    it('should handle Windows-style paths', () => {
      const workspace = 'C:\\workspace';
      const filePath = 'C:\\workspace\\src\\file.txt';
      expect(isPathContained(filePath, workspace)).toBe(true);
    });

    it('should reject path traversal with double dots', () => {
      const workspace = '/workspace/project';
      const filePath = '/workspace/project/subdir/../../etc/passwd';
      expect(isPathContained(filePath, workspace)).toBe(false);
    });

    it('should handle relative paths correctly', () => {
      const workspace = '/workspace';
      const filePath = './file.txt';
      const resolved = path.resolve(workspace, filePath);
      expect(isPathContained(resolved, workspace)).toBe(true);
    });
  });

  describe('validatePathContained', () => {
    it('should return resolved path for valid path', () => {
      const workspace = '/workspace';
      const filePath = '/workspace/src/file.txt';
      const result = validatePathContained(filePath, workspace);
      expect(result).toBe(path.resolve(filePath));
    });

    it('should throw error for path traversal attempt', () => {
      const workspace = '/workspace';
      const filePath = '/workspace/../etc/passwd';
      expect(() => validatePathContained(filePath, workspace)).toThrow(
        'Path traversal detected'
      );
    });

    it('should throw error for path outside workspace', () => {
      const workspace = '/workspace';
      const filePath = '/etc/passwd';
      expect(() => validatePathContained(filePath, workspace)).toThrow(
        'Path traversal detected'
      );
    });
  });
});

describe('Filesystem Skill - Tool Operations', () => {
  const workspaceForTests = path.join(process.cwd(), 'workspace', 'test-fs');

  beforeEach(async () => {
    await fs.mkdir(workspaceForTests, { recursive: true });
  });

  afterEach(async () => {
    await fs.rm(workspaceForTests, { recursive: true, force: true });
  });

  describe('read_file', () => {
    it('should read file content successfully', async () => {
      const testFile = path.join(workspaceForTests, 'test.txt');
      const content = 'Hello, World!\nLine 2\nLine 3';
      await fs.writeFile(testFile, content, 'utf-8');

      const result = await readFileTool(
        { path: testFile },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);
      expect(result.output.content).toBe(content);
      expect(result.output.linesRead).toBe(3);
    });

    it('should respect maxLines parameter', async () => {
      const testFile = path.join(workspaceForTests, 'test.txt');
      const content = 'Line 1\nLine 2\nLine 3\nLine 4\nLine 5';
      await fs.writeFile(testFile, content, 'utf-8');

      const result = await readFileTool(
        { path: testFile, maxLines: 3 },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);
      expect(result.output.content).toBe('Line 1\nLine 2\nLine 3');
      expect(result.output.linesRead).toBe(3);
    });

    it('should return error for non-existent file', async () => {
      const result = await readFileTool(
        { path: path.join(workspaceForTests, 'nonexistent.txt') },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('ENOENT');
    });

    it('should reject path outside workspace', async () => {
      const result = await readFileTool(
        { path: '/etc/passwd' },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('Path traversal');
    });
  });

  describe('write_file', () => {
    it('should write file content successfully', async () => {
      const testFile = path.join(workspaceForTests, 'output.txt');
      const content = 'Test content';

      const result = await writeFileTool(
        { path: testFile, content },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);

      const writtenContent = await fs.readFile(testFile, 'utf-8');
      expect(writtenContent).toBe(content);
    });

    it('should create parent directories', async () => {
      const testFile = path.join(workspaceForTests, 'deep', 'nested', 'file.txt');
      const content = 'Nested file content';

      const result = await writeFileTool(
        { path: testFile, content },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);

      const exists = await fs.access(testFile).then(() => true).catch(() => false);
      expect(exists).toBe(true);
    });

    it('should reject path outside workspace', async () => {
      const result = await writeFileTool(
        { path: '/etc/malicious.txt', content: 'evil' },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('Path traversal');
    });
  });

  describe('list_files', () => {
    it('should list files in directory', async () => {
      await fs.writeFile(path.join(workspaceForTests, 'file1.txt'), 'content1', 'utf-8');
      await fs.writeFile(path.join(workspaceForTests, 'file2.txt'), 'content2', 'utf-8');
      await fs.mkdir(path.join(workspaceForTests, 'subdir'), { recursive: true });

      const result = await listFilesTool(
        { dir: workspaceForTests },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);
      expect(result.output.files).toContain('file1.txt');
      expect(result.output.files).toContain('file2.txt');
      expect(result.output.files).toContain('subdir');
    });

    it('should list files with glob pattern', async () => {
      await fs.writeFile(path.join(workspaceForTests, 'test1.txt'), 'content1', 'utf-8');
      await fs.writeFile(path.join(workspaceForTests, 'test2.js'), 'content2', 'utf-8');
      await fs.writeFile(path.join(workspaceForTests, 'other.md'), 'content3', 'utf-8');

      const result = await listFilesTool(
        { dir: workspaceForTests, pattern: '*.txt' },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);
      expect(result.output.files).toContain('test-fs/test1.txt');
      expect(result.output.files).not.toContain('test-fs/test2.js');
      expect(result.output.files).not.toContain('test-fs/other.md');
    });

    it('should return error for non-existent directory', async () => {
      const result = await listFilesTool(
        { dir: path.join(workspaceForTests, 'nonexistent') },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
    });

    it('should reject path outside workspace', async () => {
      const result = await listFilesTool(
        { dir: '/etc' },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('Path traversal');
    });
  });

  describe('delete_file', () => {
    it('should delete file successfully', async () => {
      const testFile = path.join(workspaceForTests, 'to-delete.txt');
      await fs.writeFile(testFile, 'content', 'utf-8');

      const result = await deleteFileTool(
        { path: testFile },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(true);

      const exists = await fs.access(testFile).then(() => true).catch(() => false);
      expect(exists).toBe(false);
    });

    it('should return error for non-existent file', async () => {
      const result = await deleteFileTool(
        { path: path.join(workspaceForTests, 'nonexistent.txt') },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toMatch(/ENOENT|no such file/);
    });

    it('should reject path outside workspace', async () => {
      const result = await deleteFileTool(
        { path: '/etc/passwd' },
        mockContext as any
      ) as any;

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('Path traversal');
    });
  });
});
