# DevStack Implementation Plan — GraphQL API

Owner: Backend / API team
Stack: .NET 10, ASP.NET Core, Hot Chocolate, PostgreSQL (EF Core), OpenTelemetry
Component: `src/Server/`

---

## Design decisions

### Layering
- `DevStack.Domain` — entities, enums, value objects, domain services (status transitions), no infrastructure references.
- `DevStack.Application` — use cases, command/query handlers, MediatR or minimal mediator, interface contracts.
- `DevStack.Infrastructure` — EF Core DbContext, entity configurations, migrations, repository implementations, external service adapters.
- `DevStack.Api` — ASP.NET Core host, Hot Chocolate schema, mutation/query middleware, problem details, health checks.
- `DevStack.Contracts` — shared DTOs, input types, and output types used across layers to avoid tight coupling.
- `DevStack.Tests.Unit` — xUnit + FluentAssertions + NSubstitute for domain and application.
- `DevStack.Tests.Integration` — Testcontainers + PostgreSQL for persistence and API layer.

### Persistence
- PostgreSQL via EF Core 10 (latest).
- One schema; migrations managed via EF Core Migrations.
- Concurrency token (`xmin` system column or `RowVersion`) on `Project`, `Feature`, `Defect`, `Task`.
- Audit events captured via domain events dispatched in application layer before/after persistence.
- Long-form text fields stored as `text`; markdown rendering handled client-side.
- Secrets (GitHub token, model API keys) stored as encrypted strings — AES-256 at rest via `Microsoft.Data.Encryption` or `DPAPI` on Windows with fallback to a shared secret key injected via environment variable in phase 1. Upgrade path: Azure Key Vault reference in a future ADR.

### GraphQL
- Hot Chocolate 15+ with code-first schema.
- Queries for reads, Mutations for writes — no subscriptions in phase 1.
- DataLoader for lazy-loaded relationships to prevent N+1.
- Input validation via FluentValidation integrated into Hot Chocolate pipeline.
- Cursor-based pagination for list queries that may grow.
- Structured error codes mapped from domain exceptions.

### Observability
- OpenTelemetry: traces, metrics (Meter API), logs (ILogger semantic logging).
- Health checks: `/health` for liveness, `/ready` for readiness (DB + migrations applied).
- Trace context propagation from HTTP headers through all application layers.

---

## Phase 0 — Bootstrap the API skeleton

**Goal:** A buildable, testable .NET solution with an empty API host and CI pipeline placeholder.

### Step 0.1 — Create solution and project structure
- [x] Run `dotnet new sln -n DevStack` in `src/Server/`.
- [x] Create `DevStack.Domain` as a class library.
- [x] Create `DevStack.Application` as a class library referencing Domain.
- [x] Create `DevStack.Infrastructure` as a class library referencing Application.
- [x] Create `DevStack.Api` as a web app referencing all three.
- [x] Create `DevStack.Contracts` as a class library with DTOs used across layers.
- [x] Create `DevStack.Tests.Unit` (xUnit).
- [x] Create `DevStack.Tests.Integration` (xUnit + Testcontainers).
- [x] Add project references so Api → Application → Domain ← Infrastructure and both test projects reference Api.
- [x] Add shared NuGet packages: `FluentAssertions`, `NSubstitute` in test projects; `Microsoft.Extensions.*` in infrastructure.
- [x] Verify `dotnet build src/Server/DevStack.sln` succeeds with zero warnings.
- [x] Verify `dotnet test src/Server/DevStack.sln` runs (initially zero tests, should pass).

### Step 0.2 — Add API host baseline
- [x] In `DevStack.Api`, configure minimal ASP.NET Core pipeline: JSON serialization options (System.Text.Json, camelCase), CORS policy (allow all in development), routing.
- [x] Add a `GET /health` and `GET /ready` endpoint using `Microsoft.Extensions.Diagnostics.HealthChecks`.
- [x] Add a `GET /` minimal ping endpoint to confirm host starts.
- [x] Run `dotnet run --project src/Server/DevStack.Api` and confirm HTTP 200 on `/health`.
- [x] Add Dockerfile for Api (multi-stage: SDK build, runtime) targeting Linux container.
- [x] Add `.dockerignore` and confirm Docker build succeeds.

### Step 0.3 — Configure solution-level tools
- [x] Add `Directory.Build.props` to enforce nullable, implicit usings, warnings-as-errors, and LangVersion 12.
- [x] Add `stylecop.json` or `.editorconfig` enforcing Allman braces, 4-space indent, naming conventions from AGENTS.md.
- [x] Add a root `global.json` pinning .NET 10 SDK.
- [x] Add `dotnet format` configuration and verify `dotnet format --verify-no-changes` passes.
- [x] Add CI workflow file (`.github/workflows/build.yml` or equivalent) that runs build, test, and format check.
- [x] Confirm CI workflow passes locally.

### Step 0.4 — Add Docker Compose for local development
- [x] Create `docker-compose.yml` at repo root with a `postgres` service using the official PostgreSQL image, a named volume for data persistence, and healthcheck.
- [x] Add environment variables to `Api` service for `ConnectionStrings__DefaultConnection` pointing to the postgres container.
- [x] Confirm `docker compose up -d postgres` starts the database and `dotnet ef database update` connects.

**Complexity:** 4/10
**Dependencies:** None (first step)
**Test impact:** Establishes test projects; empty smoke test to confirm build is green.
**Risks:** .NET 10 SDK availability in CI; Docker Compose version compatibility on developer machines.

---

## Phase 1 — Domain and Persistence

**Goal:** All entities modeled, migrations created, audit trail in place, secrets strategy established.

### Step 1.1 — Define enums and base types
- [x] Create `src/Server/DevStack.Domain/Enums/FeatureStatus.cs` with values: Planning, Ready, InProgress, ReadyForTest, Testing, Done, Failed, Rejected, InReview.
- [x] Create `src/Server/DevStack.Domain/Enums/TaskStatus.cs` with values: Planning, Ready, Prepare, Code, Review, ReadyForTest, Testing, Done, Failed, Rejected, InReview.
- [x] Create `src/Server/DevStack.Domain/Enums/WorkflowType.cs` with values: Planner, DevLead, Coder, Tester, Architect.
- [x] Create `src/Server/DevStack.Domain/Enums/WorkflowRunStatus.cs` with values: Queued, Running, Succeeded, Failed, Cancelled.
- [x] Create `src/Server/DevStack.Domain/ValueObjects/ConcurrencyToken.cs` as a thin wrapper.
- [x] Create base entity `src/Server/DevStack.Domain/Entities/Entity.cs` with `Id` (Guid), `Equals(Entity)` by id, and `GetHashCode` by id.
- [x] Add unit tests for enum mapping and entity equality.

### Step 1.2 — Model the Project aggregate
- [x] Create `src/Server/DevStack.Domain/Entities/Project.cs` with:
  - `Id` (Guid, key)
  - `Name` (string, required, max 200)
  - `Description` (string, optional)
  - `Architecture` (string, markdown, optional) — notes on tech stack, patterns, constraints
  - `Memory` (string, markdown) — everything an AI agent needs to understand the project
  - `GithubUrl` (Uri, optional)
  - `GithubToken_Encrypted` (string, encrypted) — nullable
  - `Features` (ICollection<Feature>)
  - `Defects` (ICollection<Defect>)
  - `ModelConfigurations` (ICollection<ModelConfiguration>)
  - `ConcurrencyToken` (RowVersion)
  - `CreatedAt`, `UpdatedAt` timestamps
- [x] Create `src/Server/DevStack.Domain/Entities/ModelConfiguration.cs` with:
  - `Id`, `ProjectId` (FK), `Url`, `Model`, `ModelAlias`, `ApiKey_Encrypted`, `MaxComplexity` (int), `CreatedAt`, `UpdatedAt`.
- [x] Add unit tests for Project creation and validation.

### Step 1.3 — Model the Feature aggregate (work item base + Feature + Defect)
- [x] Create `src/Server/DevStack.Domain/Entities/WorkItem.cs` abstract base with shared fields:
  - `Id`, `ProjectId` (FK), `Title` (max 300), `Status`, `Description` (markdown), `AcceptanceCriteria` (markdown), `Plan` (markdown), `SecurityImpact` (markdown), `PerformanceImpact` (markdown), `TestPlan` (markdown), `DeploymentPlan` (markdown), `OpenQuestions` (markdown), `Result` (text), `Errors` (text), `ConcurrencyToken`, `CreatedAt`, `UpdatedAt`.
- [x] Create `src/Server/DevStack.Domain/Entities/Feature.cs` inheriting from `WorkItem`, adding:
  - `Tasks` (ICollection<Task>)
  - No additional fields in phase 1.
- [x] Create `src/Server/DevStack.Domain/Entities/Defect.cs` inheriting from `WorkItem`, adding:
  - `ParentFeatureId` (nullable FK to Feature) — defect may be linked to a parent feature.
  - `Severity` (enum: Low, Medium, High, Critical).
- [x] Add unit tests for Feature and Defect creation.

### Step 1.4 — Model the Task entity
- [x] Create `src/Server/DevStack.Domain/Entities/Task.cs` with:
  - `Id`, `FeatureId` (FK), `Title` (max 300), `Status`, `Deliverable` (markdown), `AcceptanceCriteria` (markdown), `Risks` (markdown), `Result` (text), `RequiredFollowUps` (markdown), `ComplexityRating` (int 1-10), `ConcurrencyToken`, `CreatedAt`, `UpdatedAt`.
- [x] Add task complexity validation: rating must be 1–10.
- [x] Add unit tests for Task creation and complexity constraint.

### Step 1.5 — Model audit and workflow run entities
- [x] Create `src/Server/DevStack.Domain/Entities/WorkflowRun.cs` with:
  - `Id`, `ProjectId` (FK), `FeatureId` (nullable FK), `TaskId` (nullable FK), `WorkflowType`, `Status`, `StartedAt`, `CompletedAt` (nullable), `ErrorMessage` (text), `InputPayload` (JSON text), `OutputPayload` (JSON text), `CreatedAt`.
- [x] Create `src/Server/DevStack.Domain/Entities/AuditEvent.cs` with:
  - `Id`, `EntityType` (string), `EntityId` (Guid), `EventType` (string — e.g., "StatusChanged"), `OldValue` (text, nullable), `NewValue` (text, nullable), `Actor` (string — operator or workflow name), `OccurredAt` (UTC timestamp).
  - `AuditEvent` is append-only; no updates or deletes.
- [x] Add unit tests for WorkflowRun state transitions.

### Step 1.6 — Implement domain services for status transitions
- [x] Create `src/Server/DevStack.Domain/Services/FeatureStatusTransitionService.cs`:
  - Method: `Result Transition(Feature feature, FeatureStatus target)` returning success or a list of validation errors.
  - Validates that the transition is legal (e.g., cannot go Done → InProgress unless explicitly allowed for rework).
  - Emits a domain event `FeatureStatusChangedEvent(feature.Id, oldStatus, newStatus, actor)`.
- [x] Create equivalent `TaskStatusTransitionService`.
- [x] Add unit tests covering all allowed and disallowed transitions for Feature and Task.

### Step 1.7 — Implement EF Core infrastructure
- [ ] Create `src/Server/DevStack.Infrastructure/Persistence/DevStackDbContext.cs` with `DbSet` for all entities.
- [ ] Create `src/Server/DevStack.Infrastructure/Persistence/Configurations/` with one file per entity using `IEntityTypeConfiguration<T>`.
  - Configure `Project` with one-to-many to Features, Defects, ModelConfigurations.
  - Configure `Feature` with one-to-many to Tasks.
  - Configure `Defect` with optional FK to Feature.
  - Configure `WorkflowRun` and `AuditEvent`.
  - Add index on `Feature.Status` and `Task.Status` for dashboard queries.
  - Add index on `AuditEvent.EntityId` for history lookup.
  - Set `RowVersion` on Project, Feature, Defect, Task using PostgreSQL `xmin`.
- [ ] Create `src/Server/DevStack.Infrastructure/Persistence/DesignTimeDbContextFactory.cs` for `dotnet ef migrations`.
- [ ] Add `Microsoft.EntityFrameworkCore.Design` package to Api project.
- [ ] Run `dotnet ef migrations add InitialCreate --output-dir Migrations` and confirm migration file is generated.
- [ ] Run `dotnet ef database update` against local postgres and confirm schema created.
- [ ] Add integration tests that apply the migration to a Testcontainers PostgreSQL instance.

### Step 1.8 — Implement encrypted secret storage
- [ ] Create `src/Server/DevStack.Infrastructure/Services/ISecretService.cs` interface: `string Encrypt(string plaintext)`, `string Decrypt(string ciphertext)`.
- [ ] Implement `AesSecretService` in `src/Server/DevStack.Infrastructure/`:
  - Derive a 256-bit key from `DEVSTACK_SECRET_KEY` environment variable using PBKDF2.
  - Use AES-GCM for encryption; store as Base64 with a version prefix (e.g., `v1:`).
  - Fall back to DPAPI on Windows if the env var is absent (dev convenience).
- [ ] Register `ISecretService` in DI.
- [ ] Update `ModelConfiguration` and `Project` entity logic to use the secret service when setting/getting token fields.
- [ ] Add unit tests for encrypt/decrypt round-trip.
- [ ] Document the secret strategy in `docs/adr-005-secret-storage.md`.

### Step 1.9 — Wire application layer and initial API
- [ ] Create `src/Server/DevStack.Application/Projects/Commands/CreateProjectCommand.cs` and handler.
- [ ] Create `src/Server/DevStack.Application/Projects/Queries/GetProjectByIdQuery.cs` and handler.
- [ ] Register MediatR (or minimal handler registrar) and FluentValidation in Api DI.
- [ ] Add health check for database connectivity.
- [ ] Write unit tests for CreateProject handler.
- [ ] Confirm `dotnet build` and `dotnet test` pass with zero warnings.

**Complexity:** 7/10
**Dependencies:** Phase 0
**Test impact:** Unit tests for status transitions, validation, encryption; integration tests for migration and repository.
**Risks:** Over-abstracting the Feature/Defect hierarchy; secret service key management in production.
**Open questions:**
- Should `Severity` on Defect be a required enum field added to the entity, or a tag-style value object? (Recommend adding it as a nullable enum field to Defect in phase 1.)
- Should `Project.Memory` be lazily loaded from a separate blob storage in the future, or kept as text? (Text in phase 1.)

---

## Phase 2 — GraphQL Schema

**Goal:** Expose all domain operations through a stable Hot Chocolate GraphQL API consumed by Admin UI and Agent Process.

### Step 2.1 — Bootstrap Hot Chocolate
- [ ] Add `HotChocolate.AspNetCore`, `HotChocolate.Data`, `HotChocolate.Validation` packages to `DevStack.Api`.
- [ ] Create `src/Server/DevStack.Api/GraphQL/Query.cs` root query type with `project(id: ID!): Project`.
- [ ] Create `src/Server/DevStack.Api/GraphQL/Mutation.cs` root mutation type (empty for now).
- [ ] Register Hot Chocolate with `AddGraphQLServer()` in Program.cs.
- [ ] Add GraphQL IDE (Banana Cake Pop) in development mode.
- [ ] Confirm `GET /graphql` returns schema introspection.

### Step 2.2 — Map domain types to GraphQL object types
- [ ] Create `src/Server/DevStack.Api/GraphQL/Types/ProjectType.cs` mapping all Project fields.
- [ ] Create `FeatureType.cs`, `DefectType.cs`, `TaskType.cs`, `ModelConfigurationType.cs`, `WorkflowRunType.cs`, `AuditEventType.cs`.
- [ ] Register all types on the schema via `AddType<>`.
- [ ] Configure DataLoader for `Project.Features`, `Project.Defects`, `Feature.Tasks` to batch+cache within a request.
- [ ] Add nullable annotations on optional fields consistently with domain model.

### Step 2.3 — Implement GraphQL queries
- [ ] Add `projects(status: [FeatureStatus]): [Project!]!` — list all projects.
- [ ] Add `project(id: ID!): Project` — project detail.
- [ ] Add `feature(id: ID!): Feature` — feature detail.
- [ ] Add `features(projectId: ID!, status: [FeatureStatus]): [Feature!]!` — filtered feature list.
- [ ] Add `defect(id: ID!): Defect`.
- [ ] Add `defects(projectId: ID!): [Defect!]!`.
- [ ] Add `task(id: ID!): Task`.
- [ ] Add `tasks(featureId: ID!, status: [TaskStatus]): [Task!]!`.
- [ ] Add `modelConfigurations(projectId: ID!): [ModelConfiguration!]!`.
- [ ] Add `auditEvents(entityId: ID!, take: Int = 50): [AuditEvent!]!`.

### Step 2.4 — Implement dashboard query
- [ ] Add `dashboardSummary: DashboardSummary!` query returning:
  - `projectsInFlight: Int!`
  - `featuresInReview: Int!`
  - `featuresFailed: Int!`
  - `tasksInProgress: Int!`
  - `tasksFailed: Int!`
  - `recentAuditEvents(take: Int): [AuditEvent!]!`
- [ ] Implement as an application query service that runs fast aggregation queries against the DB.
- [ ] Add integration test for dashboard query.

### Step 2.5 — Implement GraphQL mutations for Project
- [ ] Add `createProject(input: CreateProjectInput!): ProjectPayload!`.
  - Input fields: `name`, `description`, `architecture`, `memory`, `githubUrl`.
  - Return type includes `project` and `errors`.
- [ ] Add `updateProject(id: ID!, input: UpdateProjectInput!): ProjectPayload!` with optimistic concurrency via `RowVersion`.
- [ ] Add `deleteProject(id: ID!): DeletePayload!`.
- [ ] Add input validation using FluentValidation — name required/max length, githubUrl valid URI.
- [ ] Map errors to GraphQL error extensions with codes like `VALIDATION_ERROR`, `CONCURRENCY_CONFLICT`, `NOT_FOUND`.

### Step 2.6 — Implement GraphQL mutations for Feature
- [ ] Add `createFeature(input: CreateFeatureInput!): FeaturePayload!`.
- [ ] Add `updateFeature(id: ID!, input: UpdateFeatureInput!): FeaturePayload!`.
- [ ] Add `transitionFeatureStatus(id: ID!, targetStatus: FeatureStatus!, actor: String!): FeaturePayload!`.
  - Calls `FeatureStatusTransitionService.Transition()` server-side.
  - Returns `FEATURE_VALIDATION_ERROR` if transition is illegal.
  - Captures audit event.
- [ ] Add `deleteFeature(id: ID!): DeletePayload!`.

### Step 2.7 — Implement GraphQL mutations for Defect
- [ ] Add `createDefect(input: CreateDefectInput!): DefectPayload!`.
- [ ] Add `updateDefect(id: ID!, input: UpdateDefectInput!): DefectPayload!`.
- [ ] Add `transitionDefectStatus(id: ID!, targetStatus: FeatureStatus!, actor: String!): DefectPayload!`.
- [ ] Add `deleteDefect(id: ID!): DeletePayload!`.

### Step 2.8 — Implement GraphQL mutations for Task
- [ ] Add `createTask(input: CreateTaskInput!): TaskPayload!`.
- [ ] Add `updateTask(id: ID!, input: UpdateTaskInput!): TaskPayload!`.
- [ ] Add `transitionTaskStatus(id: ID!, targetStatus: TaskStatus!, actor: String!): TaskPayload!`.
  - Calls `TaskStatusTransitionService.Transition()` server-side.
  - Captures audit event.
- [ ] Add `deleteTask(id: ID!): DeletePayload!`.

### Step 2.9 — Implement GraphQL mutations for ModelConfiguration
- [ ] Add `createModelConfiguration(input: CreateModelConfigurationInput!): ModelConfigurationPayload!`.
- [ ] Add `updateModelConfiguration(id: ID!, input: UpdateModelConfigurationInput!): ModelConfigurationPayload!`.
- [ ] Add `deleteModelConfiguration(id: ID!): DeletePayload!`.
- [ ] Encrypt the `apiKey` field using `ISecretService` before persisting.

### Step 2.10 — Implement workflow run mutations
- [ ] Add `createWorkflowRun(input: CreateWorkflowRunInput!): WorkflowRunPayload!` — agents use this to claim work.
- [ ] Add `updateWorkflowRun(id: ID!, status: WorkflowRunStatus!, outputPayload: String): WorkflowRunPayload!` — agents report completion.
- [ ] Add `cancelWorkflowRun(id: ID!): WorkflowRunPayload!`.

### Step 2.11 — Add pagination and filtering to list queries
- [ ] Add cursor-based pagination to `features` and `tasks` using HotChocolate.Data.
- [ ] Add `where` argument filtering by status arrays and date ranges where useful.
- [ ] Document pagination conventions in schema description.

### Step 2.12 — Add OpenTelemetry to API
- [ ] Add `OpenTelemetry.Exporter.Console` in development.
- [ ] Add `OpenTelemetry.Instrumentation.AspNetCore`, `HttpClient`, `HotChocolate`.
- [ ] Add OTLP exporter configuration via environment variable for production collection.
- [ ] Add trace/span naming convention (e.g., `GraphQL.{OperationName}`).
- [ ] Add histogram metric for GraphQL mutation duration.
- [ ] Confirm traces appear in console/log output during local development.

### Step 2.13 — Containerize and CI
- [ ] Update `Dockerfile` for Api with multi-stage build, non-root user, healthcheck instruction.
- [ ] Update `docker-compose.yml` to include Api service with correct environment variables and postgres dependency.
- [ ] Add integration test suite using Testcontainers that starts the full Api container against a test postgres.
- [ ] Confirm `docker compose build` and `docker compose up` start the stack successfully.
- [ ] Add schema snapshot tests (HotChocolate.Validation) to catch unintended schema changes.

**Complexity:** 6/10
**Dependencies:** Phase 1
**Test impact:** GraphQL integration tests for all queries and mutations; schema snapshot tests.
**Risks:** Schema drift if domain evolves; mutation payloads may need expansion as agent workflows mature.
**Open questions:**
- Should long-form fields accept raw markdown strings or structured JSON arrays for acceptance criteria? (Recommendation: markdown text in phase 1, structured arrays in phase 2 if multi-item editing is needed.)
- Should audit events be queryable through GraphQL or only stored for diagnostics? (Recommendation: queryable via GraphQL with pagination.)

---

## Phase 3 — Harden and Operate

**Goal:** Production-readiness, observability, performance, and developer tooling.

### Step 3.1 — Structured logging
- [ ] Configure Serilog with JSON console output in Api.
- [ ] Add enrichers: `ApplicationName`, `Environment`, `CorrelationId` from GraphQL context.
- [ ] Redact sensitive fields (apiKey, githubToken) in log output using Serilog's masking filter.
- [ ] Add `ILogger<>` to application handlers and confirm structured log events appear.

### Step 3.2 — Problem details and error mapping
- [ ] Add `AddProblemDetails()` to ASP.NET Core pipeline.
- [ ] Map domain exceptions (`NotFoundException`, `ConcurrencyException`, `ValidationException`) to `ProblemDetails` with proper `type` URIs and `extensions`.
- [ ] Map Hot Chocolate exceptions to `IErrorFilter` so validation errors return structured GraphQL errors with `extensions.code`.

### Step 3.3 — Performance: add response caching for dashboard
- [ ] Add output caching for `dashboardSummary` query (short TTL, 30s) via response caching middleware.
- [ ] Verify DataLoader eliminates N+1 for `Project.Features` and `Feature.Tasks` using a load test with multiple projects.

### Step 3.4 — Performance: add database indexes for common queries
- [ ] Run `EXPLAIN ANALYZE` on the dashboard summary query and add covering indexes if sequential scans appear.
- [ ] Add composite index on `Features(ProjectId, Status)`.
- [ ] Add composite index on `Tasks(FeatureId, Status)`.
- [ ] Add migration for new indexes.

### Step 3.5 — API documentation
- [ ] Add schema descriptions on Hot Chocolate types using the `[Description("...")]` attribute.
- [ ] Generate a schema SDL file on build and commit it as `schema.graphql` for agent tooling reference.
- [ ] Add `docs/api-reference.md` describing authentication approach (none), rate limits (none in phase 1), and common error codes.

### Step 3.6 — Final quality gates
- [ ] Run `dotnet format --verify-no-changes` and fix any violations.
- [ ] Run all unit and integration tests; confirm 100% pass.
- [ ] Run `dotnet build` with warnings-as-errors; resolve all warnings.
- [ ] Run `docker compose up --build` and execute smoke tests against the running API.
- [ ] Confirm zero console errors in browser DevTools when browsing the GraphQL IDE.

**Complexity:** 5/10
**Dependencies:** Phase 2
**Test impact:** Adds performance tests and schema snapshot tests.
**Risks:** Index additions on large tables in production require careful migration planning.

---

## ADR Tracking

| ADR | Title | Status |
|-----|-------|--------|
| ADR-001 | Monorepo with .NET backend, React admin UI, TypeScript worker | Accepted |
| ADR-002 | Clean architecture for backend with PostgreSQL and EF Core | Accepted |
| ADR-003 | Hot Chocolate GraphQL as the sole application API | Accepted |
| ADR-004 | Encrypted secret storage with AES-GCM + PBKDF2 key derivation | Proposed |
| ADR-005 | Polling-based worker orchestration (phase 1); event-driven later | Proposed |
| ADR-006 | Shared workflow run audit model for all agents | Proposed |

---

## Deliverables checklist

- [x] Phase 0: Solution builds, tests run, docker compose starts postgres
- [ ] Phase 1: All entities modeled, migrations apply, audit events captured, secrets encrypted
- [ ] Phase 2: All queries and mutations implemented, dashboard works, GraphQL IDE functional
- [ ] Phase 3: Structured logs, problem details, indexes added, API documented, smoke tests green
