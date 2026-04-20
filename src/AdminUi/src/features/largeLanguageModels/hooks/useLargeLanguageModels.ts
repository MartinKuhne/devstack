import { useModelConfigurationsQuery } from '@/generated/graphql';

export function useLargeLanguageModels() {
    const { data, loading, error, refetch } = useModelConfigurationsQuery({
        fetchPolicy: 'cache-and-network',
    });

    return {
        largeLanguageModels: data?.largeLanguageModels ?? [],
        loading,
        error,
        refetch,
    };
}
