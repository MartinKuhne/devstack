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

describe('useAgentTasksByDeliverable', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('passes deliverableId to useAgentTasks', async () => {
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

        const { useAgentTasksByDeliverable } = await import('./useAgentTasksByDeliverable');
        renderHook(() => useAgentTasksByDeliverable('del-123'));

        expect(hooks.useGetAgentTasksQuery).toHaveBeenCalledWith(
            expect.objectContaining({
                variables: { deliverableId: 'del-123' },
                fetchPolicy: 'cache-and-network',
            }),
        );
    });

    it('skips query when no deliverableId', async () => {
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

        const { useAgentTasksByDeliverable } = await import('./useAgentTasksByDeliverable');
        renderHook(() => useAgentTasksByDeliverable());

        expect(hooks.useGetAgentTasksQuery).toHaveBeenCalledWith({
            fetchPolicy: 'cache-and-network',
            skip: true,
            variables: undefined,
        });
    });

    it('returns tasks for a deliverable', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetAgentTasksQuery.mockReturnValue({
            data: {
                agentTasks: {
                    nodes: [
                        { id: '1', status: 'READY' as AgentTaskStatus, title: 'Task 1' },
                        { id: '2', status: 'IN_PROGRESS' as AgentTaskStatus, title: 'Task 2' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useAgentTasksByDeliverable } = await import('./useAgentTasksByDeliverable');
        const { result } = renderHook(() => useAgentTasksByDeliverable('del-123'));

        expect(result.current.agentTasks).toHaveLength(2);
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

        const { useAgentTasksByDeliverable } = await import('./useAgentTasksByDeliverable');
        const { result } = renderHook(() => useAgentTasksByDeliverable('del-123', ['READY']));

        expect(result.current.agentTasks).toHaveLength(2);
        expect(result.current.agentTasks.every((t) => t.status === 'READY')).toBe(true);
    });
});
