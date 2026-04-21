import { useModelConfigurationsQuery } from '@/generated/graphql';

/**
 * Fetches all large language model configurations from the GraphQL API.
 */
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
