import { gql } from '../gql/index.js';

export const GET_PROJECTS = gql(`
  query GetProjects($first: Int) {
    projects(first: $first) {
      nodes {
        id
        name
        description
        repository
      }
    }
  }
`);

export const GET_PROJECT_BY_ID = gql(`
  query GetProjectById($id: UUID!) {
    project(id: $id) {
      id
      name
      description
      repository
      deliverables {
        id
        title
        type
        status
        description
      }
    }
  }
`);

export const GET_PROJECTS_FOR_RESOLVER = gql(`
  query GetProjectsForResolver {
    projects {
      nodes {
        id
        name
        description
        repository
        deliverables {
          id
          title
          type
          status
          description
        }
      }
    }
  }
`);
