import { useGetDeliverablesQuery } from '@/generated/graphql';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useDeliverables');

/**
 * Fetches deliverables with optional filtering by projectId, status, and type.
 * When projectId is provided, only deliverables for that project are fetched.
 * When projectId is omitted, all deliverables across all projects are fetched.
 */
export function useDeliverables(
    projectId?: string,
    statusFilter?: DeliverableStatus[],
    typeFilter?: DeliverableType[]
) {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        variables: projectId ? { projectId } : undefined,
        fetchPolicy: 'cache-and-network',
        skip: !projectId,
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
            projectId,
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
