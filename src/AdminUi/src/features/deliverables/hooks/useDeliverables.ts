import { useGetDeliverablesQuery } from '@/generated/graphql';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useDeliverables');

/**
 * Fetches all deliverables with optional filtering by status and type.
 */
export function useDeliverables(
    statusFilter?: DeliverableStatus[],
    typeFilter?: DeliverableType[]
) {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        fetchPolicy: 'cache-and-network',
    });

    const allDeliverables = data?.deliverables?.nodes ?? [];

    const filteredDeliverables = allDeliverables.filter((deliverable): deliverable is NonNullable<typeof deliverable> => deliverable !== null && (
        !statusFilter ||
        statusFilter.length === 0 ||
        statusFilter.includes(deliverable.status as DeliverableStatus)
    ) && (
        !typeFilter ||
        typeFilter.length === 0 ||
        typeFilter.includes(deliverable.type as DeliverableType)
    ));

    if (!loading && allDeliverables.length > 0) {
        logger.debug('Loaded deliverables', {
            total: allDeliverables.length,
            filtered: filteredDeliverables.length,
            statusFilter,
            typeFilter,
        });
    }

    if (error) {
        logger.error('Failed to fetch deliverables', {
            message: error.message,
        });
    }

    return {
        deliverables: filteredDeliverables,
        loading,
        error,
        refetch,
    };
}
