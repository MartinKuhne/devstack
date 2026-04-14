import { describe, it, expect, vi } from 'vitest';
import {
  validateCommand,
  isCommandAllowed,
  checkForDangerousPatterns,
  COMMAND_ALLOWLIST,
  DANGEROUS_COMMANDS,
  DANGEROUS_ARGUMENT_PATTERNS,
} from './command-skill.js';

describe('Command Skill - Allowlist Validation', () => {
  describe('isCommandAllowed', () => {
    it('should allow dotnet build', () => {
      expect(isCommandAllowed('dotnet', ['build'])).toBe(true);
    });

    it('should allow dotnet test', () => {
      expect(isCommandAllowed('dotnet', ['test'])).toBe(true);
    });

    it('should allow dotnet test with arguments', () => {
      expect(isCommandAllowed('dotnet', ['test', '--verbosity', 'normal'])).toBe(true);
    });

    it('should allow dotnet restore', () => {
      expect(isCommandAllowed('dotnet', ['restore'])).toBe(true);
    });

    it('should allow npm run build', () => {
      expect(isCommandAllowed('npm', ['run', 'build'])).toBe(true);
    });

    it('should allow npm run test', () => {
      expect(isCommandAllowed('npm', ['run', 'test'])).toBe(true);
    });

    it('should allow pnpm run build', () => {
      expect(isCommandAllowed('pnpm', ['run', 'build'])).toBe(true);
    });

    it('should allow git status', () => {
      expect(isCommandAllowed('git', ['status'])).toBe(true);
    });

    it('should allow git add with files', () => {
      expect(isCommandAllowed('git', ['add', '.', 'src/file.ts'])).toBe(true);
    });

    it('should allow git commit with message', () => {
      expect(isCommandAllowed('git', ['commit', '-m', 'test commit'])).toBe(true);
    });

    it('should allow node with script', () => {
      expect(isCommandAllowed('node', ['script.ts'])).toBe(true);
    });

    it('should allow npx with package', () => {
      expect(isCommandAllowed('npx', ['vitest', 'run'])).toBe(true);
    });

    it('should allow ls with arguments', () => {
      expect(isCommandAllowed('ls', ['-la'])).toBe(true);
    });

    it('should allow cat with file', () => {
      expect(isCommandAllowed('cat', ['file.txt'])).toBe(true);
    });

    it('should allow find with patterns', () => {
      expect(isCommandAllowed('find', ['.', '-name', '*.ts'])).toBe(true);
    });

    it('should allow grep with pattern', () => {
      expect(isCommandAllowed('grep', ['pattern', 'file.txt'])).toBe(true);
    });
  });

  describe('isCommandAllowed - rejected commands', () => {
    it('should reject rm command', () => {
      expect(isCommandAllowed('rm', ['-rf', '/'])).toBe(false);
    });

    it('should reject rmdir command', () => {
      expect(isCommandAllowed('rmdir', ['/s'])).toBe(false);
    });

    it('should reject del command', () => {
      expect(isCommandAllowed('del', ['/f', '*'])).toBe(false);
    });

    it('should reject chmod command', () => {
      expect(isCommandAllowed('chmod', ['777', '/etc'])).toBe(false);
    });

    it('should reject sudo command', () => {
      expect(isCommandAllowed('sudo', ['rm', '-rf', '/'])).toBe(false);
    });

    it('should reject unknown command', () => {
      expect(isCommandAllowed('unknowncommand', [])).toBe(false);
    });

    it('should reject dotnet with dangerous subcommand', () => {
      expect(isCommandAllowed('dotnet', ['fxprof-configure'])).toBe(false);
    });

    it('should reject npm install -g command', () => {
      expect(isCommandAllowed('npm', ['install', '-g', 'some-package'])).toBe(false);
    });
  });
});

describe('Command Skill - Dangerous Pattern Detection', () => {
  describe('checkForDangerousPatterns', () => {
    it('should detect dangerous command rm', () => {
      const error = checkForDangerousPatterns('rm', ['-rf', '/']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous command dd', () => {
      const error = checkForDangerousPatterns('dd', ['if=/dev/zero', 'of=/dev/sda']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous command mkfs', () => {
      const error = checkForDangerousPatterns('mkfs', ['-t', 'ext4', '/dev/sda1']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous command chmod', () => {
      const error = checkForDangerousPatterns('chmod', ['777', '/etc/passwd']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous command sudo', () => {
      const error = checkForDangerousPatterns('sudo', ['rm', '-rf', '/']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous pattern /dev/sda', () => {
      const error = checkForDangerousPatterns('dd', ['of=/dev/sda']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous pattern /etc/passwd', () => {
      const error = checkForDangerousPatterns('cat', ['/etc/passwd']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous redirection to /dev/', () => {
      const error = checkForDangerousPatterns('echo', ['test > /dev/sda']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_PATTERN_BLOCKED');
    });

    it('should detect dangerous pipe to rm', () => {
      const error = checkForDangerousPatterns('bash', ['-c', 'echo test | rm -rf /']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should detect dangerous chain with rm', () => {
      const error = checkForDangerousPatterns('bash', ['-c', 'ls && rm -rf /']);
      expect(error).not.toBeNull();
      expect(error?.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should allow safe git commands', () => {
      const error = checkForDangerousPatterns('git', ['status']);
      expect(error).toBeNull();
    });

    it('should allow safe dotnet commands', () => {
      const error = checkForDangerousPatterns('dotnet', ['build']);
      expect(error).toBeNull();
    });
  });
});

describe('Command Skill - Full Validation', () => {
  describe('validateCommand', () => {
    it('should validate allowed command successfully', () => {
      const result = validateCommand('dotnet', ['build']);
      expect(result).toEqual({ allowed: true });
    });

    it('should reject empty command', () => {
      const result = validateCommand('', []);
      expect(result.allowed).toBe(false);
      expect((result as any).error.code).toBe('EMPTY_COMMAND');
    });

    it('should reject dangerous command', () => {
      const result = validateCommand('rm', ['-rf', '/']);
      expect(result.allowed).toBe(false);
      expect((result as any).error.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should reject not allowed command', () => {
      const result = validateCommand('unknown', ['arg1']);
      expect(result.allowed).toBe(false);
      expect((result as any).error.code).toBe('COMMAND_NOT_ALLOWED');
    });

    it('should reject command with dangerous pattern in args', () => {
      const result = validateCommand('echo', ['test > /dev/sda']);
      expect(result.allowed).toBe(false);
      expect((result as any).error.code).toBe('DANGEROUS_PATTERN_BLOCKED');
    });

    it('should handle case insensitive dangerous commands', () => {
      const result = validateCommand('RM', ['-rf', '/']);
      expect(result.allowed).toBe(false);
      expect((result as any).error.code).toBe('DANGEROUS_COMMAND_BLOCKED');
    });

    it('should handle case insensitive allowlist commands', () => {
      const result = validateCommand('DOTNET', ['build']);
      expect(result.allowed).toBe(true);
    });
  });
});

describe('Command Skill - Allowlist Configuration', () => {
  it('should have expected commands in allowlist', () => {
    expect(COMMAND_ALLOWLIST.dotnet).toBeDefined();
    expect(COMMAND_ALLOWLIST.npm).toBeDefined();
    expect(COMMAND_ALLOWLIST.pnpm).toBeDefined();
    expect(COMMAND_ALLOWLIST.git).toBeDefined();
    expect(COMMAND_ALLOWLIST.node).toBeDefined();
    expect(COMMAND_ALLOWLIST.npx).toBeDefined();
  });

  it('should have expected dangerous commands blocked', () => {
    expect(DANGEROUS_COMMANDS).toContain('rm');
    expect(DANGEROUS_COMMANDS).toContain('chmod');
    expect(DANGEROUS_COMMANDS).toContain('sudo');
    expect(DANGEROUS_COMMANDS).toContain('dd');
    expect(DANGEROUS_COMMANDS).toContain('mkfs');
  });

  it('should have dangerous argument patterns', () => {
    expect(DANGEROUS_ARGUMENT_PATTERNS.length).toBeGreaterThan(0);
    expect(DANGEROUS_ARGUMENT_PATTERNS.some(p => p.test('/dev/sda'))).toBe(true);
    expect(DANGEROUS_ARGUMENT_PATTERNS.some(p => p.test('/etc/passwd'))).toBe(true);
  });
});

describe('Command Skill - Edge Cases', () => {
  it('should handle null args array - dotnet requires subcommand', () => {
    expect(isCommandAllowed('dotnet', null as any)).toBe(false);
  });

  it('should handle undefined args array - dotnet requires subcommand', () => {
    expect(isCommandAllowed('dotnet', undefined as any)).toBe(false);
  });

  it('should handle whitespace in command', () => {
    const result = validateCommand('  dotnet  ', ['build']);
    expect(result.allowed).toBe(true);
  });

  it('should reject command with only whitespace', () => {
    const result = validateCommand('   ', []);
    expect(result.allowed).toBe(false);
  });

  it('should allow git commands with various arguments', () => {
    expect(isCommandAllowed('git', ['log', '--oneline', '-10'])).toBe(true);
    expect(isCommandAllowed('git', ['diff', 'HEAD~1'])).toBe(true);
    expect(isCommandAllowed('git', ['show', 'abc123'])).toBe(true);
  });

  it('should allow npm run with various scripts', () => {
    expect(isCommandAllowed('npm', ['run', 'build'])).toBe(true);
    expect(isCommandAllowed('npm', ['run', 'test'])).toBe(true);
    expect(isCommandAllowed('npm', ['run', 'lint'])).toBe(true);
    expect(isCommandAllowed('npm', ['run', 'typecheck'])).toBe(true);
  });
});
