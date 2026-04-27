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
  /** UUID scalar */
  UUID: { input: any; output: any; }
  /** The `Upload` scalar type represents a file upload. */
  Upload: { input: any; output: any; }
};

/** Represents an agent task (unit of work for an AI agent). */
export type AgentTask = {
  __typename?: 'AgentTask';
  /** Agent that executed the task */
  agent?: Maybe<Scalars['String']['output']>;
  /** Git commit hash if applicable */
  commitHash?: Maybe<Scalars['String']['output']>;
  /** Number of completion tokens used */
  completionTokens?: Maybe<Scalars['Int']['output']>;
  /** Complexity rating (1-10) */
  complexityRating: Scalars['Int']['output'];
  /** Deliverable this task belongs to */
  deliverable?: Maybe<Deliverable>;
  /** ID of the deliverable this task belongs to */
  deliverableId: Scalars['UUID']['output'];
  /** Task this task depends on */
  dependsOnAgentTask?: Maybe<AgentTask>;
  /** ID of the task this task depends on */
  dependsOnAgentTaskId?: Maybe<Scalars['UUID']['output']>;
  /** Description of the task */
  description: Scalars['String']['output'];
  /** Errors encountered during execution */
  errors?: Maybe<Scalars['String']['output']>;
  /** Execution duration in seconds */
  executionDurationInSeconds?: Maybe<Scalars['Int']['output']>;
  /** Unique identifier of the task */
  id: Scalars['UUID']['output'];
  /** Project this task belongs to */
  project?: Maybe<Project>;
  /** ID of the project this task belongs to */
  projectId: Scalars['UUID']['output'];
  /** Number of prompt tokens used */
  promptTokens?: Maybe<Scalars['Int']['output']>;
  /** Result of the task execution */
  result?: Maybe<Scalars['String']['output']>;
  /** Current status of the task */
  status: AgentTaskStatus;
  /** Title of the task */
  title: Scalars['String']['output'];
};

export type AgentTaskConnection = Connection & {
  __typename?: 'AgentTaskConnection';
  edges: Array<AgentTaskEdge>;
  nodes: Array<AgentTask>;
  pageInfo: PageInfo;
  totalCount: Scalars['Int']['output'];
};

export type AgentTaskEdge = Edge & {
  __typename?: 'AgentTaskEdge';
  cursor: Scalars['String']['output'];
  node: AgentTask;
};

export type AgentTaskFilterInput = {
  and?: InputMaybe<Array<AgentTaskFilterInput>>;
  complexityRating?: InputMaybe<IntFilterInput>;
  deliverableId?: InputMaybe<UUIDFilterInput>;
  description?: InputMaybe<StringFilterInput>;
  id?: InputMaybe<UUIDFilterInput>;
  or?: InputMaybe<Array<AgentTaskFilterInput>>;
  projectId?: InputMaybe<UUIDFilterInput>;
  status?: InputMaybe<AgentTaskStatusFilterInput>;
  title?: InputMaybe<StringFilterInput>;
};

export type AgentTaskSortInput = {
  complexityRating?: InputMaybe<SortOperationKind>;
  deliverableId?: InputMaybe<SortOperationKind>;
  description?: InputMaybe<SortOperationKind>;
  id?: InputMaybe<SortOperationKind>;
  projectId?: InputMaybe<SortOperationKind>;
  status?: InputMaybe<SortOperationKind>;
  title?: InputMaybe<SortOperationKind>;
};

/** Enumeration of agent task statuses */
export const AgentTaskStatus = {
  DONE: 'DONE',
  FAILED: 'FAILED',
  IN_PROGRESS: 'IN_PROGRESS',
  NEEDS_REVIEW: 'NEEDS_REVIEW',
  READY: 'READY',
  REJECTED: 'REJECTED'
} as const;

export type AgentTaskStatus = typeof AgentTaskStatus[keyof typeof AgentTaskStatus];
export type AgentTaskStatusFilterInput = {
  eq?: InputMaybe<AgentTaskStatus>;
  in?: InputMaybe<Array<AgentTaskStatus>>;
  ne?: InputMaybe<AgentTaskStatus>;
  notIn?: InputMaybe<Array<AgentTaskStatus>>;
};

export type Connection = {
  pageInfo: PageInfo;
  totalCount: Scalars['Int']['output'];
};

/** Input for creating a new agent task */
export type CreateAgentTaskInput = {
  /** Complexity rating (1-10) */
  complexityRating?: InputMaybe<Scalars['Int']['input']>;
  /** ID of the deliverable this task belongs to */
  deliverableId: Scalars['UUID']['input'];
  /** ID of the task this task depends on */
  dependsOnAgentTaskId?: InputMaybe<Scalars['UUID']['input']>;
  /** Description of the task */
  description: Scalars['String']['input'];
  /** ID of the project this task belongs to */
  projectId: Scalars['UUID']['input'];
  /** Title of the task */
  title: Scalars['String']['input'];
};

/** Input for creating a new deliverable */
export type CreateDeliverableInput = {
  /** Acceptance criteria */
  acceptanceCriteria?: InputMaybe<Scalars['String']['input']>;
  /** Deployment plan */
  deploymentPlan?: InputMaybe<Scalars['String']['input']>;
  /** Description of the deliverable */
  description: Scalars['String']['input'];
  /** Execution plan */
  executionPlan?: InputMaybe<Scalars['String']['input']>;
  /** Initial status of the deliverable */
  initialStatus: DeliverableStatus;
  /** Performance impact assessment */
  performanceImpact?: InputMaybe<Scalars['String']['input']>;
  /** ID of the project this deliverable belongs to */
  projectId: Scalars['UUID']['input'];
  /** Security impact assessment */
  securityImpact?: InputMaybe<Scalars['String']['input']>;
  /** Test plan */
  testPlan?: InputMaybe<Scalars['String']['input']>;
  /** Title of the deliverable */
  title: Scalars['String']['input'];
  /** Type of deliverable */
  type: Scalars['String']['input'];
};

/** Input for creating a new LLM configuration */
export type CreateLargeLanguageModelInput = {
  /** API key for authentication */
  apiKey?: InputMaybe<Scalars['String']['input']>;
  /** Cost metric */
  cost?: InputMaybe<Scalars['Int']['input']>;
  /** Maximum complexity rating */
  maxComplexity?: InputMaybe<Scalars['Int']['input']>;
  /** Maximum concurrent requests */
  maxConcurrency?: InputMaybe<Scalars['Int']['input']>;
  /** Model identifier */
  model: Scalars['String']['input'];
  /** Optional alias for the model */
  modelAlias?: InputMaybe<Scalars['String']['input']>;
  /** Base URL of the LLM API */
  url: Scalars['String']['input'];
};

/** Input for creating a new project */
export type CreateProjectInput = {
  /** Description of the project */
  description?: InputMaybe<Scalars['String']['input']>;
  /** Name of the project */
  name: Scalars['String']['input'];
  /** Git repository URL */
  repository: Scalars['String']['input'];
};

/** Payload for cleanup test data mutation */
export type DeleteTestDataPayload = {
  __typename?: 'DeleteTestDataPayload';
  /** Optional message */
  message?: Maybe<Scalars['String']['output']>;
  /** Whether the operation was successful */
  success: Scalars['Boolean']['output'];
};

/** Represents a deliverable (feature, task, bug, etc.) in the DevStack system. */
export type Deliverable = {
  __typename?: 'Deliverable';
  /** Acceptance criteria for the deliverable */
  acceptanceCriteria?: Maybe<Scalars['String']['output']>;
  /** Agent feedback on the deliverable */
  agentFeedback?: Maybe<Scalars['String']['output']>;
  /** Agent tasks associated with this deliverable */
  agentTasks?: Maybe<AgentTaskConnection>;
  /** Blocking issues or dependencies */
  blocking?: Maybe<Scalars['String']['output']>;
  /** Deployment plan for the deliverable */
  deploymentPlan?: Maybe<Scalars['String']['output']>;
  /** Detailed description of the deliverable */
  description?: Maybe<Scalars['String']['output']>;
  /** Execution plan for the deliverable */
  executionPlan?: Maybe<Scalars['String']['output']>;
  /** Unique identifier of the deliverable */
  id: Scalars['UUID']['output'];
  /** Performance impact assessment */
  performanceImpact?: Maybe<Scalars['String']['output']>;
  /** Project this deliverable belongs to */
  project?: Maybe<Project>;
  /** ID of the project this deliverable belongs to */
  projectId: Scalars['UUID']['output'];
  /** Security impact assessment */
  securityImpact?: Maybe<Scalars['String']['output']>;
  /** Current status of the deliverable */
  status: DeliverableStatus;
  /** Test plan for the deliverable */
  testPlan?: Maybe<Scalars['String']['output']>;
  /** Title of the deliverable */
  title: Scalars['String']['output'];
  /** Type of deliverable (Feature, Task, Bug, Epic, etc.) */
  type: DeliverableType;
};


/** Represents a deliverable (feature, task, bug, etc.) in the DevStack system. */
export type DeliverableagentTasksArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<AgentTaskFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<AgentTaskSortInput>>;
};

export type DeliverableConnection = Connection & {
  __typename?: 'DeliverableConnection';
  edges: Array<DeliverableEdge>;
  nodes: Array<Deliverable>;
  pageInfo: PageInfo;
  totalCount: Scalars['Int']['output'];
};

export type DeliverableEdge = Edge & {
  __typename?: 'DeliverableEdge';
  cursor: Scalars['String']['output'];
  node: Deliverable;
};

export type DeliverableFilterInput = {
  and?: InputMaybe<Array<DeliverableFilterInput>>;
  description?: InputMaybe<StringFilterInput>;
  id?: InputMaybe<UUIDFilterInput>;
  or?: InputMaybe<Array<DeliverableFilterInput>>;
  projectId?: InputMaybe<UUIDFilterInput>;
  status?: InputMaybe<DeliverableStatusFilterInput>;
  title?: InputMaybe<StringFilterInput>;
  type?: InputMaybe<DeliverableTypeFilterInput>;
};

export type DeliverableSortInput = {
  description?: InputMaybe<SortOperationKind>;
  id?: InputMaybe<SortOperationKind>;
  projectId?: InputMaybe<SortOperationKind>;
  status?: InputMaybe<SortOperationKind>;
  title?: InputMaybe<SortOperationKind>;
  type?: InputMaybe<SortOperationKind>;
};

/** Enumeration of deliverable statuses */
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
export type DeliverableStatusFilterInput = {
  eq?: InputMaybe<DeliverableStatus>;
  in?: InputMaybe<Array<DeliverableStatus>>;
  ne?: InputMaybe<DeliverableStatus>;
  notIn?: InputMaybe<Array<DeliverableStatus>>;
};

/** Enumeration of deliverable types */
export const DeliverableType = {
  DEFECT: 'DEFECT',
  FEATURE: 'FEATURE',
  MAINTENANCE: 'MAINTENANCE',
  SPIKE: 'SPIKE'
} as const;

export type DeliverableType = typeof DeliverableType[keyof typeof DeliverableType];
export type DeliverableTypeFilterInput = {
  eq?: InputMaybe<DeliverableType>;
  in?: InputMaybe<Array<DeliverableType>>;
  ne?: InputMaybe<DeliverableType>;
  notIn?: InputMaybe<Array<DeliverableType>>;
};

export type Edge = {
  cursor: Scalars['String']['output'];
};

export type IntFilterInput = {
  eq?: InputMaybe<Scalars['Int']['input']>;
  gt?: InputMaybe<Scalars['Int']['input']>;
  gte?: InputMaybe<Scalars['Int']['input']>;
  in?: InputMaybe<Array<Scalars['Int']['input']>>;
  lt?: InputMaybe<Scalars['Int']['input']>;
  lte?: InputMaybe<Scalars['Int']['input']>;
  ne?: InputMaybe<Scalars['Int']['input']>;
  notIn?: InputMaybe<Array<Scalars['Int']['input']>>;
};

/** Represents a Large Language Model configuration. */
export type LargeLanguageModel = {
  __typename?: 'LargeLanguageModel';
  /** API key for authentication */
  apiKey?: Maybe<Scalars['String']['output']>;
  /** Cost metric for this model */
  cost: Scalars['Int']['output'];
  /** Unique identifier of the LLM configuration */
  id: Scalars['UUID']['output'];
  /** Maximum complexity rating supported */
  maxComplexity: Scalars['Int']['output'];
  /** Maximum concurrent requests supported */
  maxConcurrency: Scalars['Int']['output'];
  /** Model identifier */
  model: Scalars['String']['output'];
  /** Optional alias for the model */
  modelAlias?: Maybe<Scalars['String']['output']>;
  /** Base URL of the LLM API */
  url: Scalars['String']['output'];
};

export type LargeLanguageModelConnection = Connection & {
  __typename?: 'LargeLanguageModelConnection';
  edges: Array<LargeLanguageModelEdge>;
  nodes: Array<LargeLanguageModel>;
  pageInfo: PageInfo;
  totalCount: Scalars['Int']['output'];
};

export type LargeLanguageModelEdge = Edge & {
  __typename?: 'LargeLanguageModelEdge';
  cursor: Scalars['String']['output'];
  node: LargeLanguageModel;
};

export type LargeLanguageModelFilterInput = {
  and?: InputMaybe<Array<LargeLanguageModelFilterInput>>;
  cost?: InputMaybe<IntFilterInput>;
  id?: InputMaybe<UUIDFilterInput>;
  maxComplexity?: InputMaybe<IntFilterInput>;
  maxConcurrency?: InputMaybe<IntFilterInput>;
  model?: InputMaybe<StringFilterInput>;
  modelAlias?: InputMaybe<StringFilterInput>;
  or?: InputMaybe<Array<LargeLanguageModelFilterInput>>;
  url?: InputMaybe<StringFilterInput>;
};

export type LargeLanguageModelSortInput = {
  cost?: InputMaybe<SortOperationKind>;
  id?: InputMaybe<SortOperationKind>;
  maxComplexity?: InputMaybe<SortOperationKind>;
  maxConcurrency?: InputMaybe<SortOperationKind>;
  model?: InputMaybe<SortOperationKind>;
  modelAlias?: InputMaybe<SortOperationKind>;
  url?: InputMaybe<SortOperationKind>;
};

/** The root mutation type */
export type Mutation = {
  __typename?: 'Mutation';
  /** Create a new agent task */
  createAgentTask?: Maybe<AgentTask>;
  /** Create a new deliverable */
  createDeliverable?: Maybe<Deliverable>;
  /** Create a new LLM configuration */
  createLargeLanguageModel?: Maybe<LargeLanguageModel>;
  /** Create a new project */
  createProject?: Maybe<Project>;
  /** Delete an agent task */
  deleteAgentTask?: Maybe<Scalars['Boolean']['output']>;
  /** Delete a deliverable */
  deleteDeliverable?: Maybe<Scalars['Boolean']['output']>;
  /** Delete an LLM configuration */
  deleteLargeLanguageModel?: Maybe<Scalars['Boolean']['output']>;
  /** Delete a project */
  deleteProject?: Maybe<Scalars['Boolean']['output']>;
  /** Cleanup test data (development only) */
  deleteTestData: DeleteTestDataPayload;
  /** Update an existing agent task */
  updateAgentTask?: Maybe<AgentTask>;
  /** Update agent task status */
  updateAgentTaskStatus?: Maybe<AgentTaskStatus>;
  /** Update an existing deliverable */
  updateDeliverable?: Maybe<Deliverable>;
  /** Update deliverable status */
  updateDeliverableStatus?: Maybe<DeliverableStatus>;
  /** Update an existing LLM configuration */
  updateLargeLanguageModel?: Maybe<LargeLanguageModel>;
  /** Update an existing project */
  updateProject?: Maybe<Project>;
};


/** The root mutation type */
export type MutationcreateAgentTaskArgs = {
  input: CreateAgentTaskInput;
};


/** The root mutation type */
export type MutationcreateDeliverableArgs = {
  input: CreateDeliverableInput;
};


/** The root mutation type */
export type MutationcreateLargeLanguageModelArgs = {
  input: CreateLargeLanguageModelInput;
};


/** The root mutation type */
export type MutationcreateProjectArgs = {
  input: CreateProjectInput;
};


/** The root mutation type */
export type MutationdeleteAgentTaskArgs = {
  id: Scalars['UUID']['input'];
};


/** The root mutation type */
export type MutationdeleteDeliverableArgs = {
  id: Scalars['UUID']['input'];
};


/** The root mutation type */
export type MutationdeleteLargeLanguageModelArgs = {
  id: Scalars['UUID']['input'];
};


/** The root mutation type */
export type MutationdeleteProjectArgs = {
  id: Scalars['UUID']['input'];
};


/** The root mutation type */
export type MutationupdateAgentTaskArgs = {
  input: UpdateAgentTaskInput;
};


/** The root mutation type */
export type MutationupdateAgentTaskStatusArgs = {
  id: Scalars['UUID']['input'];
  targetStatus: AgentTaskStatus;
};


/** The root mutation type */
export type MutationupdateDeliverableArgs = {
  input: UpdateDeliverableInput;
};


/** The root mutation type */
export type MutationupdateDeliverableStatusArgs = {
  input: UpdateDeliverableStatusInput;
};


/** The root mutation type */
export type MutationupdateLargeLanguageModelArgs = {
  input: UpdateLargeLanguageModelInput;
};


/** The root mutation type */
export type MutationupdateProjectArgs = {
  input: UpdateProjectInput;
};

export type PageInfo = {
  __typename?: 'PageInfo';
  endCursor?: Maybe<Scalars['String']['output']>;
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  startCursor?: Maybe<Scalars['String']['output']>;
};

/** Represents a software project in the DevStack system. */
export type Project = {
  __typename?: 'Project';
  /** Deliverables associated with this project */
  deliverables?: Maybe<DeliverableConnection>;
  /** Description of the project */
  description?: Maybe<Scalars['String']['output']>;
  /** Unique identifier of the project */
  id: Scalars['UUID']['output'];
  /** Name of the project */
  name: Scalars['String']['output'];
  /** Git repository URL */
  repository: Scalars['String']['output'];
};


/** Represents a software project in the DevStack system. */
export type ProjectdeliverablesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<DeliverableFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<DeliverableSortInput>>;
};

export type ProjectConnection = Connection & {
  __typename?: 'ProjectConnection';
  edges: Array<ProjectEdge>;
  nodes: Array<Project>;
  pageInfo: PageInfo;
  totalCount: Scalars['Int']['output'];
};

export type ProjectEdge = Edge & {
  __typename?: 'ProjectEdge';
  cursor: Scalars['String']['output'];
  node: Project;
};

export type ProjectFilterInput = {
  and?: InputMaybe<Array<ProjectFilterInput>>;
  description?: InputMaybe<StringFilterInput>;
  id?: InputMaybe<UUIDFilterInput>;
  name?: InputMaybe<StringFilterInput>;
  or?: InputMaybe<Array<ProjectFilterInput>>;
  repository?: InputMaybe<StringFilterInput>;
};

export type ProjectSortInput = {
  description?: InputMaybe<SortOperationKind>;
  id?: InputMaybe<SortOperationKind>;
  name?: InputMaybe<SortOperationKind>;
  repository?: InputMaybe<SortOperationKind>;
};

/** The root query type */
export type Query = {
  __typename?: 'Query';
  /** Get a single agent task by ID */
  agentTask?: Maybe<AgentTask>;
  /** Get all agent tasks with optional filtering and pagination */
  agentTasks?: Maybe<AgentTaskConnection>;
  /** Get a single deliverable by ID */
  deliverable?: Maybe<Deliverable>;
  /** Get all deliverables with optional filtering and pagination */
  deliverables?: Maybe<DeliverableConnection>;
  /** Get a single LLM configuration by ID */
  largeLanguageModel?: Maybe<LargeLanguageModel>;
  /** Get all LLM configurations with optional filtering and pagination */
  largeLanguageModels?: Maybe<LargeLanguageModelConnection>;
  /** Get a single project by ID */
  project?: Maybe<Project>;
  /** Get all projects with optional filtering and pagination */
  projects?: Maybe<ProjectConnection>;
};


/** The root query type */
export type QueryagentTaskArgs = {
  id: Scalars['UUID']['input'];
};


/** The root query type */
export type QueryagentTasksArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<AgentTaskFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<AgentTaskSortInput>>;
};


/** The root query type */
export type QuerydeliverableArgs = {
  id: Scalars['UUID']['input'];
};


/** The root query type */
export type QuerydeliverablesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<DeliverableFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<DeliverableSortInput>>;
};


/** The root query type */
export type QuerylargeLanguageModelArgs = {
  id: Scalars['UUID']['input'];
};


/** The root query type */
export type QuerylargeLanguageModelsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<LargeLanguageModelFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<LargeLanguageModelSortInput>>;
};


/** The root query type */
export type QueryprojectArgs = {
  id: Scalars['UUID']['input'];
};


/** The root query type */
export type QueryprojectsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  filter?: InputMaybe<ProjectFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ProjectSortInput>>;
};

export const SortOperationKind = {
  ASC: 'ASC',
  ASC_NULLS_FIRST: 'ASC_NULLS_FIRST',
  ASC_NULLS_LAST: 'ASC_NULLS_LAST',
  DESC: 'DESC',
  DESC_NULLS_FIRST: 'DESC_NULLS_FIRST',
  DESC_NULLS_LAST: 'DESC_NULLS_LAST'
} as const;

export type SortOperationKind = typeof SortOperationKind[keyof typeof SortOperationKind];
export type StringFilterInput = {
  contains?: InputMaybe<Scalars['String']['input']>;
  endsWith?: InputMaybe<Scalars['String']['input']>;
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<Scalars['String']['input']>>;
  ne?: InputMaybe<Scalars['String']['input']>;
  notContains?: InputMaybe<Scalars['String']['input']>;
  notIn?: InputMaybe<Array<Scalars['String']['input']>>;
  startsWith?: InputMaybe<Scalars['String']['input']>;
};

export type UUIDFilterInput = {
  eq?: InputMaybe<Scalars['UUID']['input']>;
  in?: InputMaybe<Array<Scalars['UUID']['input']>>;
  ne?: InputMaybe<Scalars['UUID']['input']>;
  notIn?: InputMaybe<Array<Scalars['UUID']['input']>>;
};

/** Input for updating an existing agent task */
export type UpdateAgentTaskInput = {
  /** Agent that executed the task */
  agent?: InputMaybe<Scalars['String']['input']>;
  /** Git commit hash */
  commitHash?: InputMaybe<Scalars['String']['input']>;
  /** Number of completion tokens */
  completionTokens?: InputMaybe<Scalars['Int']['input']>;
  /** New complexity rating */
  complexityRating?: InputMaybe<Scalars['Int']['input']>;
  /** New dependency */
  dependsOnAgentTaskId?: InputMaybe<Scalars['UUID']['input']>;
  /** New description */
  description?: InputMaybe<Scalars['String']['input']>;
  /** Errors encountered */
  errors?: InputMaybe<Scalars['String']['input']>;
  /** Execution duration in seconds */
  executionDurationInSeconds?: InputMaybe<Scalars['Int']['input']>;
  /** Unique identifier of the task to update */
  id: Scalars['UUID']['input'];
  /** Number of prompt tokens */
  promptTokens?: InputMaybe<Scalars['Int']['input']>;
  /** Result of the task */
  result?: InputMaybe<Scalars['String']['input']>;
  /** New title */
  title?: InputMaybe<Scalars['String']['input']>;
};

/** Input for updating agent task status */
export type UpdateAgentTaskStatusInput = {
  /** Unique identifier of the task */
  id: Scalars['UUID']['input'];
  /** Target status */
  targetStatus: AgentTaskStatus;
};

/** Input for updating an existing deliverable */
export type UpdateDeliverableInput = {
  /** New acceptance criteria */
  acceptanceCriteria?: InputMaybe<Scalars['String']['input']>;
  /** Agent feedback */
  agentFeedback?: InputMaybe<Scalars['String']['input']>;
  /** Blocking issues */
  blocking?: InputMaybe<Scalars['String']['input']>;
  /** New deployment plan */
  deploymentPlan?: InputMaybe<Scalars['String']['input']>;
  /** New description */
  description?: InputMaybe<Scalars['String']['input']>;
  /** New execution plan */
  executionPlan?: InputMaybe<Scalars['String']['input']>;
  /** Unique identifier of the deliverable to update */
  id: Scalars['UUID']['input'];
  /** New performance impact assessment */
  performanceImpact?: InputMaybe<Scalars['String']['input']>;
  /** New security impact assessment */
  securityImpact?: InputMaybe<Scalars['String']['input']>;
  /** New test plan */
  testPlan?: InputMaybe<Scalars['String']['input']>;
  /** New title */
  title?: InputMaybe<Scalars['String']['input']>;
};

/** Input for updating deliverable status */
export type UpdateDeliverableStatusInput = {
  /** Actor who initiated the status change */
  actor?: InputMaybe<Scalars['String']['input']>;
  /** Unique identifier of the deliverable */
  id: Scalars['UUID']['input'];
  /** Target status */
  targetStatus: DeliverableStatus;
};

/** Input for updating an existing LLM configuration */
export type UpdateLargeLanguageModelInput = {
  /** New API key */
  apiKey?: InputMaybe<Scalars['String']['input']>;
  /** New cost metric */
  cost?: InputMaybe<Scalars['Int']['input']>;
  /** Unique identifier of the LLM configuration */
  id: Scalars['UUID']['input'];
  /** New maximum complexity */
  maxComplexity?: InputMaybe<Scalars['Int']['input']>;
  /** New maximum concurrency */
  maxConcurrency?: InputMaybe<Scalars['Int']['input']>;
  /** New model identifier */
  model?: InputMaybe<Scalars['String']['input']>;
  /** New model alias */
  modelAlias?: InputMaybe<Scalars['String']['input']>;
  /** New base URL */
  url?: InputMaybe<Scalars['String']['input']>;
};

/** Input for updating an existing project */
export type UpdateProjectInput = {
  /** New description for the project */
  description?: InputMaybe<Scalars['String']['input']>;
  /** Unique identifier of the project to update */
  id: Scalars['UUID']['input'];
  /** New name for the project */
  name?: InputMaybe<Scalars['String']['input']>;
  /** New repository URL */
  repository?: InputMaybe<Scalars['String']['input']>;
};

export type CreateAgentTaskMutationVariables = Exact<{
  input: CreateAgentTaskInput;
}>;


export type CreateAgentTaskMutation = { __typename?: 'Mutation', createAgentTask?: { __typename?: 'AgentTask', id: any, title: string, deliverableId: any, description: string, complexityRating: number, result?: string | null, errors?: string | null, commitHash?: string | null, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null } | null };

export type CreateDeliverableMutationVariables = Exact<{
  input: CreateDeliverableInput;
}>;


export type CreateDeliverableMutation = { __typename?: 'Mutation', createDeliverable?: { __typename?: 'Deliverable', id: any, title: string, status: DeliverableStatus } | null };

export type CreateLargeLanguageModelMutationVariables = Exact<{
  input: CreateLargeLanguageModelInput;
}>;


export type CreateLargeLanguageModelMutation = { __typename?: 'Mutation', createLargeLanguageModel?: { __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias?: string | null, maxComplexity: number, maxConcurrency: number } | null };

export type CreateProjectMutationVariables = Exact<{
  input: CreateProjectInput;
}>;


export type CreateProjectMutation = { __typename?: 'Mutation', createProject?: { __typename?: 'Project', id: any, name: string, description?: string | null, repository: string } | null };

export type DeleteAgentTaskMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteAgentTaskMutation = { __typename?: 'Mutation', deleteAgentTask?: boolean | null };

export type DeleteDeliverableMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteDeliverableMutation = { __typename?: 'Mutation', deleteDeliverable?: boolean | null };

export type DeleteLargeLanguageModelMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteLargeLanguageModelMutation = { __typename?: 'Mutation', deleteLargeLanguageModel?: boolean | null };

export type DeleteProjectMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteProjectMutation = { __typename?: 'Mutation', deleteProject?: boolean | null };

export type UpdateAgentTaskStatusMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  targetStatus: AgentTaskStatus;
}>;


export type UpdateAgentTaskStatusMutation = { __typename?: 'Mutation', updateAgentTaskStatus?: AgentTaskStatus | null };

export type UpdateDeliverableStatusMutationVariables = Exact<{
  input: UpdateDeliverableStatusInput;
}>;


export type UpdateDeliverableStatusMutation = { __typename?: 'Mutation', updateDeliverableStatus?: DeliverableStatus | null };

export type UpdateAgentTaskMutationVariables = Exact<{
  input: UpdateAgentTaskInput;
}>;


export type UpdateAgentTaskMutation = { __typename?: 'Mutation', updateAgentTask?: { __typename?: 'AgentTask', id: any, title: string, deliverableId: any, description: string, complexityRating: number, result?: string | null, errors?: string | null, commitHash?: string | null, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null } | null };

export type UpdateDeliverableMutationVariables = Exact<{
  input: UpdateDeliverableInput;
}>;


export type UpdateDeliverableMutation = { __typename?: 'Mutation', updateDeliverable?: { __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null } | null };

export type UpdateLargeLanguageModelMutationVariables = Exact<{
  input: UpdateLargeLanguageModelInput;
}>;


export type UpdateLargeLanguageModelMutation = { __typename?: 'Mutation', updateLargeLanguageModel?: { __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias?: string | null, maxComplexity: number, maxConcurrency: number } | null };

export type UpdateProjectMutationVariables = Exact<{
  input: UpdateProjectInput;
}>;


export type UpdateProjectMutation = { __typename?: 'Mutation', updateProject?: { __typename?: 'Project', id: any, name: string, description?: string | null, repository: string } | null };

export type GetAgentTaskQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetAgentTaskQuery = { __typename?: 'Query', agentTask?: { __typename?: 'AgentTask', id: any, title: string, status: AgentTaskStatus, deliverableId: any, description: string, result?: string | null, errors?: string | null, commitHash?: string | null, complexityRating: number, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null } | null };

export type GetAgentTasksQueryVariables = Exact<{
  deliverableId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type GetAgentTasksQuery = { __typename?: 'Query', agentTasks?: { __typename?: 'AgentTaskConnection', nodes: Array<{ __typename?: 'AgentTask', id: any, title: string, status: AgentTaskStatus, deliverableId: any, description: string, result?: string | null, errors?: string | null, commitHash?: string | null, complexityRating: number, dependsOnAgentTaskId?: any | null, promptTokens?: number | null, completionTokens?: number | null, executionDurationInSeconds?: number | null, agent?: string | null }> } | null };

export type GetDeliverablesQueryVariables = Exact<{
  projectId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type GetDeliverablesQuery = { __typename?: 'Query', deliverables?: { __typename?: 'DeliverableConnection', nodes: Array<{ __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, projectId: any, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null }> } | null };

export type GetDeliverableQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDeliverableQuery = { __typename?: 'Query', deliverable?: { __typename?: 'Deliverable', id: any, title: string, description?: string | null, status: DeliverableStatus, type: DeliverableType, projectId: any, acceptanceCriteria?: string | null, executionPlan?: string | null, agentFeedback?: string | null, securityImpact?: string | null, performanceImpact?: string | null, testPlan?: string | null, deploymentPlan?: string | null, blocking?: string | null } | null };

export type ModelConfigurationsQueryVariables = Exact<{ [key: string]: never; }>;


export type ModelConfigurationsQuery = { __typename?: 'Query', largeLanguageModels?: { __typename?: 'LargeLanguageModelConnection', nodes: Array<{ __typename?: 'LargeLanguageModel', id: any, url: string, model: string, modelAlias?: string | null, apiKey?: string | null, maxComplexity: number, maxConcurrency: number }> } | null };

export type GetProjectQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetProjectQuery = { __typename?: 'Query', project?: { __typename?: 'Project', id: any, name: string, description?: string | null, repository: string } | null };

export type GetProjectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetProjectsQuery = { __typename?: 'Query', projects?: { __typename?: 'ProjectConnection', nodes: Array<{ __typename?: 'Project', id: any, name: string, description?: string | null, repository: string }> } | null };


export const CreateAgentTaskDocument = gql`
    mutation CreateAgentTask($input: CreateAgentTaskInput!) {
  createAgentTask(input: $input) {
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
    mutation UpdateDeliverableStatus($input: UpdateDeliverableStatusInput!) {
  updateDeliverableStatus(input: $input)
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
 *      input: // value for 'input'
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
    query GetAgentTasks($deliverableId: UUID) {
  agentTasks(filter: {deliverableId: {eq: $deliverableId}}) {
    nodes {
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
    query GetDeliverables($projectId: UUID) {
  deliverables(filter: {projectId: {eq: $projectId}}) {
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
    }
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
 *      projectId: // value for 'projectId'
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