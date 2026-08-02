import { describe, it, expect } from 'vitest';
import { normalizeGithubUrl } from './gitService.js';

describe('GitHub Remote URL Normalization [AG-240]', () => {
  it('normalizes SSH GitHub URLs', () => {
    const res1 = normalizeGithubUrl('git@github.com:owner/repo.git');
    expect(res1.owner).toBe('owner');
    expect(res1.name).toBe('repo');
    expect(res1.normalizedUrl).toBe('https://github.com/owner/repo.git');

    const res2 = normalizeGithubUrl('git@github.com:owner/repo');
    expect(res2.owner).toBe('owner');
    expect(res2.name).toBe('repo');
    expect(res2.normalizedUrl).toBe('https://github.com/owner/repo.git');
  });

  it('normalizes HTTPS GitHub URLs', () => {
    const res1 = normalizeGithubUrl('https://github.com/owner/repo.git');
    expect(res1.owner).toBe('owner');
    expect(res1.name).toBe('repo');
    expect(res1.normalizedUrl).toBe('https://github.com/owner/repo.git');

    const res2 = normalizeGithubUrl('https://github.com/owner/repo');
    expect(res2.owner).toBe('owner');
    expect(res2.name).toBe('repo');
    expect(res2.normalizedUrl).toBe('https://github.com/owner/repo.git');
  });
});
