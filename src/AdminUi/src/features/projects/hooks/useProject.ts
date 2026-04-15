import { useGetProjectQuery } from '@/generated/graphql';

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
