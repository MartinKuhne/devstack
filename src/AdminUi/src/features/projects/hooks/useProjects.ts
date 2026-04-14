import { gql } from '@apollo/client/core';
import { useQuery } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { GetProjectsQuery } from '@/generated/graphql';

const GET_PROJECTS = gql`
    query GetProjects {
        projects(first: 100) {
            edges {
                node {
                    id
                    name
                    description
                    githubUrl
                    updatedAt
                }
            }
        }
    }
`;

export function useProjects() {
    const { data, loading, error, refetch } = useQuery<GetProjectsQuery>(GET_PROJECTS, {
        client: getApolloClient(),
        fetchPolicy: 'cache-and-network',
    });

    return {
        projects: data?.projects.edges.map((edge: { node: any }) => edge.node) ?? [],
        loading,
        error,
        refetch,
    };
}
