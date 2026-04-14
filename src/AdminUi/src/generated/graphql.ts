export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
};

export type AuditEvent = {
  __typename?: 'AuditEvent';
  actor: Maybe<Scalars['String']['output']>;
  entityId: Scalars['String']['output'];
  entityType: Scalars['String']['output'];
  eventType: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  occurredAt: Scalars['String']['output'];
};

export type CreateDefectInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  errors: InputMaybe<Scalars['String']['input']>;
  parentFeatureId: InputMaybe<Scalars['ID']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['ID']['input'];
  result: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  severity: DefectSeverity;
  title: Scalars['String']['input'];
};

export type CreateFeatureInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  openQuestions: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  projectId: Scalars['ID']['input'];
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
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
  complexity: TaskComplexity;
  deliverable: InputMaybe<Scalars['String']['input']>;
  featureId: Scalars['ID']['input'];
  requiredFollowUps: InputMaybe<Scalars['String']['input']>;
  risks: InputMaybe<Scalars['String']['input']>;
  title: Scalars['String']['input'];
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
  createdAt: Scalars['String']['output'];
  description: Maybe<Scalars['String']['output']>;
  errors: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  parentFeature: Maybe<Feature>;
  performanceImpact: Maybe<Scalars['String']['output']>;
  plan: Maybe<Scalars['String']['output']>;
  project: Maybe<Project>;
  result: Maybe<Scalars['String']['output']>;
  securityImpact: Maybe<Scalars['String']['output']>;
  severity: DefectSeverity;
  status: DefectStatus;
  title: Scalars['String']['output'];
  updatedAt: Scalars['String']['output'];
};

export type DefectConnection = {
  __typename?: 'DefectConnection';
  edges: Array<DefectEdge>;
  pageInfo: PageInfo;
};

export type DefectEdge = {
  __typename?: 'DefectEdge';
  cursor: Scalars['String']['output'];
  node: Defect;
};

export const DefectSeverity = {
  Critical: 'Critical',
  High: 'High',
  Low: 'Low',
  Medium: 'Medium'
} as const;

export type DefectSeverity = typeof DefectSeverity[keyof typeof DefectSeverity];
export const DefectStatus = {
  Closed: 'Closed',
  InProgress: 'InProgress',
  Reported: 'Reported',
  Resolved: 'Resolved',
  Triaged: 'Triaged'
} as const;

export type DefectStatus = typeof DefectStatus[keyof typeof DefectStatus];
export type Feature = {
  __typename?: 'Feature';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['String']['output'];
  deploymentPlan: Maybe<Scalars['String']['output']>;
  description: Maybe<Scalars['String']['output']>;
  errors: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  openQuestions: Maybe<Scalars['String']['output']>;
  performanceImpact: Maybe<Scalars['String']['output']>;
  plan: Maybe<Scalars['String']['output']>;
  project: Maybe<Project>;
  result: Maybe<Scalars['String']['output']>;
  securityImpact: Maybe<Scalars['String']['output']>;
  status: FeatureStatus;
  tasks: Array<Task>;
  testPlan: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  updatedAt: Scalars['String']['output'];
  validStatusTransitions: Array<FeatureStatus>;
};

export type FeatureConnection = {
  __typename?: 'FeatureConnection';
  edges: Array<FeatureEdge>;
  pageInfo: PageInfo;
};

export type FeatureEdge = {
  __typename?: 'FeatureEdge';
  cursor: Scalars['String']['output'];
  node: Feature;
};

export const FeatureStatus = {
  Done: 'Done',
  InProgress: 'InProgress',
  Planned: 'Planned',
  Review: 'Review'
} as const;

export type FeatureStatus = typeof FeatureStatus[keyof typeof FeatureStatus];
export type FeatureStatusTransition = {
  __typename?: 'FeatureStatusTransition';
  errors: Maybe<Array<Scalars['String']['output']>>;
  feature: Feature;
};

export type Mutation = {
  __typename?: 'Mutation';
  createDefect: Defect;
  createFeature: Feature;
  createProject: Project;
  createTask: Task;
  transitionFeatureStatus: Maybe<FeatureStatusTransition>;
  updateDefect: Maybe<Defect>;
  updateFeature: Maybe<Feature>;
  updateProject: Maybe<Project>;
  updateTask: Maybe<Task>;
};


export type MutationcreateDefectArgs = {
  input: CreateDefectInput;
};


export type MutationcreateFeatureArgs = {
  input: CreateFeatureInput;
};


export type MutationcreateProjectArgs = {
  input: CreateProjectInput;
};


export type MutationcreateTaskArgs = {
  input: CreateTaskInput;
};


export type MutationtransitionFeatureStatusArgs = {
  actor: Scalars['String']['input'];
  id: Scalars['ID']['input'];
  targetStatus: FeatureStatus;
};


export type MutationupdateDefectArgs = {
  id: Scalars['ID']['input'];
  input: UpdateDefectInput;
};


export type MutationupdateFeatureArgs = {
  id: Scalars['ID']['input'];
  input: UpdateFeatureInput;
};


export type MutationupdateProjectArgs = {
  id: Scalars['ID']['input'];
  input: UpdateProjectInput;
};


export type MutationupdateTaskArgs = {
  id: Scalars['ID']['input'];
  input: UpdateTaskInput;
};

export type PageInfo = {
  __typename?: 'PageInfo';
  endCursor: Maybe<Scalars['String']['output']>;
  hasNextPage: Scalars['Boolean']['output'];
  hasPreviousPage: Scalars['Boolean']['output'];
  startCursor: Maybe<Scalars['String']['output']>;
};

export type Project = {
  __typename?: 'Project';
  architecture: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['String']['output'];
  description: Maybe<Scalars['String']['output']>;
  githubUrl: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  memory: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
  updatedAt: Scalars['String']['output'];
};

export type ProjectConnection = {
  __typename?: 'ProjectConnection';
  edges: Array<ProjectEdge>;
  pageInfo: PageInfo;
};

export type ProjectEdge = {
  __typename?: 'ProjectEdge';
  cursor: Scalars['String']['output'];
  node: Project;
};

export type Query = {
  __typename?: 'Query';
  dashboardSummary: DashboardSummary;
  defectById: Maybe<Defect>;
  defects: DefectConnection;
  featureById: Maybe<Feature>;
  features: FeatureConnection;
  projectById: Maybe<Project>;
  projects: ProjectConnection;
  taskById: Maybe<Task>;
  tasks: TaskConnection;
};


export type QuerydefectByIdArgs = {
  id: Scalars['ID']['input'];
};


export type QuerydefectsArgs = {
  after: InputMaybe<Scalars['String']['input']>;
  first: InputMaybe<Scalars['Int']['input']>;
};


export type QueryfeatureByIdArgs = {
  id: Scalars['ID']['input'];
};


export type QueryfeaturesArgs = {
  after: InputMaybe<Scalars['String']['input']>;
  first: InputMaybe<Scalars['Int']['input']>;
  projectId: InputMaybe<Scalars['ID']['input']>;
  status: InputMaybe<Array<FeatureStatus>>;
};


export type QueryprojectByIdArgs = {
  id: Scalars['ID']['input'];
};


export type QueryprojectsArgs = {
  after: InputMaybe<Scalars['String']['input']>;
  first: InputMaybe<Scalars['Int']['input']>;
};


export type QuerytaskByIdArgs = {
  id: Scalars['ID']['input'];
};


export type QuerytasksArgs = {
  after: InputMaybe<Scalars['String']['input']>;
  first: InputMaybe<Scalars['Int']['input']>;
};

export type Task = {
  __typename?: 'Task';
  acceptanceCriteria: Maybe<Scalars['String']['output']>;
  complexity: TaskComplexity;
  createdAt: Scalars['String']['output'];
  deliverable: Maybe<Scalars['String']['output']>;
  feature: Maybe<Feature>;
  id: Scalars['ID']['output'];
  requiredFollowUps: Maybe<Scalars['String']['output']>;
  risks: Maybe<Scalars['String']['output']>;
  status: TaskStatus;
  title: Scalars['String']['output'];
  updatedAt: Scalars['String']['output'];
};

export const TaskComplexity = {
  Complex: 'Complex',
  Major: 'Major',
  Moderate: 'Moderate',
  Simple: 'Simple'
} as const;

export type TaskComplexity = typeof TaskComplexity[keyof typeof TaskComplexity];
export type TaskConnection = {
  __typename?: 'TaskConnection';
  edges: Array<TaskEdge>;
  pageInfo: PageInfo;
};

export type TaskEdge = {
  __typename?: 'TaskEdge';
  cursor: Scalars['String']['output'];
  node: Task;
};

export const TaskStatus = {
  Done: 'Done',
  InProgress: 'InProgress',
  Review: 'Review',
  Todo: 'Todo'
} as const;

export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];
export type UpdateDefectInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  errors: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  result: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  severity: InputMaybe<DefectSeverity>;
  title: InputMaybe<Scalars['String']['input']>;
  version: Scalars['Int']['input'];
};

export type UpdateFeatureInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  deploymentPlan: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  openQuestions: InputMaybe<Scalars['String']['input']>;
  performanceImpact: InputMaybe<Scalars['String']['input']>;
  plan: InputMaybe<Scalars['String']['input']>;
  securityImpact: InputMaybe<Scalars['String']['input']>;
  testPlan: InputMaybe<Scalars['String']['input']>;
  title: InputMaybe<Scalars['String']['input']>;
};

export type UpdateProjectInput = {
  architecture: InputMaybe<Scalars['String']['input']>;
  description: InputMaybe<Scalars['String']['input']>;
  githubUrl: InputMaybe<Scalars['String']['input']>;
  memory: InputMaybe<Scalars['String']['input']>;
  name: InputMaybe<Scalars['String']['input']>;
  version: Scalars['Int']['input'];
};

export type UpdateTaskInput = {
  acceptanceCriteria: InputMaybe<Scalars['String']['input']>;
  complexity: InputMaybe<TaskComplexity>;
  deliverable: InputMaybe<Scalars['String']['input']>;
  requiredFollowUps: InputMaybe<Scalars['String']['input']>;
  risks: InputMaybe<Scalars['String']['input']>;
  status: InputMaybe<TaskStatus>;
  title: InputMaybe<Scalars['String']['input']>;
  version: Scalars['Int']['input'];
};

export type CreateFeatureMutationVariables = Exact<{
  input: CreateFeatureInput;
}>;


export type CreateFeatureMutation = { __typename?: 'Mutation', createFeature: { __typename?: 'Feature', id: string, title: string, description: string | null, acceptanceCriteria: string | null, openQuestions: string | null, status: FeatureStatus } };

export type CreateProjectMutationVariables = Exact<{
  input: CreateProjectInput;
}>;


export type CreateProjectMutation = { __typename?: 'Mutation', createProject: { __typename?: 'Project', id: string, name: string, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null } };

export type CreateTaskMutationVariables = Exact<{
  input: CreateTaskInput;
}>;


export type CreateTaskMutation = { __typename?: 'Mutation', createTask: { __typename?: 'Task', id: string, title: string, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexity: TaskComplexity, status: TaskStatus } };

export type TransitionFeatureStatusMutationVariables = Exact<{
  id: Scalars['ID']['input'];
  targetStatus: FeatureStatus;
  actor: Scalars['String']['input'];
}>;


export type TransitionFeatureStatusMutation = { __typename?: 'Mutation', transitionFeatureStatus: { __typename?: 'FeatureStatusTransition', errors: Array<string> | null, feature: { __typename?: 'Feature', id: string, status: FeatureStatus } } | null };

export type UpdateFeatureMutationVariables = Exact<{
  id: Scalars['ID']['input'];
  input: UpdateFeatureInput;
}>;


export type UpdateFeatureMutation = { __typename?: 'Mutation', updateFeature: { __typename?: 'Feature', id: string, title: string, description: string | null, acceptanceCriteria: string | null, plan: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, openQuestions: string | null, status: FeatureStatus } | null };

export type UpdateProjectMutationVariables = Exact<{
  id: Scalars['ID']['input'];
  input: UpdateProjectInput;
}>;


export type UpdateProjectMutation = { __typename?: 'Mutation', updateProject: { __typename?: 'Project', id: string, name: string, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null } | null };

export type UpdateTaskMutationVariables = Exact<{
  id: Scalars['ID']['input'];
  input: UpdateTaskInput;
}>;


export type UpdateTaskMutation = { __typename?: 'Mutation', updateTask: { __typename?: 'Task', id: string, title: string, deliverable: string | null, acceptanceCriteria: string | null, risks: string | null, requiredFollowUps: string | null, complexity: TaskComplexity, status: TaskStatus } | null };

export type GetDashboardSummaryQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDashboardSummaryQuery = { __typename?: 'Query', dashboardSummary: { __typename?: 'DashboardSummary', projectsInFlight: number, featuresInReview: number, featuresFailed: number, tasksInProgress: number, tasksFailed: number, recentAuditEvents: Array<{ __typename?: 'AuditEvent', id: string, entityType: string, entityId: string, eventType: string, actor: string | null, occurredAt: string }> } };

export type GetDefectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDefectsQuery = { __typename?: 'Query', defects: { __typename?: 'DefectConnection', edges: Array<{ __typename?: 'DefectEdge', node: { __typename?: 'Defect', id: string, title: string, description: string | null, status: DefectStatus, severity: DefectSeverity, createdAt: string, updatedAt: string, parentFeature: { __typename?: 'Feature', id: string, title: string } | null, project: { __typename?: 'Project', id: string, name: string } | null } }> } };

export type GetDefectByIdQueryVariables = Exact<{
  id: Scalars['ID']['input'];
}>;


export type GetDefectByIdQuery = { __typename?: 'Query', defectById: { __typename?: 'Defect', id: string, title: string, description: string | null, acceptanceCriteria: string | null, plan: string | null, result: string | null, errors: string | null, securityImpact: string | null, performanceImpact: string | null, status: DefectStatus, severity: DefectSeverity, createdAt: string, updatedAt: string, parentFeature: { __typename?: 'Feature', id: string, title: string } | null, project: { __typename?: 'Project', id: string, name: string } | null } | null };

export type GetFeatureByIdQueryVariables = Exact<{
  id: Scalars['ID']['input'];
}>;


export type GetFeatureByIdQuery = { __typename?: 'Query', featureById: { __typename?: 'Feature', id: string, title: string, status: FeatureStatus, description: string | null, acceptanceCriteria: string | null, plan: string | null, securityImpact: string | null, performanceImpact: string | null, testPlan: string | null, deploymentPlan: string | null, openQuestions: string | null, result: string | null, errors: string | null, createdAt: string, updatedAt: string, validStatusTransitions: Array<FeatureStatus>, tasks: Array<{ __typename?: 'Task', id: string, title: string, status: TaskStatus }> } | null };

export type GetFeaturesQueryVariables = Exact<{
  projectId: InputMaybe<Scalars['ID']['input']>;
  status: InputMaybe<Array<FeatureStatus> | FeatureStatus>;
}>;


export type GetFeaturesQuery = { __typename?: 'Query', features: { __typename?: 'FeatureConnection', edges: Array<{ __typename?: 'FeatureEdge', node: { __typename?: 'Feature', id: string, title: string, status: FeatureStatus, updatedAt: string, tasks: Array<{ __typename?: 'Task', id: string }> } }> } };

export type GetProjectQueryVariables = Exact<{
  id: Scalars['ID']['input'];
}>;


export type GetProjectQuery = { __typename?: 'Query', projectById: { __typename?: 'Project', id: string, name: string, description: string | null, architecture: string | null, memory: string | null, githubUrl: string | null, createdAt: string, updatedAt: string } | null };

export type GetProjectsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetProjectsQuery = { __typename?: 'Query', projects: { __typename?: 'ProjectConnection', edges: Array<{ __typename?: 'ProjectEdge', node: { __typename?: 'Project', id: string, name: string, description: string | null, githubUrl: string | null, updatedAt: string } }> } };
