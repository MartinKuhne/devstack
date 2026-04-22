import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useGetDeliverablesQuery } from '@/generated/graphql';

vi.mock('@/generated/graphql', () => ({
    useGetDeliverablesQuery: vi.fn(),
}));

const getMockedQuery = async () => {
    const m = await import('@/generated/graphql');
    return {
        useGetDeliverablesQuery: m.useGetDeliverablesQuery as ReturnType<typeof vi.fn>,
    };
};

describe('useDeliverableCounts', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('returns all zero counts when no deliverables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: { deliverables: { nodes: [] } },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverableCounts } = await import('./useDeliverableCounts');
        const { result } = renderHook(() => useDeliverableCounts());

        expect(result.current.deliverablesDraft).toBe(0);
        expect(result.current.deliverablesPlanning).toBe(0);
        expect(result.current.deliverablesReady).toBe(0);
        expect(result.current.deliverablesInProgress).toBe(0);
        expect(result.current.deliverablesNeedsReview).toBe(0);
        expect(result.current.deliverablesDone).toBe(0);
        expect(result.current.deliverablesFailed).toBe(0);
        expect(result.current.deliverablesRejected).toBe(0);
    });

    it('counts deliverables by status', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { status: 'DRAFT' as const, id: '1' },
                        { status: 'DRAFT' as const, id: '2' },
                        { status: 'PLANNING' as const, id: '3' },
                        { status: 'READY' as const, id: '4' },
                        { status: 'IN_PROGRESS' as const, id: '5' },
                        { status: 'NEEDS_REVIEW' as const, id: '6' },
                        { status: 'DONE' as const, id: '7' },
                        { status: 'FAILED' as const, id: '8' },
                        { status: 'REJECTED' as const, id: '9' },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverableCounts } = await import('./useDeliverableCounts');
        const { result } = renderHook(() => useDeliverableCounts());

        expect(result.current.deliverablesDraft).toBe(2);
        expect(result.current.deliverablesPlanning).toBe(1);
        expect(result.current.deliverablesReady).toBe(1);
        expect(result.current.deliverablesInProgress).toBe(1);
        expect(result.current.deliverablesNeedsReview).toBe(1);
        expect(result.current.deliverablesDone).toBe(1);
        expect(result.current.deliverablesFailed).toBe(1);
        expect(result.current.deliverablesRejected).toBe(1);
    });

    it('filters out null deliverables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { status: 'DRAFT' as const, id: '1' },
                        null,
                        { status: 'READY' as const, id: '2' },
                        null,
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverableCounts } = await import('./useDeliverableCounts');
        const { result } = renderHook(() => useDeliverableCounts());

        expect(result.current.deliverablesDraft).toBe(1);
        expect(result.current.deliverablesReady).toBe(1);
    });

    it('returns loading state', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: undefined,
            loading: true,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverableCounts } = await import('./useDeliverableCounts');
        const { result } = renderHook(() => useDeliverableCounts());

        expect(result.current.loading).toBe(true);
    });

    it('returns error when present', async () => {
        const hooks = await getMockedQuery();
        const mockError = { message: 'Network error' };
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: mockError,
            refetch: vi.fn(),
        });

        const { useDeliverableCounts } = await import('./useDeliverableCounts');
        const { result } = renderHook(() => useDeliverableCounts());

        expect(result.current.error).toBe(mockError);
    });
});
