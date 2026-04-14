import { describe, it, expect, vi, beforeEach } from 'vitest';
import { z } from 'zod';
import { GraphQLClient } from 'graphql-request';
import {
  prCreateTool,
  prGetTool,
  prListCommentsTool,
  prApproveTool,
  prMergeTool,
  createPullRequestTools,
  GitHubProvider,
  GiteaProvider,
  createProvider,
  detectProviderType,
} from './pr-skill.js';
import { ToolContext } from '../tool.js';
import { Logger } from 'pino';

vi.mock('pino', () => ({
  default: vi.fn(),
}));

const createMockContext = (): ToolContext => ({
  logger: {
    info: vi.fn(),
    warn: vi.fn(),
    error: vi.fn(),
    debug: vi.fn(),
  } as unknown as Logger,
  api: {} as unknown as GraphQLClient,
  metadata: {
    gitToken: 'test-token-123',
  },
});

describe('pull-request skill', () => {
  describe('detectProviderType', () => {
    it('should detect GitHub from github.com URL', () => {
      const type = detectProviderType('https://github.com/owner/repo.git');
      expect(type).toBe('github');
    });

    it('should detect GitHub from URL without .git suffix', () => {
      const type = detectProviderType('https://github.com/owner/repo');
      expect(type).toBe('github');
    });

    it('should detect Gitea from gitea.com URL', () => {
      const type = detectProviderType('https://gitea.com/owner/repo.git');
      expect(type).toBe('gitea');
    });

    it('should detect Gitea from custom gitea instance', () => {
      const type = detectProviderType('https://git.example.com/owner/repo');
      expect(type).toBe('gitea');
    });
  });

  describe('createProvider', () => {
    it('should create GitHubProvider for github.com URLs', () => {
      const provider = createProvider({
        repoUrl: 'https://github.com/owner/repo',
        token: 'test-token',
      });
      expect(provider).toBeInstanceOf(GitHubProvider);
    });

    it('should create GiteaProvider for gitea.com URLs', () => {
      const provider = createProvider({
        repoUrl: 'https://gitea.com/owner/repo',
        token: 'test-token',
      });
      expect(provider).toBeInstanceOf(GiteaProvider);
    });

    it('should create GiteaProvider for custom gitea instances', () => {
      const provider = createProvider({
        repoUrl: 'https://git.example.com/owner/repo',
        token: 'test-token',
      });
      expect(provider).toBeInstanceOf(GiteaProvider);
    });

    it('should respect explicit provider type', () => {
      const provider = createProvider({
        repoUrl: 'https://github.com/owner/repo',
        token: 'test-token',
        type: 'gitea',
        baseUrl: 'https://custom-gitea.com',
      });
      expect(provider).toBeInstanceOf(GiteaProvider);
    });
  });

  describe('createPullRequestTools', () => {
    it('should create all PR tools', () => {
      const tools = createPullRequestTools();
      
      expect(tools).toHaveLength(5);
      expect(tools.map(t => t.name)).toEqual([
        'pr_create',
        'pr_get',
        'pr_list_comments',
        'pr_approve',
        'pr_merge',
      ]);
    });

    it('should have valid schemas for each tool', () => {
      const tools = createPullRequestTools();
      
      for (const tool of tools) {
        expect(tool.inputSchema).toBeDefined();
        expect(tool.execute).toBeDefined();
      }
    });
  });

  describe('prCreateTool', () => {
    it('should validate required fields', () => {
      const schema = createPullRequestTools()[0].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        headBranch: 'feature-branch',
        baseBranch: 'main',
        title: 'Test PR',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject invalid repo URL', () => {
      const schema = createPullRequestTools()[0].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        repoUrl: 'not-a-url',
        headBranch: 'feature',
        baseBranch: 'main',
        title: 'Test',
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });

    it('should reject missing head branch', () => {
      const schema = createPullRequestTools()[0].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        repoUrl: 'https://github.com/owner/repo',
        baseBranch: 'main',
        title: 'Test',
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });
  });

  describe('prGetTool', () => {
    it('should validate required fields', () => {
      const schema = createPullRequestTools()[1].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should reject negative PR number', () => {
      const schema = createPullRequestTools()[1].inputSchema as z.ZodTypeAny;
      
      const invalidInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: -1,
      };
      
      expect(() => schema.parse(invalidInput)).toThrow();
    });
  });

  describe('prListCommentsTool', () => {
    it('should validate required fields', () => {
      const schema = createPullRequestTools()[2].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });
  });

  describe('prApproveTool', () => {
    it('should validate required fields', () => {
      const schema = createPullRequestTools()[3].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should accept optional message', () => {
      const schema = createPullRequestTools()[3].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
        message: 'Looks good!',
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });
  });

  describe('prMergeTool', () => {
    it('should validate required fields', () => {
      const schema = createPullRequestTools()[4].inputSchema as z.ZodTypeAny;
      
      const validInput = {
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
      };
      
      expect(() => schema.parse(validInput)).not.toThrow();
    });

    it('should accept valid merge methods', () => {
      const schema = createPullRequestTools()[4].inputSchema as z.ZodTypeAny;
      
      expect(() => schema.parse({
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
        mergeMethod: 'merge',
      })).not.toThrow();

      expect(() => schema.parse({
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
        mergeMethod: 'squash',
      })).not.toThrow();

      expect(() => schema.parse({
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
        mergeMethod: 'rebase',
      })).not.toThrow();
    });

    it('should reject invalid merge method', () => {
      const schema = createPullRequestTools()[4].inputSchema as z.ZodTypeAny;
      
      expect(() => schema.parse({
        repoUrl: 'https://github.com/owner/repo',
        prNumber: 42,
        mergeMethod: 'invalid' as any,
      })).toThrow();
    });
  });

  describe('ToolContext validation', () => {
    it('should require git token in context metadata', async () => {
      const contextWithoutToken = {
        logger: {
          info: vi.fn(),
          warn: vi.fn(),
          error: vi.fn(),
          debug: vi.fn(),
        } as unknown as Logger,
        api: {} as unknown as GraphQLClient,
        metadata: {},
      };

      // The tool should return an error when token is missing
      const result = await prCreateTool({
        repoUrl: 'https://github.com/owner/repo',
        headBranch: 'feature',
        baseBranch: 'main',
        title: 'Test',
      }, contextWithoutToken as unknown as ToolContext);

      expect(result.ok).toBe(true);
      expect(result.output.ok).toBe(false);
      expect(result.output.error).toContain('Git token not found');
    });
  });
});
