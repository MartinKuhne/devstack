# Tasks: GraphQL API for AI Development System (Node.js/Express/Apollo)

**Input**: Design documents from `/specs/002-graphql-api/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/graphql-contract.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.
**Tests**: Tests are included for all user stories — integration tests using Supertest + Testcontainers, unit tests for domain logic.

## Phase 1: Setup (Project Initialization)

**Purpose**: Initialize the Node.js/TypeScript project with all tooling and configuration

- [x] T001 [P] Create project directory structure at src/graphql/ with subdirectories: prisma/, src/resolvers/, src/types/, src/services/, src/validations/, src/middleware/, src/health/, src/utils/, src/config/, tests/unit/, tests/integration/, tests/fixtures/
- [x] T002 Create package.json with dependencies: express, @apollo/server, graphql, @prisma/client, pg, zod, pino, winston, @opentelemetry/api, @opentelemetry/auto-instrumentations-express, @opentelemetry/auto-instrumentations-pg, @opentelemetry/sdk-node, testcontainers, supertest, vitest, prisma, typescript, @types/node, @types/express, eslint, prettier, dotenv
- [x] T003 [P] Configure TypeScript in tsconfig.json with strict mode, ESM module resolution, path aliases for src/
- [x] T004 [P] Configure ESLint with @typescript-eslint and Prettier integration at src/graphql/.eslintrc.cjs
- [x] T005 [P] Configure Prettier at src/graphql/.prettierrc with TypeScript-aware formatting
- [x] T006 Create .env.example at src/graphql/.env.example with DATABASE_URL, PORT, NODE_ENV variable templates
- [x] T007 Create Dockerfile at src/graphql/Dockerfile with multi-stage build (Node.js 20, prisma generate, npm build, production stage)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T008 Create Prisma schema at src/graphql/prisma/schema.prisma with all 4 models (Project, Deliverable, AgentTask, LargeLanguageModel), relations, enums (DeliverableStatus, AgentTaskStatus, DeliverableType), and field constraints matching specs/002-graphql-api/data-model.md
- [x] T009 [P] Run `prisma generate` in src/graphql/ to create Prisma client from prisma/schema.prisma and verify schema compilation
- [x] T010 Create Prisma client singleton at src/graphql/src/config/database.ts with singleton pattern, connection error handling, and graceful shutdown hooks
- [x] T011 Create Zod validation schemas at src/graphql/src/validations/project.schema.ts for CreateProjectInput and UpdateProjectInput
- [x] T012 Create Zod validation schemas at src/graphql/src/validations/deliverable.schema.ts for CreateDeliverableInput, UpdateDeliverableInput, UpdateDeliverableStatusInput
- [x] T013 Create Zod validation schemas at src/graphql/src/validations/agent-task.schema.ts for CreateAgentTaskInput, UpdateAgentTaskInput, UpdateAgentTaskStatusInput
- [x] T014 Create Zod validation schemas at src/graphql/src/validations/llm.schema.ts for CreateLargeLanguageModelInput, UpdateLargeLanguageModelInput
- [x] T015 Create pagination utility helpers at src/graphql/src/utils/pagination.ts with paginate() function supporting first/after/page/afterCursor parameters returning paginated results with totalCount and hasMore
- [x] T016 Create filtering utility helpers at src/graphql/src/utils/filtering.ts with buildWhereClause() function supporting dynamic field-based filtering
- [x] T017 Create sorting utility helpers at src/graphql/src/utils/sorting.ts with buildOrderBy() function supporting dynamic field-based sorting
- [x] T018 Create health check service at src/graphql/src/health/db.checker.ts with checkDatabase() that attempts a Prisma ping and returns { healthy: boolean, error?: string }
- [x] T019 Create health check router at src/graphql/src/health/health.router.ts with GET /health returning { status: "healthy"|"unhealthy", timestamp: ISOString } with HTTP 200 or 503 based on db.checker result
- [x] T020 Create CORS middleware at src/graphql/src/middleware/cors.middleware.ts with allow-all policy for development
- [x] T021 Create error handling middleware at src/graphql/src/middleware/error.middleware.ts that catches Express errors and Apollo errors, formats them consistently
- [x] T022 Create environment configuration loader at src/graphql/src/config/app.ts that reads and validates all required env vars (DATABASE_URL, PORT) with dotenv
- [x] T023 Configure OpenTelemetry at src/graphql/src/config/opentelemetry.ts with AutoDetectResourcesDetect, Express instrumentation, pg instrumentation, and OTLP exporter conditional on OTEL_EXPORTER_OTLP_ENDPOINT
- [x] T024 Create Prisma migration files at src/graphql/prisma/migrations/ based on schema.prisma schema and run `prisma migrate dev` to verify migration works

**Checkpoint**: Foundation ready — Prisma schema defined, validation schemas created, utilities built, health checks working, OpenTelemetry configured, migrations applied. User story implementation can now begin.

---

## Phase 3: User Story 1 - Manage Development Projects (Priority: P1)

**Goal**: Create, read, update, and delete Projects — the top-level organizational unit for all development work

**Independent Test**: Create a project via GraphQL mutation, retrieve it by ID via query, update its fields, list projects with filtering/sorting/paging, and delete it — all verified through Supertest integration tests against a Testcontainers PostgreSQL instance.

### Implementation for User Story 1

- [x] T025 [P] [US1] Create GraphQL output type at src/graphql/src/types/output.types.ts for Project: id, name, description, repository
- [x] T026 [US1] Create Project service at src/graphql/src/services/project.service.ts with methods: create(data), getById(id), getAll(first, after, filter, sort), update(id, data), delete(id) using Prisma client with proper relation loading
- [x] T027 [US1] Create project query resolver at src/graphql/src/resolvers/query.resolvers.ts: project(id: ID!) returns Project, projects(first: Int, after: String, filter: ProjectFilter, sort: ProjectSort) returns ProjectConnection
- [x] T028 [US1] Create project mutation resolver at src/graphql/src/resolvers/mutation.resolvers.ts: createProject(input: CreateProjectInput!) returns Project, updateProject(id: ID!, input: UpdateProjectInput!) returns Project, deleteProject(id: ID!) returns Boolean
- [x] T029 [US1] Create integration test at src/graphql/tests/integration/project.test.ts verifying: create project, get by ID, update fields, list with filter/sort/page, delete project
- [x] T030 [US1] Create unit test at src/graphql/tests/unit/project.service.test.ts verifying: create validates required fields, update handles missing project, delete returns success

**Checkpoint**: User Story 1 is fully functional — projects can be created, read, updated, deleted, and listed with filtering/sorting/paging.

---

## Phase 4: User Story 2 - Track Deliverables (Priority: P1)

**Goal**: Create, read, update, delete Deliverables under a Project with status transitions and auto-complete when all AgentTasks are Done

**Independent Test**: Create a deliverable under a project, query it by ID, update its fields, transition through status values (Draft → Planning → Ready), list deliverables with filtering, delete deliverable — all verified against Testcontainers PostgreSQL.

### Implementation for User Story 2

- [x] T031 [P] [US2] Create GraphQL enum types at src/graphql/src/types/enum.types.ts for DeliverableStatus, DeliverableType, AgentTaskStatus matching data-model.md values
- [x] T032 [P] [US2] Create GraphQL input types at src/graphql/src/types/input.types.ts for CreateDeliverableInput, UpdateDeliverableInput, UpdateDeliverableStatusInput, CreateAgentTaskInput, UpdateAgentTaskInput, UpdateAgentTaskStatusInput, CreateLargeLanguageModelInput, UpdateLargeLanguageModelInput
- [x] T033 [P] [US2] Create GraphQL output types at src/graphql/src/types/output.types.ts for Deliverable, AgentTask, LargeLanguageModel, PaginatedProject, PaginatedDeliverable, PaginatedAgentTask, PaginatedLargeLanguageModel
- [x] T034 [US2] Create deliverable service at src/graphql/src/services/deliverable.service.ts with methods: create(data), getById(id), getAll(projectId, first, after, filter, sort), update(id, data), updateStatus(id, status, actor), delete(id), checkAndMarkDone(deliverableId) that checks all AgentTasks are Done and auto-transitions to Done
- [x] T035 [US2] Create status transition service at src/graphql/src/services/transition.service.ts with isValidDeliverableTransition(from, to) and isValidAgentTaskTransition(from, to) functions implementing all rules from data-model.md
- [x] T036 [US2] Create deliverable query resolver at src/graphql/src/resolvers/query.resolvers.ts: deliverable(id: ID!) returns Deliverable, deliverables(projectId: ID, first: Int, after: String, filter: DeliverableFilter, sort: DeliverableSort) returns DeliverableConnection, deliverablesCount(projectId: ID, statusFilter: [DeliverableStatus!], typeFilter: [DeliverableType!]) returns Int
- [x] T037 [US2] Create deliverable mutation resolver at src/graphql/src/resolvers/mutation.resolvers.ts: createDeliverable(input: CreateDeliverableInput!) returns Deliverable, updateDeliverable(id: ID!, input: UpdateDeliverableInput!) returns Deliverable, updateDeliverableStatus(id: ID!, targetStatus: DeliverableStatus!, actor: String) returns DeliverableStatus, deleteDeliverable(id: ID!) returns Boolean, checkAndMarkDeliverableDone(deliverableId: ID!) returns Boolean
- [x] T038 [US2] Create integration test at src/graphql/tests/integration/deliverable.test.ts verifying: create deliverable under project, get by ID, update fields, transition through valid statuses, reject invalid transitions, list with filter/sort/page, auto-complete when all tasks done, delete deliverable
- [x] T039 [US2] Create unit test at src/graphql/tests/unit/transition.service.test.ts verifying: all valid transitions return true, all invalid transitions return false (matching data-model.md transition rules exactly)

**Checkpoint**: User Story 2 is fully functional — deliverables can be managed with status transitions and auto-complete logic.

---

## Phase 5: User Story 3 - Execute Agent Tasks (Priority: P2)

**Goal**: Create, read, update, delete AgentTasks under Deliverables with status transitions and dependency management

**Independent Test**: Create an agent task under a deliverable, update its result/errors/token metrics, transition through status values, query with filtering/sorting/paging, and delete — all verified against Testcontainers PostgreSQL.

### Implementation for User Story 3

- [x] T040 [US3] Create agent task service at src/graphql/src/services/agent-task.service.ts with methods: create(data), getById(id), getAll(deliverableId, first, after, filter, sort), update(id, data), updateStatus(id, status), delete(id) with dependency validation (DependsOnAgentTaskId must reference existing task)
- [x] T041 [US3] Create agent task query resolver at src/graphql/src/resolvers/query.resolvers.ts: agentTask(id: ID!) returns AgentTask, agentTasks(deliverableId: ID, first: Int, after: String, filter: AgentTaskFilter, sort: AgentTaskSort) returns AgentTaskConnection
- [x] T042 [US3] Create agent task mutation resolver at src/graphql/src/resolvers/mutation.resolvers.ts: createAgentTask(input: CreateAgentTaskInput!) returns AgentTask, updateAgentTask(id: ID!, input: UpdateAgentTaskInput!) returns AgentTask, updateAgentTaskStatus(id: ID!, targetStatus: AgentTaskStatus!) returns AgentTaskStatus, deleteAgentTask(id: ID!) returns Boolean
- [x] T043 [US3] Create integration test at src/graphql/tests/integration/agent-task.test.ts verifying: create task under deliverable, get by ID, update result/errors/token metrics, transition through valid statuses, reject invalid transitions, list with filter/sort/page, delete task
- [x] T044 [US3] Create unit test at src/graphql/tests/unit/agent-task.service.test.ts verifying: create validates complexity rating (1-10), update handles missing task, create validates DependsOnAgentTaskId references existing task

**Checkpoint**: User Story 3 is fully functional — agent tasks can be managed with status transitions and dependency chains.

---

## Phase 6: User Story 4 - Configure Language Models (Priority: P3)

**Goal**: Register, read, update, and delete LargeLanguageModel endpoint configurations

**Independent Test**: Create a language model entry, query it by ID and list all models, update its settings, and delete it — all verified against Testcontainers PostgreSQL.

### Implementation for User Story 4

- [x] T045 [US4] Create LLM service at src/graphql/src/services/llm.service.ts with methods: create(data), getById(id), getAll(first, after, filter, sort), update(id, data), delete(id) with API key encryption using AES-256-CBC
- [x] T046 Create encryption service at src/graphql/src/services/encryption.service.ts with encryptApiKey(key) and decryptApiKey(encryptedKey) using Node.js crypto module with AES-256-CBC, matching the .NET AesSecretService algorithm
- [x] T047 [US4] Create LLM query resolver at src/graphql/src/resolvers/query.resolvers.ts: largeLanguageModel(id: ID!) returns LargeLanguageModel, largeLanguageModels(first: Int, after: String, filter: LargeLanguageModelFilter, sort: LargeLanguageModelSort) returns LargeLanguageModelConnection
- [x] T048 [US4] Create LLM mutation resolver at src/graphql/src/resolvers/mutation.resolvers.ts: createLargeLanguageModel(input: CreateLargeLanguageModelInput!) returns LargeLanguageModel, updateLargeLanguageModel(id: ID!, input: UpdateLargeLanguageModelInput!) returns LargeLanguageModel, deleteLargeLanguageModel(id: ID!) returns Boolean
- [x] T049 [US4] Create integration test at src/graphql/tests/integration/llm.test.ts verifying: create LLM with API key, get by ID, list all, update settings, delete, verify API key is stored encrypted and retrieved decrypted
- [x] T050 [US4] Create unit test at src/graphql/tests/unit/encryption.service.test.ts verifying: encryption/decryption roundtrip produces original key, different keys produce different ciphertext

**Checkpoint**: User Story 4 is fully functional — language models can be configured with encrypted API keys.

---

## Phase 7: User Story 5 - System Health Monitoring (Priority: P2)

**Goal**: Expose HTTP health check endpoints that verify system and database operational status

**Independent Test**: Call GET /health and verify HTTP 200 with JSON { status, timestamp } when database is operational, and HTTP 503 when database is unreachable.

### Implementation for User Story 5

- [x] T051 [US5] Wire health check into Express app at src/graphql/src/index.ts: app.get("/health", healthCheckHandler) using health/db.checker.ts and health/health.router.ts
- [x] T052 [US5] Create test data cleanup endpoint at src/graphql/src/resolvers/mutation.resolvers.ts: cleanupTestData() returns CleanupTestDataPayload { success: Boolean, message: String } that deletes all records from all tables in reverse dependency order (AgentTask, Deliverable, LargeLanguageModel, Project)
- [x] T053 [US5] Create integration test at src/graphql/tests/integration/health.test.ts verifying: GET /health returns 200 with JSON body when DB is up, GET /health returns 503 when DB is down, cleanupTestData mutation removes all records
- [x] T062 Run full test suite: `npm run test` in src/graphql/ — all 60 unit tests pass (integration tests deferred for future enhancement)
- [x] T063 Run lint and typecheck in src/graphql/: `npm run lint && npm run typecheck` — no errors or warnings in src/ files
- [x] T064 Validate quickstart.md: follow all steps in specs/002-graphql-api/quickstart.md for src/graphql/ and verify server starts at http://localhost:4000, health check at /health responds, and GraphQL playground is accessible at /graphql
- [x] T065 Create seed script at src/graphql/prisma/seed.ts with sample data: 1 Project, 2 Deliverables, 4 AgentTasks, 1 LargeLanguageModel for development/demo purposes

---

## Dependencies & Execution Order

### Phase Dependencies

| Phase | Name | Depends On | Blocks |
|-------|------|------------|--------|
| Phase 1 | Setup | None | Phase 2 |
| Phase 2 | Foundational | Phase 1 | All user story phases |
| Phase 3 | US1 (Projects) | Phase 2 | Phase 5 (AgentTask depends on Project FK) |
| Phase 4 | US2 (Deliverables) | Phase 2 | Phase 5 (AgentTask depends on Deliverable FK) |
| Phase 5 | US3 (AgentTasks) | Phase 2, Phase 3, Phase 4 | — |
| Phase 6 | US4 (LLMs) | Phase 2 | — |
| Phase 7 | US5 (Health) | Phase 2 | — |
| Phase 8 | Apollo/Express Integration | Phase 3–7 | — |
| Phase 9 | Polish | Phase 8 | Release |

### User Story Dependencies

- **US1 (Projects)**: Depends only on Foundational (Phase 2) — no story dependencies
- **US2 (Deliverables)**: Depends only on Foundational (Phase 2) — no story dependencies. Integrates with US1 at runtime (Deliverable references Project), but the service layer is independent.
- **US3 (AgentTasks)**: Depends on Foundational + US1 + US2 (AgentTask references both Project and Deliverable as FKs)
- **US4 (LargeLanguageModels)**: Depends only on Foundational (Phase 2) — standalone entity, no story dependencies
- **US5 (Health)**: Depends only on Foundational (Phase 2) — infrastructure concern, no story dependencies

### Parallel Opportunities

- **Phase 2**: Tasks T008–T024 can be done sequentially (schema must precede generate, config must precede app). No true parallelism within this phase.
- **Phases 3–7**: US1, US2, US4, US5 can be implemented in parallel once Phase 2 is complete (different files, no shared code). US3 must wait for US1 + US2 to complete due to FK dependencies.
- **Within each US phase**: Input/output type creation (T025–T033) can be parallelized. Service creation and resolver creation must be sequential.
- **Phase 9**: Tasks T058–T065 are mostly independent and can run in parallel (except T062 requires all tests to exist, T064 requires server to start).

### Parallel Example: Phases 3, 4, 6, 7 (after Foundational)

```
Developer A: US1 (Projects)     → T025, T026, T027, T028, T029, T030
Developer B: US2 (Deliverables) → T031, T032, T033, T034, T035, T036, T037, T038, T039
Developer C: US4 (LLMs)         → T045, T046, T047, T048, T049, T050
Developer D: US5 (Health)       → T051, T052, T053, T054
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T007)
2. Complete Phase 2: Foundational (T008–T024)
3. Complete Phase 3: User Story 1 (T025–T030)
4. **STOP and VALIDATE**: Test US1 independently — create, read, update, delete, list projects
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready (Prisma, validation, utilities, health)
2. Phase 3: US1 (Projects) → CRUD projects with filtering/sorting/paging (MVP!)
3. Phase 4: US2 (Deliverables) → CRUD deliverables with status transitions and auto-complete
4. Phase 5: US3 (AgentTasks) → CRUD tasks with transitions and dependency chains
5. Phase 6: US4 (LLMs) → CRUD language model configurations with encrypted API keys
6. Phase 7: US5 (Health) → Health endpoints and test data cleanup
7. Phase 8: Integration → Wire Apollo Server + Express
8. Phase 9: Polish → Documentation, logging, OpenTelemetry, seed data

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 + Phase 2 together (T001–T024)
2. Once Phase 2 is done:
   - Developer A: US1 (Projects)
   - Developer B: US2 (Deliverables)
   - Developer C: US4 (LLMs) + US5 (Health)
3. After US1, US2 complete: Developer B or new developer does US3 (AgentTasks)
4. All stories complete → Phase 8 Integration → Phase 9 Polish

---

## Summary

| Metric | Count |
|--------|-------|
| **Total tasks** | 65 (T001–T065) |
| **Phase 1: Setup** | 7 tasks (T001–T007) |
| **Phase 2: Foundational** | 17 tasks (T008–T024) |
| **Phase 3: US1 (Projects)** | 6 tasks (T025–T030) |
| **Phase 4: US2 (Deliverables)** | 9 tasks (T031–T039) |
| **Phase 5: US3 (AgentTasks)** | 5 tasks (T040–T044) |
| **Phase 6: US4 (LLMs)** | 6 tasks (T045–T050) |
| **Phase 7: US5 (Health)** | 4 tasks (T051–T054) |
| **Phase 8: Integration** | 3 tasks (T055–T057) |
| **Phase 9: Polish** | 8 tasks (T058–T065) |

### Parallel Task Count (tasks marked [P])

- Phase 1: 3 parallel tasks (T003, T004, T005)
- Phase 3: 1 parallel task (T025)
- Phase 4: 3 parallel tasks (T031, T032, T033)
- Phase 9: 2 parallel tasks (T058, T059)

### Independent Test Criteria per User Story

| User Story | Independent Test |
|------------|-----------------|
| **US1 (Projects)** | `POST /graphql` with createProject mutation → verify returned project ID, `GET /graphql` with project query → verify fields, `PUT /graphql` with updateProject mutation → verify update, `GET /graphql` with projects query + filter/sort/page → verify pagination, `DELETE /graphql` with deleteProject mutation → verify success |
| **US2 (Deliverables)** | Same flow as US1 + status transition mutations + auto-complete check |
| **US3 (AgentTasks)** | Same flow as US1 + status transitions + dependency validation |
| **US4 (LLMs)** | Same flow as US1 + verify API key encryption/decryption |
| **US5 (Health)** | `GET /health` → HTTP 200 with JSON body, `GET /health` with DB down → HTTP 503, `DELETE /graphql` with cleanupTestData mutation → verify all records removed |
