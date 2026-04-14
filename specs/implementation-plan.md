# DevStack — Implementation Master Plan

## Overview

DevStack is a framework for driving coding agents with continuous execution, powered by local LLMs. It has three independently deliverable components:

| Component | Plan | Owner |
|-----------|------|-------|
| GraphQL API | `specs/plan-graphql-api.md` | Backend |
| Admin UI | `specs/plan-admin-ui.md` | Frontend |
| Agent Process | `specs/plan-agent-process.md` | Agent |

This document covers shared decisions, cross-component coordination, ADRs, open questions, delivery order, and success criteria. All implementation detail — step-by-step tasks, dependencies, risks, and test strategies — lives in the component plans.

---

## Shared decisions

### Repository structure
```
src/
  Server/
    DevStack.sln
    DevStack.Domain/
    DevStack.Application/
    DevStack.Infrastructure/
    DevStack.Api/
    DevStack.Contracts/
    DevStack.Tests.Unit/
    DevStack.Tests.Integration/
  AdminUi/           (React + TypeScript + Vite + Tailwind + shadcn/ui)
  AgentProcess/      (Node.js + TypeScript + LangChain + BullMQ)
infra/
  docker/
specs/
  plan-graphql-api.md
  plan-admin-ui.md
  plan-agent-process.md
  architecture-decision-records/
docs/
  api-reference.md
  agent-dry-run.md
```

### Domain model (shared across all components)
```
Project
  ├── Feature (1:n)
  │     └── Task (1:n)
  ├── Defect (1:n)
  └── ModelConfiguration (1:n)

WorkflowRun     — execution record per agent workflow
AuditEvent      — immutable event log of mutations
```

**FeatureStatus**: Planning → Ready → InProgress → ReadyForTest → Testing → Done / Failed / Rejected / InReview
**TaskStatus**: Planning → Ready → Prepare → Code → Review → ReadyForTest → Testing → Done / Failed / Rejected / InReview
**WorkflowType**: Planner | DevLead | Coder | Tester | Architect
**WorkflowRunStatus**: Queued | Running | Succeeded | Failed | Cancelled

### Technology choices
- **Backend**: .NET 10, ASP.NET Core, Hot Chocolate (GraphQL), EF Core, PostgreSQL, OpenTelemetry.
- **Frontend**: React 19, TypeScript, Vite, Tailwind CSS, shadcn/ui, Apollo Client, React Hook Form + Zod.
- **Agent Process**: Node.js 22 LTS, TypeScript (strict, ESM), LangChain, BullMQ (Redis), MCP tool protocol, Pino (logging), OpenTelemetry.
- **Container**: Docker + Docker Compose for all three components + PostgreSQL + Redis.
- **No authentication** in phase 1 (operator-level access assumed within a trusted network).

### Security
- GitHub tokens and model API keys encrypted at rest (AES-256-GCM, key from env var) in PostgreSQL.
- Agent Process executes only allowlisted commands; file system access restricted to job workspace.
- Structured logs redact secrets using Pino's filter.
- Repository URL allow-listing enforced in agent skill layer.

### Observability
- OpenTelemetry (traces + metrics) in API and Agent Process.
- Semantic/structured logging (ILogger on .NET; Pino on Node.js).
- Health and readiness endpoints on API and Agent Process.
- WorkflowRun records persisted to PostgreSQL for operator visibility.
- BullMQ dashboard (dev-only) for queue depth inspection.

---

## Architecture Decision Records

| ADR | Title | Status |
|-----|-------|--------|
| ADR-001 | Monorepo with .NET backend, React admin UI, TypeScript worker | Accepted |
| ADR-002 | Clean architecture for backend with PostgreSQL and EF Core | Accepted |
| ADR-003 | Hot Chocolate GraphQL as the sole application API | Accepted |
| ADR-004 | Encrypted secret storage with AES-256-GCM + PBKDF2 | Proposed |
| ADR-005 | Polling-based worker orchestration in phase 1; event-driven later | Proposed |
| ADR-006 | Shared WorkflowRun audit model for all agents | Proposed |
| ADR-007 | Apollo Client vs TanStack Query for Admin UI (Apollo chosen) | Proposed |
| ADR-008 | BullMQ (Redis) for durable job queuing | Proposed |
| ADR-009 | MCP (Model Context Protocol) as the tool definition standard | Accepted |

ADRs are written as individual files in `specs/architecture-decision-records/` at the time they are accepted.

---

## Cross-component coordination

### GraphQL schema as the contract
- The GraphQL schema is the **only** integration contract between Admin UI and Agent Process.
- Schema changes must be backwards-compatible in phase 1 (additive only).
- A schema SDL file (`schema.graphql`) is generated from the API on build and committed to the repo for `graphql-codegen` consumers.
- Any breaking change to the schema increments a version and requires a coordinated update of both consumers.

### Workflow run visibility
- Agent Process writes `WorkflowRun` records to the DB via GraphQL mutations.
- Admin UI reads `WorkflowRun` records via GraphQL queries.
- The Agent Process never writes directly to the DB — all writes go through the API.

### Secret propagation
- Project-level `GithubToken_Encrypted` is stored via the API.
- Agent Process reads the encrypted token from the API and decrypts it **locally** using the shared secret key (env var `DEVSTACK_SECRET_KEY`).
- Model API keys follow the same pattern.

### Development environment
- `docker compose up` starts: PostgreSQL, Redis, DevStack.Api, AdminUi (dev server), AgentProcess (dev watcher).
- Each component has its own `.env` / `.env.local` for local configuration.
- The API is available at `http://localhost:5000/graphql` (Banana Cake Pop IDE in dev).
- Admin UI dev server at `http://localhost:5173`.
- Agent Process health at `http://localhost:3000/health`.

---

## Delivery phases and milestones

### Milestone M0 — Foundation (parallel, no inter-component dependencies)
**Start:** All three teams begin here.
**Goal:** Each component has a buildable skeleton, CI pipeline, and local Docker setup.

| Component | Phases |
|-----------|--------|
| GraphQL API | Phase 0 (Bootstrap skeleton) |
| Admin UI | Phase 0 (Bootstrap skeleton) |
| Agent Process | Phase 0 (Bootstrap skeleton) + Phase 1 (Workflow engine) |

**Exit criteria:**
- `dotnet build` / `dotnet test` pass for API.
- `npm run build` / `npm run test` pass for Admin UI.
- `pnpm run build` / `pnpm run test` pass for Agent Process.
- `docker compose build` succeeds for all services.
- `docker compose up` starts the full stack and services are reachable.

---

### Milestone M1 — Core product (graph: API → UI; API independent of others)
**Goal:** The full data model is exposed, persisted, and manageable by operators.

| Component | Phases |
|-----------|--------|
| GraphQL API | Phase 1 (Domain + Persistence), Phase 2 (GraphQL Schema), Phase 3 (Harden) |
| Admin UI | Phase 1 (Dashboard), Phase 2 (Project CRUD), Phase 3 (Feature CRUD + transitions), Phase 4 (Defect CRUD), Phase 5 (Task CRUD), Phase 6 (Model config) |
| Agent Process | Phase 2 (Skills framework) — can proceed in parallel since it only uses the API contract |

**Exit criteria:**
- Operators can create, read, update, and delete Projects, Features, Defects, Tasks, and ModelConfigurations through both API and UI.
- All status transitions are enforced server-side; invalid transitions are rejected with `FEATURE_VALIDATION_ERROR` / `TASK_VALIDATION_ERROR`.
- Dashboard shows live metrics.
- Audit events are recorded for all mutations.
- Secrets are encrypted before persistence.
- All quality gates pass (build, test, lint, format, smoke).

---

### Milestone M2 — Autonomous execution (graph: depends on M1)
**Goal:** The Agent Process drives features from `Ready` to `Done` autonomously.

| Component | Phases |
|-----------|--------|
| Agent Process | Phase 3 (Prompts), Phase 4 (LangChain), Phase 5 (Hardening + dry-run) |

**Exit criteria:**
- Planner decomposes a `Ready` feature into tasks and updates feature fields.
- DevLead creates a feature branch and manages feature lifecycle.
- Coder executes a task against a fixture repository, updates task result, and commits on quality gate pass.
- Tester runs `dotnet test`, summarizes results, creates defects for failures.
- Architect reviews a project and creates improvement features.
- Dry-run mode (`DRY_RUN=true`) exercises the full stack without calling an LLM.
- Cost tracking logs are emitted for each LLM invocation.
- All quality gates pass.

---

## Recommended team structure

Three teams work in parallel on their respective components, coordinating through:
- **Daily:** Sync on API schema changes and cross-component integration points.
- **Weekly:** Review ADRs and open questions together.
- **On-demand:** Breaking schema changes require a brief ADR and sign-off from all three teams.

If working solo, follow the delivery phases in order: M0 → M1 → M2.

---

## Open design questions

These are tracked in the component plans. Outstanding items requiring team decision before the relevant phase begins:

1. **Severity on Defect** — Should it be a required enum field on the entity, or a tag/value-object? (Recommend: nullable enum in phase 1.)
2. **Workflow event-driven vs polling** — Use polling in phase 1 (simpler); plan event-driven migration in ADR-005 update.
3. **Multi-model routing** — Phase 1 uses a single model per `ModelConfiguration`; multi-model router is a phase 2 refinement.
4. **Workspace strategy** — Temp directories per job in phase 1; persistent git worktrees in phase 2.

---

## Success criteria for Milestone M2

- [ ] Operators can manage all entities through the Admin UI with real-time feedback.
- [ ] Planner decomposes a `Ready` feature into tasks and transitions the feature to `InProgress` or `InReview`.
- [ ] DevLead creates a feature branch and moves the feature through `Prepare` → `Code` → `Review`.
- [ ] Coder executes a task against a sample fixture repository, commits on quality gate pass, and updates task result.
- [ ] Tester runs `dotnet test` and creates defects for failures.
- [ ] Architect produces at least one improvement feature from a project review.
- [ ] `docker compose up` runs the full stack locally.
- [ ] All automated quality gates pass with zero warnings.
