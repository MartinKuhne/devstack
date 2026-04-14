/* eslint-disable @typescript-eslint/unbound-method */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import * as queuesModule from './queues.js';
import {
  enqueuePlannerRun,
  enqueueDevleadRun,
  enqueueCoderRun,
  enqueueTesterRun,
  enqueueArchitectRun,
} from './scheduler.js';

vi.mock('./queues.js', () => ({
  plannerQueue: {
    add: vi.fn().mockResolvedValue({ id: '1' }),
  },
  devleadQueue: {
    add: vi.fn().mockResolvedValue({ id: '1' }),
  },
  coderQueue: {
    add: vi.fn().mockResolvedValue({ id: '1' }),
  },
  testerQueue: {
    add: vi.fn().mockResolvedValue({ id: '1' }),
  },
  architectQueue: {
    add: vi.fn().mockResolvedValue({ id: '1' }),
  },
  closeQueals: vi.fn().mockResolvedValue(undefined),
}));

describe('Scheduler', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(async () => {
    await queuesModule.closeQueals();
  });

  it('should enqueue planner run', async () => {
    const data = { projectId: 'proj-1', featureId: 'feat-1' };
    await enqueuePlannerRun(data);

    expect(queuesModule.plannerQueue.add).toHaveBeenCalledWith('planner-run', data);
  });

  it('should enqueue devlead run', async () => {
    const data = { projectId: 'proj-1', featureId: 'feat-1' };
    await enqueueDevleadRun(data);

    expect(queuesModule.devleadQueue.add).toHaveBeenCalledWith('devlead-run', data);
  });

  it('should enqueue coder run', async () => {
    const data = { projectId: 'proj-1', featureId: 'feat-1', taskId: 'task-1' };
    await enqueueCoderRun(data);

    expect(queuesModule.coderQueue.add).toHaveBeenCalledWith('coder-run', data);
  });

  it('should enqueue tester run', async () => {
    const data = { projectId: 'proj-1', featureId: 'feat-1', taskId: 'task-1' };
    await enqueueTesterRun(data);

    expect(queuesModule.testerQueue.add).toHaveBeenCalledWith('tester-run', data);
  });

  it('should enqueue architect run', async () => {
    const data = { projectId: 'proj-1', featureId: 'feat-1', taskId: 'task-1' };
    await enqueueArchitectRun(data);

    expect(queuesModule.architectQueue.add).toHaveBeenCalledWith('architect-run', data);
  });
});
