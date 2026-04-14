import { gql } from 'graphql-request';

export const createProjectMutation = gql`
  mutation CreateProject($name: String!, $description: String) {
    createProject(name: $name, description: $description) {
      id
      name
      description
      status
      createdAt
    }
  }
`;

export const updateProjectMutation = gql`
  mutation UpdateProject($id: ID!, $name: String, $description: String, $status: ProjectStatus) {
    updateProject(id: $id, name: $name, description: $description, status: $status) {
      id
      name
      description
      status
      updatedAt
    }
  }
`;

export const createFeatureMutation = gql`
  mutation CreateFeature($projectId: ID!, $name: String!, $description: String) {
    createFeature(projectId: $projectId, name: $name, description: $description) {
      id
      name
      description
      status
      projectId
      createdAt
    }
  }
`;

export const updateFeatureMutation = gql`
  mutation UpdateFeature($id: ID!, $name: String, $description: String, $status: FeatureStatus) {
    updateFeature(id: $id, name: $name, description: $description, status: $status) {
      id
      name
      description
      status
      updatedAt
    }
  }
`;

export const createTaskMutation = gql`
  mutation CreateTask($featureId: ID!, $title: String!, $description: String, $complexity: Int) {
    createTask(featureId: $featureId, title: $title, description: $description, complexity: $complexity) {
      id
      title
      description
      status
      complexity
      featureId
      createdAt
    }
  }
`;

export const updateTaskMutation = gql`
  mutation UpdateTask($id: ID!, $title: String, $description: String, $status: TaskStatus, $complexity: Int) {
    updateTask(id: $id, title: $title, description: $description, status: $status, complexity: $complexity) {
      id
      title
      description
      status
      complexity
      updatedAt
    }
  }
`;
