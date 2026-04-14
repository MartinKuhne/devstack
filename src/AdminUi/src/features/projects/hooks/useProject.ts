import { gql } from '@apollo/client/core';
import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { GetProjectQuery, GetProjectQueryVariables } from '@/generated/graphql';

const GET_PROJECT = gql`
    query GetProjectById($id: ID!) {
        projectById(id: $id) {
            id
            name
            description
            architecture
            memory
            githubUrl
            createdAt
            updatedAt
        }
    }
`;

export function useProject(id: string) {
    const { data, loading, error, refetch } = useQuery<GetProjectQuery, GetProjectQueryVariables>(GET_PROJECT, {
        client: getApolloClient(),
        variables: { id },
        fetchPolicy: 'cache-and-network',
    });

    return {
        project: data?.projectById ?? null,
        loading,
        error,
        refetch,
    };
}
