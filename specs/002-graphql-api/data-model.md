# Data Model: GraphQL API for AI Development System

**Branch**: `002-graphql-api` | **Date**: 2026-04-24

## Entity Relationship Diagram

```
Project 1 ──┬── * Deliverable 1 ──┬── * AgentTask
            │                      └── (self-reference) DependsOnAgentTask
            │
            └── (no direct relationship to AgentTask)

LargeLanguageModel (standalone)
```

## Entity Definitions

### Project

Represents a software development initiative.

| Attribute | Type | Required | Max Length | Description |
|-----------|------|----------|------------|-------------|
| Id | Guid | Yes | - | Unique identifier (auto-generated) |
| Name | string | Yes | 200 | Project name |
| Description | string | No | - | Project description |
| Repository | string | Yes | 500 | Source repository URL |

**Relationships**:
- Has many Deliverables (one-to-many)
- Has many AgentTasks directly (one-to-many, via ProjectId)

**GraphQL Operations**:
- Query: `GetProject(id: ID!)`, `GetProjects` (paginated, filtered, sorted)
- Mutation: `CreateProjectAsync`, `UpdateProjectAsync`, `DeleteProjectAsync`

---

### Deliverable

Represents a work item within a project (feature, defect, or maintenance task).

| Attribute | Type | Required | Max Length | Description |
|-----------|------|----------|------------|-------------|
| Id | Guid | Yes | - | Unique identifier (auto-generated) |
| ProjectId | Guid | Yes | - | Foreign key to Project |
| Type | DeliverableType | Yes | - | Feature, Defect, or Maintenance |
| Title | string | Yes | 200 | Deliverable title |
| Status | DeliverableStatus | Yes | - | Current workflow status |
| Description | string? | No | - | Deliverable description |
| AcceptanceCriteria | string? | No | - | Acceptance criteria text |
| ExecutionPlan | string? | No | - | Execution plan text |
| AgentFeedback | string? | No | - | Agent feedback text |
| SecurityImpact | string? | No | - | Security impact assessment |
| PerformanceImpact | string? | No | - | Performance impact assessment |
| TestPlan | string? | No | - | Test plan text |
| DeploymentPlan | string? | No | - | Deployment plan text |
| Blocking | string? | No | - | Blocking issues text |

**Relationships**:
- Belongs to a Project (many-to-one)
- Has many AgentTasks (one-to-many)

**Status Values**: `Draft` | `Planning` | `Ready` | `InProgress` | `Done` | `Failed` | `Rejected` | `NeedsReview`

**GraphQL Operations**:
- Query: `GetDeliverable(id: ID!)`, `GetDeliverables` (paginated, filtered, sorted), `GetDeliverablesCount`
- Mutation: `CreateDeliverableAsync`, `UpdateDeliverableAsync`, `UpdateDeliverableStatusAsync`, `DeleteDeliverableAsync`, `CheckAndMarkDeliverableDoneAsync`

---

### AgentTask

Represents an individual AI-executed task within a deliverable.

| Attribute | Type | Required | Max Length | Description |
|-----------|------|----------|------------|-------------|
| Id | Guid | Yes | - | Unique identifier (auto-generated) |
| ProjectId | Guid | Yes | - | Foreign key to Project |
| DeliverableId | Guid | Yes | - | Foreign key to Deliverable |
| Title | string | Yes | 300 | Task title |
| Status | AgentTaskStatus | Yes | - | Current workflow status |
| Description | string | No | - | Task description (default: empty) |
| Result | string? | No | - | Task execution result |
| Errors | string? | No | - | Error messages |
| CommitHash | string? | No | - | Git commit hash |
| ComplexityRating | int | No | - | Complexity rating (1-10, default: 1) |
| DependsOnAgentTaskId | Guid? | No | - | Self-reference to parent task |
| PromptTokens | int? | No | - | LLM prompt token count |
| CompletionTokens | int? | No | - | LLM completion token count |
| ExecutionDurationInSeconds | int? | No | - | Task execution duration |
| Agent | string? | No | - | Agent identifier |

**Relationships**:
- Belongs to a Project (many-to-one)
- Belongs to a Deliverable (many-to-one)
- Optionally depends on another AgentTask (self-reference, one-to-many)

**Status Values**: `Ready` | `InProgress` | `Done` | `Failed` | `Rejected` | `NeedsReview`

**GraphQL Operations**:
- Query: `GetAgentTask(id: ID!)`, `GetAgentTasks` (paginated, filtered, sorted)
- Mutation: `CreateAgentTaskAsync`, `UpdateAgentTaskAsync`, `UpdateAgentTaskStatusAsync`, `DeleteAgentTaskAsync`

---

### LargeLanguageModel

Represents an LLM endpoint configuration.

| Attribute | Type | Required | Max Length | Description |
|-----------|------|----------|------------|-------------|
| Id | Guid | Yes | - | Unique identifier (auto-generated) |
| Url | string | Yes | 500 | API endpoint URL |
| Model | string | Yes | 200 | Model identifier |
| ModelAlias | string? | No | 100 | Human-readable alias |
| ApiKey | string | Yes | 1000 | API key (encrypted at rest) |
| MaxComplexity | int | Yes | - | Maximum complexity rating (1-10) |
| MaxConcurrency | int | No | - | Maximum concurrent executions |

**Relationships**: None (standalone entity)

**GraphQL Operations**:
- Query: `GetLargeLanguageModel(id: ID!)`, `GetLargeLanguageModels` (paginated, filtered, sorted)
- Mutation: `CreateLargeLanguageModelAsync`, `UpdateLargeLanguageModelAsync`, `DeleteLargeLanguageModelAsync`

---

## Enums

### DeliverableType

| Value | Description |
|-------|-------------|
| Feature | New feature implementation |
| Defect | Bug fix or defect resolution |
| Maintenance | Maintenance task |

### DeliverableStatus

| Value | Description |
|-------|-------------|
| Draft | Initial state, not yet planned |
| Planning | Being planned |
| Ready | Ready for execution |
| InProgress | Currently being executed |
| Done | Successfully completed |
| Failed | Execution failed |
| Rejected | Explicitly rejected |
| NeedsReview | Requires review |

### AgentTaskStatus

| Value | Description |
|-------|-------------|
| Ready | Ready for execution |
| InProgress | Currently being executed |
| Done | Successfully completed |
| Failed | Execution failed |
| Rejected | Explicitly rejected |
| NeedsReview | Requires review |

---

## Status Transition Rules

### Deliverable Transitions

Valid transitions are enforced by `DeliverableTransitionService`:

- `Draft` → `Planning`, `Rejected`
- `Planning` → `Ready`, `Rejected`
- `Ready` → `InProgress`, `Rejected`
- `InProgress` → `NeedsReview`, `Failed`, `Rejected`
- `NeedsReview` → `InProgress`, `Done`, `Rejected`
- `Done`, `Failed`, `Rejected` → terminal states (no outgoing transitions)

### AgentTask Transitions

Valid transitions are enforced by `AgentTaskStatusTransitionService`:

- `Ready` → `InProgress`, `Rejected`
- `InProgress` → `Done`, `Failed`, `NeedsReview`, `Rejected`
- `NeedsReview` → `InProgress`, `Done`, `Rejected`
- `Done`, `Failed`, `Rejected` → terminal states (no outgoing transitions)

---

## Database Configuration

- **Provider**: Npgsql (PostgreSQL)
- **ORM**: Entity Framework Core
- **DbContext**: `DevStackDbContext`
- **Migrations**: Managed via EF Core migrations in `DevStack.Persistence/Migrations/`
- **Connection**: Configured via environment variable or appsettings
- **Auto-migrate**: `db.Database.MigrateAsync()` called at application startup

---

## Special Features

### Auto-Complete Deliverable

When all AgentTasks under a Deliverable are marked as `Done`, the `CheckAndMarkDeliverableDoneAsync` mutation automatically transitions the Deliverable to `Done`.

### Cascade Delete

When a Project is deleted, all associated Deliverables and their AgentTasks are cascade-deleted via EF Core relationship configuration.

### Test Data Cleanup

The `CleanupTestDataAsync` mutation clears all test data from all tables, supporting integration test isolation.

### API Key Encryption

API keys in LargeLanguageModel are encrypted at rest using AES encryption via `AesSecretService`.
