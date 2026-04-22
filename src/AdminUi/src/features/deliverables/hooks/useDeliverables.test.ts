import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';

vi.mock('@/generated/graphql', () => ({
    useGetDeliverablesQuery: vi.fn(),
}));

const getMockedQuery = async () => {
    const m = await import('@/generated/graphql');
    return {
        useGetDeliverablesQuery: m.useGetDeliverablesQuery as ReturnType<typeof vi.fn>,
    };
};

describe('useDeliverables', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('returns all deliverables when no filters', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                        { id: '2', status: 'READY' as DeliverableStatus, type: 'DEFECT' as DeliverableType },
                        { id: '3', status: 'DONE' as DeliverableStatus, type: 'MAINTENANCE' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables());

        expect(result.current.deliverables).toHaveLength(3);
    });

    it('filters by status', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                        { id: '2', status: 'READY' as DeliverableStatus, type: 'DEFECT' as DeliverableType },
                        { id: '3', status: 'DRAFT' as DeliverableStatus, type: 'MAINTENANCE' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables(['DRAFT']));

        expect(result.current.deliverables).toHaveLength(2);
        expect(result.current.deliverables.every((d) => d.status === 'DRAFT')).toBe(true);
    });

    it('filters by type', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                        { id: '2', status: 'READY' as DeliverableStatus, type: 'DEFECT' as DeliverableType },
                        { id: '3', status: 'DONE' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables(undefined, ['FEATURE']));

        expect(result.current.deliverables).toHaveLength(2);
        expect(result.current.deliverables.every((d) => d.type === 'FEATURE')).toBe(true);
    });

    it('filters by both status and type', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                        { id: '2', status: 'DRAFT' as DeliverableStatus, type: 'DEFECT' as DeliverableType },
                        { id: '3', status: 'READY' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables(['DRAFT'], ['FEATURE']));

        expect(result.current.deliverables).toHaveLength(1);
        expect(result.current.deliverables[0]!.id).toBe('1');
    });

    it('filters out null deliverables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                        null,
                        { id: '2', status: 'READY' as DeliverableStatus, type: 'DEFECT' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables());

        expect(result.current.deliverables).toHaveLength(2);
    });

    it('returns empty array when no deliverables', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: { deliverables: { nodes: [] } },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables());

        expect(result.current.deliverables).toHaveLength(0);
    });

    it('returns loading and error states', async () => {
        const hooks = await getMockedQuery();
        const mockError = { message: 'Failed' };
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: undefined,
            loading: true,
            error: mockError,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables());

        expect(result.current.loading).toBe(true);
        expect(result.current.error).toBe(mockError);
    });

    it('treats empty filter arrays as no filter', async () => {
        const hooks = await getMockedQuery();
        hooks.useGetDeliverablesQuery.mockReturnValue({
            data: {
                deliverables: {
                    nodes: [
                        { id: '1', status: 'DRAFT' as DeliverableStatus, type: 'FEATURE' as DeliverableType },
                    ],
                },
            },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useDeliverables } = await import('./useDeliverables');
        const { result } = renderHook(() => useDeliverables([], []));

        expect(result.current.deliverables).toHaveLength(1);
    });
});
