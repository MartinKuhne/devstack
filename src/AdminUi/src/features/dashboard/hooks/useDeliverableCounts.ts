import { useGetDeliverablesQuery } from '@/generated/graphql';

/**
 * Fetches all deliverables and returns counts categorized by status.
 */
export function useDeliverableCounts() {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        fetchPolicy: 'cache-and-network',
    });

    const deliverables = data?.deliverables ?? [];

    const deliverablesPlanning = deliverables.filter(d => d.status === 'PLANNING').length;
    const deliverablesReady = deliverables.filter(d => d.status === 'READY').length;
    const deliverablesInProgress = deliverables.filter(d => d.status === 'IN_PROGRESS').length;
    const deliverablesNeedsReview = deliverables.filter(d => d.status === 'NEEDS_REVIEW').length;

    return {
        deliverablesPlanning,
        deliverablesReady,
        deliverablesInProgress,
        deliverablesNeedsReview,
        loading,
        error,
        refetch,
    };
}
