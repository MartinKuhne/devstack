import { gql } from '@apollo/client/core';
import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { GetFeaturesQuery, GetFeaturesQueryVariables, FeatureStatus } from '@/generated/graphql';

const GET_FEATURES = gql`
    query GetFeatures($projectId: ID, $status: [FeatureStatus!]) {
        features(first: 100, projectId: $projectId, status: $status) {
            edges {
                node {
                    id
                    title
                    status
                    updatedAt
                    tasks {
                        id
                    }
                }
            }
        }
    }
`;

export function useFeatures(projectId?: string, status?: FeatureStatus[]) {
    const { data, loading, error, refetch } = useQuery<GetFeaturesQuery, GetFeaturesQueryVariables>(GET_FEATURES, {
        client: getApolloClient(),
        variables: {
            projectId: projectId ?? null,
            status: status ?? null,
        } as GetFeaturesQueryVariables,
        fetchPolicy: 'cache-and-network',
    });

    return {
        features: data?.features?.edges?.map((edge) => edge.node) ?? [],
        loading,
        error,
        refetch,
    };
}
