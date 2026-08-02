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
