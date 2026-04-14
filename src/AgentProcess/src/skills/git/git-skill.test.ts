import { describe, it, expect, vi } from 'vitest';
import {
  isGitCommandAllowed,
  GIT_ALLOWLIST,
  DANGEROUS_GIT_COMMANDS,
} from './git-skill.js';

const mockContext = {
  logger: {
    debug: vi.fn(),
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
  },
  api: {},
};

describe('Git Skill - Command Allowlist', () => {
  describe('isGitCommandAllowed', () => {
    it('should allow git clone', () => {
      expect(isGitCommandAllowed('git', ['clone', 'https://github.com/test/repo.git'])).toBe(true);
    });

    it('should allow git checkout', () => {
      expect(isGitCommandAllowed('git', ['checkout', 'main'])).toBe(true);
    });

    it('should allow git checkout -b', () => {
      expect(isGitCommandAllowed('git', ['checkout', '-b', 'feature-branch'])).toBe(true);
    });

    it('should allow git add', () => {
      expect(isGitCommandAllowed('git', ['add', '.'])).toBe(true);
      expect(isGitCommandAllowed('git', ['add', 'file.ts'])).toBe(true);
      expect(isGitCommandAllowed('git', ['add', 'file1.ts', 'file2.ts'])).toBe(true);
    });

    it('should allow git commit', () => {
      expect(isGitCommandAllowed('git', ['commit', '-m', 'test commit'])).toBe(true);
    });

    it('should allow git status', () => {
      expect(isGitCommandAllowed('git', ['status'])).toBe(true);
    });

    it('should allow git diff', () => {
      expect(isGitCommandAllowed('git', ['diff'])).toBe(true);
      expect(isGitCommandAllowed('git', ['diff', 'file.ts'])).toBe(true);
    });

    it('should allow git log', () => {
      expect(isGitCommandAllowed('git', ['log', '--oneline'])).toBe(true);
      expect(isGitCommandAllowed('git', ['log', '--oneline', '-n', '10'])).toBe(true);
    });

    it('should allow git push', () => {
      expect(isGitCommandAllowed('git', ['push'])).toBe(true);
      expect(isGitCommandAllowed('git', ['push', 'origin', 'main'])).toBe(true);
    });

    it('should allow git pull', () => {
      expect(isGitCommandAllowed('git', ['pull'])).toBe(true);
      expect(isGitCommandAllowed('git', ['pull', 'origin', 'main'])).toBe(true);
    });

    it('should allow git fetch', () => {
      expect(isGitCommandAllowed('git', ['fetch'])).toBe(true);
      expect(isGitCommandAllowed('git', ['fetch', 'origin'])).toBe(true);
    });

    it('should allow git branch', () => {
      expect(isGitCommandAllowed('git', ['branch'])).toBe(true);
      expect(isGitCommandAllowed('git', ['branch', 'feature'])).toBe(true);
    });

    it('should allow git rev-parse', () => {
      expect(isGitCommandAllowed('git', ['rev-parse', 'HEAD'])).toBe(true);
    });

    it('should allow git show', () => {
      expect(isGitCommandAllowed('git', ['show', 'HEAD'])).toBe(true);
    });

    it('should allow git remote', () => {
      expect(isGitCommandAllowed('git', ['remote'])).toBe(true);
      expect(isGitCommandAllowed('git', ['remote', '-v'])).toBe(true);
      expect(isGitCommandAllowed('git', ['remote', 'add', 'origin', 'url'])).toBe(true);
    });
  });

  describe('isGitCommandAllowed - rejected commands', () => {
    it('should reject git filter-branch', () => {
      expect(isGitCommandAllowed('git', ['filter-branch', '--tree-less'])).toBe(false);
    });

    it('should reject git push --force', () => {
      expect(isGitCommandAllowed('git', ['push', '--force'])).toBe(false);
      expect(isGitCommandAllowed('git', ['push', '-f'])).toBe(false);
    });

    it('should reject git reset --hard', () => {
      expect(isGitCommandAllowed('git', ['reset', '--hard'])).toBe(false);
    });

    it('should reject git clean -fd', () => {
      expect(isGitCommandAllowed('git', ['clean', '-fd'])).toBe(false);
    });

    it('should reject git gc --prune=all', () => {
      expect(isGitCommandAllowed('git', ['gc', '--prune=all'])).toBe(false);
    });

    it('should reject git update-ref', () => {
      expect(isGitCommandAllowed('git', ['update-ref', 'HEAD', 'abc123'])).toBe(false);
    });

    it('should reject git replace', () => {
      expect(isGitCommandAllowed('git', ['replace', 'HEAD'])).toBe(false);
    });

    it('should reject unknown git subcommands', () => {
      expect(isGitCommandAllowed('git', ['unknown-command'])).toBe(false);
    });
  });
});

describe('Git Skill - Dangerous Commands Configuration', () => {
  it('should have expected dangerous commands blocked', () => {
    expect(DANGEROUS_GIT_COMMANDS).toContain('filter-branch');
    expect(DANGEROUS_GIT_COMMANDS).toContain('push --force');
    expect(DANGEROUS_GIT_COMMANDS).toContain('push -f');
    expect(DANGEROUS_GIT_COMMANDS).toContain('reset --hard');
    expect(DANGEROUS_GIT_COMMANDS).toContain('clean -fd');
  });

  it('should have expected commands in allowlist', () => {
    expect(GIT_ALLOWLIST).toContain('clone');
    expect(GIT_ALLOWLIST).toContain('checkout');
    expect(GIT_ALLOWLIST).toContain('add');
    expect(GIT_ALLOWLIST).toContain('commit');
    expect(GIT_ALLOWLIST).toContain('status');
    expect(GIT_ALLOWLIST).toContain('diff');
    expect(GIT_ALLOWLIST).toContain('log');
    expect(GIT_ALLOWLIST).toContain('push');
    expect(GIT_ALLOWLIST).toContain('pull');
    expect(GIT_ALLOWLIST).toContain('fetch');
    expect(GIT_ALLOWLIST).toContain('branch');
  });
});

describe('Git Skill - Edge Cases', () => {
  it('should handle case insensitive commands', () => {
    expect(isGitCommandAllowed('git', ['STATUS'])).toBe(true);
    expect(isGitCommandAllowed('git', ['ADD', '.'])).toBe(true);
  });

  it('should handle empty args array', () => {
    expect(isGitCommandAllowed('git', [])).toBe(false);
  });

  it('should handle whitespace in arguments', () => {
    expect(isGitCommandAllowed('git', ['commit', '-m', 'test'])).toBe(true);
  });
});




