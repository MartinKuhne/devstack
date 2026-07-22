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
  agent?: Maybe<Scalars['String']['output']>;
  commitHash?: Maybe<Scalars['String']['output']>;
  completionTokens?: Maybe<Scalars['Int']['output']>;
  complexityRating: Scalars['Int']['output'];
  deliverable?: Maybe<Deliverable>;
  deliverableId: Scalars['UUID']['output'];
  dependsOnAgentTask?: Maybe<AgentTask>;
  dependsOnAgentTaskId?: Maybe<Scalars['UUID']['output']>;
  description: Scalars['String']['output'];
  errors?: Maybe<Scalars['String']['output']>;
  executionDurationInSeconds?: Maybe<Scalars['Int']['output']>;
  id: Scalars['UUID']['output'];
  project?: Maybe<Project>;
  projectId: Scalars['UUID']['output'];
  promptTokens?: Maybe<Scalars['Int']['output']>;
  result?: Maybe<Scalars['String']['output']>;
  status: AgentTaskStatus;
  title: Scalars['String']['output'];
};

export type AgentTaskFilterInput = {
  agent?: InputMaybe<StringOperationFilterInput>;
  and?: InputMaybe<Array<AgentTaskFilterInput>>;
  commitHash?: InputMaybe<StringOperationFilterInput>;
  completionTokens?: InputMaybe<IntOperationFilterInput>;
  complexityRating?: InputMaybe<IntOperationFilterInput>;
  deliverable?: InputMaybe<DeliverableFilterInput>;
  deliverableId?: InputMaybe<UuidOperationFilterInput>;
  dependsOnAgentTask?: InputMaybe<AgentTaskFilterInput>;
  dependsOnAgentTaskId?: InputMaybe<UuidOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  errors?: InputMaybe<StringOperationFilterInput>;
  executionDurationInSeconds?: InputMaybe<IntOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  or?: InputMaybe<Array<AgentTaskFilterInput>>;
  project?: InputMaybe<ProjectFilterInput>;
  projectId?: InputMaybe<UuidOperationFilterInput>;
  promptTokens?: InputMaybe<IntOperationFilterInput>;
  result?: InputMaybe<StringOperationFilterInput>;
  status?: InputMaybe<AgentTaskStatusOperationFilterInput>;
  title?: InputMaybe<StringOperationFilterInput>;
};

export type AgentTaskSortInput = {
  agent?: InputMaybe<SortEnumType>;
  commitHash?: InputMaybe<SortEnumType>;
  completionTokens?: InputMaybe<SortEnumType>;
  complexityRating?: InputMaybe<SortEnumType>;
  deliverable?: InputMaybe<DeliverableSortInput>;
  deliverableId?: InputMaybe<SortEnumType>;
  dependsOnAgentTask?: InputMaybe<AgentTaskSortInput>;
  dependsOnAgentTaskId?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  errors?: InputMaybe<SortEnumType>;
  executionDurationInSeconds?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  project?: InputMaybe<ProjectSortInput>;
  projectId?: InputMaybe<SortEnumType>;
  promptTokens?: InputMaybe<SortEnumType>;
  result?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  title?: InputMaybe<SortEnumType>;
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
export type AgentTaskStatusOperationFilterInput = {
  eq?: InputMaybe<AgentTaskStatus>;
  in?: InputMaybe<Array<AgentTaskStatus>>;
  neq?: InputMaybe<AgentTaskStatus>;
  nin?: InputMaybe<Array<AgentTaskStatus>>;
};

/** A connection to a list of items. */
export type AgentTasksConnection = {
  __typename?: 'AgentTasksConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AgentTasksEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<AgentTask>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type AgentTasksEdge = {
  __typename?: 'AgentTasksEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: AgentTask;
};

export type CreateAgentTaskInput = {
  complexityRating?: Scalars['Int']['input'];
  deliverableId: Scalars['UUID']['input'];
  dependsOnAgentTaskId?: InputMaybe<Scalars['UUID']['input']>;
  description: Scalars['String']['input'];
  projectId: Scalars['UUID']['input'];
  title: Scalars['String']['input'];
};

export type CreateDeliverableInput = {
  acceptanceCriteria?: InputMaybe<Scalars['String']['input']>;
  deploymentPlan?: InputMaybe<Scalars['String']['input']>;
  description: Scalars['String']['input'];
  design?: InputMaybe<Scalars['String']['input']>;
  executionPlan?: InputMaybe<Scalars['String']['input']>;
  initialStatus: DeliverableStatus;
  performanceImpact?: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['UUID']['input'];
  securityImpact?: InputMaybe<Scalars['String']['input']>;
  testPlan?: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
  type: Scalars['String']['input'];
};

export type CreateLargeLanguageModelInput = {
  apiKey?: InputMaybe<Scalars['String']['input']>;
  cost?: Scalars['Int']['input'];
  maxComplexity?: Scalars['Int']['input'];
  maxConcurrency?: Scalars['Int']['input'];
  model: Scalars['String']['input'];
  modelAlias?: InputMaybe<Scalars['String']['input']>;
  url: Scalars['String']['input'];
};

export type CreateProjectInput = {
  description?: InputMaybe<Scalars['String']['input']>;
  name: Scalars['String']['input'];
  repository: Scalars['String']['input'];
};

export type DeleteTestDataPayload = {
  __typename?: 'DeleteTestDataPayload';
  message?: Maybe<Scalars['String']['output']>;
  success: Scalars['Boolean']['output'];
};

export type Deliverable = {
  __typename?: 'Deliverable';
  acceptanceCriteria?: Maybe<Scalars['String']['output']>;
  agentFeedback?: Maybe<Scalars['String']['output']>;
  agentTasks: Array<AgentTask>;
  blocking?: Maybe<Scalars['String']['output']>;
  deploymentPlan?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  design?: Maybe<Scalars['String']['output']>;
  executionPlan?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  performanceImpact?: Maybe<Scalars['String']['output']>;
  project?: Maybe<Project>;
  projectId: Scalars['UUID']['output'];
  securityImpact?: Maybe<Scalars['String']['output']>;
  status: DeliverableStatus;
  testPlan?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  type: DeliverableType;
};

export type DeliverableFilterInput = {
  acceptanceCriteria?: InputMaybe<StringOperationFilterInput>;
  agentFeedback?: InputMaybe<StringOperationFilterInput>;
  agentTasks?: InputMaybe<ListFilterInputTypeOfAgentTaskFilterInput>;
  and?: InputMaybe<Array<DeliverableFilterInput>>;
  blocking?: InputMaybe<StringOperationFilterInput>;
  deploymentPlan?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  design?: InputMaybe<StringOperationFilterInput>;
  executionPlan?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  or?: InputMaybe<Array<DeliverableFilterInput>>;
  performanceImpact?: InputMaybe<StringOperationFilterInput>;
  project?: InputMaybe<ProjectFilterInput>;
  projectId?: InputMaybe<UuidOperationFilterInput>;
  securityImpact?: InputMaybe<StringOperationFilterInput>;
  status?: InputMaybe<DeliverableStatusOperationFilterInput>;
  testPlan?: InputMaybe<StringOperationFilterInput>;
  title?: InputMaybe<StringOperationFilterInput>;
  type?: InputMaybe<DeliverableTypeOperationFilterInput>;
};

export type DeliverableSortInput = {
  acceptanceCriteria?: InputMaybe<SortEnumType>;
  agentFeedback?: InputMaybe<SortEnumType>;
  blocking?: InputMaybe<SortEnumType>;
  deploymentPlan?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  design?: InputMaybe<SortEnumType>;
  executionPlan?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  performanceImpact?: InputMaybe<SortEnumType>;
  project?: InputMaybe<ProjectSortInput>;
  projectId?: InputMaybe<SortEnumType>;
  securityImpact?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  testPlan?: InputMaybe<SortEnumType>;
  title?: InputMaybe<SortEnumType>;
  type?: InputMaybe<SortEnumType>;
};

export const DeliverableStatus = {
  DEPLOY: 'DEPLOY',
  DESIGN: 'DESIGN',
  DONE: 'DONE',
  DRAFT: 'DRAFT',
  FAILED: 'FAILED',
  IMPLEMENT: 'IMPLEMENT',
  MERGE: 'MERGE',
  NEEDS_REVIEW: 'NEEDS_REVIEW',
  PLAN: 'PLAN',
  REJECTED: 'REJECTED',
  TEST: 'TEST'
} as const;

export type DeliverableStatus = typeof DeliverableStatus[keyof typeof DeliverableStatus];
export type DeliverableStatusOperationFilterInput = {
  eq?: InputMaybe<DeliverableStatus>;
  in?: InputMaybe<Array<DeliverableStatus>>;
  neq?: InputMaybe<DeliverableStatus>;
  nin?: InputMaybe<Array<DeliverableStatus>>;
};

export const DeliverableType = {
  DEFECT: 'DEFECT',
  FEATURE: 'FEATURE',
  MAINTENANCE: 'MAINTENANCE',
  SPIKE: 'SPIKE'
} as const;

export type DeliverableType = typeof DeliverableType[keyof typeof DeliverableType];
export type DeliverableTypeOperationFilterInput = {
  eq?: InputMaybe<DeliverableType>;
  in?: InputMaybe<Array<DeliverableType>>;
  neq?: InputMaybe<DeliverableType>;
  nin?: InputMaybe<Array<DeliverableType>>;
};

/** A connection to a list of items. */
export type DeliverablesConnection = {
  __typename?: 'DeliverablesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<DeliverablesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Deliverable>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type DeliverablesEdge = {
  __typename?: 'DeliverablesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Deliverable;
};

export type IntOperationFilterInput = {
  eq?: InputMaybe<Scalars['Int']['input']>;
  gt?: InputMaybe<Scalars['Int']['input']>;
  gte?: InputMaybe<Scalars['Int']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  lt?: InputMaybe<Scalars['Int']['input']>;
  lte?: InputMaybe<Scalars['Int']['input']>;
  neq?: InputMaybe<Scalars['Int']['input']>;
  ngt?: InputMaybe<Scalars['Int']['input']>;
  ngte?: InputMaybe<Scalars['Int']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  nlt?: InputMaybe<Scalars['Int']['input']>;
  nlte?: InputMaybe<Scalars['Int']['input']>;
};

export type LargeLanguageModel = {
  __typename?: 'LargeLanguageModel';
  apiKey: Scalars['String']['output'];
  cost: Scalars['Int']['output'];
  id: Scalars['UUID']['output'];
  maxComplexity: Scalars['Int']['output'];
  maxConcurrency: Scalars['Int']['output'];
  model: Scalars['String']['output'];
  modelAlias: Scalars['String']['output'];
  url: Scalars['String']['output'];
};

export type LargeLanguageModelFilterInput = {
  and?: InputMaybe<Array<LargeLanguageModelFilterInput>>;
  apiKey?: InputMaybe<StringOperationFilterInput>;
  cost?: InputMaybe<IntOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  maxComplexity?: InputMaybe<IntOperationFilterInput>;
  maxConcurrency?: InputMaybe<IntOperationFilterInput>;
  model?: InputMaybe<StringOperationFilterInput>;
  modelAlias?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<LargeLanguageModelFilterInput>>;
  url?: InputMaybe<StringOperationFilterInput>;
};

export type LargeLanguageModelSortInput = {
  apiKey?: InputMaybe<SortEnumType>;
  cost?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  maxComplexity?: InputMaybe<SortEnumType>;
  maxConcurrency?: InputMaybe<SortEnumType>;
  model?: InputMaybe<SortEnumType>;
  modelAlias?: InputMaybe<SortEnumType>;
  url?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type LargeLanguageModelsConnection = {
  __typename?: 'LargeLanguageModelsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<LargeLanguageModelsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<LargeLanguageModel>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type LargeLanguageModelsEdge = {
  __typename?: 'LargeLanguageModelsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: LargeLanguageModel;
};

export type ListFilterInputTypeOfAgentTaskFilterInput = {
  all?: InputMaybe<AgentTaskFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<AgentTaskFilterInput>;
  some?: InputMaybe<AgentTaskFilterInput>;
};

export type ListFilterInputTypeOfDeliverableFilterInput = {
  all?: InputMaybe<DeliverableFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<DeliverableFilterInput>;
  some?: InputMaybe<DeliverableFilterInput>;
};

export type Mutation = {
  __typename?: 'Mutation';
  checkAndMarkDeliverableDone: Scalars['Boolean']['output'];
  createAgentTask: AgentTask;
  createDeliverable?: Maybe<Deliverable>;
  createLargeLanguageModel?: Maybe<LargeLanguageModel>;
  createProject?: Maybe<Project>;
  deleteAgentTask: Scalars['Boolean']['output'];
  deleteDeliverable: Scalars['Boolean']['output'];
  deleteLargeLanguageModel: Scalars['Boolean']['output'];
  deleteProject: Scalars['Boolean']['output'];
  deleteTestData: DeleteTestDataPayload;
  updateAgentTask: AgentTask;
  updateAgentTaskStatus: AgentTaskStatus;
  updateDeliverable: Deliverable;
  updateDeliverableStatus: DeliverableStatus;
  updateLargeLanguageModel?: Maybe<LargeLanguageModel>;
  updateProject?: Maybe<Project>;
};


export type MutationcheckAndMarkDeliverableDoneArgs = {
  deliverableId: Scalars['UUID']['input'];
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
  id: Scalars['UUID']['input'];
};


export type MutationdeleteDeliverableArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationdeleteLargeLanguageModelArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationdeleteProjectArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationupdateAgentTaskArgs = {
  input: UpdateAgentTaskInput;
};


export type MutationupdateAgentTaskStatusArgs = {
  id: Scalars['UUID']['input'];
  targetStatus: AgentTaskStatus;
};


export type MutationupdateDeliverableArgs = {
  input: UpdateDeliverableInput;
};


export type MutationupdateDeliverableStatusArgs = {
  actor?: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  targetStatus: DeliverableStatus;
};


export type MutationupdateLargeLanguageModelArgs = {
  input: UpdateLargeLanguageModelInput;
};


export type MutationupdateProjectArgs = {
  input: UpdateProjectInput;
};

/** Information about pagination in a connection. */
export type PageInfo = {
  __typename?: 'PageInfo';
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']['output']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean']['output'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean']['output'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']['output']>;
};

export type Project = {
  __typename?: 'Project';
  deliverables: Array<Deliverable>;
  description: Scalars['String']['output'];
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
  repository: Scalars['String']['output'];
};

export type ProjectFilterInput = {
  and?: InputMaybe<Array<ProjectFilterInput>>;
  deliverables?: InputMaybe<ListFilterInputTypeOfDeliverableFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ProjectFilterInput>>;
  repository?: InputMaybe<StringOperationFilterInput>;
};

export type ProjectSortInput = {
  description?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  repository?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type ProjectsConnection = {
  __typename?: 'ProjectsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<ProjectsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Project>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type ProjectsEdge = {
  __typename?: 'ProjectsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Project;
};

export type Query = {
  __typename?: 'Query';
  agentTask?: Maybe<AgentTask>;
  agentTasks?: Maybe<AgentTasksConnection>;
  deliverable?: Maybe<Deliverable>;
  deliverables?: Maybe<DeliverablesConnection>;
  largeLanguageModel?: Maybe<LargeLanguageModel>;
  largeLanguageModels?: Maybe<LargeLanguageModelsConnection>;
  project?: Maybe<Project>;
  projects?: Maybe<ProjectsConnection>;
};


export type QueryagentTaskArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryagentTasksArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<AgentTaskSortInput>>;
  where?: InputMaybe<AgentTaskFilterInput>;
};


export type QuerydeliverableArgs = {
  id: Scalars['UUID']['input'];
};


export type QuerydeliverablesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<DeliverableSortInput>>;
  where?: InputMaybe<DeliverableFilterInput>;
};


export type QuerylargeLanguageModelArgs = {
  id: Scalars['UUID']['input'];
};


export type QuerylargeLanguageModelsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<LargeLanguageModelSortInput>>;
  where?: InputMaybe<LargeLanguageModelFilterInput>;
};


export type QueryprojectArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryprojectsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ProjectSortInput>>;
  where?: InputMaybe<ProjectFilterInput>;
};

export const SortEnumType = {
  ASC: 'ASC',
  DESC: 'DESC'
} as const;

export type SortEnumType = typeof SortEnumType[keyof typeof SortEnumType];
export type StringOperationFilterInput = {
  and?: InputMaybe<Array<StringOperationFilterInput>>;
  contains?: InputMaybe<Scalars['String']['input']>;
  endsWith?: InputMaybe<Scalars['String']['input']>;
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  ncontains?: InputMaybe<Scalars['String']['input']>;
  nendsWith?: InputMaybe<Scalars['String']['input']>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  nstartsWith?: InputMaybe<Scalars['String']['input']>;
  or?: InputMaybe<Array<StringOperationFilterInput>>;
  startsWith?: InputMaybe<Scalars['String']['input']>;
};

export type UpdateAgentTaskInput = {
  agent?: InputMaybe<Scalars['String']['input']>;
  commitHash?: InputMaybe<Scalars['String']['input']>;
  completionTokens?: InputMaybe<Scalars['Int']['input']>;
  complexityRating?: InputMaybe<Scalars['Int']['input']>;
  dependsOnAgentTaskId?: InputMaybe<Scalars['UUID']['input']>;
  description?: InputMaybe<Scalars['String']['input']>;
  errors?: InputMaybe<Scalars['String']['input']>;
  executionDurationInSeconds?: InputMaybe<Scalars['Int']['input']>;
  id: Scalars['UUID']['input'];
  promptTokens?: InputMaybe<Scalars['Int']['input']>;
  result?: InputMaybe<Scalars['String']['input']>;
  title?: InputMaybe<Scalars['String']['input']>;
};

export type UpdateDeliverableInput = {
  acceptanceCriteria?: InputMaybe<Scalars['String']['input']>;
  agentFeedback?: InputMaybe<Scalars['String']['input']>;
  blocking?: InputMaybe<Scalars['String']['input']>;
  deploymentPlan?: InputMaybe<Scalars['String']['input']>;
  description?: InputMaybe<Scalars['String']['input']>;
  design?: InputMaybe<Scalars['String']['input']>;
  executionPlan?: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  performanceImpact?: InputMaybe<Scalars['String']['input']>;
  securityImpact?: InputMaybe<Scalars['String']['input']>;
  testPlan?: InputMaybe<Scalars['String']['input']>;
  title?: InputMaybe<Scalars['String']['input']>;
};

export type UpdateLargeLanguageModelInput = {
  apiKey?: InputMaybe<Scalars['String']['input']>;
  cost?: InputMaybe<Scalars['Int']['input']>;
  id: Scalars['UUID']['input'];
  maxComplexity?: InputMaybe<Scalars['Int']['input']>;
  maxConcurrency?: InputMaybe<Scalars['Int']['input']>;
  model?: InputMaybe<Scalars['String']['input']>;
  modelAlias?: InputMaybe<Scalars['String']['input']>;
  url?: InputMaybe<Scalars['String']['input']>;
};

export type UpdateProjectInput = {
  description?: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  name?: InputMaybe<Scalars['String']['input']>;
  repository?: InputMaybe<Scalars['String']['input']>;
};

export type UuidOperationFilterInput = {
  eq?: InputMaybe<Scalars['UUID']['input']>;
  gt?: InputMaybe<Scalars['UUID']['input']>;
  gte?: InputMaybe<Scalars['UUID']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['UUID']['input']>>>;
  lt?: InputMaybe<Scalars['UUID']['input']>;
  lte?: InputMaybe<Scalars['UUID']['input']>;
  neq?: InputMaybe<Scalars['UUID']['input']>;
  ngt?: InputMaybe<Scalars['UUID']['input']>;
  ngte?: InputMaybe<Scalars['UUID']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['UUID']['input']>>>;
  nlt?: InputMaybe<Scalars['UUID']['input']>;
  nlte?: InputMaybe<Scalars['UUID']['input']>;
};

export type CreateAgentTaskMutationVariables = Exact<{
  input: CreateAgentTaskInput;
}>;


export type CreateAgentTaskMutation = { __typename?: 'Mutation', createAgentTask: { __typename?: 'AgentTask', id: any, projectId: any, deliverableId: any, title: string, status: AgentTaskStatus, description: string, complexityRating: number, result?: string | null, errors?: string | null, commitHash?: string | null, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null } };

export type CreateDeliverableMutationVariables = Exact<{
  input: CreateDeliverableInput;
}>;


export type CreateDeliverableMutation = { __typename?: 'Mutation', createDeliverable?: { __typename?: 'Deliverable', id: any, title: string, status: DeliverableStatus, design?: string | null } | null };

export type CreateLargeLanguageModelMutationVariables = Exact<{
  input: CreateLargeLanguageModelInput;
}>;


export type CreateLargeLanguageModelMutation = { __typename?: 'Mutation', createLargeLanguageModel?: { __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias: string, cost: number, maxComplexity: number, maxConcurrency: number } | null };

export type CreateProjectMutationVariables = Exact<{
  input: CreateProjectInput;
}>;


export type CreateProjectMutation = { __typename?: 'Mutation', createProject?: { __typename?: 'Project', id: any, name: string, description: string, repository: string } | null };

export type DeleteAgentTaskMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteAgentTaskMutation = { __typename?: 'Mutation', deleteAgentTask: boolean };

export type DeleteDeliverableMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteDeliverableMutation = { __typename?: 'Mutation', deleteDeliverable: boolean };

export type DeleteLargeLanguageModelMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteLargeLanguageModelMutation = { __typename?: 'Mutation', deleteLargeLanguageModel: boolean };

export type DeleteProjectMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteProjectMutation = { __typename?: 'Mutation', deleteProject: boolean };

export type UpdateAgentTaskStatusMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  targetStatus: AgentTaskStatus;
}>;


export type UpdateAgentTaskStatusMutation = { __typename?: 'Mutation', updateAgentTaskStatus: AgentTaskStatus };

export type UpdateDeliverableStatusMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  targetStatus: DeliverableStatus;
}>;


export type UpdateDeliverableStatusMutation = { __typename?: 'Mutation', updateDeliverableStatus: DeliverableStatus };

export type UpdateAgentTaskMutationVariables = Exact<{
  input: UpdateAgentTaskInput;
}>;


export type UpdateAgentTaskMutation = { __typename?: 'Mutation', updateAgentTask: { __typename?: 'AgentTask', id: any, projectId: any, deliverableId: any, title: string, status: AgentTaskStatus, description: string, result?: string | null, errors?: string | null, commitHash?: string | null, complexityRating: number, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null } };

export type UpdateDeliverableMutationVariables = Exact<{
  input: UpdateDeliverableInput;
}>;


export type UpdateDeliverableMutation = { __typename?: 'Mutation', updateDeliverable: { __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null, design?: string | null } };

export type UpdateLargeLanguageModelMutationVariables = Exact<{
  input: UpdateLargeLanguageModelInput;
}>;


export type UpdateLargeLanguageModelMutation = { __typename?: 'Mutation', updateLargeLanguageModel?: { __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias: string, cost: number, maxComplexity: number, maxConcurrency: number } | null };

export type UpdateProjectMutationVariables = Exact<{
  input: UpdateProjectInput;
}>;


export type UpdateProjectMutation = { __typename?: 'Mutation', updateProject?: { __typename?: 'Project', id: any, name: string, description: string, repository: string } | null };

export type GetAgentTaskQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetAgentTaskQuery = { __typename?: 'Query', agentTask?: { __typename?: 'AgentTask', id: any, projectId: any, deliverableId: any, title: string, status: AgentTaskStatus, description: string, result?: string | null, errors?: string | null, commitHash?: string | null, complexityRating: number, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null, deliverable?: { __typename?: 'Deliverable', id: any, title: string } | null, project?: { __typename?: 'Project', id: any, name: string, repository: string } | null } | null };

export type GetAgentTasksQueryVariables = Exact<{
  deliverableId: Scalars['UUID']['input'];
}>;


export type GetAgentTasksQuery = { __typename?: 'Query', agentTasks?: { __typename?: 'AgentTasksConnection', nodes?: Array<{ __typename?: 'AgentTask', id: any, projectId: any, deliverableId: any, title: string, status: AgentTaskStatus, description: string, result?: string | null, errors?: string | null, commitHash?: string | null, complexityRating: number, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null, deliverable?: { __typename?: 'Deliverable', id: any, title: string } | null, project?: { __typename?: 'Project', id: any, name: string } | null }> | null } | null };

export type GetAllDeliverablesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetAllDeliverablesQuery = { __typename?: 'Query', deliverables?: { __typename?: 'DeliverablesConnection', nodes?: Array<{ __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, projectId: any, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null, design?: string | null }> | null } | null };

export type GetDeliverablesByProjectQueryVariables = Exact<{
  projectId: Scalars['UUID']['input'];
}>;


export type GetDeliverablesByProjectQuery = { __typename?: 'Query', deliverables?: { __typename?: 'DeliverablesConnection', nodes?: Array<{ __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, projectId: any, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null, design?: string | null }> | null } | null };

export type GetDeliverableQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDeliverableQuery = { __typename?: 'Query', deliverable?: { __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, projectId: any, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null, design?: string | null, agentTasks: Array<{ __typename?: 'AgentTask', id: any, title: string, status: AgentTaskStatus, promptTokens?: number | null, completionTokens?: number | null }> } | null };

export type ModelConfigurationsQueryVariables = Exact<{ [key: string]: never; }>;


export type ModelConfigurationsQuery = { __typename?: 'Query', largeLanguageModels?: { __typename?: 'LargeLanguageModelsConnection', nodes?: Array<{ __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias: string, apiKey: string, cost: number, maxComplexity: number, maxConcurrency: number }> | null } | null };

export type GetProjectQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetProjectQuery = { __typename?: 'Query', project?: { __typename?: 'Project', id: any, name: string, description: string, repository: string } | null };

export type GetProjectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetProjectsQuery = { __typename?: 'Query', projects?: { __typename?: 'ProjectsConnection', nodes?: Array<{ __typename?: 'Project', id: any, name: string, description: string, repository: string }> | null } | null };


export const CreateAgentTaskDocument = gql`
    mutation CreateAgentTask($input: CreateAgentTaskInput!) {
  createAgentTask(input: $input) {
    id
    projectId
    deliverableId
    title
    status
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
    id
    title
    status
    design
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
    id
    url
    model
    modelAlias
    cost
    maxComplexity
    maxConcurrency
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
    id
    name
    description
    repository
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
    mutation DeleteAgentTask($id: UUID!) {
  deleteAgentTask(id: $id)
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
 *      id: // value for 'id'
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
    mutation DeleteDeliverable($id: UUID!) {
  deleteDeliverable(id: $id)
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
 *      id: // value for 'id'
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
    mutation DeleteLargeLanguageModel($id: UUID!) {
  deleteLargeLanguageModel(id: $id)
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
 *      id: // value for 'id'
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
    mutation DeleteProject($id: UUID!) {
  deleteProject(id: $id)
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
 *      id: // value for 'id'
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
export const UpdateAgentTaskStatusDocument = gql`
    mutation UpdateAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) {
  updateAgentTaskStatus(id: $id, targetStatus: $targetStatus)
}
    `;
export type UpdateAgentTaskStatusMutationFn = Apollo.MutationFunction<UpdateAgentTaskStatusMutation, UpdateAgentTaskStatusMutationVariables>;

/**
 * __useUpdateAgentTaskStatusMutation__
 *
 * To run a mutation, you first call `useUpdateAgentTaskStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateAgentTaskStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateAgentTaskStatusMutation, { data, loading, error }] = useUpdateAgentTaskStatusMutation({
 *   variables: {
 *      id: // value for 'id'
 *      targetStatus: // value for 'targetStatus'
 *   },
 * });
 */
export function useUpdateAgentTaskStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateAgentTaskStatusMutation, UpdateAgentTaskStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateAgentTaskStatusMutation, UpdateAgentTaskStatusMutationVariables>(UpdateAgentTaskStatusDocument, options);
      }
export type UpdateAgentTaskStatusMutationHookResult = ReturnType<typeof useUpdateAgentTaskStatusMutation>;
export type UpdateAgentTaskStatusMutationResult = Apollo.MutationResult<UpdateAgentTaskStatusMutation>;
export type UpdateAgentTaskStatusMutationOptions = Apollo.BaseMutationOptions<UpdateAgentTaskStatusMutation, UpdateAgentTaskStatusMutationVariables>;
export const UpdateDeliverableStatusDocument = gql`
    mutation UpdateDeliverableStatus($id: UUID!, $targetStatus: DeliverableStatus!) {
  updateDeliverableStatus(id: $id, targetStatus: $targetStatus)
}
    `;
export type UpdateDeliverableStatusMutationFn = Apollo.MutationFunction<UpdateDeliverableStatusMutation, UpdateDeliverableStatusMutationVariables>;

/**
 * __useUpdateDeliverableStatusMutation__
 *
 * To run a mutation, you first call `useUpdateDeliverableStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateDeliverableStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateDeliverableStatusMutation, { data, loading, error }] = useUpdateDeliverableStatusMutation({
 *   variables: {
 *      id: // value for 'id'
 *      targetStatus: // value for 'targetStatus'
 *   },
 * });
 */
export function useUpdateDeliverableStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateDeliverableStatusMutation, UpdateDeliverableStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateDeliverableStatusMutation, UpdateDeliverableStatusMutationVariables>(UpdateDeliverableStatusDocument, options);
      }
export type UpdateDeliverableStatusMutationHookResult = ReturnType<typeof useUpdateDeliverableStatusMutation>;
export type UpdateDeliverableStatusMutationResult = Apollo.MutationResult<UpdateDeliverableStatusMutation>;
export type UpdateDeliverableStatusMutationOptions = Apollo.BaseMutationOptions<UpdateDeliverableStatusMutation, UpdateDeliverableStatusMutationVariables>;
export const UpdateAgentTaskDocument = gql`
    mutation UpdateAgentTask($input: UpdateAgentTaskInput!) {
  updateAgentTask(input: $input) {
    id
    projectId
    deliverableId
    title
    status
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
    design
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
    id
    url
    model
    modelAlias
    cost
    maxComplexity
    maxConcurrency
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
    id
    name
    description
    repository
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
export const GetAgentTaskDocument = gql`
    query GetAgentTask($id: UUID!) {
  agentTask(id: $id) {
    id
    projectId
    deliverableId
    title
    status
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
    deliverable {
      id
      title
    }
    project {
      id
      name
      repository
    }
  }
}
    `;

/**
 * __useGetAgentTaskQuery__
 *
 * To run a query within a React component, call `useGetAgentTaskQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetAgentTaskQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetAgentTaskQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetAgentTaskQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetAgentTaskQuery, GetAgentTaskQueryVariables> & ({ variables: GetAgentTaskQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetAgentTaskQuery, GetAgentTaskQueryVariables>(GetAgentTaskDocument, options);
      }
export function useGetAgentTaskLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetAgentTaskQuery, GetAgentTaskQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetAgentTaskQuery, GetAgentTaskQueryVariables>(GetAgentTaskDocument, options);
        }
// @ts-ignore
export function useGetAgentTaskSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskQuery, GetAgentTaskQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTaskQuery, GetAgentTaskQueryVariables>;
export function useGetAgentTaskSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskQuery, GetAgentTaskQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAgentTaskQuery | undefined, GetAgentTaskQueryVariables>;
export function useGetAgentTaskSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAgentTaskQuery, GetAgentTaskQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetAgentTaskQuery, GetAgentTaskQueryVariables>(GetAgentTaskDocument, options);
        }
export type GetAgentTaskQueryHookResult = ReturnType<typeof useGetAgentTaskQuery>;
export type GetAgentTaskLazyQueryHookResult = ReturnType<typeof useGetAgentTaskLazyQuery>;
export type GetAgentTaskSuspenseQueryHookResult = ReturnType<typeof useGetAgentTaskSuspenseQuery>;
export type GetAgentTaskQueryResult = Apollo.QueryResult<GetAgentTaskQuery, GetAgentTaskQueryVariables>;
export const GetAgentTasksDocument = gql`
    query GetAgentTasks($deliverableId: UUID!) {
  agentTasks(where: {deliverableId: {eq: $deliverableId}}) {
    nodes {
      id
      projectId
      deliverableId
      title
      status
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
      deliverable {
        id
        title
      }
      project {
        id
        name
      }
    }
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
 *      deliverableId: // value for 'deliverableId'
 *   },
 * });
 */
export function useGetAgentTasksQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetAgentTasksQuery, GetAgentTasksQueryVariables> & ({ variables: GetAgentTasksQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
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
export const GetAllDeliverablesDocument = gql`
    query GetAllDeliverables {
  deliverables {
    nodes {
      id
      title
      description
      status
      type
      projectId
      acceptanceCriteria
      executionPlan
      agentFeedback
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      blocking
      design
    }
  }
}
    `;

/**
 * __useGetAllDeliverablesQuery__
 *
 * To run a query within a React component, call `useGetAllDeliverablesQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetAllDeliverablesQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetAllDeliverablesQuery({
 *   variables: {
 *   },
 * });
 */
export function useGetAllDeliverablesQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>(GetAllDeliverablesDocument, options);
      }
export function useGetAllDeliverablesLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>(GetAllDeliverablesDocument, options);
        }
// @ts-ignore
export function useGetAllDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>;
export function useGetAllDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetAllDeliverablesQuery | undefined, GetAllDeliverablesQueryVariables>;
export function useGetAllDeliverablesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>(GetAllDeliverablesDocument, options);
        }
export type GetAllDeliverablesQueryHookResult = ReturnType<typeof useGetAllDeliverablesQuery>;
export type GetAllDeliverablesLazyQueryHookResult = ReturnType<typeof useGetAllDeliverablesLazyQuery>;
export type GetAllDeliverablesSuspenseQueryHookResult = ReturnType<typeof useGetAllDeliverablesSuspenseQuery>;
export type GetAllDeliverablesQueryResult = Apollo.QueryResult<GetAllDeliverablesQuery, GetAllDeliverablesQueryVariables>;
export const GetDeliverablesByProjectDocument = gql`
    query GetDeliverablesByProject($projectId: UUID!) {
  deliverables(where: {projectId: {eq: $projectId}}) {
    nodes {
      id
      title
      description
      status
      type
      projectId
      acceptanceCriteria
      executionPlan
      agentFeedback
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      blocking
      design
    }
  }
}
    `;

/**
 * __useGetDeliverablesByProjectQuery__
 *
 * To run a query within a React component, call `useGetDeliverablesByProjectQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDeliverablesByProjectQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDeliverablesByProjectQuery({
 *   variables: {
 *      projectId: // value for 'projectId'
 *   },
 * });
 */
export function useGetDeliverablesByProjectQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables> & ({ variables: GetDeliverablesByProjectQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>(GetDeliverablesByProjectDocument, options);
      }
export function useGetDeliverablesByProjectLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>(GetDeliverablesByProjectDocument, options);
        }
// @ts-ignore
export function useGetDeliverablesByProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>;
export function useGetDeliverablesByProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverablesByProjectQuery | undefined, GetDeliverablesByProjectQueryVariables>;
export function useGetDeliverablesByProjectSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>(GetDeliverablesByProjectDocument, options);
        }
export type GetDeliverablesByProjectQueryHookResult = ReturnType<typeof useGetDeliverablesByProjectQuery>;
export type GetDeliverablesByProjectLazyQueryHookResult = ReturnType<typeof useGetDeliverablesByProjectLazyQuery>;
export type GetDeliverablesByProjectSuspenseQueryHookResult = ReturnType<typeof useGetDeliverablesByProjectSuspenseQuery>;
export type GetDeliverablesByProjectQueryResult = Apollo.QueryResult<GetDeliverablesByProjectQuery, GetDeliverablesByProjectQueryVariables>;
export const GetDeliverableDocument = gql`
    query GetDeliverable($id: UUID!) {
  deliverable(id: $id) {
    id
    title
    description
    status
    type
    projectId
    acceptanceCriteria
    executionPlan
    agentFeedback
    securityImpact
    performanceImpact
    testPlan
    deploymentPlan
    blocking
    design
    agentTasks {
      id
      title
      status
      promptTokens
      completionTokens
    }
  }
}
    `;

/**
 * __useGetDeliverableQuery__
 *
 * To run a query within a React component, call `useGetDeliverableQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDeliverableQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDeliverableQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetDeliverableQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetDeliverableQuery, GetDeliverableQueryVariables> & ({ variables: GetDeliverableQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDeliverableQuery, GetDeliverableQueryVariables>(GetDeliverableDocument, options);
      }
export function useGetDeliverableLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDeliverableQuery, GetDeliverableQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDeliverableQuery, GetDeliverableQueryVariables>(GetDeliverableDocument, options);
        }
// @ts-ignore
export function useGetDeliverableSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableQuery, GetDeliverableQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverableQuery, GetDeliverableQueryVariables>;
export function useGetDeliverableSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableQuery, GetDeliverableQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDeliverableQuery | undefined, GetDeliverableQueryVariables>;
export function useGetDeliverableSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDeliverableQuery, GetDeliverableQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDeliverableQuery, GetDeliverableQueryVariables>(GetDeliverableDocument, options);
        }
export type GetDeliverableQueryHookResult = ReturnType<typeof useGetDeliverableQuery>;
export type GetDeliverableLazyQueryHookResult = ReturnType<typeof useGetDeliverableLazyQuery>;
export type GetDeliverableSuspenseQueryHookResult = ReturnType<typeof useGetDeliverableSuspenseQuery>;
export type GetDeliverableQueryResult = Apollo.QueryResult<GetDeliverableQuery, GetDeliverableQueryVariables>;
export const ModelConfigurationsDocument = gql`
    query ModelConfigurations {
  largeLanguageModels {
    nodes {
      id
      url
      model
      modelAlias
      apiKey
      cost
      maxComplexity
      maxConcurrency
    }
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
  project(id: $id) {
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