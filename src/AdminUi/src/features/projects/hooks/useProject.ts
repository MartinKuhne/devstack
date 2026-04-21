import { useGetProjectQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useProject');

/**
 * Fetches a single project by ID from the GraphQL API.
 */
export function useProject(id: string) {
    const { data, loading, error, refetch } = useGetProjectQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    const project = data?.projectById ?? null;

    if (!loading && !project && !error && id) {
        logger.warn('Project not found', { id });
    }

    if (error) {
        logger.error('Failed to fetch project', {
            id,
            message: error.message,
        });
    }

    if (!loading && project) {
        logger.debug('Loaded project', { id: project.id, name: project.name });
    }

    return {
        project,
        loading,
        error,
        refetch,
    };
}
