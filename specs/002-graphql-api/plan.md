# Implementation Plan: GraphQL API for AI Development System (Node.js/Express/Apollo)

**Branch**: `002-graphql-api` | **Date**: 2026-04-24 | **Spec**: [spec.md](../spec.md)
**Input**: Feature specification from `/specs/002-graphql-api/spec.md`

**Clarification**: This is a second, independent implementation of the GraphQL API using a **Node.js/Express/Apollo/PostgreSQL** tech stack in `/src/graphql`, separate from the existing .NET implementation in `src/Server/`. The existing .NET codebase serves as the reference for behavior, data model, and API contracts.

## Summary

This plan implements a Node.js/Express/Apollo GraphQL server for managing AI-driven development work, mirroring the requirements of the existing .NET server. The new implementation exposes CRUD operations for four entities (Projects, Deliverables, AgentTasks, LargeLanguageModels) through an Apollo Server with filtering, sorting, and paging support. PostgreSQL is accessed via Prisma ORM. The existing `.NET 10` codebase in `src/Server/` provides the reference data model, status transition rules, and API behavior.

## Technical Context

**Language/Version**: TypeScript (Node.js 20+)  
**Primary Dependencies**: Express.js, Apollo Server, Prisma ORM, `pg` (PostgreSQL driver), Zod (validation), Pino (logging)  
**Storage**: PostgreSQL  
**Testing**: Vitest (unit tests), Supertest (integration tests), Testcontainers (containerized PostgreSQL)  
**Target Platform**: Linux container (Docker), Linux server  
**Project Type**: Web service (API)  
**Performance Goals**: Individual CRUD operations <1 second; List queries with filtering/sorting/paging <2 seconds for up to 10,000 entities; 95% of health checks respond within 500ms  
**Constraints**: No authentication/authorization; API keys encrypted at rest; OpenTelemetry tracing and metrics required; Infrastructure as Code (Docker Compose)  
**Scale/Scope**: 4 entity types with hierarchical relationships; development/internal use system

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Open Source First: All code will be developed as open source
- [x] Infrastructure as Code: Docker Compose for containerized deployment; Prisma migrations in version control
- [x] Observability-Driven Development: Pino structured logging; OpenTelemetry auto-instrumentation for Express and `pg`
- [x] Uncompromising Quality: Integration tests with Testcontainers; unit tests for domain logic; TypeScript strict mode; ESLint + Prettier
- [x] Progress Over Perfection: Implementation decomposed into independently testable increments (entity-by-entity CRUD)

## Research Findings

### Existing .NET Reference Architecture Analysis

The existing codebase in `src/Server/` provides the reference behavior:

**Layers** (reference only, not replicated in Node.js):

| Layer | Project | Purpose |
|-------|---------|---------|
| API | `DevStack.Api` | HTTP server, GraphQL endpoint, health checks |
| Application | `DevStack.Application` | Commands, queries via mediator pattern |
| Domain | `DevStack.Domain` | Entities, enums, transition services |
| Infrastructure | `DevStack.Infrastructure` | Repository implementations, AES secret service |
| Persistence | `DevStack.Persistence` | EF Core DbContext, migrations |

**Existing GraphQL Server** (`DevStack.Api/GraphQL/`):
- Hot Chocolate configured in `Program.cs`
- `Query.cs`: Single-item and paginated list queries with `[UsePaging]`, `[UseFiltering]`, `[UseSorting]`
- `Mutation.cs`: Full CRUD for all 4 entities + status transitions + test data cleanup
- `GraphQLErrorFilter.cs`: Error handling

**Existing Domain Services**:
- `DeliverableTransitionService`: Validates Deliverable status transitions
- `AgentTaskStatusTransitionService`: Validates AgentTask status transitions
- `TransitionResult`: Transition outcome model

**Existing Health Checks**:
- `/health` (liveness) with self and database checks → HTTP 200 or 503
- `/ready` (readiness) with ready-tagged checks → HTTP 200 or 503
- JSON response: `{ status, timestamp }` in camelCase

**Existing Testing**:
- Unit tests (xUnit) for transition services
- Integration tests (SpecFlow BDD) for GraphQL operations
- Testcontainers with PostgreSQL

### Node.js Implementation Approach

The Node.js implementation in `/src/graphql` will be a **self-contained project** independent of the .NET codebase. It will replicate the same functionality using:

| .NET Reference | Node.js Equivalent |
|----------------|-------------------|
| Hot Chocolate | Apollo Server |
| Entity Framework Core | Prisma ORM |
| Npgsql (PostgreSQL) | `pg` |
| xUnit + SpecFlow | Vitest + Supertest |
| Serilog | Pino |
| ASP.NET Core | Express.js |
| AutoMapper / manual mapping | Zod schemas + manual mapping |
| .NET 10 | Node.js 20+, TypeScript |

### Key Design Decisions

1. **Prisma over raw `pg`**: Prisma provides type-safe queries, migrations, and auto-generated TypeScript types, reducing boilerplate and preventing runtime errors.

2. **Zod for validation**: Zod schemas validate incoming GraphQL inputs at runtime, providing TypeScript type inference and clear error messages.

3. **Monorepo project structure**: The new implementation lives at `src/graphql/` as a standalone TypeScript project with its own `package.json`, Prisma schema, and tests.

4. **No shared code between stacks**: The .NET and Node.js implementations are fully independent. The .NET codebase is read-only reference material.

5. **Prisma schema alignment**: The Prisma schema will exactly match the data model defined in `specs/graphql/data-model.mmd` and the .NET entities.

### Gaps to Address

1. Implement status transition logic (mirroring `DeliverableTransitionService` and `AgentTaskStatusTransitionService`)
2. Implement auto-complete deliverable logic (mirroring `CheckAndMarkDeliverableDoneAsync`)
3. Implement test data cleanup (mirroring `CleanupTestDataAsync`)
4. Implement API key encryption service (mirroring `AesSecretService`)
5. Configure Apollo Server plugins for OpenTelemetry metrics
6. Set up Prisma middleware for request timing metrics

## Project Structure

### Documentation (this feature)

```text
specs/002-graphql-api/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (this document)
├── data-model.md        # Phase 1 output (entity relationship details)
├── quickstart.md        # Phase 1 output (local development setup)
├── contracts/           # Phase 1 output (GraphQL API contract)
│   └── graphql-contract.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (new implementation)

```text
src/
├── graphql/                          # NEW: Node.js/Express/Apollo GraphQL server
│   ├── prisma/
│   │   ├── schema.prisma             # Prisma data model
│   │   └── migrations/               # Prisma migration files
│   ├── src/
│   │   ├── index.ts                  # Express app entry point
│   │   ├── server.ts                 # Apollo Server setup
│   │   ├── resolvers/
│   │   │   ├── query.resolvers.ts    # GraphQL queries (CRUD)
│   │   │   ├── mutation.resolvers.ts # GraphQL mutations (CRUD, transitions)
│   │   │   └── common.resolvers.ts   # Shared resolver helpers
│   │   ├── types/
│   │   │   ├── input.types.ts        # GraphQL input types
│   │   │   ├── output.types.ts       # GraphQL output types
│   │   │   └── enum.types.ts         # GraphQL enum types
│   │   ├── services/
│   │   │   ├── project.service.ts    # Project CRUD service
│   │   │   ├── deliverable.service.ts# Deliverable CRUD + auto-complete
│   │   │   ├── agent-task.service.ts # AgentTask CRUD + transitions
│   │   │   ├── llm.service.ts        # LargeLanguageModel CRUD
│   │   │   ├── transition.service.ts # Status transition validation
│   │   │   └── encryption.service.ts # AES encryption for API keys
│   │   ├── validations/
│   │   │   ├── project.schema.ts     # Zod validation schemas
│   │   │   ├── deliverable.schema.ts
│   │   │   ├── agent-task.schema.ts
│   │   │   └── llm.schema.ts
│   │   ├── middleware/
│   │   │   ├── error.middleware.ts   # Express error handler
│   │   │   └── cors.middleware.ts    # CORS configuration
│   │   ├── health/
│   │   │   ├── health.router.ts      # Health check endpoints
│   │   │   └── db.checker.ts         # PostgreSQL connectivity check
│   │   ├── utils/
│   │   │   ├── pagination.ts         # Pagination helpers
│   │   │   ├── filtering.ts          # Filtering helpers
│   │   │   └── sorting.ts            # Sorting helpers
│   │   └── config/
│   │       ├── database.ts           # Prisma client setup
│   │       └── app.ts                # App configuration (env vars)
│   ├── tests/
│   │   ├── unit/                     # Vitest unit tests
│   │   │   ├── transition.service.test.ts
│   │   │   └── pagination.test.ts
│   │   ├── integration/              # Supertest + Testcontainers tests
│   │   │   ├── project.test.ts
│   │   │   ├── deliverable.test.ts
│   │   │   ├── agent-task.test.ts
│   │   │   ├── llm.test.ts
│   │   │   └── health.test.ts
│   │   └── fixtures/                 # Test data fixtures
│   ├── package.json
│   ├── tsconfig.json
│   ├── .eslintrc.cjs
│   └── Dockerfile
└── Server/                           # EXISTING: .NET reference (read-only)
    └── [unchanged]
```

**Structure Decision**: The new Node.js implementation is a standalone project at `src/graphql/`. It uses Prisma for ORM, Zod for validation, Express for HTTP, and Apollo Server for GraphQL. The .NET server at `src/Server/` is not modified — it serves as the behavioral reference. Both stacks can run independently and share the same PostgreSQL database schema.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No constitutional violations identified | The existing architecture already satisfies all principles |
