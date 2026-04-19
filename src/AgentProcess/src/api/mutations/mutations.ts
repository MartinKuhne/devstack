import { gql } from 'graphql-request';

export const createProjectMutation = gql`
  mutation CreateProject($input: CreateProjectInput!) {
    createProject(input: $input) {
      project {
        id
        name
        description
        createdAt
      }
      errors
    }
  }
`;

export const updateProjectMutation = gql`
  mutation UpdateProject($input: UpdateProjectInput!) {
    updateProject(input: $input) {
      project {
        id
        name
        description
        updatedAt
      }
      errors
    }
  }
`;

export const createFeatureMutation = gql`
  mutation CreateFeature($input: CreateFeatureInput!) {
    createFeature(input: $input) {
      item {
        id
        title
        description
        status
        projectId
        createdAt
      }
      errors
    }
  }
`;

export const updateFeatureMutation = gql`
  mutation UpdateFeature($input: UpdateFeatureInput!) {
    updateFeature(input: $input) {
      item {
        id
        title
        description
        status
        updatedAt
      }
      errors
    }
  }
`;

export const createTaskMutation = gql`
  mutation CreateTask($input: CreateTaskInput!) {
    createTask(input: $input) {
      task {
        id
        title
        deliverable
        status
        complexityRating
        itemId
        createdAt
      }
      errors
    }
  }
`;

export const updateTaskMutation = gql`
  mutation UpdateTask($input: UpdateTaskInput!) {
    updateTask(input: $input) {
      task {
        id
        title
        deliverable
        status
        complexityRating
        updatedAt
      }
      errors
    }
  }
`;
