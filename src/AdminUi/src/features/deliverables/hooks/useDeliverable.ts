import { useGetDeliverableQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useDeliverable');

export function useDeliverable(id: string) {
    const { data, loading, error, refetch } = useGetDeliverableQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    const deliverable = data?.deliverable ?? null;

    if (!loading && !deliverable && !error && id) {
        logger.warn('Deliverable not found', { id });
    }

    if (error) {
        logger.error('Failed to fetch deliverable', {
            id,
            message: error.message,
        });
    }

    if (!loading && deliverable) {
        logger.debug('Loaded deliverable', { id: deliverable.id, title: deliverable.title });
    }

    return {
        deliverable,
        loading,
        error,
        refetch,
    };
}
