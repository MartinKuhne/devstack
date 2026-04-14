import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import { GetProjectDocument } from '@/graphql/queries/project.graphql';
import type { GetProjectQuery, GetProjectQueryVariables } from '@/generated/graphql';

export function useProject(id: string) {
    const { data, loading, error, refetch } = useQuery<GetProjectQuery, GetProjectQueryVariables>(
        GetProjectDocument,
        {
            client: getApolloClient(),
            variables: { id },
            fetchPolicy: 'cache-and-network',
        }
    );

    return {
        project: data?.projectById ?? null,
        loading,
        error,
        refetch,
    };
}
