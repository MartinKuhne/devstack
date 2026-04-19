# Implementation Plan: Align Code with GraphQL Specification

**Source of truth**: `specs/graphql/SPEC.md` and `specs/graphql/data-model.mmd`

## Summary

The current implementation has diverged significantly from the specification. This plan removes all dead code, fixes entity definitions to match the spec, and cleans up the GraphQL layer.

---

## Gap Analysis

### Entity: Project

| Spec Field | Current State | Action |
|------------|---------------|--------|
| Name | `string` (present) | Keep |
| Description | `string?` (present) | Keep |
| Repository | `string` (present) | Keep |
| Architecture | `string?` (NOT in spec) | **DELETE** |
| Memory | `string` (NOT in spec) | **DELETE** |
| GithubUrl | `Uri?` (NOT in spec) | **DELETE** |
| GithubToken_Encrypted | `string?` (NOT in spec) | **DELETE** |
| Items (collection) | `ICollection<Item>` (NOT in spec) | **DELETE** (Item entity is dead) |
| LargeLanguageModels (collection) | `ICollection<LargeLanguageModel>` (NOT in spec) | **KEEP** (referenced in spec) |
| CreatedAt/UpdatedAt | `DateTime` (NOT in spec) | **DELETE** |

### Entity: Deliverable

| Spec Field | Current State | Action |
|------------|---------------|--------|
| Title | `string` (present) | Keep |
| Status | `DeliverableStatus` (present) | Keep enum as-is |
| Type | `DeliverableType` (present) | Keep enum as-is |
| Description | `string?` (present) | Keep |
| AcceptanceCriteria | `string?` (present) | Keep |
| AgentFeedback | **MISSING** | **ADD** |
| ExecutionPlan | `Plan` (wrong name) | **RENAME Plan -> ExecutionPlan** |
| SecurityImpact | `string?` (present) | Keep |
| PerformanceImpact | `string?` (present) | Keep |
| TestPlan | `string?` (present) | Keep |
| DeploymentPlan | `string?` (present) | Keep |
| Blocking | **MISSING** | **ADD** |
| Plan | `string?` (wrong name) | **RENAME** |
| OpenQuestions | `string?` (NOT in spec) | **DELETE** |
| Result | `string?` (NOT in spec) | **DELETE** |
| Errors | `string?` (NOT in spec) | **DELETE** |
| Severity | `Severity?` (NOT in spec) | **DELETE** |
| RootCause | `string?` (NOT in spec) | **DELETE** |
| ProjectId/Project | NOT in spec (but needed for FK) | **KEEP** (required for data model) |
| CreatedAt/UpdatedAt | NOT in spec | **DELETE** |

### Entity: AgentTask

| Spec Field | Current State | Action |
|------------|---------------|--------|
| Title | `string` (present) | Keep |
| Status | `AgentTaskStatus` (present) | Keep enum as-is |
| DeliverableId | `Guid` (present) | Keep |
| Result | `string?` (present) | Keep |
| Errors | `string?` (present) | Keep |
| CommitHash | `string?` (present) | Keep |
| ComplexityRating | `int` (present) | Keep |
| DependsOnDevTask | `string?` (present) | Keep |
| PromptTokens | `int?` (present) | Keep |
| CompletionTokens | `int?` (present) | Keep |
| ExecutionDurationInSeconds | `double?` (present) | Keep |
| Model | `string?` (present) | Keep |
| ProjectId/Project | NOT in spec | **KEEP** (needed for FK) |
| CreatedAt/UpdatedAt | NOT in spec | **DELETE** |

### Entity: LargeLanguageModel

| Spec Field | Current State | Action |
|------------|---------------|--------|
| Url | `string` (present) | Keep |
| Model | `string` (present) | Keep |
| ModelAlias | `string?` (present) | Keep |
| ApiKey | `ApiKey_Encrypted` (wrong name, encrypted) | **RENAME to ApiKey**, keep encryption for security |
| MaxComplexity | `int` (present) | Keep |
| MaxConcurrency | `int` (present) | Keep |
| ProjectId/Project | NOT in spec | **DELETE** (remove project association per spec) |
| CreatedAt/UpdatedAt | NOT in spec | **DELETE** |

### Dead Code (NOT in spec at all)

| Code | Action |
|------|--------|
| `Item` entity | **DELETE** |
| `WorkItem` entity | **DELETE** |
| `Entity` base class | **DELETE** (Project, Deliverable, AgentTask, LargeLanguageModel will get simple Guid Id) |
| `ItemSubtype` enum | **DELETE** |
| `FeatureStatus` enum | **DELETE** |
| `TaskStatus` enum | **DELETE** |
| `WorkflowType` enum | **DELETE** |
| `WorkflowRunStatus` enum | **DELETE** |
| `Severity` enum | **DELETE** (used only by Item/obsolete code) |
| `Project.Architecture`, `Project.Memory`, `Project.GithubUrl`, `Project.GithubToken_Encrypted`, `Project.Items`, `Project.Features` | **DELETE** |
| `DeliverableConfiguration` (EF config) | **UPDATE** |
| `ItemConfiguration` (EF config) | **DELETE** |
| `ProjectType` GraphQL type (extra fields) | **FIX** |
| `ItemType` GraphQL type | **DELETE** |
| `ProjectPageInfo`, `ItemPageInfo` types | **KEEP** `ProjectPageInfo`, **DELETE** `ItemPageInfo` |
| `DashboardSummary` type | **DELETE** (not in spec) |
| `GetItems`, `GetItemById`, `GetFeatures`, `GetDefects`, `GetFeatureById`, `GetDefectById`, `GetValidStatusTransitions`, `GetDashboardSummary` queries | **DELETE** (Item/Feature/Defect/Task queries) |
| `ProjectConnection`, `ItemConnection` classes | **FIX** `ProjectConnection`, **DELETE** `ItemConnection` |
| `DevStackDbContext.Features`, `DevStackDbContext.Defects`, `DevStackDbContext.Tasks` | **DELETE** |
| `ItemStatusTransitionService` | **DELETE** |
| `DeliverableTransitionService` | **KEEP** (used for Deliverable status transitions) |
| All Admin UI Epic/Feature/Defect/Task components and hooks | **DELETE** (these are separate from GraphQL spec) |
| `Events/DomainEvents.cs` (TaskStatusChangedEvent) | **DELETE** if TaskStatus related |

---

## Execution Plan (ordered)

### Phase 1: Delete dead code

**Complexity: 4/10**

1. Delete `Item.cs` entity
2. Delete `WorkItem.cs` entity
3. Delete `Entity.cs` base class (replace with simple `Guid Id` on each entity)
4. Delete `ItemSubtype.cs` enum
5. Delete `FeatureStatus.cs` enum
6. Delete `Severity.cs` enum
7. Delete `ItemConfiguration.cs` EF config
8. Delete `ItemStatusTransitionService.cs`
9. Delete `FeaturePaginationTypes.cs`
10. Delete `DashboardSummary.cs`

### Phase 2: Fix entity definitions

**Complexity: 5/10**

1. **Project**: Remove `Architecture`, `Memory`, `GithubUrl`, `GithubToken_Encrypted`, `Items`, `Features`, `CreatedAt`, `UpdatedAt`. Add `LargeLanguageModels` navigation property. Replace `Entity` base with direct `Guid Id`.

2. **Deliverable**: Add `AgentFeedback`, `Blocking`. Rename `Plan` to `ExecutionPlan`. Remove `OpenQuestions`, `Result`, `Errors`, `Severity`, `RootCause`, `CreatedAt`, `UpdatedAt`. Replace `Entity` base with direct `Guid Id`.

3. **AgentTask**: Remove `CreatedAt`, `UpdatedAt`. Replace `Entity` base with direct `Guid Id`.

4. **LargeLanguageModel**: Rename `ApiKey_Encrypted` to `ApiKey`. Remove `ProjectId`, `Project`, `CreatedAt`, `UpdatedAt`. Replace `Entity` base with direct `Guid Id`.

### Phase 3: Update EF Core configurations

**Complexity: 3/10**

1. Update `ProjectConfiguration.cs` - remove Architecture, Memory, GithubUrl, GithubToken_Encrypted, Items relationship
2. Update `DeliverableConfiguration.cs` - rename Plan->ExecutionPlan, add AgentFeedback, Blocking, remove OpenQuestions, Result, Errors, Severity, RootCause
3. Update `AgentTaskConfiguration.cs` - remove CreatedAt, UpdatedAt
4. Update `LargeLanguageModelConfiguration.cs` - rename ApiKey_Encrypted->ApiKey, remove ProjectId, Project relationship, CreatedAt, UpdatedAt
5. Delete `ItemConfiguration.cs`

### Phase 4: Update DbContext

**Complexity: 2/10**

1. Remove `DbSet<Item> Items`
2. Remove `Features`, `Defects`, `Tasks` obsolete derived queries
3. Remove OnModelCreating index configs for Item

### Phase 5: Update GraphQL types and mutations

**Complexity: 5/10**

1. Fix `ProjectType` - remove Architecture, Memory, GithubUrl, GithubToken_Encrypted, Items, Features
2. Delete `ItemType`
3. Update `Deliverable` GraphQL type to include AgentFeedback, ExecutionPlan, Blocking
4. Fix `CreateDeliverableInput` and `UpdateDeliverableInput` - map AgentFeedback, ExecutionPlan, Blocking properly
5. Delete `ItemConnection`, `ItemPageInfo`, `DashboardSummary`
6. Fix `ProjectConnection` and `ProjectPageInfo`
7. Update `Mutation.cs` - fix CreateDeliverable to map AgentFeedback, ExecutionPlan, Blocking; fix LargeLanguageModel to remove ProjectId
8. Update `Query.cs` - delete all Item/Feature/Defect/Task/Dashboard queries
9. Delete `ProjectPaginationTypes.cs` and update `FeaturePaginationTypes.cs`

### Phase 6: Update infrastructure

**Complexity: 3/10**

1. Update `CreateProjectHandler` - remove Architecture, Memory parameters
2. Update `UpdateProjectHandler` - remove Architecture, Memory, GithubToken parameters
3. Update `AesSecretService` and `ISecretService` if no longer needed for ApiKey encryption (or keep for security)
4. Update `DevStackDbContextExtensions` - remove Item cleanup logic

### Phase 7: Create new migration

**Complexity: 3/10**

1. Run `dotnet ef migrations add AlignWithSpec` to generate migration
2. Review migration for correctness

### Phase 8: Build and test

**Complexity: 3/10**

1. `dotnet build src/server` - ensure clean build
2. `dotnet test src/server` - run tests
3. Fix any issues
4. Commit

---

## Complexity: 7/10

This is a medium-complexity refactoring because it touches all layers (Domain, Persistence, API, Client). The key risks are:
- The Admin UI has many dead GraphQL queries/mutations that reference deleted types
- Tests may reference Item/Feature/Defect entities
- The `LargeLanguageModel` no longer has a ProjectId which changes the relationship model

## Dependencies

- All changes are interdependent within a phase
- Phase 2 must complete before Phase 3
- Phase 7 must run after all entity/config changes
- Phase 8 is the final validation
