import { describe, it, expect, vi } from 'vitest';
import path from 'path';
import fs from 'fs';
import { executeRunPlan, executeShowPlan } from './planExecutor.js';
import { DevStackGraphQLClient } from '../graphql/client.js';
import { OpenCodeAgentEngine } from '../opencode/opencodeEngine.js';
import * as output from '../cli/output.js';

describe('PlanExecutor Section 6 (--run-plan) Unit Tests', () => {
  it('AG-143: exits with code 2 if plan prompt template does not exist', async () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stderrSpy = vi.spyOn(output, 'printError').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const mockProject2 = {
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Task 1', type: 'FEATURE', status: 'PLAN' },
      ],
    };
    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue(mockProject2);
    vi.spyOn(client, 'getPlanDeliverables').mockResolvedValue(mockProject2.deliverables);

    await executeRunPlan({
      graphqlClient: client,
      engine,
      planPromptPath: 'non-existent-prompt.prompt',
      repositoryRootOverride: process.cwd()
    });

    expect(exitSpy).toHaveBeenCalledWith(2);
    expect(stderrSpy).toHaveBeenCalledWith(
      expect.stringContaining('does not exist')
    );

    exitSpy.mockRestore();
    stderrSpy.mockRestore();
  });

  it('AG-144: exits with code 2 if plan prompt template lacks {{DeliverableId}}', async () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stderrSpy = vi.spyOn(output, 'printError').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const tempFile = path.resolve(process.cwd(), 'invalid-test.prompt');
    fs.writeFileSync(tempFile, 'No placeholder here');

    try {
      const mockProject3 = {
        id: 'proj-1',
        name: 'Test Project',
        repository: 'https://github.com/owner/repo.git',
        deliverables: [
          { id: 'deliv-1', title: 'Test Deliv', type: 'FEATURE', status: 'PLAN' },
        ],
      };
      vi.spyOn(client, 'findProjectByRepository').mockResolvedValue(mockProject3);
      vi.spyOn(client, 'getPlanDeliverables').mockResolvedValue(mockProject3.deliverables);

      await executeRunPlan({
        graphqlClient: client,
        engine,
        planPromptPath: tempFile,
        repositoryRootOverride: process.cwd()
      });

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

  it('AG-145..152: executes runPlan per deliverable, returns summary and exits 0 on success', async () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stdoutSpy = vi.spyOn(output, 'printMessage').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const tempFile = path.resolve(process.cwd(), 'valid-test.prompt');
    fs.writeFileSync(tempFile, 'Plan for {{DeliverableId}}');

    const mockProject = {
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Task 1', type: 'FEATURE', status: 'PLAN' },
        { id: 'deliv-2', title: 'Task 2', type: 'DEFECT', status: 'PLAN' },
      ],
    };

    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue(mockProject);
    vi.spyOn(client, 'getPlanDeliverables').mockResolvedValue(mockProject.deliverables);

    const runPromptSpy = vi.spyOn(engine, 'runPrompt').mockResolvedValue('session-mock-123');

    try {
      const summary = await executeRunPlan({
        graphqlClient: client,
        engine,
        planPromptPath: tempFile,
        repositoryRootOverride: process.cwd()
      });

      expect(summary.processedIds).toEqual(['deliv-1', 'deliv-2']);
      expect(summary.succeededCount).toBe(2);
      expect(summary.failedCount).toBe(0);
      expect(runPromptSpy).toHaveBeenCalledTimes(2);
      expect(exitSpy).toHaveBeenCalledWith(0);
    } finally {
      if (fs.existsSync(tempFile)) fs.unlinkSync(tempFile);
      exitSpy.mockRestore();
      stdoutSpy.mockRestore();
    }
  });

  it('AG-148 & AG-152: records deliverable failure and exits 3 when a deliverable fails', async () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stdoutSpy = vi.spyOn(output, 'printMessage').mockImplementation(() => {});
    const stderrSpy = vi.spyOn(output, 'printError').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });
    const engine = new OpenCodeAgentEngine();

    const tempFile = path.resolve(process.cwd(), 'fail-test.prompt');
    fs.writeFileSync(tempFile, 'Plan for {{DeliverableId}}');

    const mockProjectFail = {
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-fail', title: 'Test Deliv Fail', type: 'FEATURE', status: 'PLAN' },
      ],
    };
    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue(mockProjectFail);
    vi.spyOn(client, 'getPlanDeliverables').mockResolvedValue(mockProjectFail.deliverables);

    vi.spyOn(engine, 'runPrompt').mockRejectedValue(new Error('LLM execution timeout'));

    try {
      const summary = await executeRunPlan({
        graphqlClient: client,
        engine,
        planPromptPath: tempFile,
        repositoryRootOverride: process.cwd()
      });

      expect(summary.failedCount).toBe(1);
      expect(summary.failedDeliverables['deliv-fail']).toBe('LLM execution timeout');
      expect(exitSpy).toHaveBeenCalledWith(3);
    } finally {
      if (fs.existsSync(tempFile)) fs.unlinkSync(tempFile);
      exitSpy.mockRestore();
      stdoutSpy.mockRestore();
      stderrSpy.mockRestore();
    }
  });

  it('AG-120..123: prints tabular report on executeShowPlan', async () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stdoutSpy = vi.spyOn(output, 'printMessage').mockImplementation(() => {});

    const client = new DevStackGraphQLClient({ endpoint: 'http://localhost:8087/graphql' });

    const mockProjectShow = {
      id: 'proj-1',
      name: 'Test Project',
      repository: 'https://github.com/owner/repo.git',
      deliverables: [
        { id: 'deliv-1', title: 'Task 1', type: 'FEATURE', status: 'PLAN' },
      ],
    };
    vi.spyOn(client, 'findProjectByRepository').mockResolvedValue(mockProjectShow);
    vi.spyOn(client, 'getPlanDeliverables').mockResolvedValue(mockProjectShow.deliverables);

    await executeShowPlan(client, process.cwd());

    expect(stdoutSpy).toHaveBeenCalledWith(
      expect.stringContaining('TYPE         ID')
    );
    expect(exitSpy).toHaveBeenCalledWith(0);

    exitSpy.mockRestore();
    stdoutSpy.mockRestore();
  });
});
