import { useGetFeaturesQuery } from '@/generated/graphql';
import type { FeatureStatus } from '@/generated/graphql';

export function useFeatures(projectId?: string, status?: FeatureStatus[]) {
    const { data, loading, error, refetch } = useGetFeaturesQuery({
        variables: {
            projectId: projectId ?? null,
            status: status ?? null,
        },
        fetchPolicy: 'cache-and-network',
    });

    return {
        features: data?.features?.nodes ?? [],
        loading,
        error,
        refetch,
    };
}
