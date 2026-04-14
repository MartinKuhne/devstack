import { gql } from '@apollo/client/core';
import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';

const GET_MODEL_CONFIGURATIONS = gql`
    query GetModelConfigurations($projectId: ID!) {
        modelConfigurations(projectId: $projectId) {
            id
            projectId
            url
            model
            modelAlias
            maxComplexity
            createdAt
            updatedAt
        }
    }
`;

export interface ModelConfiguration {
    id: string;
    projectId: string;
    url: string;
    model: string;
    modelAlias: string | null;
    maxComplexity: number;
    createdAt: string;
    updatedAt: string;
}

export function useModelConfigurations(projectId: string) {
    const { data, loading, error, refetch } = useQuery<{
        modelConfigurations: ModelConfiguration[];
    }, { projectId: string }>(GET_MODEL_CONFIGURATIONS, {
        client: getApolloClient(),
        variables: { projectId },
        fetchPolicy: 'cache-and-network',
        skip: !projectId,
    });

    return {
        modelConfigurations: data?.modelConfigurations ?? [],
        loading,
        error,
        refetch,
    };
}
