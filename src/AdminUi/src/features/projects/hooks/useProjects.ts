import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import { GetProjectsDocument } from '@/graphql/queries/projects.graphql';
import type { GetProjectsQuery } from '@/generated/graphql';

export function useProjects() {
    const { data, loading, error, refetch } = useQuery<GetProjectsQuery>(GetProjectsDocument, {
        client: getApolloClient(),
        fetchPolicy: 'cache-and-network',
    });

    return {
        projects: data?.projects.edges.map(edge => edge.node) ?? [],
        loading,
        error,
        refetch,
    };
}
