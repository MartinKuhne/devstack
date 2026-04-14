import { gql } from '@apollo/client/core';
import { useMutation } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';
import type { UpdateProjectMutation, UpdateProjectMutationVariables } from '@/generated/graphql';

const UPDATE_PROJECT = gql`
    mutation UpdateProject($id: ID!, $input: UpdateProjectInput!) {
        updateProject(id: $id, input: $input) {
            id
            name
            description
            architecture
            memory
            githubUrl
        }
    }
`;

export function useUpdateProject() {
    return useMutation<UpdateProjectMutation, UpdateProjectMutationVariables>(UPDATE_PROJECT, {
        client: getApolloClient(),
    });
}
