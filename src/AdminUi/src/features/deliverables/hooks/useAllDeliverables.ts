import { useGetAllDeliverablesQuery } from '@/generated/graphql';
import type { DeliverableStatus, DeliverableType, DeliverableFilterInput } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useAllDeliverables');

interface UseAllDeliverablesParams {
    statusFilter?: DeliverableStatus;
    typeFilter?: DeliverableType;
}

/**
 * Fetches deliverables with server-side filtering by status and type.
 * Returns all matching deliverables for client-side pagination.
 */
export function useAllDeliverables(params: UseAllDeliverablesParams = {}) {
    const { statusFilter, typeFilter } = params;

    const where: DeliverableFilterInput = {};
    if (statusFilter) {
        where.status = { eq: statusFilter };
    }
    if (typeFilter) {
        where.type = { eq: typeFilter };
    }

    const hasFilters = statusFilter || typeFilter;

    const { data, loading, error, refetch } = useGetAllDeliverablesQuery({
        variables: {
            first: 100,
            where: hasFilters ? where : undefined,
        },
        fetchPolicy: 'cache-and-network',
    });

    const allDeliverables = data?.deliverables?.nodes ?? [];

    const filteredDeliverables = allDeliverables.filter(
        (deliverable): deliverable is NonNullable<typeof deliverable> => deliverable !== null
    );

    if (!loading && allDeliverables.length > 0) {
        logger.debug('Loaded deliverables', {
            total: allDeliverables.length,
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
