import { useModelConfigurationsQuery } from '@/generated/graphql';

export function useModelConfigurations(projectId: string) {
    const { data, loading, error, refetch } = useModelConfigurationsQuery({
        variables: { projectId },
        fetchPolicy: 'cache-and-network',
        skip: !projectId,
    });

    return {
        modelConfigurations: data?.modelConfigurations ?? [],
        loading,
        error,
        refetch,
    };
}
