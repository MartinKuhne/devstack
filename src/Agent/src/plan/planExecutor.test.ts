import { describe, it, expect, vi } from 'vitest';
import path from 'path';
import fs from 'fs';
import { executeRunPlan, executeShowPlan } from './planExecutor.js';
import { DevStackGraphQLClient } from '../graphql/client.js';
import { OpenCodeAgentEngine } from '../opencode/opencodeEngine.js';

describe('PlanExecutor Unit Tests', () => {
  it('exits with code 2 if plan prompt template does not exist', async () => {
    const exitSpy = vi.spyOn(process, 'exit').mockImplementation((_code?: string | number | null) => undefined as never);
    const stderrSpy = vi.spyOn(process.stderr, 'write').mockImplementation(() => true);

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue({
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Test Deliv', type: 'FEATURE', status: 'PLAN' },
      ],
    });

    await executeRunPlan(
      client,
      engine,
      'non-existent-prompt.prompt',
      process.cwd()
    );

    expect(exitSpy).toHaveBeenCalledWith(2);
    expect(stderrSpy).toHaveBeenCalledWith(
      expect.stringContaining('does not exist')
    );

    exitSpy.mockRestore();
    stderrSpy.mockRestore();
  });

  it('exits with code 2 if plan prompt template lacks {{DeliverableId}}', async () => {
    const exitSpy = vi.spyOn(process, 'exit').mockImplementation((_code?: string | number | null) => undefined as never);
    const stderrSpy = vi.spyOn(process.stderr, 'write').mockImplementation(() => true);

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const tempFile = path.resolve(process.cwd(), 'invalid-test.prompt');
    fs.writeFileSync(tempFile, 'No placeholder here');

    try {
      vi.spyOn(client, 'findProjectByRepository').mockResolvedValue({
        id: 'proj-1',
        name: 'Test Project',
        repository: 'https://github.com/owner/repo.git',
        deliverables: [
          { id: 'deliv-1', title: 'Test Deliv', type: 'FEATURE', status: 'PLAN' },
        ],
      });

      await executeRunPlan(
        client,
        engine,
        tempFile,
        process.cwd()
      );

      expect(exitSpy).toHaveBeenCalledWith(2);
      expect(stderrSpy).toHaveBeenCalledWith(
        expect.stringContaining('does not contain required placeholder')
      );
    } finally {
      if (fs.existsSync(tempFile)) fs.unlinkSync(tempFile);
      exitSpy.mockRestore();
      stderrSpy.mockRestore();
    }
  });

  it('executes runPlan per deliverable when valid template is provided', async () => {
    const exitSpy = vi.spyOn(process, 'exit').mockImplementation((_code?: string | number | null) => undefined as never);
    const stdoutSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const tempFile = path.resolve(process.cwd(), 'valid-test.prompt');
    fs.writeFileSync(tempFile, 'Plan for {{DeliverableId}}');

    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue({
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Test Deliv', type: 'FEATURE', status: 'PLAN' },
      ],
    });

    const runPromptSpy = vi.spyOn(engine, 'runPrompt').mockResolvedValue('session-mock-123');

    try {
      await executeRunPlan(
        client,
        engine,
        tempFile,
        process.cwd()
      );

      expect(runPromptSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          prompt: 'Plan for deliv-1',
          title: 'Plan: Test Deliv',
        })
      );
      expect(exitSpy).toHaveBeenCalledWith(0);
    } finally {
      if (fs.existsSync(tempFile)) fs.unlinkSync(tempFile);
      exitSpy.mockRestore();
      stdoutSpy.mockRestore();
    }
  });

  it('prints tabular report on executeShowPlan', async () => {
    const exitSpy = vi.spyOn(process, 'exit').mockImplementation((_code?: string | number | null) => undefined as never);
    const stdoutSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });

    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue({
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Test Feature', type: 'FEATURE', status: 'PLAN' },
      ],
    });

    await executeShowPlan(client, process.cwd());

    expect(stdoutSpy).toHaveBeenCalledWith(
      expect.stringContaining('TYPE         ID')
    );
    expect(exitSpy).toHaveBeenCalledWith(0);

    exitSpy.mockRestore();
    stdoutSpy.mockRestore();
  });
});
