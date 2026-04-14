import { gql } from 'graphql-request';

export const dashboardSummaryQuery = gql`
  query DashboardSummary {
    dashboardSummary {
      totalProjects
      totalFeatures
      totalTasks
      activeWorkflows
      recentActivity {
        id
        entityType
        action
        timestamp
        description
      }
    }
  }
`;

export const getProjectQuery = gql`
  query GetProject($id: ID!) {
    project(id: $id) {
      id
      name
      description
      status
      createdAt
      updatedAt
      githubUrl
      features {
        id
        name
        status
      }
    }
  }
`;

export const getFeaturesQuery = gql`
  query GetFeatures($projectId: ID!) {
    features(projectId: $projectId) {
      id
      name
      description
      status
      projectId
      createdAt
      updatedAt
    }
  }
`;

export const getTasksQuery = gql`
  query GetTasks($featureId: ID!) {
    tasks(featureId: $featureId) {
      id
      title
      description
      status
      complexity
      featureId
      createdAt
      updatedAt
    }
  }
`;
