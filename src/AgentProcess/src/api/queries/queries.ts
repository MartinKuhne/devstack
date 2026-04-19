import { gql } from 'graphql-request';

export const dashboardSummaryQuery = gql`
  query DashboardSummary {
    dashboardSummary {
      projectsInFlight
      featuresInReview
      featuresFailed
      tasksInProgress
      tasksFailed
      recentAuditEvents {
        id
        entityType
        eventType
        oldValue
        newValue
        actor
        occurredAt
      }
    }
  }
`;

export const getProjectQuery = gql`
  query GetProjectById($id: UUID!) {
    projectById(id: $id) {
      id
      name
      description
      createdAt
      updatedAt
      githubUrl
      items {
        id
        title
        status
      }
    }
  }
`;

export const getFeaturesQuery = gql`
  query GetItems($projectId: UUID!) {
    items(projectId: $projectId, first: 50) {
      nodes {
        id
        title
        description
        status
        projectId
        createdAt
        updatedAt
      }
    }
  }
`;

export const getTasksQuery = gql`
  query GetTasks($itemId: UUID!) {
    tasks(itemId: $itemId, first: 50) {
      nodes {
        id
        title
        deliverable
        status
        complexityRating
        itemId
        createdAt
        updatedAt
      }
    }
  }
`;
