// @ts-nocheck
import { gql } from '@apollo/client';
import * as Apollo from '@apollo/client';
import * as ApolloReactHooks from '@apollo/client/react';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
const defaultOptions = {} as const;
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  UUID: { input: any; output: any; }
};

export type AgentTask = {
  __typename?: 'AgentTask';
  agent: Maybe<Scalars['String']['output']>;
  commitHash: Maybe<Scalars['String']['output']>;
  completionTokens: Maybe<Scalars['Int']['output']>;
  complexityRating: Maybe<Scalars['Int']['output']>;
  deliverable: Maybe<Deliverable>;
  deliverableId: Maybe<Scalars['ID']['output']>;
  dependsOnAgentTask: Maybe<AgentTask>;
  dependsOnAgentTaskId: Maybe<Scalars['ID']['output']>;
  description: Maybe<Scalars['String']['output']>;
  errors: Maybe<Scalars['String']['output']>;
  executionDurationInSeconds: Maybe<Scalars['Int']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  project: Maybe<Project>;
  projectId: Scalars['UUID']['output'];
  promptTokens: Maybe<Scalars['Int']['output']>;
  result: Maybe<Scalars['String']['output']>;
  status: Maybe<AgentTaskStatus>;
  title: Maybe<Scalars['String']['output']>;
};

export type AgentTaskPayload = {
  __typename?: 'AgentTaskPayload';
  agentTask: Maybe<AgentTask>;
  errors: Array<Scalars['String']['output']>;
};

export const AgentTaskStatus = {
  DONE: 'DONE',
  FAILED: 'FAILED',
  IN_PROGRESS: 'IN_PROGRESS',
  NEEDS_REVIEW: 'NEEDS_REVIEW',
  READY: 'READY',
  REJECTED: 'REJECTED'
} as const;

export type AgentTaskStatus = typeof AgentTaskStatus[keyof typeof AgentTaskStatus];
export type CleanupTestDataPayload = {
  __typename?: 'CleanupTestDataPayload';
  message: Maybe<Scalars['String']['output']>;
  success: Scalars['Boolean']['output'];
};

export type CreateAgentTaskInput = {
  agent: InputMaybe<Scalars['String']['input']>;
  commitHash: InputMaybe<Scalars['String']['input']>;
  completionTokens: InputMaybe<Scalars['Int']['input']>;
  complexityRating: Scalars['Int']['input'];
  deliverableId: Scalars['UUID']['input'];
  dependsOnAgentTaskId: InputMaybe<Scalars['UUID']['input']>;
  description: Scalars['String']['input'];
  errors: InputMaybe<Scalars['String']['input']>;
  executionDurationInSeconds: InputMaybe<Scalars['Int']['input']>;
  promptTokens: InputMaybe<Scalars['Int']['input']>;
  result: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
};

export type CreateDeliverableInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  agentFeedback: InputMaybe<Scalars['String']['input']>;
  blocking: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  executionPlan: InputMaybe<Scalars['String']['input']>;
  initialStatus: InputMaybe<DeliverableStatus>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['UUID']['input'];
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
  type: Scalars['String']['input'];
};

export type CreateLargeLanguageModelInput = {
  apiKey: Scalars['String']['input'];
  maxComplexity: Scalars['Int']['input'];
  maxConcurrency: InputMaybe<Scalars['Int']['input']>;
  model: Scalars['String']['input'];
  modelAlias: InputMaybe<Scalars['String']['input']>;
  url: Scalars['String']['input'];
};

export type CreateProjectInput = {
  description: InputMaybe<Scalars['String']['input']>;
  name: Scalars['String']['input'];
  repository: InputMaybe<Scalars['String']['input']>;
};

export type DeleteAgentTaskInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteDeliverableInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteLargeLanguageModelInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteProjectInput = {
  id: Scalars['UUID']['input'];
};

export type Deliverable = {
  __typename?: 'Deliverable';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  agentFeedback: Maybe<Scalars['String']['output']>;
  agentTasks: Maybe<Array<Maybe<AgentTask>>>;
  blocking: Maybe<Scalars['String']['output']>;
  deploymentPlan: Maybe<Scalars['String']['output']>;
  description: Maybe<Scalars['String']['output']>;
  executionPlan: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  performanceImpact: Maybe<Scalars['String']['output']>;
  project: Maybe<Project>;
  projectId: Maybe<Scalars['ID']['output']>;
  securityImpact: Maybe<Scalars['String']['output']>;
  status: Maybe<DeliverableStatus>;
  testPlan: Maybe<Scalars['String']['output']>;
  title: Maybe<Scalars['String']['output']>;
  type: Maybe<DeliverableType>;
};

export type DeliverablePayload = {
  __typename?: 'DeliverablePayload';
  deliverable: Maybe<Deliverable>;
  errors: Array<Scalars['String']['output']>;
};

export const DeliverableStatus = {
  DONE: 'DONE',
  DRAFT: 'DRAFT',
  FAILED: 'FAILED',
  IN_PROGRESS: 'IN_PROGRESS',
  NEEDS_REVIEW: 'NEEDS_REVIEW',
  PLANNING: 'PLANNING',
  READY: 'READY',
  REJECTED: 'REJECTED'
} as const;

export type DeliverableStatus = typeof DeliverableStatus[keyof typeof DeliverableStatus];
export const DeliverableType = {
  DEFECT: 'DEFECT',
  FEATURE: 'FEATURE',
  MAINTENANCE: 'MAINTENANCE'
} as const;

export type DeliverableType = typeof DeliverableType[keyof typeof DeliverableType];
export type LargeLanguageModel = {
  __typename?: 'LargeLanguageModel';
  apiKey: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  maxComplexity: Maybe<Scalars['Int']['output']>;
  maxConcurrency: Maybe<Scalars['Int']['output']>;
  model: Maybe<Scalars['String']['output']>;
  modelAlias: Maybe<Scalars['String']['output']>;
  url: Maybe<Scalars['String']['output']>;
};

export type LargeLanguageModelPayload = {
  __typename?: 'LargeLanguageModelPayload';
  errors: Array<Scalars['String']['output']>;
  largeLanguageModel: Maybe<LargeLanguageModel>;
};

export type Mutation = {
  __typename?: 'Mutation';
  cleanupTestData: CleanupTestDataPayload;
  createAgentTask: AgentTaskPayload;
  createDeliverable: DeliverablePayload;
  createLargeLanguageModel: LargeLanguageModelPayload;
  createProject: ProjectPayload;
  deleteAgentTask: AgentTaskPayload;
  deleteDeliverable: DeliverablePayload;
  deleteLargeLanguageModel: LargeLanguageModelPayload;
  deleteProject: ProjectPayload;
  transitionAgentTaskStatus: AgentTaskPayload;
  transitionDeliverableStatus: DeliverablePayload;
  updateAgentTask: AgentTaskPayload;
  updateDeliverable: DeliverablePayload;
  updateLargeLanguageModel: LargeLanguageModelPayload;
  updateProject: ProjectPayload;
};


export type MutationcreateAgentTaskArgs = {
  input: CreateAgentTaskInput;
};


export type MutationcreateDeliverableArgs = {
  input: CreateDeliverableInput;
};


export type MutationcreateLargeLanguageModelArgs = {
  input: CreateLargeLanguageModelInput;
};


export type MutationcreateProjectArgs = {
  input: CreateProjectInput;
};


export type MutationdeleteAgentTaskArgs = {
  input: DeleteAgentTaskInput;
};


export type MutationdeleteDeliverableArgs = {
  input: DeleteDeliverableInput;
};


export type MutationdeleteLargeLanguageModelArgs = {
  input: DeleteLargeLanguageModelInput;
};


export type MutationdeleteProjectArgs = {
  input: DeleteProjectInput;
};


export type MutationtransitionAgentTaskStatusArgs = {
  input: TransitionAgentTaskInput;
};


export type MutationtransitionDeliverableStatusArgs = {
  input: TransitionDeliverableInput;
};


export type MutationupdateAgentTaskArgs = {
  input: UpdateAgentTaskInput;
};


export type MutationupdateDeliverableArgs = {
  input: UpdateDeliverableInput;
};


export type MutationupdateLargeLanguageModelArgs = {
  input: UpdateLargeLanguageModelInput;
};


export type MutationupdateProjectArgs = {
  input: UpdateProjectInput;
};

export type Project = {
  __typename?: 'Project';
  deliverables: Array<Deliverable>;
  description: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  name: Maybe<Scalars['String']['output']>;
  repository: Maybe<Scalars['String']['output']>;
};

export type ProjectConnection = {
  __typename?: 'ProjectConnection';
  nodes: Array<Project>;
  pageInfo: ProjectPageInfo;
  totalCount: Scalars['Int']['output'];
};

export type ProjectPageInfo = {
  __typename?: 'ProjectPageInfo';
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  totalCount: Scalars['Int']['output'];
};

export type ProjectPayload = {
  __typename?: 'ProjectPayload';
  errors: Array<Scalars['String']['output']>;
  project: Maybe<Project>;
};

export type Query = {
  __typename?: 'Query';
  agentTaskById: Maybe<AgentTask>;
  agentTasks: Array<AgentTask>;
  agentTasksByDeliverableId: Array<AgentTask>;
  deliverableById: Maybe<Deliverable>;
  deliverables: Array<Deliverable>;
  deliverablesByProjectId: Array<Deliverable>;
  largeLanguageModelById: Maybe<LargeLanguageModel>;
  largeLanguageModels: Array<LargeLanguageModel>;
  projectById: Maybe<Project>;
  projects: ProjectConnection;
};


export type QueryagentTaskByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryagentTasksArgs = {
  itemId: InputMaybe<Scalars['UUID']['input']>;
};


export type QueryagentTasksByDeliverableIdArgs = {
  deliverableId: Scalars['UUID']['input'];
};


export type QuerydeliverableByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QuerydeliverablesByProjectIdArgs = {
  projectId: Scalars['UUID']['input'];
};


export type QuerylargeLanguageModelByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryprojectByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryprojectsArgs = {
  first?: Scalars['Int']['input'];
  skip: InputMaybe<Scalars['Int']['input']>;
};

export type TransitionAgentTaskInput = {
  actor: Scalars['String']['input'];
  id: Scalars['UUID']['input'];
  targetStatus: AgentTaskStatus;
};

export type TransitionDeliverableInput = {
  actor: Scalars['String']['input'];
  id: Scalars['UUID']['input'];
  targetStatus: DeliverableStatus;
};

export type UpdateAgentTaskInput = {
  agent: InputMaybe<Scalars['String']['input']>;
  commitHash: InputMaybe<Scalars['String']['input']>;
  completionTokens: InputMaybe<Scalars['Int']['input']>;
  complexityRating: InputMaybe<Scalars['Int']['input']>;
  dependsOnAgentTaskId: InputMaybe<Scalars['UUID']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  errors: InputMaybe<Scalars['String']['input']>;
  executionDurationInSeconds: InputMaybe<Scalars['Int']['input']>;
  id: Scalars['UUID']['input'];
  promptTokens: InputMaybe<Scalars['Int']['input']>;
  result: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateDeliverableInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  agentFeedback: InputMaybe<Scalars['String']['input']>;
  blocking: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  executionPlan: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateLargeLanguageModelInput = {
  apiKey: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  maxComplexity: InputMaybe<Scalars['Int']['input']>;
  maxConcurrency: InputMaybe<Scalars['Int']['input']>;
  model: InputMaybe<Scalars['String']['input']>;
  modelAlias: InputMaybe<Scalars['String']['input']>;
  url: InputMaybe<Scalars['String']['input']>;
};

export type UpdateProjectInput = {
  description: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  name: InputMaybe<Scalars['String']['input']>;
  repository: InputMaybe<Scalars['String']['input']>;
};

export type CreateAgentTaskMutationVariables = Exact<{
  input: CreateAgentTaskInput;
}>;


export type CreateAgentTaskMutation = { __typename?: 'Mutation', createAgentTask: { __typename?: 'AgentTaskPayload', errors: Array<string>, agentTask: { __typename?: 'AgentTask', id: string | null, title: string | null, deliverableId: string | null, description: string | null, complexityRating: number | null, result: string | null, errors: string | null, commitHash: string | null, dependsOnAgentTaskId: string | null, promptTokens: number | null, completionTokens: number | null, executionDurationInSeconds: number | null, agent: string | null } | null } };

export type CreateDeliverableMutationVariables = Exact<{
  input: CreateDeliverableInput;
}>;


export type CreateDeliverableMutation = { __typename?: 'Mutation', createDeliverable: { __typename?: 'DeliverablePayload', errors: Array<string>, deliverable: { __typename?: 'Deliverable', id: string | null, title: string | null, status: DeliverableStatus | null } | null } };

export type CreateLargeLanguageModelMutationVariables = Exact<{
  input: CreateLargeLanguageModelInput;
}>;


export type CreateLargeLanguageModelMutation = { __typename?: 'Mutation', createLargeLanguageModel: { __typename?: 'LargeLanguageModelPayload', errors: Array<string>, largeLanguageModel: { __typename?: 'LargeLanguageModel', id: string | null, url: string | null, model: string | null, modelAlias: string | null, maxComplexity: number | null, maxConcurrency: number | null } | null } };

export type CreateProjectMutationVariables = Exact<{
  input: CreateProjectInput;
}>;


export type CreateProjectMutation = { __typename?: 'Mutation', createProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, repository: string | null } | null } };

export type DeleteAgentTaskMutationVariables = Exact<{
  input: DeleteAgentTaskInput;
}>;


export type DeleteAgentTaskMutation = { __typename?: 'Mutation', deleteAgentTask: { __typename?: 'AgentTaskPayload', errors: Array<string>, agentTask: { __typename?: 'AgentTask', id: string | null } | null } };

export type DeleteDeliverableMutationVariables = Exact<{
  input: DeleteDeliverableInput;
}>;


export type DeleteDeliverableMutation = { __typename?: 'Mutation', deleteDeliverable: { __typename?: 'DeliverablePayload', errors: Array<string>, deliverable: { __typename?: 'Deliverable', id: string | null } | null } };

export type DeleteLargeLanguageModelMutationVariables = Exact<{
  input: DeleteLargeLanguageModelInput;
}>;


export type DeleteLargeLanguageModelMutation = { __typename?: 'Mutation', deleteLargeLanguageModel: { __typename?: 'LargeLanguageModelPayload', errors: Array<string>, largeLanguageModel: { __typename?: 'LargeLanguageModel', id: string | null } | null } };

export type DeleteProjectMutationVariables = Exact<{
  input: DeleteProjectInput;
}>;


export type DeleteProjectMutation = { __typename?: 'Mutation', deleteProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null } | null } };

export type TransitionAgentTaskStatusMutationVariables = Exact<{
  input: TransitionAgentTaskInput;
}>;


export type TransitionAgentTaskStatusMutation = { __typename?: 'Mutation', transitionAgentTaskStatus: { __typename?: 'AgentTaskPayload', errors: Array<string>, agentTask: { __typename?: 'AgentTask', id: string | null, title: string | null, status: AgentTaskStatus | null } | null } };

export type TransitionDeliverableStatusMutationVariables = Exact<{
  input: TransitionDeliverableInput;
}>;


export type TransitionDeliverableStatusMutation = { __typename?: 'Mutation', transitionDeliverableStatus: { __typename?: 'DeliverablePayload', errors: Array<string>, deliverable: { __typename?: 'Deliverable', id: string | null, status: DeliverableStatus | null } | null } };

export type UpdateAgentTaskMutationVariables = Exact<{
  input: UpdateAgentTaskInput;
}>;


export type UpdateAgentTaskMutation = { __typename?: 'Mutation', updateAgentTask: { __typename?: 'AgentTaskPayload', errors: Array<string>, agentTask: { __typename?: 'AgentTask', id: string | null, title: string | null, deliverableId: string | null, description: string | null, complexityRating: number | null, result: string | null, errors: string | null, commitHash: string | null, dependsOnAgentTaskId: string | null, promptTokens: number | null, completionTokens: number | null, executionDurationInSeconds: number | null, agent: string | null } | null } };

export type UpdateDeliverableMutationVariables = Exact<{
  input: UpdateDeliverableInput;
}>;


export type UpdateDeliverableMutation = { __typename?: 'Mutation', updateDeliverable: { __typename?: 'DeliverablePayload', errors: Array<string>, deliverable: { __typename?: 'Deliverable', id: string | null, title: string | null, status: DeliverableStatus | null } | null } };

export type UpdateLargeLanguageModelMutationVariables = Exact<{
  input: UpdateLargeLanguageModelInput;
}>;


export type UpdateLargeLanguageModelMutation = { __typename?: 'Mutation', updateLargeLanguageModel: { __typename?: 'LargeLanguageModelPayload', errors: Array<string>, largeLanguageModel: { __typename?: 'LargeLanguageModel', id: string | null, url: string | null, model: string | null, modelAlias: string | null, maxComplexity: number | null, maxConcurrency: number | null } | null } };

export type UpdateProjectMutationVariables = Exact<{
  input: UpdateProjectInput;
}>;


export type UpdateProjectMutation = { __typename?: 'Mutation', updateProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, repository: string | null } | null } };

export type GetAgentTaskByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetAgentTaskByIdQuery = { __typename?: 'Query', agentTaskById: { __typename?: 'AgentTask', id: string | null, title: string | null, status: AgentTaskStatus | null, deliverableId: string | null, description: string | null, result: string | null, errors: string | null, commitHash: string | null, complexityRating: number | null, dependsOnAgentTaskId: string | null, promptTokens: number | null, completionTokens: number | null, executionDurationInSeconds: number | null, agent: string | null } | null };

export type GetAgentTasksQueryVariables = Exact<{
  projectId: InputMaybe<Scalars['UUID']['input']>;
}>;


export type GetAgentTasksQuery = { __typename?: 'Query', agentTasks: Array<{ __typename?: 'AgentTask', id: string | null, title: string | null, status: AgentTaskStatus | null, deliverableId: string | null, description: string | null, result: string | null, errors: string | null, commitHash: string | null, complexityRating: number | null, dependsOnAgentTaskId: string | null, promptTokens: number | null, completionTokens: number | null, executionDurationInSeconds: number | null, agent: string | null }> };

export type GetDeliverablesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDeliverablesQuery = { __typename?: 'Query', deliverables: Array<{ __typename?: 'Deliverable', id: string | null, title: string | null, description: string | null, status: DeliverableStatus | null, type: DeliverableType | null, acceptanceCriteria: string | null, executionPlan: string | null, agentFeedback: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, blocking: string | null }> };

export type GetDeliverableByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDeliverableByIdQuery = { __typename?: 'Query', deliverableById: { __typename?: 'Deliverable', id: string | null, title: string | null, description: string | null, status: DeliverableStatus | null, type: DeliverableType | null, acceptanceCriteria: string | null, executionPlan: string | null, agentFeedback: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, blocking: string | null } | null };

export type ModelConfigurationsQueryVariables = Exact<{ [key: string]: never; }>;


export type ModelConfigurationsQuery = { __typename?: 'Query', largeLanguageModels: Array<{ __typename?: 'LargeLanguageModel', id: string | null, url: string | null, model: string | null, modelAlias: string | null, apiKey: string | null, maxComplexity: number | null, maxConcurrency: number | null }> };

export type GetProjectQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetProjectQuery = { __typename?: 'Query', projectById: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, repository: string | null } | null };

export type GetProjectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetProjectsQuery = { __typename?: 'Query', projects: { __typename?: 'ProjectConnection', nodes: Array<{ __typename?: 'Project', id: string | null, name: string | null, description: string | null, repository: string | null }> } };


export const CreateAgentTaskDocument = gql`
    mutation CreateAgentTask($input: CreateAgentTaskInput!) {
  createAgentTask(input: $input) {
    agentTask {
      id
      title
      deliverableId
      description
      complexityRating
      result
      errors
      commitHash
      dependsOnAgentTaskId
      promptTokens
      completionTokens
      executionDurationInSeconds
      agent
    }
    errors
  }
}
    `;
export type CreateAgentTaskMutationFn = Apollo.MutationFunction<CreateAgentTaskMutation, CreateAgentTaskMutationVariables>;

/**
 * __useCreateAgentTaskMutation__
 *
 * To run a mutation, you first call `useCreateAgentTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateAgentTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createAgentTaskMutation, { data, loading, error }] = useCreateAgentTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateAgentTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateAgentTaskMutation, CreateAgentTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateAgentTaskMutation, CreateAgentTaskMutationVariables>(CreateAgentTaskDocument, options);
      }
export type CreateAgentTaskMutationHookResult = ReturnType<typeof useCreateAgentTaskMutation>;
export type CreateAgentTaskMutationResult = Apollo.MutationResult<CreateAgentTaskMutation>;
export type CreateAgentTaskMutationOptions = Apollo.BaseMutationOptions<CreateAgentTaskMutation, CreateAgentTaskMutationVariables>;
export const CreateDeliverableDocument = gql`
    mutation CreateDeliverable($input: CreateDeliverableInput!) {
  createDeliverable(input: $input) {
    deliverable {
      id
      title
      status
    }
    errors
  }
}
    `;
export type CreateDeliverableMutationFn = Apollo.MutationFunction<CreateDeliverableMutation, CreateDeliverableMutationVariables>;

/**
 * __useCreateDeliverableMutation__
 *
 * To run a mutation, you first call `useCreateDeliverableMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateDeliverableMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createDeliverableMutation, { data, loading, error }] = useCreateDeliverableMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateDeliverableMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateDeliverableMutation, CreateDeliverableMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateDeliverableMutation, CreateDeliverableMutationVariables>(CreateDeliverableDocument, options);
      }
export type CreateDeliverableMutationHookResult = ReturnType<typeof useCreateDeliverableMutation>;
export type CreateDeliverableMutationResult = Apollo.MutationResult<CreateDeliverableMutation>;
export type CreateDeliverableMutationOptions = Apollo.BaseMutationOptions<CreateDeliverableMutation, CreateDeliverableMutationVariables>;
export const CreateLargeLanguageModelDocument = gql`
    mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) {
  createLargeLanguageModel(input: $input) {
    largeLanguageModel {
      id
      url
      model
      modelAlias
      maxComplexity
      maxConcurrency
    }
    errors
  }
}
    `;
export type CreateLargeLanguageModelMutationFn = Apollo.MutationFunction<CreateLargeLanguageModelMutation, CreateLargeLanguageModelMutationVariables>;

/**
 * __useCreateLargeLanguageModelMutation__
 *
 * To run a mutation, you first call `useCreateLargeLanguageModelMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateLargeLanguageModelMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createLargeLanguageModelMutation, { data, loading, error }] = useCreateLargeLanguageModelMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateLargeLanguageModelMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateLargeLanguageModelMutation, CreateLargeLanguageModelMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateLargeLanguageModelMutation, CreateLargeLanguageModelMutationVariables>(CreateLargeLanguageModelDocument, options);
      }
export type CreateLargeLanguageModelMutationHookResult = ReturnType<typeof useCreateLargeLanguageModelMutation>;
export type CreateLargeLanguageModelMutationResult = Apollo.MutationResult<CreateLargeLanguageModelMutation>;
export type CreateLargeLanguageModelMutationOptions = Apollo.BaseMutationOptions<CreateLargeLanguageModelMutation, CreateLargeLanguageModelMutationVariables>;
export const CreateProjectDocument = gql`
    mutation CreateProject($input: CreateProjectInput!) {
  createProject(input: $input) {
    project {
      id
      name
      description
      repository
    }
    errors
  }
}
    `;
export type CreateProjectMutationFn = Apollo.MutationFunction<CreateProjectMutation, CreateProjectMutationVariables>;

/**
 * __useCreateProjectMutation__
 *
 * To run a mutation, you first call `useCreateProjectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateProjectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createProjectMutation, { data, loading, error }] = useCreateProjectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateProjectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateProjectMutation, CreateProjectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateProjectMutation, CreateProjectMutationVariables>(CreateProjectDocument, options);
      }
export type CreateProjectMutationHookResult = ReturnType<typeof useCreateProjectMutation>;
export type CreateProjectMutationResult = Apollo.MutationResult<CreateProjectMutation>;
export type CreateProjectMutationOptions = Apollo.BaseMutationOptions<CreateProjectMutation, CreateProjectMutationVariables>;
export const DeleteAgentTaskDocument = gql`
    mutation DeleteAgentTask($input: DeleteAgentTaskInput!) {
  deleteAgentTask(input: $input) {
    agentTask {
      id
    }
    errors
  }
}
    `;
export type DeleteAgentTaskMutationFn = Apollo.MutationFunction<DeleteAgentTaskMutation, DeleteAgentTaskMutationVariables>;

/**
 * __useDeleteAgentTaskMutation__
 *
 * To run a mutation, you first call `useDeleteAgentTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteAgentTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteAgentTaskMutation, { data, loading, error }] = useDeleteAgentTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteAgentTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteAgentTaskMutation, DeleteAgentTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteAgentTaskMutation, DeleteAgentTaskMutationVariables>(DeleteAgentTaskDocument, options);
      }
export type DeleteAgentTaskMutationHookResult = ReturnType<typeof useDeleteAgentTaskMutation>;
export type DeleteAgentTaskMutationResult = Apollo.MutationResult<DeleteAgentTaskMutation>;
export type DeleteAgentTaskMutationOptions = Apollo.BaseMutationOptions<DeleteAgentTaskMutation, DeleteAgentTaskMutationVariables>;
export const DeleteDeliverableDocument = gql`
    mutation DeleteDeliverable($input: DeleteDeliverableInput!) {
  deleteDeliverable(input: $input) {
    deliverable {
      id
    }
    errors
  }
}
    `;
export type DeleteDeliverableMutationFn = Apollo.MutationFunction<DeleteDeliverableMutation, DeleteDeliverableMutationVariables>;

/**
 * __useDeleteDeliverableMutation__
 *
 * To run a mutation, you first call `useDeleteDeliverableMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteDeliverableMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteDeliverableMutation, { data, loading, error }] = useDeleteDeliverableMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteDeliverableMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteDeliverableMutation, DeleteDeliverableMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteDeliverableMutation, DeleteDeliverableMutationVariables>(DeleteDeliverableDocument, options);
      }
export type DeleteDeliverableMutationHookResult = ReturnType<typeof useDeleteDeliverableMutation>;
export type DeleteDeliverableMutationResult = Apollo.MutationResult<DeleteDeliverableMutation>;
export type DeleteDeliverableMutationOptions = Apollo.BaseMutationOptions<DeleteDeliverableMutation, DeleteDeliverableMutationVariables>;
export const DeleteLargeLanguageModelDocument = gql`
    mutation DeleteLargeLanguageModel($input: DeleteLargeLanguageModelInput!) {
  deleteLargeLanguageModel(input: $input) {
    largeLanguageModel {
      id
    }
    errors
  }
}
    `;
export type DeleteLargeLanguageModelMutationFn = Apollo.MutationFunction<DeleteLargeLanguageModelMutation, DeleteLargeLanguageModelMutationVariables>;

/**
 * __useDeleteLargeLanguageModelMutation__
 *
 * To run a mutation, you first call `useDeleteLargeLanguageModelMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteLargeLanguageModelMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteLargeLanguageModelMutation, { data, loading, error }] = useDeleteLargeLanguageModelMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteLargeLanguageModelMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteLargeLanguageModelMutation, DeleteLargeLanguageModelMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteLargeLanguageModelMutation, DeleteLargeLanguageModelMutationVariables>(DeleteLargeLanguageModelDocument, options);
      }
export type DeleteLargeLanguageModelMutationHookResult = ReturnType<typeof useDeleteLargeLanguageModelMutation>;
export type DeleteLargeLanguageModelMutationResult = Apollo.MutationResult<DeleteLargeLanguageModelMutation>;
export type DeleteLargeLanguageModelMutationOptions = Apollo.BaseMutationOptions<DeleteLargeLanguageModelMutation, DeleteLargeLanguageModelMutationVariables>;
export const DeleteProjectDocument = gql`
    mutation DeleteProject($input: DeleteProjectInput!) {
  deleteProject(input: $input) {
    project {
      id
    }
    errors
  }
}
    `;
export type DeleteProjectMutationFn = Apollo.MutationFunction<DeleteProjectMutation, DeleteProjectMutationVariables>;

/**
 * __useDeleteProjectMutation__
 *
 * To run a mutation, you first call `useDeleteProjectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteProjectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteProjectMutation, { data, loading, error }] = useDeleteProjectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteProjectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteProjectMutation, DeleteProjectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteProjectMutation, DeleteProjectMutationVariables>(DeleteProjectDocument, options);
      }
export type DeleteProjectMutationHookResult = ReturnType<typeof useDeleteProjectMutation>;
export type DeleteProjectMutationResult = Apollo.MutationResult<DeleteProjectMutation>;
export type DeleteProjectMutationOptions = Apollo.BaseMutationOptions<DeleteProjectMutation, DeleteProjectMutationVariables>;
export const TransitionAgentTaskStatusDocument = gql`
    mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) {
  transitionAgentTaskStatus(input: $input) {
    agentTask {
      id
      title
      status
    }
    errors
  }
}
    `;
export type TransitionAgentTaskStatusMutationFn = Apollo.MutationFunction<TransitionAgentTaskStatusMutation, TransitionAgentTaskStatusMutationVariables>;

/**
 * __useTransitionAgentTaskStatusMutation__
 *
 * To run a mutation, you first call `useTransitionAgentTaskStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useTransitionAgentTaskStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [transitionAgentTaskStatusMutation, { data, loading, error }] = useTransitionAgentTaskStatusMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useTransitionAgentTaskStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<TransitionAgentTaskStatusMutation, TransitionAgentTaskStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<TransitionAgentTaskStatusMutation, TransitionAgentTaskStatusMutationVariables>(TransitionAgentTaskStatusDocument, options);
      }
export type TransitionAgentTaskStatusMutationHookResult = ReturnType<typeof useTransitionAgentTaskStatusMutation>;
export type TransitionAgentTaskStatusMutationResult = Apollo.MutationResult<TransitionAgentTaskStatusMutation>;
export type TransitionAgentTaskStatusMutationOptions = Apollo.BaseMutationOptions<TransitionAgentTaskStatusMutation, TransitionAgentTaskStatusMutationVariables>;
export const TransitionDeliverableStatusDocument = gql`
    mutation TransitionDeliverableStatus($input: TransitionDeliverableInput!) {
  transitionDeliverableStatus(input: $input) {
    deliverable {
      id
      status
    }
    errors
  }
}
    `;
export type TransitionDeliverableStatusMutationFn = Apollo.MutationFunction<TransitionDeliverableStatusMutation, TransitionDeliverableStatusMutationVariables>;

/**
 * __useTransitionDeliverableStatusMutation__
 *
 * To run a mutation, you first call `useTransitionDeliverableStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useTransitionDeliverableStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [transitionDeliverableStatusMutation, { data, loading, error }] = useTransitionDeliverableStatusMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useTransitionDeliverableStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<TransitionDeliverableStatusMutation, TransitionDeliverableStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<TransitionDeliverableStatusMutation, TransitionDeliverableStatusMutationVariables>(TransitionDeliverableStatusDocument, options);
      }
export type TransitionDeliverableStatusMutationHookResult = ReturnType<typeof useTransitionDeliverableStatusMutation>;
export type TransitionDeliverableStatusMutationResult = Apollo.MutationResult<TransitionDeliverableStatusMutation>;
export type TransitionDeliverableStatusMutationOptions = Apollo.BaseMutationOptions<TransitionDeliverableStatusMutation, TransitionDeliverableStatusMutationVariables>;
export const UpdateAgentTaskDocument = gql`
    mutation UpdateAgentTask($input: UpdateAgentTaskInput!) {
  updateAgentTask(input: $input) {
    agentTask {
      id
      title
      deliverableId
      description
      complexityRating
      result
      errors
      commitHash
      dependsOnAgentTaskId
      promptTokens
      completionTokens
      executionDurationInSeconds
      agent
    }
    errors
  }
}
    `;
export type UpdateAgentTaskMutationFn = Apollo.MutationFunction<UpdateAgentTaskMutation, UpdateAgentTaskMutationVariables>;

/**
 * __useUpdateAgentTaskMutation__
 *
 * To run a mutation, you first call `useUpdateAgentTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateAgentTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateAgentTaskMutation, { data, loading, error }] = useUpdateAgentTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateAgentTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateAgentTaskMutation, UpdateAgentTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateAgentTaskMutation, UpdateAgentTaskMutationVariables>(UpdateAgentTaskDocument, options);
      }
export type UpdateAgentTaskMutationHookResult = ReturnType<typeof useUpdateAgentTaskMutation>;
export type UpdateAgentTaskMutationResult = Apollo.MutationResult<UpdateAgentTaskMutation>;
export type UpdateAgentTaskMutationOptions = Apollo.BaseMutationOptions<UpdateAgentTaskMutation, UpdateAgentTaskMutationVariables>;
export const UpdateDeliverableDocument = gql`
    mutation UpdateDeliverable($input: UpdateDeliverableInput!) {
  updateDeliverable(input: $input) {
    deliverable {
      id
      title
      status
    }
    errors
  }
}
    `;
export type UpdateDeliverableMutationFn = Apollo.MutationFunction<UpdateDeliverableMutation, UpdateDeliverableMutationVariables>;

/**
 * __useUpdateDeliverableMutation__
 *
 * To run a mutation, you first call `useUpdateDeliverableMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateDeliverableMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateDeliverableMutation, { data, loading, error }] = useUpdateDeliverableMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateDeliverableMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateDeliverableMutation, UpdateDeliverableMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateDeliverableMutation, UpdateDeliverableMutationVariables>(UpdateDeliverableDocument, options);
      }
export type UpdateDeliverableMutationHookResult = ReturnType<typeof useUpdateDeliverableMutation>;
export type UpdateDeliverableMutationResult = Apollo.MutationResult<UpdateDeliverableMutation>;
export type UpdateDeliverableMutationOptions = Apollo.BaseMutationOptions<UpdateDeliverableMutation, UpdateDeliverableMutationVariables>;
export const UpdateLargeLanguageModelDocument = gql`
    mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) {
  updateLargeLanguageModel(input: $input) {
    largeLanguageModel {
      id
      url
      model
      modelAlias
      maxComplexity
      maxConcurrency
    }
    errors
  }
}
    `;
export type UpdateLargeLanguageModelMutationFn = Apollo.MutationFunction<UpdateLargeLanguageModelMutation, UpdateLargeLanguageModelMutationVariables>;

/**
 * __useUpdateLargeLanguageModelMutation__
 *
 * To run a mutation, you first call `useUpdateLargeLanguageModelMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateLargeLanguageModelMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateLargeLanguageModelMutation, { data, loading, error }] = useUpdateLargeLanguageModelMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateLargeLanguageModelMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateLargeLanguageModelMutation, UpdateLargeLanguageModelMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateLargeLanguageModelMutation, UpdateLargeLanguageModelMutationVariables>(UpdateLargeLanguageModelDocument, options);
      }
export type UpdateLargeLanguageModelMutationHookResult = ReturnType<typeof useUpdateLargeLanguageModelMutation>;
export type UpdateLargeLanguageModelMutationResult = Apollo.MutationResult<UpdateLargeLanguageModelMutation>;
export type UpdateLargeLanguageModelMutationOptions = Apollo.BaseMutationOptions<UpdateLargeLanguageModelMutation, UpdateLargeLanguageModelMutationVariables>;
export const UpdateProjectDocument = gql`
    mutation UpdateProject($input: UpdateProjectInput!) {
  updateProject(input: $input) {
    project {
      id
      name
      description
      repository
    }
    errors
  }
}
    `;
export type UpdateProjectMutationFn = Apollo.MutationFunction<UpdateProjectMutation, UpdateProjectMutationVariables>;

/**
 * __useUpdateProjectMutation__
 *
 * To run a mutation, you first call `useUpdateProjectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateProjectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateProjectMutation, { data, loading, error }] = useUpdateProjectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateProjectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateProjectMutation, UpdateProjectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateProjectMutation, UpdateProjectMutationVariables>(UpdateProjectDocument, options);
      }
export type UpdateProjectMutationHookResult = ReturnType<typeof useUpdateProjectMutation>;
export type UpdateProjectMutationResult = Apollo.MutationResult<UpdateProjectMutation>;
export type UpdateProjectMutationOptions = Apollo.BaseMutationOptions<UpdateProjectMutation, UpdateProjectMutationVariables>;
export const GetAgentTaskByIdDocument = gql`
    query GetAgentTaskById($id: UUID!) {
  agentTaskById(id: $id) {
    id
    title
    status
    deliverableId
    description
    result
    errors
    commitHash
    complexityRating
    dependsOnAgentTaskId
    promptTokens
    completionTokens
    executionDurationInSeconds
    agent
  }
}
    `;

/**
 * __useGetAgentTaskByIdQuery__
 *
 * To run a query within a React component, call `useGetAgentTaskByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetAgentTaskByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetAgentTaskByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetAgentTaskByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables> & ({ variables: GetAgentTaskByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>(GetAgentTaskByIdDocument, options);
      }
export function useGetAgentTaskByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>(GetAgentTaskByIdDocument, options);
        }
// @ts-ignore
export function useGetAgentTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>;
export function useGetAgentTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTaskByIdQuery | undefined, GetAgentTaskByIdQueryVariables>;
export function useGetAgentTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>(GetAgentTaskByIdDocument, options);
        }
export type GetAgentTaskByIdQueryHookResult = ReturnType<typeof useGetAgentTaskByIdQuery>;
export type GetAgentTaskByIdLazyQueryHookResult = ReturnType<typeof useGetAgentTaskByIdLazyQuery>;
export type GetAgentTaskByIdSuspenseQueryHookResult = ReturnType<typeof useGetAgentTaskByIdSuspenseQuery>;
export type GetAgentTaskByIdQueryResult = Apollo.QueryResult<GetAgentTaskByIdQuery, GetAgentTaskByIdQueryVariables>;
export const GetAgentTasksDocument = gql`
    query GetAgentTasks($projectId: UUID) {
  agentTasks(itemId: $projectId) {
    id
    title
    status
    deliverableId
    description
    result
    errors
    commitHash
    complexityRating
    dependsOnAgentTaskId
    promptTokens
    completionTokens
    executionDurationInSeconds
    agent
  }
}
    `;

/**
 * __useGetAgentTasksQuery__
 *
 * To run a query within a React component, call `useGetAgentTasksQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetAgentTasksQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetAgentTasksQuery({
 *   variables: {
 *      projectId: // value for 'projectId'
 *   },
 * });
 */
export function useGetAgentTasksQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetAgentTasksQuery, GetAgentTasksQueryVariables>(GetAgentTasksDocument, options);
      }
export function useGetAgentTasksLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetAgentTasksQuery, GetAgentTasksQueryVariables>(GetAgentTasksDocument, options);
        }
// @ts-ignore
export function useGetAgentTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTasksQuery, GetAgentTasksQueryVariables>;
export function useGetAgentTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTasksQuery | undefined, GetAgentTasksQueryVariables>;
export function useGetAgentTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetAgentTasksQuery, GetAgentTasksQueryVariables>(GetAgentTasksDocument, options);
        }
export type GetAgentTasksQueryHookResult = ReturnType<typeof useGetAgentTasksQuery>;
export type GetAgentTasksLazyQueryHookResult = ReturnType<typeof useGetAgentTasksLazyQuery>;
export type GetAgentTasksSuspenseQueryHookResult = ReturnType<typeof useGetAgentTasksSuspenseQuery>;
export type GetAgentTasksQueryResult = Apollo.QueryResult<GetAgentTasksQuery, GetAgentTasksQueryVariables>;
export const GetDeliverablesDocument = gql`
    query GetDeliverables {
  deliverables {
    id
    title
    description
    status
    type
    acceptanceCriteria
    executionPlan
    agentFeedback
    securityImpact
    performanceImpact
    testPlan
    deploymentPlan
    blocking
  }
}
    `;

/**
 * __useGetDeliverablesQuery__
 *
 * To run a query within a React component, call `useGetDeliverablesQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDeliverablesQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDeliverablesQuery({
 *   variables: {
 *   },
 * });
 */
export function useGetDeliverablesQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetDeliverablesQuery, GetDeliverablesQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDeliverablesQuery, GetDeliverablesQueryVariables>(GetDeliverablesDocument, options);
      }
export function useGetDeliverablesLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDeliverablesQuery, GetDeliverablesQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDeliverablesQuery, GetDeliverablesQueryVariables>(GetDeliverablesDocument, options);
        }
// @ts-ignore
export function useGetDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesQuery, GetDeliverablesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverablesQuery, GetDeliverablesQueryVariables>;
export function useGetDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesQuery, GetDeliverablesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverablesQuery | undefined, GetDeliverablesQueryVariables>;
export function useGetDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesQuery, GetDeliverablesQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDeliverablesQuery, GetDeliverablesQueryVariables>(GetDeliverablesDocument, options);
        }
export type GetDeliverablesQueryHookResult = ReturnType<typeof useGetDeliverablesQuery>;
export type GetDeliverablesLazyQueryHookResult = ReturnType<typeof useGetDeliverablesLazyQuery>;
export type GetDeliverablesSuspenseQueryHookResult = ReturnType<typeof useGetDeliverablesSuspenseQuery>;
export type GetDeliverablesQueryResult = Apollo.QueryResult<GetDeliverablesQuery, GetDeliverablesQueryVariables>;
export const GetDeliverableByIdDocument = gql`
    query GetDeliverableById($id: UUID!) {
  deliverableById(id: $id) {
    id
    title
    description
    status
    type
    acceptanceCriteria
    executionPlan
    agentFeedback
    securityImpact
    performanceImpact
    testPlan
    deploymentPlan
    blocking
  }
}
    `;

/**
 * __useGetDeliverableByIdQuery__
 *
 * To run a query within a React component, call `useGetDeliverableByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDeliverableByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDeliverableByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetDeliverableByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables> & ({ variables: GetDeliverableByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>(GetDeliverableByIdDocument, options);
      }
export function useGetDeliverableByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>(GetDeliverableByIdDocument, options);
        }
// @ts-ignore
export function useGetDeliverableByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>;
export function useGetDeliverableByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverableByIdQuery | undefined, GetDeliverableByIdQueryVariables>;
export function useGetDeliverableByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>(GetDeliverableByIdDocument, options);
        }
export type GetDeliverableByIdQueryHookResult = ReturnType<typeof useGetDeliverableByIdQuery>;
export type GetDeliverableByIdLazyQueryHookResult = ReturnType<typeof useGetDeliverableByIdLazyQuery>;
export type GetDeliverableByIdSuspenseQueryHookResult = ReturnType<typeof useGetDeliverableByIdSuspenseQuery>;
export type GetDeliverableByIdQueryResult = Apollo.QueryResult<GetDeliverableByIdQuery, GetDeliverableByIdQueryVariables>;
export const ModelConfigurationsDocument = gql`
    query ModelConfigurations {
  largeLanguageModels {
    id
    url
    model
    modelAlias
    apiKey
    maxComplexity
    maxConcurrency
  }
}
    `;

/**
 * __useModelConfigurationsQuery__
 *
 * To run a query within a React component, call `useModelConfigurationsQuery` and pass it any options that fit your needs.
 * When your component renders, `useModelConfigurationsQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useModelConfigurationsQuery({
 *   variables: {
 *   },
 * });
 */
export function useModelConfigurationsQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>(ModelConfigurationsDocument, options);
      }
export function useModelConfigurationsLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>(ModelConfigurationsDocument, options);
        }
// @ts-ignore
export function useModelConfigurationsSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>;
export function useModelConfigurationsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<ModelConfigurationsQuery | undefined, ModelConfigurationsQueryVariables>;
export function useModelConfigurationsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>(ModelConfigurationsDocument, options);
        }
export type ModelConfigurationsQueryHookResult = ReturnType<typeof useModelConfigurationsQuery>;
export type ModelConfigurationsLazyQueryHookResult = ReturnType<typeof useModelConfigurationsLazyQuery>;
export type ModelConfigurationsSuspenseQueryHookResult = ReturnType<typeof useModelConfigurationsSuspenseQuery>;
export type ModelConfigurationsQueryResult = Apollo.QueryResult<ModelConfigurationsQuery, ModelConfigurationsQueryVariables>;
export const GetProjectDocument = gql`
    query GetProject($id: UUID!) {
  projectById(id: $id) {
    id
    name
    description
    repository
  }
}
    `;

/**
 * __useGetProjectQuery__
 *
 * To run a query within a React component, call `useGetProjectQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetProjectQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetProjectQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetProjectQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetProjectQuery, GetProjectQueryVariables> & ({ variables: GetProjectQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetProjectQuery, GetProjectQueryVariables>(GetProjectDocument, options);
      }
export function useGetProjectLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetProjectQuery, GetProjectQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetProjectQuery, GetProjectQueryVariables>(GetProjectDocument, options);
        }
// @ts-ignore
export function useGetProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetProjectQuery, GetProjectQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetProjectQuery, GetProjectQueryVariables>;
export function useGetProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetProjectQuery, GetProjectQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetProjectQuery | undefined, GetProjectQueryVariables>;
export function useGetProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetProjectQuery, GetProjectQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetProjectQuery, GetProjectQueryVariables>(GetProjectDocument, options);
        }
export type GetProjectQueryHookResult = ReturnType<typeof useGetProjectQuery>;
export type GetProjectLazyQueryHookResult = ReturnType<typeof useGetProjectLazyQuery>;
export type GetProjectSuspenseQueryHookResult = ReturnType<typeof useGetProjectSuspenseQuery>;
export type GetProjectQueryResult = Apollo.QueryResult<GetProjectQuery, GetProjectQueryVariables>;
export const GetProjectsDocument = gql`
    query GetProjects {
  projects {
    nodes {
      id
      name
      description
      repository
    }
  }
}
    `;

/**
 * __useGetProjectsQuery__
 *
 * To run a query within a React component, call `useGetProjectsQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetProjectsQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetProjectsQuery({
 *   variables: {
 *   },
 * });
 */
export function useGetProjectsQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetProjectsQuery, GetProjectsQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetProjectsQuery, GetProjectsQueryVariables>(GetProjectsDocument, options);
      }
export function useGetProjectsLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetProjectsQuery, GetProjectsQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetProjectsQuery, GetProjectsQueryVariables>(GetProjectsDocument, options);
        }
// @ts-ignore
export function useGetProjectsSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetProjectsQuery, GetProjectsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetProjectsQuery, GetProjectsQueryVariables>;
export function useGetProjectsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetProjectsQuery, GetProjectsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetProjectsQuery | undefined, GetProjectsQueryVariables>;
export function useGetProjectsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetProjectsQuery, GetProjectsQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetProjectsQuery, GetProjectsQueryVariables>(GetProjectsDocument, options);
        }
export type GetProjectsQueryHookResult = ReturnType<typeof useGetProjectsQuery>;
export type GetProjectsLazyQueryHookResult = ReturnType<typeof useGetProjectsLazyQuery>;
export type GetProjectsSuspenseQueryHookResult = ReturnType<typeof useGetProjectsSuspenseQuery>;
export type GetProjectsQueryResult = Apollo.QueryResult<GetProjectsQuery, GetProjectsQueryVariables>;