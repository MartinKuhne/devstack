import { useGetProjectQuery } from '@/generated/graphql';

/**
 * Fetches a single project by ID from the GraphQL API.
 */
export function useProject(id: string) {
    const { data, loading, error, refetch } = useGetProjectQuery({
        variables: { id },
        fetchPolicy: 'cache-and-network',
        skip: !id,
    });

    return {
        project: data?.projectById ?? null,
        loading,
        error,
        refetch,
    };
}
