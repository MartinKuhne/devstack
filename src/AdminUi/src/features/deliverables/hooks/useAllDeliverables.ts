import { useGetAllDeliverablesQuery } from '@/generated/graphql';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAllDeliverables');

/**
 * Fetches all deliverables across all projects with optional client-side filtering by status and type.
 */
export function useAllDeliverables(
    statusFilter?: DeliverableStatus[],
    typeFilter?: DeliverableType[]
) {
    const { data, loading, error, refetch } = useGetAllDeliverablesQuery({
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
        logger.debug('Loaded all deliverables', {
            total: allDeliverables.length,
            filtered: filteredDeliverables.length,
            statusFilter,
            typeFilter,
        });
    }

    if (error) {
        logger.error('Failed to fetch all deliverables', {
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
