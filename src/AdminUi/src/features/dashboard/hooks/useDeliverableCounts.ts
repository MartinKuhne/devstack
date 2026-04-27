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

    const deliverablesDraft = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'DRAFT').length;
    const deliverablesDesign = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'DESIGN').length;
    const deliverablesPlan = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'PLAN').length;
    const deliverablesImplement = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'IMPLEMENT').length;
    const deliverablesMerge = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'MERGE').length;
    const deliverablesDeploy = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'DEPLOY').length;
    const deliverablesTest = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'TEST').length;
    const deliverablesDone = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'DONE').length;
    const deliverablesNeedsReview = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'NEEDS_REVIEW').length;
    const deliverablesFailed = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'FAILED').length;
    const deliverablesRejected = deliverables.filter((d): d is NonNullable<typeof d> => d !== null && d.status === 'REJECTED').length;

    if (!loading && deliverables.length > 0) {
        logger.debug('Loaded deliverable counts', {
            draft: deliverablesDraft,
            design: deliverablesDesign,
            plan: deliverablesPlan,
            implement: deliverablesImplement,
            merge: deliverablesMerge,
            deploy: deliverablesDeploy,
            test: deliverablesTest,
            done: deliverablesDone,
            needsReview: deliverablesNeedsReview,
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
        deliverablesDesign,
        deliverablesPlan,
        deliverablesImplement,
        deliverablesMerge,
        deliverablesDeploy,
        deliverablesTest,
        deliverablesDone,
        deliverablesNeedsReview,
        deliverablesFailed,
        deliverablesRejected,
        loading,
        error,
        refetch,
    };
}
