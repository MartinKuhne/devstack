import { gql } from "@apollo/client";
import { useMutation } from "@apollo/client/react";

export const TRANSITION_FEATURE_STATUS = gql`
  mutation TransitionFeatureStatus($id: ID!, $targetStatus: FeatureStatus!, $actor: String!) {
    transitionFeatureStatus(id: $id, targetStatus: $targetStatus, actor: $actor) {
      feature {
        id
        status
      }
      errors
    }
  }
`;

interface TransitionFeatureStatusData {
  transitionFeatureStatus: {
    feature: {
      id: string;
      status: string;
    };
    errors: string[];
  };
}

interface TransitionFeatureStatusVariables {
  id: string;
  targetStatus: string;
  actor: string;
}

export function useTransitionFeatureStatus() {
  return useMutation<TransitionFeatureStatusData, TransitionFeatureStatusVariables>(
    TRANSITION_FEATURE_STATUS
  );
}
