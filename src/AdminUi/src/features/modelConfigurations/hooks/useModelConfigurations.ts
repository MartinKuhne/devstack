import { useModelConfigurationsQuery } from '@/generated/graphql';

export function useModelConfigurations() {
    const { data, loading, error, refetch } = useModelConfigurationsQuery({
        fetchPolicy: 'cache-and-network',
    });

    return {
        modelConfigurations: data?.modelConfigurations ?? [],
        loading,
        error,
        refetch,
    };
}
