import { gql } from "@apollo/client";
import { useQuery } from "@apollo/client/react";

export const GET_FEATURE_BY_ID = gql`
  query GetFeatureById($id: ID!) {
    featureById(id: $id) {
      id
      title
      status
      description
      acceptanceCriteria
      plan
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      openQuestions
      result
      errors
      createdAt
      updatedAt
      validStatusTransitions
      tasks {
        id
        title
        status
      }
    }
  }
`;

interface Task {
  id: string;
  title: string;
  status: string;
}

interface Feature {
  id: string;
  title: string;
  status: string;
  description: string;
  acceptanceCriteria: string;
  plan: string;
  securityImpact: string;
  performanceImpact: string;
  testPlan: string;
  deploymentPlan: string;
  openQuestions: string;
  result: string;
  errors: string;
  createdAt: string;
  updatedAt: string;
  validStatusTransitions: string[];
  tasks: Task[];
}

interface GetFeatureByIdResponse {
  featureById: Feature;
}

interface GetFeatureByIdVariables {
  id: string;
}

export function useFeature(featureId: string) {
  return useQuery<GetFeatureByIdResponse, GetFeatureByIdVariables>(
    GET_FEATURE_BY_ID,
    {
      variables: { id: featureId },
      fetchPolicy: "cache-and-network",
    }
  );
}
