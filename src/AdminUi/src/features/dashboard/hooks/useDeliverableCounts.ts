import { useGetDeliverablesQuery } from '@/generated/graphql';

export function useDeliverableCounts() {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        variables: { projectId: null, epicId: null, status: null, type: null },
        fetchPolicy: 'cache-and-network',
    });

    const items = data?.items?.nodes ?? [];

    const deliverablesPlanning = items.filter(item => item.status === 'PLANNING').length;
    const deliverablesReady = items.filter(item => item.status === 'READY').length;
    const deliverablesInProgress = items.filter(item => item.status === 'IN_PROGRESS').length;
    const deliverablesNeedsReview = items.filter(item => item.status === 'IN_REVIEW').length;

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
