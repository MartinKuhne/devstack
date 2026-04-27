import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useGetDeliverablesQuery, DeliverableStatus } from '@/generated/graphql';

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
        expect(result.current.deliverablesDesign).toBe(0);
        expect(result.current.deliverablesPlan).toBe(0);
        expect(result.current.deliverablesImplement).toBe(0);
        expect(result.current.deliverablesMerge).toBe(0);
        expect(result.current.deliverablesDeploy).toBe(0);
        expect(result.current.deliverablesTest).toBe(0);
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
                        { status: 'DRAFT' as DeliverableStatus, id: '1' },
                        { status: 'DRAFT' as DeliverableStatus, id: '2' },
                        { status: 'DESIGN' as DeliverableStatus, id: '3' },
                        { status: 'PLAN' as DeliverableStatus, id: '4' },
                        { status: 'IMPLEMENT' as DeliverableStatus, id: '5' },
                        { status: 'MERGE' as DeliverableStatus, id: '6' },
                        { status: 'DEPLOY' as DeliverableStatus, id: '7' },
                        { status: 'TEST' as DeliverableStatus, id: '8' },
                        { status: 'DONE' as DeliverableStatus, id: '9' },
                        { status: 'NEEDS_REVIEW' as DeliverableStatus, id: '10' },
                        { status: 'FAILED' as DeliverableStatus, id: '11' },
                        { status: 'REJECTED' as DeliverableStatus, id: '12' },
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
        expect(result.current.deliverablesDesign).toBe(1);
        expect(result.current.deliverablesPlan).toBe(1);
        expect(result.current.deliverablesImplement).toBe(1);
        expect(result.current.deliverablesMerge).toBe(1);
        expect(result.current.deliverablesDeploy).toBe(1);
        expect(result.current.deliverablesTest).toBe(1);
        expect(result.current.deliverablesDone).toBe(1);
        expect(result.current.deliverablesNeedsReview).toBe(1);
        expect(result.current.deliverablesFailed).toBe(1);
        expect(result.current.deliverablesRejected).toBe(1);
    });

    it('filters out null deliverables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { status: 'DRAFT' as DeliverableStatus, id: '1' },
                        null,
                        { status: 'PLAN' as DeliverableStatus, id: '2' },
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
        expect(result.current.deliverablesPlan).toBe(1);
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