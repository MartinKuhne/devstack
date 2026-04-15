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
  DateTime: { input: any; output: any; }
  UUID: { input: any; output: any; }
};

export type AgentTask = {
  __typename?: 'AgentTask';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  complexityRating: Maybe<Scalars['Int']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  deliverable: Maybe<Scalars['String']['output']>;
  feature: Maybe<Feature>;
  featureId: Maybe<Scalars['ID']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  requiredFollowUps: Maybe<Scalars['String']['output']>;
  result: Maybe<Scalars['String']['output']>;
  risks: Maybe<Scalars['String']['output']>;
  status: Maybe<TaskStatus>;
  title: Maybe<Scalars['String']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
};

export type AuditEvent = {
  __typename?: 'AuditEvent';
  actor: Maybe<Scalars['String']['output']>;
  entityId: Maybe<Scalars['ID']['output']>;
  entityType: Maybe<Scalars['String']['output']>;
  eventType: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  newValue: Maybe<Scalars['String']['output']>;
  occurredAt: Maybe<Scalars['DateTime']['output']>;
  oldValue: Maybe<Scalars['String']['output']>;
};

export type CancelWorkflowRunInput = {
  id: Scalars['UUID']['input'];
};

export type CreateDefectInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  initialStatus: InputMaybe<FeatureStatus>;
  openQuestions: InputMaybe<Scalars['String']['input']>;
  parentFeatureId: InputMaybe<Scalars['UUID']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['UUID']['input'];
  securityImpact: InputMaybe<Scalars['String']['input']>;
  severity: InputMaybe<Severity>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
};

export type CreateEpicInput = {
  description: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
};

export type CreateFeatureInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  initialStatus: InputMaybe<FeatureStatus>;
  openQuestions: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['UUID']['input'];
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
};

export type CreateModelConfigurationInput = {
  apiKey: Scalars['String']['input'];
  maxComplexity: Scalars['Int']['input'];
  model: Scalars['String']['input'];
  modelAlias: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['UUID']['input'];
  url: Scalars['String']['input'];
};

export type CreateProjectInput = {
  architecture: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  githubUrl: InputMaybe<Scalars['String']['input']>;
  memory: InputMaybe<Scalars['String']['input']>;
  name: Scalars['String']['input'];
};

export type CreateTaskInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  complexityRating: Scalars['Int']['input'];
  deliverable: InputMaybe<Scalars['String']['input']>;
  featureId: Scalars['UUID']['input'];
  requiredFollowUps: InputMaybe<Scalars['String']['input']>;
  result: InputMaybe<Scalars['String']['input']>;
  risks: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
};

export type CreateWorkflowRunInput = {
  featureId: InputMaybe<Scalars['UUID']['input']>;
  inputPayload: Scalars['String']['input'];
  projectId: Scalars['UUID']['input'];
  taskId: InputMaybe<Scalars['UUID']['input']>;
  workflowType: WorkflowType;
};

export type DashboardSummary = {
  __typename?: 'DashboardSummary';
  featuresFailed: Scalars['Int']['output'];
  featuresInReview: Scalars['Int']['output'];
  projectsInFlight: Scalars['Int']['output'];
  recentAuditEvents: Array<AuditEvent>;
  tasksFailed: Scalars['Int']['output'];
  tasksInProgress: Scalars['Int']['output'];
};

export type Defect = {
  __typename?: 'Defect';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  deploymentPlan: Maybe<Scalars['String']['output']>;
  description: Maybe<Scalars['String']['output']>;
  errors: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  openQuestions: Maybe<Scalars['String']['output']>;
  parentFeature: Maybe<Feature>;
  parentFeatureId: Maybe<Scalars['ID']['output']>;
  performanceImpact: Maybe<Scalars['String']['output']>;
  plan: Maybe<Scalars['String']['output']>;
  projectId: Maybe<Scalars['ID']['output']>;
  result: Maybe<Scalars['String']['output']>;
  securityImpact: Maybe<Scalars['String']['output']>;
  severity: Maybe<Severity>;
  status: Maybe<FeatureStatus>;
  testPlan: Maybe<Scalars['String']['output']>;
  title: Maybe<Scalars['String']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
  version: Maybe<Scalars['Int']['output']>;
};

export type DefectConnection = {
  __typename?: 'DefectConnection';
  nodes: Array<Defect>;
  pageInfo: DefectPageInfo;
  totalCount: Scalars['Int']['output'];
};

export type DefectPageInfo = {
  __typename?: 'DefectPageInfo';
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  totalCount: Scalars['Int']['output'];
};

export type DefectPayload = {
  __typename?: 'DefectPayload';
  defect: Maybe<Defect>;
  errors: Array<Scalars['String']['output']>;
};

export type DeleteDefectInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteEpicInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteFeatureInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteModelConfigurationInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteProjectInput = {
  id: Scalars['UUID']['input'];
};

export type DeleteTaskInput = {
  id: Scalars['UUID']['input'];
};

export type Epic = {
  __typename?: 'Epic';
  createdAt: Maybe<Scalars['DateTime']['output']>;
  description: Maybe<Scalars['String']['output']>;
  features: Maybe<Array<Maybe<Feature>>>;
  id: Maybe<Scalars['ID']['output']>;
  title: Maybe<Scalars['String']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
};

export type EpicConnection = {
  __typename?: 'EpicConnection';
  nodes: Array<Epic>;
  pageInfo: EpicPageInfo;
  totalCount: Scalars['Int']['output'];
};

export type EpicPageInfo = {
  __typename?: 'EpicPageInfo';
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  totalCount: Scalars['Int']['output'];
};

export type EpicPayload = {
  __typename?: 'EpicPayload';
  epic: Maybe<Epic>;
  errors: Array<Scalars['String']['output']>;
};

export type Feature = {
  __typename?: 'Feature';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  deploymentPlan: Maybe<Scalars['String']['output']>;
  description: Maybe<Scalars['String']['output']>;
  epic: Maybe<Epic>;
  epicId: Maybe<Scalars['ID']['output']>;
  errors: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  openQuestions: Maybe<Scalars['String']['output']>;
  performanceImpact: Maybe<Scalars['String']['output']>;
  plan: Maybe<Scalars['String']['output']>;
  projectId: Maybe<Scalars['ID']['output']>;
  result: Maybe<Scalars['String']['output']>;
  securityImpact: Maybe<Scalars['String']['output']>;
  status: Maybe<FeatureStatus>;
  tasks: Maybe<Array<Maybe<AgentTask>>>;
  testPlan: Maybe<Scalars['String']['output']>;
  title: Maybe<Scalars['String']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
  validStatusTransitions: Maybe<Array<Maybe<FeatureStatus>>>;
};

export type FeatureConnection = {
  __typename?: 'FeatureConnection';
  nodes: Array<Feature>;
  pageInfo: FeaturePageInfo;
  totalCount: Scalars['Int']['output'];
};

export type FeaturePageInfo = {
  __typename?: 'FeaturePageInfo';
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  totalCount: Scalars['Int']['output'];
};

export type FeaturePayload = {
  __typename?: 'FeaturePayload';
  errors: Array<Scalars['String']['output']>;
  feature: Maybe<Feature>;
};

export const FeatureStatus = {
  DONE: 'DONE',
  FAILED: 'FAILED',
  IN_PROGRESS: 'IN_PROGRESS',
  IN_REVIEW: 'IN_REVIEW',
  PLANNING: 'PLANNING',
  READY: 'READY',
  READY_FOR_TEST: 'READY_FOR_TEST',
  REJECTED: 'REJECTED',
  TESTING: 'TESTING'
} as const;

export type FeatureStatus = typeof FeatureStatus[keyof typeof FeatureStatus];
export type ModelConfiguration = {
  __typename?: 'ModelConfiguration';
  apiKey_Encrypted: Maybe<Scalars['String']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  maxComplexity: Maybe<Scalars['Int']['output']>;
  model: Maybe<Scalars['String']['output']>;
  modelAlias: Maybe<Scalars['String']['output']>;
  projectId: Maybe<Scalars['ID']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
  url: Maybe<Scalars['String']['output']>;
};

export type ModelConfigurationPayload = {
  __typename?: 'ModelConfigurationPayload';
  errors: Array<Scalars['String']['output']>;
  modelConfiguration: Maybe<ModelConfiguration>;
};

export type Mutation = {
  __typename?: 'Mutation';
  cancelWorkflowRun: WorkflowRunPayload;
  createDefect: DefectPayload;
  createEpic: EpicPayload;
  createFeature: FeaturePayload;
  createModelConfiguration: ModelConfigurationPayload;
  createProject: ProjectPayload;
  createTask: TaskPayload;
  createWorkflowRun: WorkflowRunPayload;
  deleteDefect: DefectPayload;
  deleteEpic: EpicPayload;
  deleteFeature: FeaturePayload;
  deleteModelConfiguration: ModelConfigurationPayload;
  deleteProject: ProjectPayload;
  deleteTask: TaskPayload;
  transitionDefectStatus: DefectPayload;
  transitionFeatureStatus: FeaturePayload;
  transitionTaskStatus: TaskPayload;
  updateDefect: DefectPayload;
  updateEpic: EpicPayload;
  updateFeature: FeaturePayload;
  updateModelConfiguration: ModelConfigurationPayload;
  updateProject: ProjectPayload;
  updateTask: TaskPayload;
  updateWorkflowRun: WorkflowRunPayload;
};


export type MutationcancelWorkflowRunArgs = {
  input: CancelWorkflowRunInput;
};


export type MutationcreateDefectArgs = {
  input: CreateDefectInput;
};


export type MutationcreateEpicArgs = {
  input: CreateEpicInput;
};


export type MutationcreateFeatureArgs = {
  input: CreateFeatureInput;
};


export type MutationcreateModelConfigurationArgs = {
  input: CreateModelConfigurationInput;
};


export type MutationcreateProjectArgs = {
  input: CreateProjectInput;
};


export type MutationcreateTaskArgs = {
  input: CreateTaskInput;
};


export type MutationcreateWorkflowRunArgs = {
  input: CreateWorkflowRunInput;
};


export type MutationdeleteDefectArgs = {
  input: DeleteDefectInput;
};


export type MutationdeleteEpicArgs = {
  input: DeleteEpicInput;
};


export type MutationdeleteFeatureArgs = {
  input: DeleteFeatureInput;
};


export type MutationdeleteModelConfigurationArgs = {
  input: DeleteModelConfigurationInput;
};


export type MutationdeleteProjectArgs = {
  input: DeleteProjectInput;
};


export type MutationdeleteTaskArgs = {
  input: DeleteTaskInput;
};


export type MutationtransitionDefectStatusArgs = {
  input: TransitionDefectInput;
};


export type MutationtransitionFeatureStatusArgs = {
  input: TransitionFeatureInput;
};


export type MutationtransitionTaskStatusArgs = {
  input: TransitionTaskInput;
};


export type MutationupdateDefectArgs = {
  input: UpdateDefectInput;
};


export type MutationupdateEpicArgs = {
  input: UpdateEpicInput;
};


export type MutationupdateFeatureArgs = {
  input: UpdateFeatureInput;
};


export type MutationupdateModelConfigurationArgs = {
  input: UpdateModelConfigurationInput;
};


export type MutationupdateProjectArgs = {
  input: UpdateProjectInput;
};


export type MutationupdateTaskArgs = {
  input: UpdateTaskInput;
};


export type MutationupdateWorkflowRunArgs = {
  input: UpdateWorkflowRunInput;
};

export type Project = {
  __typename?: 'Project';
  architecture: Maybe<Scalars['String']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  defects: Maybe<Array<Maybe<Defect>>>;
  description: Maybe<Scalars['String']['output']>;
  features: Maybe<Array<Maybe<Feature>>>;
  githubToken_Encrypted: Maybe<Scalars['String']['output']>;
  githubUrl: Maybe<Scalars['String']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  memory: Maybe<Scalars['String']['output']>;
  modelConfigurations: Maybe<Array<Maybe<ModelConfiguration>>>;
  name: Maybe<Scalars['String']['output']>;
  updatedAt: Maybe<Scalars['DateTime']['output']>;
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
  auditEvents: Array<AuditEvent>;
  dashboardSummary: DashboardSummary;
  defectById: Maybe<Defect>;
  defects: DefectConnection;
  epicById: Maybe<Epic>;
  epics: EpicConnection;
  featureById: Maybe<Feature>;
  features: FeatureConnection;
  modelConfigurations: Array<ModelConfiguration>;
  projectById: Maybe<Project>;
  projects: ProjectConnection;
  taskById: Maybe<AgentTask>;
  tasks: TaskConnection;
  validStatusTransitions: Array<FeatureStatus>;
};


export type QueryauditEventsArgs = {
  entityId: Scalars['UUID']['input'];
  take?: Scalars['Int']['input'];
};


export type QuerydefectByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QuerydefectsArgs = {
  first?: Scalars['Int']['input'];
  projectId: InputMaybe<Scalars['UUID']['input']>;
  skip: InputMaybe<Scalars['Int']['input']>;
};


export type QueryepicByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryepicsArgs = {
  first?: Scalars['Int']['input'];
  skip: InputMaybe<Scalars['Int']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};


export type QueryfeatureByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryfeaturesArgs = {
  createdAfter: InputMaybe<Scalars['DateTime']['input']>;
  createdBefore: InputMaybe<Scalars['DateTime']['input']>;
  epicId: InputMaybe<Scalars['UUID']['input']>;
  first?: Scalars['Int']['input'];
  projectId: InputMaybe<Scalars['UUID']['input']>;
  skip: InputMaybe<Scalars['Int']['input']>;
  status: InputMaybe<Array<FeatureStatus>>;
};


export type QuerymodelConfigurationsArgs = {
  projectId: Scalars['UUID']['input'];
};


export type QueryprojectByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryprojectsArgs = {
  first?: Scalars['Int']['input'];
  skip: InputMaybe<Scalars['Int']['input']>;
};


export type QuerytaskByIdArgs = {
  id: Scalars['UUID']['input'];
};


export type QuerytasksArgs = {
  createdAfter: InputMaybe<Scalars['DateTime']['input']>;
  createdBefore: InputMaybe<Scalars['DateTime']['input']>;
  featureId: InputMaybe<Scalars['UUID']['input']>;
  first?: Scalars['Int']['input'];
  skip: InputMaybe<Scalars['Int']['input']>;
  status: InputMaybe<Array<TaskStatus>>;
};


export type QueryvalidStatusTransitionsArgs = {
  featureId: Scalars['UUID']['input'];
};

export const Severity = {
  CRITICAL: 'CRITICAL',
  HIGH: 'HIGH',
  LOW: 'LOW',
  MEDIUM: 'MEDIUM'
} as const;

export type Severity = typeof Severity[keyof typeof Severity];
export type TaskConnection = {
  __typename?: 'TaskConnection';
  nodes: Array<AgentTask>;
  pageInfo: TaskPageInfo;
  totalCount: Scalars['Int']['output'];
};

export type TaskPageInfo = {
  __typename?: 'TaskPageInfo';
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  totalCount: Scalars['Int']['output'];
};

export type TaskPayload = {
  __typename?: 'TaskPayload';
  errors: Array<Scalars['String']['output']>;
  task: Maybe<AgentTask>;
};

export const TaskStatus = {
  CODE: 'CODE',
  DONE: 'DONE',
  FAILED: 'FAILED',
  IN_REVIEW: 'IN_REVIEW',
  PLANNING: 'PLANNING',
  PREPARE: 'PREPARE',
  READY: 'READY',
  READY_FOR_TEST: 'READY_FOR_TEST',
  REJECTED: 'REJECTED',
  REVIEW: 'REVIEW',
  TESTING: 'TESTING'
} as const;

export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];
export type TransitionDefectInput = {
  actor: Scalars['String']['input'];
  id: Scalars['UUID']['input'];
  targetStatus: FeatureStatus;
};

export type TransitionFeatureInput = {
  actor: Scalars['String']['input'];
  id: Scalars['UUID']['input'];
  targetStatus: FeatureStatus;
};

export type TransitionTaskInput = {
  actor: Scalars['String']['input'];
  id: Scalars['UUID']['input'];
  targetStatus: TaskStatus;
};

export type UpdateDefectInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  openQuestions: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateEpicInput = {
  description: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateFeatureInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  openQuestions: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateModelConfigurationInput = {
  apiKey: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  maxComplexity: InputMaybe<Scalars['Int']['input']>;
  model: InputMaybe<Scalars['String']['input']>;
  modelAlias: InputMaybe<Scalars['String']['input']>;
  url: InputMaybe<Scalars['String']['input']>;
};

export type UpdateProjectInput = {
  architecture: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  githubToken_Encrypted: InputMaybe<Scalars['String']['input']>;
  githubUrl: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  memory: InputMaybe<Scalars['String']['input']>;
  name: InputMaybe<Scalars['String']['input']>;
};

export type UpdateTaskInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  complexityRating: InputMaybe<Scalars['Int']['input']>;
  deliverable: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  requiredFollowUps: InputMaybe<Scalars['String']['input']>;
  result: InputMaybe<Scalars['String']['input']>;
  risks: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateWorkflowRunInput = {
  id: Scalars['UUID']['input'];
  outputPayload: InputMaybe<Scalars['String']['input']>;
  status: WorkflowRunStatus;
};

export type WorkflowRun = {
  __typename?: 'WorkflowRun';
  completedAt: Maybe<Scalars['DateTime']['output']>;
  createdAt: Maybe<Scalars['DateTime']['output']>;
  errorMessage: Maybe<Scalars['String']['output']>;
  feature: Maybe<Feature>;
  featureId: Maybe<Scalars['ID']['output']>;
  id: Maybe<Scalars['ID']['output']>;
  inputPayload: Maybe<Scalars['String']['output']>;
  outputPayload: Maybe<Scalars['String']['output']>;
  project: Maybe<Project>;
  projectId: Maybe<Scalars['ID']['output']>;
  startedAt: Maybe<Scalars['DateTime']['output']>;
  status: Maybe<WorkflowRunStatus>;
  task: Maybe<AgentTask>;
  taskId: Maybe<Scalars['ID']['output']>;
  workflowType: Maybe<WorkflowType>;
};

export type WorkflowRunPayload = {
  __typename?: 'WorkflowRunPayload';
  errors: Array<Scalars['String']['output']>;
  workflowRun: Maybe<WorkflowRun>;
};

export const WorkflowRunStatus = {
  CANCELLED: 'CANCELLED',
  FAILED: 'FAILED',
  QUEUED: 'QUEUED',
  RUNNING: 'RUNNING',
  SUCCEEDED: 'SUCCEEDED'
} as const;

export type WorkflowRunStatus = typeof WorkflowRunStatus[keyof typeof WorkflowRunStatus];
export const WorkflowType = {
  ARCHITECT: 'ARCHITECT',
  CODER: 'CODER',
  DEV_LEAD: 'DEV_LEAD',
  PLANNER: 'PLANNER',
  TESTER: 'TESTER'
} as const;

export type WorkflowType = typeof WorkflowType[keyof typeof WorkflowType];
export type CreateDefectMutationVariables = Exact<{
  input: CreateDefectInput;
}>;


export type CreateDefectMutation = { __typename?: 'Mutation', createDefect: { __typename?: 'DefectPayload', errors: Array<string>, defect: { __typename?: 'Defect', id: string | null, title: string | null, description: string | null, acceptanceCriteria: string | null, plan: string | null, result: string | null, errors: string | null, securityImpact: string | null, performanceImpact: string | null, status: FeatureStatus | null, severity: Severity | null, projectId: string | null, parentFeatureId: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateEpicMutationVariables = Exact<{
  input: CreateEpicInput;
}>;


export type CreateEpicMutation = { __typename?: 'Mutation', createEpic: { __typename?: 'EpicPayload', errors: Array<string>, epic: { __typename?: 'Epic', id: string | null, title: string | null, description: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateFeatureMutationVariables = Exact<{
  input: CreateFeatureInput;
}>;


export type CreateFeatureMutation = { __typename?: 'Mutation', createFeature: { __typename?: 'FeaturePayload', errors: Array<string>, feature: { __typename?: 'Feature', id: string | null, title: string | null, description: string | null, acceptanceCriteria: string | null, openQuestions: string | null, status: FeatureStatus | null, projectId: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateModelConfigurationMutationVariables = Exact<{
  input: CreateModelConfigurationInput;
}>;


export type CreateModelConfigurationMutation = { __typename?: 'Mutation', createModelConfiguration: { __typename?: 'ModelConfigurationPayload', errors: Array<string>, modelConfiguration: { __typename?: 'ModelConfiguration', id: string | null, projectId: string | null, url: string | null, model: string | null, modelAlias: string | null, maxComplexity: number | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateProjectMutationVariables = Exact<{
  input: CreateProjectInput;
}>;


export type CreateProjectMutation = { __typename?: 'Mutation', createProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateTaskMutationVariables = Exact<{
  input: CreateTaskInput;
}>;


export type CreateTaskMutation = { __typename?: 'Mutation', createTask: { __typename?: 'TaskPayload', errors: Array<string>, task: { __typename?: 'AgentTask', id: string | null, title: string | null, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexityRating: number | null, result: string | null, status: TaskStatus | null, featureId: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type CreateWorkflowRunMutationVariables = Exact<{
  input: CreateWorkflowRunInput;
}>;


export type CreateWorkflowRunMutation = { __typename?: 'Mutation', createWorkflowRun: { __typename?: 'WorkflowRunPayload', errors: Array<string>, workflowRun: { __typename?: 'WorkflowRun', id: string | null, projectId: string | null, featureId: string | null, taskId: string | null, workflowType: WorkflowType | null, status: WorkflowRunStatus | null, startedAt: any | null, createdAt: any | null } | null } };

export type DeleteDefectMutationVariables = Exact<{
  input: DeleteDefectInput;
}>;


export type DeleteDefectMutation = { __typename?: 'Mutation', deleteDefect: { __typename?: 'DefectPayload', errors: Array<string>, defect: { __typename?: 'Defect', id: string | null } | null } };

export type DeleteEpicMutationVariables = Exact<{
  input: DeleteEpicInput;
}>;


export type DeleteEpicMutation = { __typename?: 'Mutation', deleteEpic: { __typename?: 'EpicPayload', errors: Array<string>, epic: { __typename?: 'Epic', id: string | null } | null } };

export type DeleteFeatureMutationVariables = Exact<{
  input: DeleteFeatureInput;
}>;


export type DeleteFeatureMutation = { __typename?: 'Mutation', deleteFeature: { __typename?: 'FeaturePayload', errors: Array<string>, feature: { __typename?: 'Feature', id: string | null } | null } };

export type DeleteModelConfigurationMutationVariables = Exact<{
  input: DeleteModelConfigurationInput;
}>;


export type DeleteModelConfigurationMutation = { __typename?: 'Mutation', deleteModelConfiguration: { __typename?: 'ModelConfigurationPayload', errors: Array<string>, modelConfiguration: { __typename?: 'ModelConfiguration', id: string | null } | null } };

export type DeleteProjectMutationVariables = Exact<{
  input: DeleteProjectInput;
}>;


export type DeleteProjectMutation = { __typename?: 'Mutation', deleteProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null } | null } };

export type DeleteTaskMutationVariables = Exact<{
  input: DeleteTaskInput;
}>;


export type DeleteTaskMutation = { __typename?: 'Mutation', deleteTask: { __typename?: 'TaskPayload', errors: Array<string>, task: { __typename?: 'AgentTask', id: string | null } | null } };

export type TransitionDefectStatusMutationVariables = Exact<{
  input: TransitionDefectInput;
}>;


export type TransitionDefectStatusMutation = { __typename?: 'Mutation', transitionDefectStatus: { __typename?: 'DefectPayload', errors: Array<string>, defect: { __typename?: 'Defect', id: string | null, status: FeatureStatus | null } | null } };

export type TransitionFeatureStatusMutationVariables = Exact<{
  input: TransitionFeatureInput;
}>;


export type TransitionFeatureStatusMutation = { __typename?: 'Mutation', transitionFeatureStatus: { __typename?: 'FeaturePayload', errors: Array<string>, feature: { __typename?: 'Feature', id: string | null, status: FeatureStatus | null, validStatusTransitions: Array<FeatureStatus | null> | null } | null } };

export type TransitionTaskStatusMutationVariables = Exact<{
  input: TransitionTaskInput;
}>;


export type TransitionTaskStatusMutation = { __typename?: 'Mutation', transitionTaskStatus: { __typename?: 'TaskPayload', errors: Array<string>, task: { __typename?: 'AgentTask', id: string | null, status: TaskStatus | null } | null } };

export type UpdateDefectMutationVariables = Exact<{
  input: UpdateDefectInput;
}>;


export type UpdateDefectMutation = { __typename?: 'Mutation', updateDefect: { __typename?: 'DefectPayload', errors: Array<string>, defect: { __typename?: 'Defect', id: string | null, title: string | null, description: string | null, acceptanceCriteria: string | null, plan: string | null, result: string | null, errors: string | null, securityImpact: string | null, performanceImpact: string | null, status: FeatureStatus | null, severity: Severity | null, projectId: string | null, parentFeatureId: string | null, updatedAt: any | null } | null } };

export type UpdateEpicMutationVariables = Exact<{
  input: UpdateEpicInput;
}>;


export type UpdateEpicMutation = { __typename?: 'Mutation', updateEpic: { __typename?: 'EpicPayload', errors: Array<string>, epic: { __typename?: 'Epic', id: string | null, title: string | null, description: string | null, updatedAt: any | null } | null } };

export type UpdateFeatureMutationVariables = Exact<{
  input: UpdateFeatureInput;
}>;


export type UpdateFeatureMutation = { __typename?: 'Mutation', updateFeature: { __typename?: 'FeaturePayload', errors: Array<string>, feature: { __typename?: 'Feature', id: string | null, title: string | null, description: string | null, acceptanceCriteria: string | null, plan: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, openQuestions: string | null, status: FeatureStatus | null, updatedAt: any | null } | null } };

export type UpdateModelConfigurationMutationVariables = Exact<{
  input: UpdateModelConfigurationInput;
}>;


export type UpdateModelConfigurationMutation = { __typename?: 'Mutation', updateModelConfiguration: { __typename?: 'ModelConfigurationPayload', errors: Array<string>, modelConfiguration: { __typename?: 'ModelConfiguration', id: string | null, projectId: string | null, url: string | null, model: string | null, modelAlias: string | null, maxComplexity: number | null, updatedAt: any | null } | null } };

export type UpdateProjectMutationVariables = Exact<{
  input: UpdateProjectInput;
}>;


export type UpdateProjectMutation = { __typename?: 'Mutation', updateProject: { __typename?: 'ProjectPayload', errors: Array<string>, project: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null, createdAt: any | null, updatedAt: any | null } | null } };

export type UpdateTaskMutationVariables = Exact<{
  input: UpdateTaskInput;
}>;


export type UpdateTaskMutation = { __typename?: 'Mutation', updateTask: { __typename?: 'TaskPayload', errors: Array<string>, task: { __typename?: 'AgentTask', id: string | null, title: string | null, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexityRating: number | null, result: string | null, status: TaskStatus | null, featureId: string | null, updatedAt: any | null } | null } };

export type GetDashboardSummaryQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDashboardSummaryQuery = { __typename?: 'Query', dashboardSummary: { __typename?: 'DashboardSummary', projectsInFlight: number, featuresInReview: number, featuresFailed: number, tasksInProgress: number, tasksFailed: number, recentAuditEvents: Array<{ __typename?: 'AuditEvent', id: string | null, entityType: string | null, entityId: string | null, eventType: string | null, actor: string | null, occurredAt: any | null }> } };

export type GetDefectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDefectsQuery = { __typename?: 'Query', defects: { __typename?: 'DefectConnection', nodes: Array<{ __typename?: 'Defect', id: string | null, title: string | null, description: string | null, status: FeatureStatus | null, severity: Severity | null, projectId: string | null, parentFeatureId: string | null, createdAt: any | null, updatedAt: any | null }> } };

export type GetDefectByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDefectByIdQuery = { __typename?: 'Query', defectById: { __typename?: 'Defect', id: string | null, title: string | null, description: string | null, acceptanceCriteria: string | null, plan: string | null, result: string | null, errors: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, openQuestions: string | null, status: FeatureStatus | null, severity: Severity | null, projectId: string | null, parentFeatureId: string | null, createdAt: any | null, updatedAt: any | null, version: number | null } | null };

export type GetEpicsQueryVariables = Exact<{
  title: InputMaybe<Scalars['String']['input']>;
}>;


export type GetEpicsQuery = { __typename?: 'Query', epics: { __typename?: 'EpicConnection', nodes: Array<{ __typename?: 'Epic', id: string | null, title: string | null, description: string | null, createdAt: any | null, updatedAt: any | null }> } };

export type GetEpicByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetEpicByIdQuery = { __typename?: 'Query', epicById: { __typename?: 'Epic', id: string | null, title: string | null, description: string | null, createdAt: any | null, updatedAt: any | null } | null };

export type GetFeatureByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetFeatureByIdQuery = { __typename?: 'Query', featureById: { __typename?: 'Feature', id: string | null, title: string | null, status: FeatureStatus | null, description: string | null, acceptanceCriteria: string | null, plan: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, openQuestions: string | null, result: string | null, errors: string | null, createdAt: any | null, updatedAt: any | null, validStatusTransitions: Array<FeatureStatus | null> | null, tasks: Array<{ __typename?: 'AgentTask', id: string | null, title: string | null, status: TaskStatus | null } | null> | null } | null };

export type GetFeaturesQueryVariables = Exact<{
  projectId: InputMaybe<Scalars['UUID']['input']>;
  status: InputMaybe<Array<FeatureStatus> | FeatureStatus>;
}>;


export type GetFeaturesQuery = { __typename?: 'Query', features: { __typename?: 'FeatureConnection', nodes: Array<{ __typename?: 'Feature', id: string | null, title: string | null, status: FeatureStatus | null, updatedAt: any | null, tasks: Array<{ __typename?: 'AgentTask', id: string | null } | null> | null }> } };

export type ModelConfigurationsQueryVariables = Exact<{
  projectId: Scalars['UUID']['input'];
}>;


export type ModelConfigurationsQuery = { __typename?: 'Query', modelConfigurations: Array<{ __typename?: 'ModelConfiguration', id: string | null, projectId: string | null, url: string | null, model: string | null, modelAlias: string | null, maxComplexity: number | null, createdAt: any | null, updatedAt: any | null }> };

export type GetProjectQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetProjectQuery = { __typename?: 'Query', projectById: { __typename?: 'Project', id: string | null, name: string | null, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null, createdAt: any | null, updatedAt: any | null } | null };

export type GetProjectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetProjectsQuery = { __typename?: 'Query', projects: { __typename?: 'ProjectConnection', nodes: Array<{ __typename?: 'Project', id: string | null, name: string | null, description: string | null, githubUrl: string | null, updatedAt: any | null }> } };

export type GetTaskByIdQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetTaskByIdQuery = { __typename?: 'Query', taskById: { __typename?: 'AgentTask', id: string | null, title: string | null, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexityRating: number | null, result: string | null, status: TaskStatus | null, featureId: string | null, createdAt: any | null, updatedAt: any | null } | null };

export type GetTasksQueryVariables = Exact<{
  featureId: InputMaybe<Scalars['UUID']['input']>;
  status: InputMaybe<Array<TaskStatus> | TaskStatus>;
}>;


export type GetTasksQuery = { __typename?: 'Query', tasks: { __typename?: 'TaskConnection', nodes: Array<{ __typename?: 'AgentTask', id: string | null, title: string | null, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexityRating: number | null, result: string | null, status: TaskStatus | null, featureId: string | null, createdAt: any | null, updatedAt: any | null }> } };


export const CreateDefectDocument = gql`
    mutation CreateDefect($input: CreateDefectInput!) {
  createDefect(input: $input) {
    defect {
      id
      title
      description
      acceptanceCriteria
      plan
      result
      errors
      securityImpact
      performanceImpact
      status
      severity
      projectId
      parentFeatureId
      createdAt
      updatedAt
    }
    errors
  }
}
    `;
export type CreateDefectMutationFn = Apollo.MutationFunction<CreateDefectMutation, CreateDefectMutationVariables>;

/**
 * __useCreateDefectMutation__
 *
 * To run a mutation, you first call `useCreateDefectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateDefectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createDefectMutation, { data, loading, error }] = useCreateDefectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateDefectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateDefectMutation, CreateDefectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateDefectMutation, CreateDefectMutationVariables>(CreateDefectDocument, options);
      }
export type CreateDefectMutationHookResult = ReturnType<typeof useCreateDefectMutation>;
export type CreateDefectMutationResult = Apollo.MutationResult<CreateDefectMutation>;
export type CreateDefectMutationOptions = Apollo.BaseMutationOptions<CreateDefectMutation, CreateDefectMutationVariables>;
export const CreateEpicDocument = gql`
    mutation CreateEpic($input: CreateEpicInput!) {
  createEpic(input: $input) {
    epic {
      id
      title
      description
      createdAt
      updatedAt
    }
    errors
  }
}
    `;
export type CreateEpicMutationFn = Apollo.MutationFunction<CreateEpicMutation, CreateEpicMutationVariables>;

/**
 * __useCreateEpicMutation__
 *
 * To run a mutation, you first call `useCreateEpicMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateEpicMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createEpicMutation, { data, loading, error }] = useCreateEpicMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateEpicMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateEpicMutation, CreateEpicMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateEpicMutation, CreateEpicMutationVariables>(CreateEpicDocument, options);
      }
export type CreateEpicMutationHookResult = ReturnType<typeof useCreateEpicMutation>;
export type CreateEpicMutationResult = Apollo.MutationResult<CreateEpicMutation>;
export type CreateEpicMutationOptions = Apollo.BaseMutationOptions<CreateEpicMutation, CreateEpicMutationVariables>;
export const CreateFeatureDocument = gql`
    mutation CreateFeature($input: CreateFeatureInput!) {
  createFeature(input: $input) {
    feature {
      id
      title
      description
      acceptanceCriteria
      openQuestions
      status
      projectId
      createdAt
      updatedAt
    }
    errors
  }
}
    `;
export type CreateFeatureMutationFn = Apollo.MutationFunction<CreateFeatureMutation, CreateFeatureMutationVariables>;

/**
 * __useCreateFeatureMutation__
 *
 * To run a mutation, you first call `useCreateFeatureMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateFeatureMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createFeatureMutation, { data, loading, error }] = useCreateFeatureMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateFeatureMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateFeatureMutation, CreateFeatureMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateFeatureMutation, CreateFeatureMutationVariables>(CreateFeatureDocument, options);
      }
export type CreateFeatureMutationHookResult = ReturnType<typeof useCreateFeatureMutation>;
export type CreateFeatureMutationResult = Apollo.MutationResult<CreateFeatureMutation>;
export type CreateFeatureMutationOptions = Apollo.BaseMutationOptions<CreateFeatureMutation, CreateFeatureMutationVariables>;
export const CreateModelConfigurationDocument = gql`
    mutation CreateModelConfiguration($input: CreateModelConfigurationInput!) {
  createModelConfiguration(input: $input) {
    modelConfiguration {
      id
      projectId
      url
      model
      modelAlias
      maxComplexity
      createdAt
      updatedAt
    }
    errors
  }
}
    `;
export type CreateModelConfigurationMutationFn = Apollo.MutationFunction<CreateModelConfigurationMutation, CreateModelConfigurationMutationVariables>;

/**
 * __useCreateModelConfigurationMutation__
 *
 * To run a mutation, you first call `useCreateModelConfigurationMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateModelConfigurationMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createModelConfigurationMutation, { data, loading, error }] = useCreateModelConfigurationMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateModelConfigurationMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateModelConfigurationMutation, CreateModelConfigurationMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateModelConfigurationMutation, CreateModelConfigurationMutationVariables>(CreateModelConfigurationDocument, options);
      }
export type CreateModelConfigurationMutationHookResult = ReturnType<typeof useCreateModelConfigurationMutation>;
export type CreateModelConfigurationMutationResult = Apollo.MutationResult<CreateModelConfigurationMutation>;
export type CreateModelConfigurationMutationOptions = Apollo.BaseMutationOptions<CreateModelConfigurationMutation, CreateModelConfigurationMutationVariables>;
export const CreateProjectDocument = gql`
    mutation CreateProject($input: CreateProjectInput!) {
  createProject(input: $input) {
    project {
      id
      name
      description
      architecture
      memory
      githubUrl
      createdAt
      updatedAt
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
export const CreateTaskDocument = gql`
    mutation CreateTask($input: CreateTaskInput!) {
  createTask(input: $input) {
    task {
      id
      title
      deliverable
      acceptanceCriteria
      risks
      requiredFollowUps
      complexityRating
      result
      status
      featureId
      createdAt
      updatedAt
    }
    errors
  }
}
    `;
export type CreateTaskMutationFn = Apollo.MutationFunction<CreateTaskMutation, CreateTaskMutationVariables>;

/**
 * __useCreateTaskMutation__
 *
 * To run a mutation, you first call `useCreateTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createTaskMutation, { data, loading, error }] = useCreateTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateTaskMutation, CreateTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateTaskMutation, CreateTaskMutationVariables>(CreateTaskDocument, options);
      }
export type CreateTaskMutationHookResult = ReturnType<typeof useCreateTaskMutation>;
export type CreateTaskMutationResult = Apollo.MutationResult<CreateTaskMutation>;
export type CreateTaskMutationOptions = Apollo.BaseMutationOptions<CreateTaskMutation, CreateTaskMutationVariables>;
export const CreateWorkflowRunDocument = gql`
    mutation CreateWorkflowRun($input: CreateWorkflowRunInput!) {
  createWorkflowRun(input: $input) {
    workflowRun {
      id
      projectId
      featureId
      taskId
      workflowType
      status
      startedAt
      createdAt
    }
    errors
  }
}
    `;
export type CreateWorkflowRunMutationFn = Apollo.MutationFunction<CreateWorkflowRunMutation, CreateWorkflowRunMutationVariables>;

/**
 * __useCreateWorkflowRunMutation__
 *
 * To run a mutation, you first call `useCreateWorkflowRunMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useCreateWorkflowRunMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [createWorkflowRunMutation, { data, loading, error }] = useCreateWorkflowRunMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useCreateWorkflowRunMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<CreateWorkflowRunMutation, CreateWorkflowRunMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<CreateWorkflowRunMutation, CreateWorkflowRunMutationVariables>(CreateWorkflowRunDocument, options);
      }
export type CreateWorkflowRunMutationHookResult = ReturnType<typeof useCreateWorkflowRunMutation>;
export type CreateWorkflowRunMutationResult = Apollo.MutationResult<CreateWorkflowRunMutation>;
export type CreateWorkflowRunMutationOptions = Apollo.BaseMutationOptions<CreateWorkflowRunMutation, CreateWorkflowRunMutationVariables>;
export const DeleteDefectDocument = gql`
    mutation DeleteDefect($input: DeleteDefectInput!) {
  deleteDefect(input: $input) {
    defect {
      id
    }
    errors
  }
}
    `;
export type DeleteDefectMutationFn = Apollo.MutationFunction<DeleteDefectMutation, DeleteDefectMutationVariables>;

/**
 * __useDeleteDefectMutation__
 *
 * To run a mutation, you first call `useDeleteDefectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteDefectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteDefectMutation, { data, loading, error }] = useDeleteDefectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteDefectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteDefectMutation, DeleteDefectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteDefectMutation, DeleteDefectMutationVariables>(DeleteDefectDocument, options);
      }
export type DeleteDefectMutationHookResult = ReturnType<typeof useDeleteDefectMutation>;
export type DeleteDefectMutationResult = Apollo.MutationResult<DeleteDefectMutation>;
export type DeleteDefectMutationOptions = Apollo.BaseMutationOptions<DeleteDefectMutation, DeleteDefectMutationVariables>;
export const DeleteEpicDocument = gql`
    mutation DeleteEpic($input: DeleteEpicInput!) {
  deleteEpic(input: $input) {
    epic {
      id
    }
    errors
  }
}
    `;
export type DeleteEpicMutationFn = Apollo.MutationFunction<DeleteEpicMutation, DeleteEpicMutationVariables>;

/**
 * __useDeleteEpicMutation__
 *
 * To run a mutation, you first call `useDeleteEpicMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteEpicMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteEpicMutation, { data, loading, error }] = useDeleteEpicMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteEpicMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteEpicMutation, DeleteEpicMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteEpicMutation, DeleteEpicMutationVariables>(DeleteEpicDocument, options);
      }
export type DeleteEpicMutationHookResult = ReturnType<typeof useDeleteEpicMutation>;
export type DeleteEpicMutationResult = Apollo.MutationResult<DeleteEpicMutation>;
export type DeleteEpicMutationOptions = Apollo.BaseMutationOptions<DeleteEpicMutation, DeleteEpicMutationVariables>;
export const DeleteFeatureDocument = gql`
    mutation DeleteFeature($input: DeleteFeatureInput!) {
  deleteFeature(input: $input) {
    feature {
      id
    }
    errors
  }
}
    `;
export type DeleteFeatureMutationFn = Apollo.MutationFunction<DeleteFeatureMutation, DeleteFeatureMutationVariables>;

/**
 * __useDeleteFeatureMutation__
 *
 * To run a mutation, you first call `useDeleteFeatureMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteFeatureMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteFeatureMutation, { data, loading, error }] = useDeleteFeatureMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteFeatureMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteFeatureMutation, DeleteFeatureMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteFeatureMutation, DeleteFeatureMutationVariables>(DeleteFeatureDocument, options);
      }
export type DeleteFeatureMutationHookResult = ReturnType<typeof useDeleteFeatureMutation>;
export type DeleteFeatureMutationResult = Apollo.MutationResult<DeleteFeatureMutation>;
export type DeleteFeatureMutationOptions = Apollo.BaseMutationOptions<DeleteFeatureMutation, DeleteFeatureMutationVariables>;
export const DeleteModelConfigurationDocument = gql`
    mutation DeleteModelConfiguration($input: DeleteModelConfigurationInput!) {
  deleteModelConfiguration(input: $input) {
    modelConfiguration {
      id
    }
    errors
  }
}
    `;
export type DeleteModelConfigurationMutationFn = Apollo.MutationFunction<DeleteModelConfigurationMutation, DeleteModelConfigurationMutationVariables>;

/**
 * __useDeleteModelConfigurationMutation__
 *
 * To run a mutation, you first call `useDeleteModelConfigurationMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteModelConfigurationMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteModelConfigurationMutation, { data, loading, error }] = useDeleteModelConfigurationMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteModelConfigurationMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteModelConfigurationMutation, DeleteModelConfigurationMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteModelConfigurationMutation, DeleteModelConfigurationMutationVariables>(DeleteModelConfigurationDocument, options);
      }
export type DeleteModelConfigurationMutationHookResult = ReturnType<typeof useDeleteModelConfigurationMutation>;
export type DeleteModelConfigurationMutationResult = Apollo.MutationResult<DeleteModelConfigurationMutation>;
export type DeleteModelConfigurationMutationOptions = Apollo.BaseMutationOptions<DeleteModelConfigurationMutation, DeleteModelConfigurationMutationVariables>;
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
export const DeleteTaskDocument = gql`
    mutation DeleteTask($input: DeleteTaskInput!) {
  deleteTask(input: $input) {
    task {
      id
    }
    errors
  }
}
    `;
export type DeleteTaskMutationFn = Apollo.MutationFunction<DeleteTaskMutation, DeleteTaskMutationVariables>;

/**
 * __useDeleteTaskMutation__
 *
 * To run a mutation, you first call `useDeleteTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useDeleteTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [deleteTaskMutation, { data, loading, error }] = useDeleteTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useDeleteTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<DeleteTaskMutation, DeleteTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<DeleteTaskMutation, DeleteTaskMutationVariables>(DeleteTaskDocument, options);
      }
export type DeleteTaskMutationHookResult = ReturnType<typeof useDeleteTaskMutation>;
export type DeleteTaskMutationResult = Apollo.MutationResult<DeleteTaskMutation>;
export type DeleteTaskMutationOptions = Apollo.BaseMutationOptions<DeleteTaskMutation, DeleteTaskMutationVariables>;
export const TransitionDefectStatusDocument = gql`
    mutation TransitionDefectStatus($input: TransitionDefectInput!) {
  transitionDefectStatus(input: $input) {
    defect {
      id
      status
    }
    errors
  }
}
    `;
export type TransitionDefectStatusMutationFn = Apollo.MutationFunction<TransitionDefectStatusMutation, TransitionDefectStatusMutationVariables>;

/**
 * __useTransitionDefectStatusMutation__
 *
 * To run a mutation, you first call `useTransitionDefectStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useTransitionDefectStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [transitionDefectStatusMutation, { data, loading, error }] = useTransitionDefectStatusMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useTransitionDefectStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<TransitionDefectStatusMutation, TransitionDefectStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<TransitionDefectStatusMutation, TransitionDefectStatusMutationVariables>(TransitionDefectStatusDocument, options);
      }
export type TransitionDefectStatusMutationHookResult = ReturnType<typeof useTransitionDefectStatusMutation>;
export type TransitionDefectStatusMutationResult = Apollo.MutationResult<TransitionDefectStatusMutation>;
export type TransitionDefectStatusMutationOptions = Apollo.BaseMutationOptions<TransitionDefectStatusMutation, TransitionDefectStatusMutationVariables>;
export const TransitionFeatureStatusDocument = gql`
    mutation TransitionFeatureStatus($input: TransitionFeatureInput!) {
  transitionFeatureStatus(input: $input) {
    feature {
      id
      status
      validStatusTransitions
    }
    errors
  }
}
    `;
export type TransitionFeatureStatusMutationFn = Apollo.MutationFunction<TransitionFeatureStatusMutation, TransitionFeatureStatusMutationVariables>;

/**
 * __useTransitionFeatureStatusMutation__
 *
 * To run a mutation, you first call `useTransitionFeatureStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useTransitionFeatureStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [transitionFeatureStatusMutation, { data, loading, error }] = useTransitionFeatureStatusMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useTransitionFeatureStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<TransitionFeatureStatusMutation, TransitionFeatureStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<TransitionFeatureStatusMutation, TransitionFeatureStatusMutationVariables>(TransitionFeatureStatusDocument, options);
      }
export type TransitionFeatureStatusMutationHookResult = ReturnType<typeof useTransitionFeatureStatusMutation>;
export type TransitionFeatureStatusMutationResult = Apollo.MutationResult<TransitionFeatureStatusMutation>;
export type TransitionFeatureStatusMutationOptions = Apollo.BaseMutationOptions<TransitionFeatureStatusMutation, TransitionFeatureStatusMutationVariables>;
export const TransitionTaskStatusDocument = gql`
    mutation TransitionTaskStatus($input: TransitionTaskInput!) {
  transitionTaskStatus(input: $input) {
    task {
      id
      status
    }
    errors
  }
}
    `;
export type TransitionTaskStatusMutationFn = Apollo.MutationFunction<TransitionTaskStatusMutation, TransitionTaskStatusMutationVariables>;

/**
 * __useTransitionTaskStatusMutation__
 *
 * To run a mutation, you first call `useTransitionTaskStatusMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useTransitionTaskStatusMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [transitionTaskStatusMutation, { data, loading, error }] = useTransitionTaskStatusMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useTransitionTaskStatusMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<TransitionTaskStatusMutation, TransitionTaskStatusMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<TransitionTaskStatusMutation, TransitionTaskStatusMutationVariables>(TransitionTaskStatusDocument, options);
      }
export type TransitionTaskStatusMutationHookResult = ReturnType<typeof useTransitionTaskStatusMutation>;
export type TransitionTaskStatusMutationResult = Apollo.MutationResult<TransitionTaskStatusMutation>;
export type TransitionTaskStatusMutationOptions = Apollo.BaseMutationOptions<TransitionTaskStatusMutation, TransitionTaskStatusMutationVariables>;
export const UpdateDefectDocument = gql`
    mutation UpdateDefect($input: UpdateDefectInput!) {
  updateDefect(input: $input) {
    defect {
      id
      title
      description
      acceptanceCriteria
      plan
      result
      errors
      securityImpact
      performanceImpact
      status
      severity
      projectId
      parentFeatureId
      updatedAt
    }
    errors
  }
}
    `;
export type UpdateDefectMutationFn = Apollo.MutationFunction<UpdateDefectMutation, UpdateDefectMutationVariables>;

/**
 * __useUpdateDefectMutation__
 *
 * To run a mutation, you first call `useUpdateDefectMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateDefectMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateDefectMutation, { data, loading, error }] = useUpdateDefectMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateDefectMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateDefectMutation, UpdateDefectMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateDefectMutation, UpdateDefectMutationVariables>(UpdateDefectDocument, options);
      }
export type UpdateDefectMutationHookResult = ReturnType<typeof useUpdateDefectMutation>;
export type UpdateDefectMutationResult = Apollo.MutationResult<UpdateDefectMutation>;
export type UpdateDefectMutationOptions = Apollo.BaseMutationOptions<UpdateDefectMutation, UpdateDefectMutationVariables>;
export const UpdateEpicDocument = gql`
    mutation UpdateEpic($input: UpdateEpicInput!) {
  updateEpic(input: $input) {
    epic {
      id
      title
      description
      updatedAt
    }
    errors
  }
}
    `;
export type UpdateEpicMutationFn = Apollo.MutationFunction<UpdateEpicMutation, UpdateEpicMutationVariables>;

/**
 * __useUpdateEpicMutation__
 *
 * To run a mutation, you first call `useUpdateEpicMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateEpicMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateEpicMutation, { data, loading, error }] = useUpdateEpicMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateEpicMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateEpicMutation, UpdateEpicMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateEpicMutation, UpdateEpicMutationVariables>(UpdateEpicDocument, options);
      }
export type UpdateEpicMutationHookResult = ReturnType<typeof useUpdateEpicMutation>;
export type UpdateEpicMutationResult = Apollo.MutationResult<UpdateEpicMutation>;
export type UpdateEpicMutationOptions = Apollo.BaseMutationOptions<UpdateEpicMutation, UpdateEpicMutationVariables>;
export const UpdateFeatureDocument = gql`
    mutation UpdateFeature($input: UpdateFeatureInput!) {
  updateFeature(input: $input) {
    feature {
      id
      title
      description
      acceptanceCriteria
      plan
      securityImpact
      performanceImpact
      testPlan
      deploymentPlan
      openQuestions
      status
      updatedAt
    }
    errors
  }
}
    `;
export type UpdateFeatureMutationFn = Apollo.MutationFunction<UpdateFeatureMutation, UpdateFeatureMutationVariables>;

/**
 * __useUpdateFeatureMutation__
 *
 * To run a mutation, you first call `useUpdateFeatureMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateFeatureMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateFeatureMutation, { data, loading, error }] = useUpdateFeatureMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateFeatureMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateFeatureMutation, UpdateFeatureMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateFeatureMutation, UpdateFeatureMutationVariables>(UpdateFeatureDocument, options);
      }
export type UpdateFeatureMutationHookResult = ReturnType<typeof useUpdateFeatureMutation>;
export type UpdateFeatureMutationResult = Apollo.MutationResult<UpdateFeatureMutation>;
export type UpdateFeatureMutationOptions = Apollo.BaseMutationOptions<UpdateFeatureMutation, UpdateFeatureMutationVariables>;
export const UpdateModelConfigurationDocument = gql`
    mutation UpdateModelConfiguration($input: UpdateModelConfigurationInput!) {
  updateModelConfiguration(input: $input) {
    modelConfiguration {
      id
      projectId
      url
      model
      modelAlias
      maxComplexity
      updatedAt
    }
    errors
  }
}
    `;
export type UpdateModelConfigurationMutationFn = Apollo.MutationFunction<UpdateModelConfigurationMutation, UpdateModelConfigurationMutationVariables>;

/**
 * __useUpdateModelConfigurationMutation__
 *
 * To run a mutation, you first call `useUpdateModelConfigurationMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateModelConfigurationMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateModelConfigurationMutation, { data, loading, error }] = useUpdateModelConfigurationMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateModelConfigurationMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateModelConfigurationMutation, UpdateModelConfigurationMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateModelConfigurationMutation, UpdateModelConfigurationMutationVariables>(UpdateModelConfigurationDocument, options);
      }
export type UpdateModelConfigurationMutationHookResult = ReturnType<typeof useUpdateModelConfigurationMutation>;
export type UpdateModelConfigurationMutationResult = Apollo.MutationResult<UpdateModelConfigurationMutation>;
export type UpdateModelConfigurationMutationOptions = Apollo.BaseMutationOptions<UpdateModelConfigurationMutation, UpdateModelConfigurationMutationVariables>;
export const UpdateProjectDocument = gql`
    mutation UpdateProject($input: UpdateProjectInput!) {
  updateProject(input: $input) {
    project {
      id
      name
      description
      architecture
      memory
      githubUrl
      createdAt
      updatedAt
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
export const UpdateTaskDocument = gql`
    mutation UpdateTask($input: UpdateTaskInput!) {
  updateTask(input: $input) {
    task {
      id
      title
      deliverable
      acceptanceCriteria
      risks
      requiredFollowUps
      complexityRating
      result
      status
      featureId
      updatedAt
    }
    errors
  }
}
    `;
export type UpdateTaskMutationFn = Apollo.MutationFunction<UpdateTaskMutation, UpdateTaskMutationVariables>;

/**
 * __useUpdateTaskMutation__
 *
 * To run a mutation, you first call `useUpdateTaskMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useUpdateTaskMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [updateTaskMutation, { data, loading, error }] = useUpdateTaskMutation({
 *   variables: {
 *      input: // value for 'input'
 *   },
 * });
 */
export function useUpdateTaskMutation(baseOptions?: ApolloReactHooks.MutationHookOptions<UpdateTaskMutation, UpdateTaskMutationVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useMutation<UpdateTaskMutation, UpdateTaskMutationVariables>(UpdateTaskDocument, options);
      }
export type UpdateTaskMutationHookResult = ReturnType<typeof useUpdateTaskMutation>;
export type UpdateTaskMutationResult = Apollo.MutationResult<UpdateTaskMutation>;
export type UpdateTaskMutationOptions = Apollo.BaseMutationOptions<UpdateTaskMutation, UpdateTaskMutationVariables>;
export const GetDashboardSummaryDocument = gql`
    query GetDashboardSummary {
  dashboardSummary {
    projectsInFlight
    featuresInReview
    featuresFailed
    tasksInProgress
    tasksFailed
    recentAuditEvents {
      id
      entityType
      entityId
      eventType
      actor
      occurredAt
    }
  }
}
    `;

/**
 * __useGetDashboardSummaryQuery__
 *
 * To run a query within a React component, call `useGetDashboardSummaryQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDashboardSummaryQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDashboardSummaryQuery({
 *   variables: {
 *   },
 * });
 */
export function useGetDashboardSummaryQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>(GetDashboardSummaryDocument, options);
      }
export function useGetDashboardSummaryLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>(GetDashboardSummaryDocument, options);
        }
// @ts-ignore
export function useGetDashboardSummarySuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>;
export function useGetDashboardSummarySuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDashboardSummaryQuery | undefined, GetDashboardSummaryQueryVariables>;
export function useGetDashboardSummarySuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>(GetDashboardSummaryDocument, options);
        }
export type GetDashboardSummaryQueryHookResult = ReturnType<typeof useGetDashboardSummaryQuery>;
export type GetDashboardSummaryLazyQueryHookResult = ReturnType<typeof useGetDashboardSummaryLazyQuery>;
export type GetDashboardSummarySuspenseQueryHookResult = ReturnType<typeof useGetDashboardSummarySuspenseQuery>;
export type GetDashboardSummaryQueryResult = Apollo.QueryResult<GetDashboardSummaryQuery, GetDashboardSummaryQueryVariables>;
export const GetDefectsDocument = gql`
    query GetDefects {
  defects {
    nodes {
      id
      title
      description
      status
      severity
      projectId
      parentFeatureId
      createdAt
      updatedAt
    }
  }
}
    `;

/**
 * __useGetDefectsQuery__
 *
 * To run a query within a React component, call `useGetDefectsQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDefectsQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDefectsQuery({
 *   variables: {
 *   },
 * });
 */
export function useGetDefectsQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetDefectsQuery, GetDefectsQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDefectsQuery, GetDefectsQueryVariables>(GetDefectsDocument, options);
      }
export function useGetDefectsLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDefectsQuery, GetDefectsQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDefectsQuery, GetDefectsQueryVariables>(GetDefectsDocument, options);
        }
// @ts-ignore
export function useGetDefectsSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDefectsQuery, GetDefectsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDefectsQuery, GetDefectsQueryVariables>;
export function useGetDefectsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDefectsQuery, GetDefectsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDefectsQuery | undefined, GetDefectsQueryVariables>;
export function useGetDefectsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDefectsQuery, GetDefectsQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDefectsQuery, GetDefectsQueryVariables>(GetDefectsDocument, options);
        }
export type GetDefectsQueryHookResult = ReturnType<typeof useGetDefectsQuery>;
export type GetDefectsLazyQueryHookResult = ReturnType<typeof useGetDefectsLazyQuery>;
export type GetDefectsSuspenseQueryHookResult = ReturnType<typeof useGetDefectsSuspenseQuery>;
export type GetDefectsQueryResult = Apollo.QueryResult<GetDefectsQuery, GetDefectsQueryVariables>;
export const GetDefectByIdDocument = gql`
    query GetDefectById($id: UUID!) {
  defectById(id: $id) {
    id
    title
    description
    acceptanceCriteria
    plan
    result
    errors
    securityImpact
    performanceImpact
    testPlan
    deploymentPlan
    openQuestions
    status
    severity
    projectId
    parentFeatureId
    createdAt
    updatedAt
    version
  }
}
    `;

/**
 * __useGetDefectByIdQuery__
 *
 * To run a query within a React component, call `useGetDefectByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetDefectByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetDefectByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetDefectByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetDefectByIdQuery, GetDefectByIdQueryVariables> & ({ variables: GetDefectByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetDefectByIdQuery, GetDefectByIdQueryVariables>(GetDefectByIdDocument, options);
      }
export function useGetDefectByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetDefectByIdQuery, GetDefectByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetDefectByIdQuery, GetDefectByIdQueryVariables>(GetDefectByIdDocument, options);
        }
// @ts-ignore
export function useGetDefectByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetDefectByIdQuery, GetDefectByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDefectByIdQuery, GetDefectByIdQueryVariables>;
export function useGetDefectByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDefectByIdQuery, GetDefectByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetDefectByIdQuery | undefined, GetDefectByIdQueryVariables>;
export function useGetDefectByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetDefectByIdQuery, GetDefectByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetDefectByIdQuery, GetDefectByIdQueryVariables>(GetDefectByIdDocument, options);
        }
export type GetDefectByIdQueryHookResult = ReturnType<typeof useGetDefectByIdQuery>;
export type GetDefectByIdLazyQueryHookResult = ReturnType<typeof useGetDefectByIdLazyQuery>;
export type GetDefectByIdSuspenseQueryHookResult = ReturnType<typeof useGetDefectByIdSuspenseQuery>;
export type GetDefectByIdQueryResult = Apollo.QueryResult<GetDefectByIdQuery, GetDefectByIdQueryVariables>;
export const GetEpicsDocument = gql`
    query GetEpics($title: String) {
  epics(title: $title) {
    nodes {
      id
      title
      description
      createdAt
      updatedAt
    }
  }
}
    `;

/**
 * __useGetEpicsQuery__
 *
 * To run a query within a React component, call `useGetEpicsQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetEpicsQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetEpicsQuery({
 *   variables: {
 *      title: // value for 'title'
 *   },
 * });
 */
export function useGetEpicsQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetEpicsQuery, GetEpicsQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetEpicsQuery, GetEpicsQueryVariables>(GetEpicsDocument, options);
      }
export function useGetEpicsLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetEpicsQuery, GetEpicsQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetEpicsQuery, GetEpicsQueryVariables>(GetEpicsDocument, options);
        }
// @ts-ignore
export function useGetEpicsSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetEpicsQuery, GetEpicsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetEpicsQuery, GetEpicsQueryVariables>;
export function useGetEpicsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetEpicsQuery, GetEpicsQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetEpicsQuery | undefined, GetEpicsQueryVariables>;
export function useGetEpicsSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetEpicsQuery, GetEpicsQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetEpicsQuery, GetEpicsQueryVariables>(GetEpicsDocument, options);
        }
export type GetEpicsQueryHookResult = ReturnType<typeof useGetEpicsQuery>;
export type GetEpicsLazyQueryHookResult = ReturnType<typeof useGetEpicsLazyQuery>;
export type GetEpicsSuspenseQueryHookResult = ReturnType<typeof useGetEpicsSuspenseQuery>;
export type GetEpicsQueryResult = Apollo.QueryResult<GetEpicsQuery, GetEpicsQueryVariables>;
export const GetEpicByIdDocument = gql`
    query GetEpicById($id: UUID!) {
  epicById(id: $id) {
    id
    title
    description
    createdAt
    updatedAt
  }
}
    `;

/**
 * __useGetEpicByIdQuery__
 *
 * To run a query within a React component, call `useGetEpicByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetEpicByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetEpicByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetEpicByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetEpicByIdQuery, GetEpicByIdQueryVariables> & ({ variables: GetEpicByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetEpicByIdQuery, GetEpicByIdQueryVariables>(GetEpicByIdDocument, options);
      }
export function useGetEpicByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetEpicByIdQuery, GetEpicByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetEpicByIdQuery, GetEpicByIdQueryVariables>(GetEpicByIdDocument, options);
        }
// @ts-ignore
export function useGetEpicByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetEpicByIdQuery, GetEpicByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetEpicByIdQuery, GetEpicByIdQueryVariables>;
export function useGetEpicByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetEpicByIdQuery, GetEpicByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetEpicByIdQuery | undefined, GetEpicByIdQueryVariables>;
export function useGetEpicByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetEpicByIdQuery, GetEpicByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetEpicByIdQuery, GetEpicByIdQueryVariables>(GetEpicByIdDocument, options);
        }
export type GetEpicByIdQueryHookResult = ReturnType<typeof useGetEpicByIdQuery>;
export type GetEpicByIdLazyQueryHookResult = ReturnType<typeof useGetEpicByIdLazyQuery>;
export type GetEpicByIdSuspenseQueryHookResult = ReturnType<typeof useGetEpicByIdSuspenseQuery>;
export type GetEpicByIdQueryResult = Apollo.QueryResult<GetEpicByIdQuery, GetEpicByIdQueryVariables>;
export const GetFeatureByIdDocument = gql`
    query GetFeatureById($id: UUID!) {
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

/**
 * __useGetFeatureByIdQuery__
 *
 * To run a query within a React component, call `useGetFeatureByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetFeatureByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetFeatureByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetFeatureByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetFeatureByIdQuery, GetFeatureByIdQueryVariables> & ({ variables: GetFeatureByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>(GetFeatureByIdDocument, options);
      }
export function useGetFeatureByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>(GetFeatureByIdDocument, options);
        }
// @ts-ignore
export function useGetFeatureByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>;
export function useGetFeatureByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetFeatureByIdQuery | undefined, GetFeatureByIdQueryVariables>;
export function useGetFeatureByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>(GetFeatureByIdDocument, options);
        }
export type GetFeatureByIdQueryHookResult = ReturnType<typeof useGetFeatureByIdQuery>;
export type GetFeatureByIdLazyQueryHookResult = ReturnType<typeof useGetFeatureByIdLazyQuery>;
export type GetFeatureByIdSuspenseQueryHookResult = ReturnType<typeof useGetFeatureByIdSuspenseQuery>;
export type GetFeatureByIdQueryResult = Apollo.QueryResult<GetFeatureByIdQuery, GetFeatureByIdQueryVariables>;
export const GetFeaturesDocument = gql`
    query GetFeatures($projectId: UUID, $status: [FeatureStatus!]) {
  features(projectId: $projectId, status: $status) {
    nodes {
      id
      title
      status
      updatedAt
      tasks {
        id
      }
    }
  }
}
    `;

/**
 * __useGetFeaturesQuery__
 *
 * To run a query within a React component, call `useGetFeaturesQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetFeaturesQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetFeaturesQuery({
 *   variables: {
 *      projectId: // value for 'projectId'
 *      status: // value for 'status'
 *   },
 * });
 */
export function useGetFeaturesQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetFeaturesQuery, GetFeaturesQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetFeaturesQuery, GetFeaturesQueryVariables>(GetFeaturesDocument, options);
      }
export function useGetFeaturesLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetFeaturesQuery, GetFeaturesQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetFeaturesQuery, GetFeaturesQueryVariables>(GetFeaturesDocument, options);
        }
// @ts-ignore
export function useGetFeaturesSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetFeaturesQuery, GetFeaturesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetFeaturesQuery, GetFeaturesQueryVariables>;
export function useGetFeaturesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetFeaturesQuery, GetFeaturesQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetFeaturesQuery | undefined, GetFeaturesQueryVariables>;
export function useGetFeaturesSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetFeaturesQuery, GetFeaturesQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetFeaturesQuery, GetFeaturesQueryVariables>(GetFeaturesDocument, options);
        }
export type GetFeaturesQueryHookResult = ReturnType<typeof useGetFeaturesQuery>;
export type GetFeaturesLazyQueryHookResult = ReturnType<typeof useGetFeaturesLazyQuery>;
export type GetFeaturesSuspenseQueryHookResult = ReturnType<typeof useGetFeaturesSuspenseQuery>;
export type GetFeaturesQueryResult = Apollo.QueryResult<GetFeaturesQuery, GetFeaturesQueryVariables>;
export const ModelConfigurationsDocument = gql`
    query ModelConfigurations($projectId: UUID!) {
  modelConfigurations(projectId: $projectId) {
    id
    projectId
    url
    model
    modelAlias
    maxComplexity
    createdAt
    updatedAt
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
 *      projectId: // value for 'projectId'
 *   },
 * });
 */
export function useModelConfigurationsQuery(baseOptions: ApolloReactHooks.QueryHookOptions<ModelConfigurationsQuery, ModelConfigurationsQueryVariables> & ({ variables: ModelConfigurationsQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
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
    architecture
    memory
    githubUrl
    createdAt
    updatedAt
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
      githubUrl
      updatedAt
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
export const GetTaskByIdDocument = gql`
    query GetTaskById($id: UUID!) {
  taskById(id: $id) {
    id
    title
    deliverable
    acceptanceCriteria
    risks
    requiredFollowUps
    complexityRating
    result
    status
    featureId
    createdAt
    updatedAt
  }
}
    `;

/**
 * __useGetTaskByIdQuery__
 *
 * To run a query within a React component, call `useGetTaskByIdQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetTaskByIdQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetTaskByIdQuery({
 *   variables: {
 *      id: // value for 'id'
 *   },
 * });
 */
export function useGetTaskByIdQuery(baseOptions: ApolloReactHooks.QueryHookOptions<GetTaskByIdQuery, GetTaskByIdQueryVariables> & ({ variables: GetTaskByIdQueryVariables; skip?: boolean; } | { skip: boolean; }) ) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetTaskByIdQuery, GetTaskByIdQueryVariables>(GetTaskByIdDocument, options);
      }
export function useGetTaskByIdLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetTaskByIdQuery, GetTaskByIdQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetTaskByIdQuery, GetTaskByIdQueryVariables>(GetTaskByIdDocument, options);
        }
// @ts-ignore
export function useGetTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetTaskByIdQuery, GetTaskByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetTaskByIdQuery, GetTaskByIdQueryVariables>;
export function useGetTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetTaskByIdQuery, GetTaskByIdQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetTaskByIdQuery | undefined, GetTaskByIdQueryVariables>;
export function useGetTaskByIdSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetTaskByIdQuery, GetTaskByIdQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetTaskByIdQuery, GetTaskByIdQueryVariables>(GetTaskByIdDocument, options);
        }
export type GetTaskByIdQueryHookResult = ReturnType<typeof useGetTaskByIdQuery>;
export type GetTaskByIdLazyQueryHookResult = ReturnType<typeof useGetTaskByIdLazyQuery>;
export type GetTaskByIdSuspenseQueryHookResult = ReturnType<typeof useGetTaskByIdSuspenseQuery>;
export type GetTaskByIdQueryResult = Apollo.QueryResult<GetTaskByIdQuery, GetTaskByIdQueryVariables>;
export const GetTasksDocument = gql`
    query GetTasks($featureId: UUID, $status: [TaskStatus!]) {
  tasks(featureId: $featureId, status: $status) {
    nodes {
      id
      title
      deliverable
      acceptanceCriteria
      risks
      requiredFollowUps
      complexityRating
      result
      status
      featureId
      createdAt
      updatedAt
    }
  }
}
    `;

/**
 * __useGetTasksQuery__
 *
 * To run a query within a React component, call `useGetTasksQuery` and pass it any options that fit your needs.
 * When your component renders, `useGetTasksQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useGetTasksQuery({
 *   variables: {
 *      featureId: // value for 'featureId'
 *      status: // value for 'status'
 *   },
 * });
 */
export function useGetTasksQuery(baseOptions?: ApolloReactHooks.QueryHookOptions<GetTasksQuery, GetTasksQueryVariables>) {
        const options = {...defaultOptions, ...baseOptions}
        return ApolloReactHooks.useQuery<GetTasksQuery, GetTasksQueryVariables>(GetTasksDocument, options);
      }
export function useGetTasksLazyQuery(baseOptions?: ApolloReactHooks.LazyQueryHookOptions<GetTasksQuery, GetTasksQueryVariables>) {
          const options = {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useLazyQuery<GetTasksQuery, GetTasksQueryVariables>(GetTasksDocument, options);
        }
// @ts-ignore
export function useGetTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SuspenseQueryHookOptions<GetTasksQuery, GetTasksQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetTasksQuery, GetTasksQueryVariables>;
export function useGetTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetTasksQuery, GetTasksQueryVariables>): ApolloReactHooks.UseSuspenseQueryResult<GetTasksQuery | undefined, GetTasksQueryVariables>;
export function useGetTasksSuspenseQuery(baseOptions?: ApolloReactHooks.SkipToken | ApolloReactHooks.SuspenseQueryHookOptions<GetTasksQuery, GetTasksQueryVariables>) {
          const options = baseOptions === ApolloReactHooks.skipToken ? baseOptions : {...defaultOptions, ...baseOptions}
          return ApolloReactHooks.useSuspenseQuery<GetTasksQuery, GetTasksQueryVariables>(GetTasksDocument, options);
        }
export type GetTasksQueryHookResult = ReturnType<typeof useGetTasksQuery>;
export type GetTasksLazyQueryHookResult = ReturnType<typeof useGetTasksLazyQuery>;
export type GetTasksSuspenseQueryHookResult = ReturnType<typeof useGetTasksSuspenseQuery>;
export type GetTasksQueryResult = Apollo.QueryResult<GetTasksQuery, GetTasksQueryVariables>;