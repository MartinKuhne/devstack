import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import { GetFeaturesDocument } from '@/graphql/queries/features.graphql';
import type { GetFeaturesQuery, GetFeaturesQueryVariables } from '@/generated/graphql';

export function useFeatures(projectId?: string, status?: string[]) {
    const { data, loading, error, refetch } = useQuery<GetFeaturesQuery, GetFeaturesQueryVariables>(
        GetFeaturesDocument,
        {
            client: getApolloClient(),
            variables: {
                projectId: projectId ?? null,
                status: status as any ?? null,
            },
            fetchPolicy: 'cache-and-network',
        }
    );

    return {
        features: data?.features.edges.map(edge => edge.node) ?? [],
        loading,
        error,
        refetch,
    };
}
