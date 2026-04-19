import { useGetDeliverablesQuery } from '@/generated/graphql';
import type { FeatureStatus, ItemSubtype } from '@/generated/graphql';

export function useDeliverables(projectId?: string, status?: FeatureStatus[], subtype?: ItemSubtype[]) {
    const { data, loading, error, refetch } = useGetDeliverablesQuery({
        variables: { projectId: projectId || null, epicId: null, status: status || null, type: subtype || null },
        fetchPolicy: 'cache-and-network',
        skip: !projectId && !status && !subtype,
    });

    return {
        deliverables: data?.items?.nodes ?? [],
        loading,
        error,
        refetch,
    };
}
