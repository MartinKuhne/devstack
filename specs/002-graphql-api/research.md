# Research: GraphQL API for AI Development System (Node.js/Express/Apollo)

**Branch**: `002-graphql-api` | **Date**: 2026-04-24
**Input**: Feature specification from `/specs/002-graphql-api/spec.md`

## Executive Summary

Research of the existing `.NET 10` server codebase in `src/Server/` confirms the reference implementation details. The new Node.js/Express/Apollo implementation at `src/graphql/` will replicate the same data model, API contracts, and status transition behavior using Prisma ORM, Zod validation, and Apollo Server.

## Existing .NET Reference Architecture

### Layer Breakdown

| Layer | Project | Purpose |
|-------|---------|---------|
| API | `DevStack.Api` | HTTP server, GraphQL endpoint, health checks |
| Application | `DevStack.Application` | Commands, queries, mediator pattern |
| Domain | `DevStack.Domain` | Entities, enums, transition services |
| Infrastructure | `DevStack.Infrastructure` | Repository implementations, AES secret service |
| Persistence | `DevStack.Persistence` | EF Core DbContext, migrations |

### Existing GraphQL Server Details

**Query.cs** (`DevStack.Api/GraphQL/Query.cs`):
- Single-item: `GetProject(id)`, `GetDeliverable(id)`, `GetAgentTask(id)`, `GetLargeLanguageModel(id)`
- List: `GetProjects`, `GetDeliverables`, `GetAgentTasks`, `GetLargeLanguageModels` (all paginated, filtered, sorted)
- Count: `GetDeliverablesCount(projectId, statusFilter, typeFilter)`
- Uses Hot Chocolate attributes: `[UsePaging(MaxPageSize = 100)]`, `[UseFiltering]`, `[UseSorting]`

**Mutation.cs** (`DevStack.Api/GraphQL/Mutation.cs`):
- Project: `CreateProjectAsync`, `UpdateProjectAsync`, `DeleteProjectAsync`
- Deliverable: `CreateDeliverableAsync`, `UpdateDeliverableAsync`, `UpdateDeliverableStatusAsync`, `DeleteDeliverableAsync`, `CheckAndMarkDeliverableDoneAsync`
- AgentTask: `CreateAgentTaskAsync`, `UpdateAgentTaskAsync`, `UpdateAgentTaskStatusAsync`, `DeleteAgentTaskAsync`
- LLM: `CreateLargeLanguageModelAsync`, `UpdateLargeLanguageModelAsync`, `DeleteLargeLanguageModelAsync`
- Test: `CleanupTestDataAsync`

### Existing Domain Entities

**Project** (`DevStack.Domain/Entities/Project.cs`):
- `Id` (Guid), `Name` (string, max 200), `Description` (string), `Repository` (string, max 500)
- Relationships: Has many `Deliverables`

**Deliverable** (`DevStack.Domain/Entities/Deliverable.cs`):
- `Id` (Guid), `ProjectId` (Guid FK), `Type` (DeliverableType enum), `Title` (string, max 200), `Status` (DeliverableStatus enum)
- Optional: `Description`, `AcceptanceCriteria`, `ExecutionPlan`, `AgentFeedback`, `SecurityImpact`, `PerformanceImpact`, `TestPlan`, `DeploymentPlan`, `Blocking`
- Relationships: Belongs to `Project`, has many `AgentTasks`

**AgentTask** (`DevStack.Domain/Entities/AgentTask.cs`):
- `Id` (Guid), `ProjectId` (Guid FK), `DeliverableId` (Guid FK), `Title` (string, max 300), `Status` (AgentTaskStatus enum)
- Optional: `Description`, `Result`, `Errors`, `CommitHash`, `DependsOnAgentTaskId` (self-FK), `PromptTokens`, `CompletionTokens`, `ExecutionDurationInSeconds`, `Agent`
- Required: `ComplexityRating` (int, default 1, method validates 1-10)

**LargeLanguageModel** (`DevStack.Domain/Entities/LargeLanguageModel.cs`):
- `Id` (Guid), `Url` (string, max 500), `Model` (string, max 200), `ModelAlias` (string, max 100), `ApiKey` (string, max 1000), `MaxComplexity` (int), `MaxConcurrency` (int)

### Status Enums

**DeliverableStatus**: `Draft` | `Planning` | `Ready` | `InProgress` | `Done` | `Failed` | `Rejected` | `NeedsReview`

**AgentTaskStatus**: `Ready` | `InProgress` | `Done` | `Failed` | `Rejected` | `NeedsReview`

**DeliverableType**: `Feature` | `Defect` | `Maintenance`

### Status Transition Services

**DeliverableTransitionService**:
- `Draft` → `Planning`, `Rejected`
- `Planning` → `Ready`, `Rejected`
- `Ready` → `InProgress`, `Rejected`
- `InProgress` → `NeedsReview`, `Failed`, `Rejected`
- `NeedsReview` → `InProgress`, `Done`, `Rejected`
- Terminal: `Done`, `Failed`, `Rejected`

**AgentTaskStatusTransitionService**:
- `Ready` → `InProgress`, `Rejected`
- `InProgress` → `Done`, `Failed`, `NeedsReview`, `Rejected`
- `NeedsReview` → `InProgress`, `Done`, `Rejected`
- Terminal: `Done`, `Failed`, `Rejected`

### Existing Health Check

- `/health` → HTTP 200 OK (JSON: `{ status, timestamp }`) when operational
- `/health` → HTTP 503 Service Unavailable on database connection loss
- No authentication required
- Verifies PostgreSQL connectivity

### Existing Testing Infrastructure

- **Unit tests**: `DevStack.Tests.Unit` (xUnit) — transition services, AES encryption, enums
- **Integration tests**: `DevStack.Tests.Integration.GraphQL` (SpecFlow BDD) — GraphQL operations
- **Shared test env**: `DevStack.Tests.Integration.Shared` — Testcontainers with PostgreSQL

### Node.js Technology Stack Analysis

**Prisma ORM**:
- Type-safe database queries with auto-generated TypeScript types
- Declarative schema in `schema.prisma`
- Migration system (`prisma migrate`)
- Built-in relation support matching EF Core conventions

**Apollo Server**:
- `@apollo/server` v5 (latest) with Express integration
- Schema-first or code-first approach
- Plugin system for metrics (OpenTelemetry compatible)
- Built-in error handling via `formatError`

**Zod**:
- Runtime type validation with TypeScript type inference
- Schema composition for nested inputs
- Clear error messages for invalid data

**Express.js**:
- Minimal HTTP server framework
- Middleware chain for CORS, error handling, logging
- Route definitions for non-GraphQL endpoints (`/health`)

**Pino**:
- High-performance structured JSON logger
- Request/response logging middleware
- OpenTelemetry-compatible log enrichment

**OpenTelemetry**:
- Auto-instrumentation for Express (`@opentelemetry/instrumentation-express`)
- Auto-instrumentation for `pg` (`@opentelemetry/instrumentation-pg`)
- OTLP exporter for traces and metrics

### Implementation Mapping

| .NET Component | Node.js Equivalent | Notes |
|----------------|-------------------|-------|
| Hot Chocolate Query | Apollo `Query` resolvers | Same GraphQL schema surface |
| Hot Chocolate Mutation | Apollo `Mutation` resolvers | Same GraphQL schema surface |
| `[UsePaging]` | Custom pagination wrapper | Cursor-based pagination with `first`/`after`/`page`/`after` |
| `[UseFiltering]` | Prisma `WHERE` clauses | Dynamic filter building |
| `[UseSorting]` | Prisma `orderBy` | Dynamic sort building |
| `ICommandHandler<T>` | Service layer methods | Simplified: direct service methods |
| `DeliverableTransitionService` | `transition.service.ts` | Same transition rules |
| `AesSecretService` | `encryption.service.ts` | Same AES-256-CBC algorithm |
| Serilog | Pino | Structured JSON logging |
| OpenTelemetry .NET | `@opentelemetry/sdk-node` | Same OTLP protocol |
| EF Core Migrations | Prisma Migrations | Same database schema |
| Testcontainers (.NET) | `testcontainers` (Node.js) | Same PostgreSQL container |

### Key Findings

1. **Complete reference available**: All entity definitions, enums, status transitions, and API contracts exist in the .NET codebase
2. **Data model is stable**: The latest migration (`20260420135156_AlignAgentTaskWithSpec`) reflects the current data model
3. **Status transition rules are well-defined**: Two services with clear valid transition matrices
4. **Health check pattern is simple**: HTTP 200/503 with JSON body, DB connectivity check
5. **Test infrastructure is proven**: Testcontainers + BDD pattern works, needs porting to Node.js
6. **Encryption is straightforward**: AES-256-CBC via Node.js `crypto` module (no external dependency)

### Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Prisma relation configuration differs from EF Core | Medium | Study EF Core configurations in `DevStack.Persistence/Configurations/` and mirror in Prisma schema |
| Apollo Server pagination vs Hot Chocolate pagination | Low | Hot Chocolate uses GraphQL Connections spec; Apollo can use same spec with `@graphql-tools` |
| OpenTelemetry auto-instrumentation coverage | Low | Use official OTel SDK for Node.js; verify Express and pg instrumentation |
| Testcontainers Node.js package maturity | Low | `testcontainers` npm package is stable and widely used |
