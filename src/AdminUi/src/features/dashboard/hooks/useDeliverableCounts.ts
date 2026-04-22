import { useGetDeliverablesQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useDeliverableCounts');

/**
 * Fetches all deliverables and returns counts categorized by status.
 */
export function useDeliverableCounts() {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        fetchPolicy: 'cache-and-network',
    });

    const deliverables = data?.deliverables?.nodes ?? [];

    const deliverablesDraft = deliverables.filter((d) => d.status === 'DRAFT').length;
    const deliverablesPlanning = deliverables.filter((d) => d.status === 'PLANNING').length;
    const deliverablesReady = deliverables.filter((d) => d.status === 'READY').length;
    const deliverablesInProgress = deliverables.filter((d) => d.status === 'IN_PROGRESS').length;
    const deliverablesNeedsReview = deliverables.filter((d) => d.status === 'NEEDS_REVIEW').length;
    const deliverablesDone = deliverables.filter((d) => d.status === 'DONE').length;
    const deliverablesFailed = deliverables.filter((d) => d.status === 'FAILED').length;
    const deliverablesRejected = deliverables.filter((d) => d.status === 'REJECTED').length;

    if (!loading && deliverables.length > 0) {
        logger.debug('Loaded deliverable counts', {
            draft: deliverablesDraft,
            planning: deliverablesPlanning,
            ready: deliverablesReady,
            inProgress: deliverablesInProgress,
            needsReview: deliverablesNeedsReview,
            done: deliverablesDone,
            failed: deliverablesFailed,
            rejected: deliverablesRejected,
        });
    }

    if (error) {
        logger.error('Failed to fetch deliverable counts', {
            message: error.message,
        });
    }

    return {
        deliverablesDraft,
        deliverablesPlanning,
        deliverablesReady,
        deliverablesInProgress,
        deliverablesNeedsReview,
        deliverablesDone,
        deliverablesFailed,
        deliverablesRejected,
        loading,
        error,
        refetch,
    };
}
