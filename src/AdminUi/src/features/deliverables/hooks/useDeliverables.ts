import { useGetDeliverablesQuery } from '@/generated/graphql';
import type { DeliverableStatus, DeliverableType } from '@/generated/graphql';

/**
 * Fetches all deliverables with optional filtering by status and type.
 */
export function useDeliverables(statusFilter?: DeliverableStatus[], typeFilter?: DeliverableType[]) {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        fetchPolicy: 'cache-and-network',
    });

    const allDeliverables = data?.deliverables ?? [];
    
    const filteredDeliverables = allDeliverables.filter(deliverable => {
        if (statusFilter && statusFilter.length > 0 && !statusFilter.includes(deliverable.status as DeliverableStatus)) {
            return false;
        }
        if (typeFilter && typeFilter.length > 0 && !typeFilter.includes(deliverable.type as DeliverableType)) {
            return false;
        }
        return true;
    });

    return {
        deliverables: filteredDeliverables,
        loading,
        error,
        refetch,
    };
}
