import { useGetProjectsQuery } from '@/generated/graphql';
import { createModuleLogger } from '@/lib/logging';

const logger = createModuleLogger('useProjects');

/**
 * Fetches all projects from the GraphQL API with cache-and-network policy.
 */
export function useProjects() {
    const { data, loading, error, refetch } = useGetProjectsQuery({
        fetchPolicy: 'cache-and-network',
    });

    const projects = data?.projects?.nodes ?? [];

    if (!loading && projects.length === 0 && !error) {
        logger.debug('No projects found');
    }

    if (error) {
        logger.error('Failed to fetch projects', {
            message: error.message,
        });
    }

    if (!loading && projects.length > 0) {
        logger.debug('Loaded projects', { count: projects.length });
    }

    return {
        projects,
        loading,
        error,
        refetch,
    };
}
