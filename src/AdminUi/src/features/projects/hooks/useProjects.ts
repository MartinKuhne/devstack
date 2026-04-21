import { useGetProjectsQuery } from '@/generated/graphql';

/**
 * Fetches all projects from the GraphQL API with cache-and-network policy.
 */
export function useProjects() {
    const { data, loading, error, refetch } = useGetProjectsQuery({
        fetchPolicy: 'cache-and-network',
    });

    return {
        projects: data?.projects.nodes ?? [],
        loading,
        error,
        refetch,
    };
}
