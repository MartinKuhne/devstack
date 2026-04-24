# GraphQL Contract

**Branch**: `002-graphql-api` | **Date**: 2026-04-24

This contract documents the GraphQL API for the AI Development System.

## Schema Overview

### Queries

| Query | Description | Parameters | Returns |
|-------|-------------|------------|---------|
| `GetProject` | Get a single project by ID | `id: ID!` | `Project` |
| `GetProjects` | List all projects (paginated) | Filter, Sort, Page | `[Project]` |
| `GetDeliverable` | Get a single deliverable by ID | `id: ID!` | `Deliverable` |
| `GetDeliverables` | List all deliverables (paginated) | Filter, Sort, Page | `[Deliverable]` |
| `GetDeliverablesCount` | Count deliverables with filters | `projectId`, `statusFilter`, `typeFilter` | `Int` |
| `GetAgentTask` | Get a single agent task by ID | `id: ID!` | `AgentTask` |
| `GetAgentTasks` | List all agent tasks (paginated) | Filter, Sort, Page | `[AgentTask]` |
| `GetLargeLanguageModel` | Get a single LLM by ID | `id: ID!` | `LargeLanguageModel` |
| `GetLargeLanguageModels` | List all LLMs (paginated) | Filter, Sort, Page | `[LargeLanguageModel]` |

### Mutations

| Mutation | Description | Parameters | Returns |
|----------|-------------|------------|---------|
| `CreateProjectAsync` | Create a new project | `CreateProjectInput` | `Project` |
| `UpdateProjectAsync` | Update an existing project | `UpdateProjectInput` | `Project` |
| `DeleteProjectAsync` | Delete a project | `id: ID!` | `Boolean` |
| `CreateDeliverableAsync` | Create a new deliverable | `CreateDeliverableInput` | `Deliverable` |
| `UpdateDeliverableAsync` | Update an existing deliverable | `UpdateDeliverableInput` | `Deliverable` |
| `UpdateDeliverableStatusAsync` | Transition deliverable status | `id`, `targetStatus`, `actor?` | `DeliverableStatus` |
| `DeleteDeliverableAsync` | Delete a deliverable | `id: ID!` | `Boolean` |
| `CheckAndMarkDeliverableDoneAsync` | Auto-complete deliverable if all tasks done | `deliverableId` | `Boolean` |
| `CreateAgentTaskAsync` | Create a new agent task | `CreateAgentTaskInput` | `AgentTask` |
| `UpdateAgentTaskAsync` | Update an existing agent task | `UpdateAgentTaskInput` | `AgentTask` |
| `UpdateAgentTaskStatusAsync` | Transition agent task status | `id`, `targetStatus` | `AgentTaskStatus` |
| `DeleteAgentTaskAsync` | Delete an agent task | `id: ID!` | `Boolean` |
| `CreateLargeLanguageModelAsync` | Register a new LLM | `CreateLargeLanguageModelInput` | `LargeLanguageModel` |
| `UpdateLargeLanguageModelAsync` | Update an existing LLM | `UpdateLargeLanguageModelInput` | `LargeLanguageModel` |
| `DeleteLargeLanguageModelAsync` | Delete an LLM | `id: ID!` | `Boolean` |
| `CleanupTestDataAsync` | Remove all test data | None | `CleanupTestDataPayload` |

## Input Types

### CreateProjectInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `String!` | Yes | Project name |
| `repository` | `String!` | Yes | Repository URL |
| `description` | `String` | No | Project description |

### UpdateProjectInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | Project ID |
| `name` | `String` | No | New name |
| `description` | `String` | No | New description |
| `repository` | `String` | No | New repository |

### CreateDeliverableInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `projectId` | `ID!` | Yes | Parent project ID |
| `title` | `String!` | Yes | Deliverable title |
| `type` | `String!` | Yes | Type (Feature/Defect/Maintenance) |
| `description` | `String!` | Yes | Deliverable description |
| `initialStatus` | `DeliverableStatus!` | Yes | Initial status |
| `acceptanceCriteria` | `String` | No | Acceptance criteria |
| `executionPlan` | `String` | No | Execution plan |
| `securityImpact` | `String` | No | Security impact |
| `performanceImpact` | `String` | No | Performance impact |
| `testPlan` | `String` | No | Test plan |
| `deploymentPlan` | `String` | No | Deployment plan |

### UpdateDeliverableInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | Deliverable ID |
| `title` | `String` | No | New title |
| `description` | `String` | No | New description |
| `acceptanceCriteria` | `String` | No | New acceptance criteria |
| `agentFeedback` | `String` | No | Agent feedback |
| `executionPlan` | `String` | No | New execution plan |
| `securityImpact` | `String` | No | New security impact |
| `performanceImpact` | `String` | No | New performance impact |
| `testPlan` | `String` | No | New test plan |
| `deploymentPlan` | `String` | No | New deployment plan |
| `blocking` | `String` | No | Blocking issues |

### UpdateDeliverableStatusInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | Deliverable ID |
| `targetStatus` | `DeliverableStatus!` | Yes | Target status |
| `actor` | `String` | No | Actor name |

### CreateAgentTaskInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `deliverableId` | `ID!` | Yes | Parent deliverable ID |
| `projectId` | `ID!` | Yes | Parent project ID |
| `title` | `String!` | Yes | Task title |
| `description` | `String!` | Yes | Task description |
| `dependsOnAgentTaskId` | `ID` | No | Parent task ID |
| `complexityRating` | `Int` | No | Complexity (1-10, default: 5) |

### UpdateAgentTaskInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | Task ID |
| `title` | `String` | No | New title |
| `description` | `String` | No | New description |
| `result` | `String` | No | Task result |
| `errors` | `String` | No | Error messages |
| `commitHash` | `String` | No | Git commit hash |
| `dependsOnAgentTaskId` | `ID` | No | New parent task ID |
| `complexityRating` | `Int` | No | New complexity |
| `promptTokens` | `Int` | No | Prompt token count |
| `completionTokens` | `Int` | No | Completion token count |
| `executionDurationInSeconds` | `Int` | No | Execution duration |
| `agent` | `String` | No | Agent identifier |

### UpdateAgentTaskStatusInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | Task ID |
| `targetStatus` | `AgentTaskStatus!` | Yes | Target status |

### CreateLargeLanguageModelInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `url` | `String!` | Yes | API endpoint URL |
| `model` | `String!` | Yes | Model identifier |
| `modelAlias` | `String` | No | Human-readable alias |
| `apiKey` | `String` | No | API key |
| `maxComplexity` | `Int` | No | Max complexity (default: 10) |
| `maxConcurrency` | `Int` | No | Max concurrency (default: 1) |

### UpdateLargeLanguageModelInput

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `ID!` | Yes | LLM ID |
| `url` | `String` | No | New URL |
| `model` | `String` | No | New model |
| `modelAlias` | `String` | No | New alias |
| `apiKey` | `String` | No | New API key |
| `maxComplexity` | `Int` | No | New max complexity |
| `maxConcurrency` | `Int` | No | New max concurrency |

## Output Types

### Project

| Field | Type | Description |
|-------|------|-------------|
| `id` | `ID!` | Unique identifier |
| `name` | `String!` | Project name |
| `description` | `String` | Project description |
| `repository` | `String!` | Repository URL |

### Deliverable

| Field | Type | Description |
|-------|------|-------------|
| `id` | `ID!` | Unique identifier |
| `projectId` | `ID!` | Parent project ID |
| `project` | `Project` | Parent project object |
| `type` | `DeliverableType!` | Type enum |
| `title` | `String!` | Deliverable title |
| `status` | `DeliverableStatus!` | Current status |
| `description` | `String` | Description |
| `acceptanceCriteria` | `String` | Acceptance criteria |
| `executionPlan` | `String` | Execution plan |
| `agentFeedback` | `String` | Agent feedback |
| `securityImpact` | `String` | Security impact |
| `performanceImpact` | `String` | Performance impact |
| `testPlan` | `String` | Test plan |
| `deploymentPlan` | `String` | Deployment plan |
| `blocking` | `String` | Blocking issues |

### AgentTask

| Field | Type | Description |
|-------|------|-------------|
| `id` | `ID!` | Unique identifier |
| `projectId` | `ID!` | Parent project ID |
| `deliverableId` | `ID!` | Parent deliverable ID |
| `title` | `String!` | Task title |
| `status` | `AgentTaskStatus!` | Current status |
| `description` | `String!` | Description |
| `result` | `String` | Execution result |
| `errors` | `String` | Error messages |
| `commitHash` | `String` | Git commit hash |
| `complexityRating` | `Int!` | Complexity (1-10) |
| `dependsOnAgentTaskId` | `ID` | Parent task ID |
| `promptTokens` | `Int` | Prompt token count |
| `completionTokens` | `Int` | Completion token count |
| `executionDurationInSeconds` | `Int` | Execution duration |
| `agent` | `String` | Agent identifier |

### LargeLanguageModel

| Field | Type | Description |
|-------|------|-------------|
| `id` | `ID!` | Unique identifier |
| `url` | `String!` | API endpoint URL |
| `model` | `String!` | Model identifier |
| `modelAlias` | `String` | Human-readable alias |
| `apiKey` | `String!` | API key (encrypted) |
| `maxComplexity` | `Int!` | Max complexity |
| `maxConcurrency` | `Int` | Max concurrency |

### CleanupTestDataPayload

| Field | Type | Description |
|-------|------|-------------|
| `success` | `Boolean!` | Whether cleanup succeeded |
| `message` | `String` | Result message |

## Enum Types

### DeliverableStatus

`DRAFT` | `PLANNING` | `READY` | `INPROGRESS` | `DONE` | `FAILED` | `REJECTED` | `NEEDSREVIEW`

### DeliverableType

`FEATURE` | `DEFECT` | `MAINTENANCE`

### AgentTaskStatus

`READY` | `INPROGRESS` | `DONE` | `FAILED` | `REJECTED` | `NEEDSREVIEW`
