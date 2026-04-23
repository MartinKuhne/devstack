import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import type { AgentTaskStatus } from '@/generated/graphql';

vi.mock('@/generated/graphql', () => ({
    useGetAgentTasksQuery: vi.fn(),
}));

const getMockedQuery = async () => {
    const m = await import('@/generated/graphql');
    return {
        useGetAgentTasksQuery: m.useGetAgentTasksQuery as ReturnType<typeof vi.fn>,
    };
};

describe('useAgentTasks', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('returns all tasks when no filter', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                        { id: '2', status: 'IN_PROGRESS' as AgentTaskStatus, title: 'Task 2' },
                        { id: '3', status: 'DONE' as AgentTaskStatus, title: 'Task 3' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        const { result } = renderHook(() => useAgentTasks());

        expect(result.current.agentTasks).toHaveLength(3);
    });

    it('filters by status', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                        { id: '2', status: 'IN_PROGRESS' as AgentTaskStatus, title: 'Task 2' },
                        { id: '3', status: 'READY' as AgentTaskStatus, title: 'Task 3' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        const { result } = renderHook(() => useAgentTasks(undefined, ['READY']));

        expect(result.current.agentTasks).toHaveLength(2);
        expect(result.current.agentTasks.every((t) => t.status === 'READY')).toBe(true);
    });

    it('includes null tasks when no status filter is applied', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                        null,
                        { id: '2', status: 'DONE' as AgentTaskStatus, title: 'Task 2' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        const { result } = renderHook(() => useAgentTasks());

        expect(result.current.agentTasks).toHaveLength(3);
    });

    it('returns empty array when no tasks', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: { agentTasks: { nodes: [] } },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        const { result } = renderHook(() => useAgentTasks());

        expect(result.current.agentTasks).toHaveLength(0);
    });

    it('passes deliverableId to query variables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        renderHook(() => useAgentTasks('del-123'));

        expect(hooks.useGetAgentTasksQuery).toHaveBeenCalledWith(
            expect.objectContaining({
                variables: { deliverableId: 'del-123' },
                fetchPolicy: 'cache-and-network',
            }),
        );
    });

    it('passes no variables when deliverableId is omitted', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        renderHook(() => useAgentTasks());

        expect(hooks.useGetAgentTasksQuery).toHaveBeenCalledWith({
            fetchPolicy: 'cache-and-network',
        });
    });

    it('returns loading and error states', async () => {
        const hooks = await getMockedQuery();
        const mockError = { message: 'Failed' };
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: undefined,
            loading: true,
            error: mockError,
            refetch: vi.fn(),
        });

        const { useAgentTasks } = await import('./useAgentTasks');
        const { result } = renderHook(() => useAgentTasks());

        expect(result.current.loading).toBe(true);
        expect(result.current.error).toBe(mockError);
    });
});
