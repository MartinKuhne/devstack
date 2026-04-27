import { useModelConfigurationsQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useLargeLanguageModels');

/**
 * Fetches all large language model configurations from the GraphQL API.
 */
export function useLargeLanguageModels() {
    const { data, loading, error, refetch } = useModelConfigurationsQuery({
        fetchPolicy: 'cache-and-network',
    });

    const models = data?.largeLanguageModels?.nodes ?? [];
    const totalCount = models.length;

    if (!loading && totalCount > 0) {
        logger.debug('Loaded LLM configurations', { count: totalCount });
    }

    if (error) {
        logger.error('Failed to fetch LLM configurations', {
            message: error.message,
        });
    }

    return {
        largeLanguageModels: models,
        totalCount,
        loading,
        error,
        refetch,
    };
}
