import { describe, it, expect, vi } from 'vitest';
import { parseCliArgs } from './args.js';
import * as output from './output.js';

describe('CLI Argument Parsing', () => {
  it('defaults prompt to Hello when no positional prompt provided', () => {
    const options = parseCliArgs([]);
    expect(options.prompt).toBe('Hello');
  });

  it('uses first positional argument as prompt', () => {
    const options = parseCliArgs(['Custom Prompt']);
    expect(options.prompt).toBe('Custom Prompt');
  });

  it('parses --model provider/model', () => {
    const options = parseCliArgs(['--model', 'anthropic/claude-3-5-sonnet']);
    expect(options.modelProvider).toBe('anthropic');
    expect(options.modelName).toBe('claude-3-5-sonnet');
  });

  it('parses --list-projects flag and optional count', () => {
    const options = parseCliArgs(['--list-projects', '10']);
    expect(options.listProjects).toBe(true);
    expect(options.listProjectsCount).toBe(10);
  });

  it('parses --show-plan flag', () => {
    const options = parseCliArgs(['--show-plan']);
    expect(options.showPlan).toBe(true);
  });

  it('parses --run-plan flag and --plan-prompt', () => {
    const options = parseCliArgs(['--run-plan', '--plan-prompt', 'custom/plan.prompt']);
    expect(options.runPlan).toBe(true);
    expect(options.planPrompt).toBe('custom/plan.prompt');
  });

  it('parses --repositoryRoot', () => {
    const options = parseCliArgs(['--repositoryRoot', '/path/to/repo']);
    expect(options.repositoryRoot).toBe('/path/to/repo');
  });

  it('exits with code 2 on invalid --get-project UUID', () => {
    const exitSpy = vi.spyOn(output, 'exitProcess').mockImplementation((_code?: number) => undefined as never);
    const stderrSpy = vi.spyOn(output, 'printError').mockImplementation(() => {});

    parseCliArgs(['--get-project', 'invalid-uuid']);

    expect(exitSpy).toHaveBeenCalledWith(2);
    expect(stderrSpy).toHaveBeenCalled();
    exitSpy.mockRestore();
    stderrSpy.mockRestore();
  });
});
