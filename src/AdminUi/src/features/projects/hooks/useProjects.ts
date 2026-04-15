import { useGetProjectsQuery } from '@/generated/graphql';

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
