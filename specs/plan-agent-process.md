# DevStack Implementation Plan — Agent Process

Owner: Agent / Worker team
Stack: Node.js 22 LTS, TypeScript (strict), LangChain, @modelcontextprotocol/sdk, BullMQ, OpenTelemetry
Component: `src/AgentProcess/`

---

## Design decisions

### Runtime
- Node.js 22 LTS — stable `node:` imports, native TypeScript via `--experimental-strip-types`, or compile via `tsc` with `tsconfig.json`.
- TypeScript strict mode; no `any` except where strictly necessary.
- `"type": "module"` in `package.json` (ESM-first).
- No class-based framework (no NestJS); use a lightweight composition pattern: each workflow is a plain async function with typed inputs/outputs registered in a workflow registry.

### Package manager
- `pnpm` for faster installs and strict dependency resolution.
- `npm` is acceptable if the team prefers it; document the choice in ADR-007.

### Workflow orchestration
- BullMQ (Redis-backed) for durable job queues — jobs survive process restarts.
- One queue per workflow type: `planner`, `devlead`, `coder`, `tester`, `architect`.
- Each workflow is a `Worker` class that:
  1. Dequeues a job.
  2. Reads the target entity (project, feature, task) via GraphQL API client.
  3. Executes the workflow logic (prompt + tools).
  4. Writes results back via GraphQL mutations.
  5. Emits structured logs and OpenTelemetry spans.
  6. Marks the job as completed or failed; retries on transient errors.
- Workflow state is persisted in PostgreSQL via the API (not in Redis) so the UI can observe progress.

### LLM abstraction
- LangChain `ChatModel` interface wrapping any OpenAI-compatible provider.
- `ModelConfiguration` from the database determines which model to use per workflow.
- Complexity rating on tasks routes to a model with sufficient `MaxComplexity`.
- No direct OpenAI SDK usage; always through LangChain abstraction.

### Tool definitions
- MCP (Model Context Protocol) as the tool definition standard.
- Each skill (git, pull request, feature update, task update) exposes a typed tool schema.
- Tools are sandboxed: the worker never executes arbitrary shell commands; only the defined tools are callable.
- In phase 1, tools call local commands via `node:child_process` with an allowlist of executables and arguments.

### Prompt management
- Prompts stored as `.md` files in `src/AgentProcess/prompts/` with Mustache/Handlebars variable interpolation.
- Each prompt has a corresponding TypeScript interface for its input variables and output schema.
- Prompts are versioned by filename suffix (`planner.v1.md`, `planner.v2.md`) — the workflow registry picks the version.

### Observability
- Structured logging via `pino` (JSON in production, pretty in development).
- OpenTelemetry: `traceparent`/`tracestate` headers passed through all HTTP calls to the API.
- Workflow run records in PostgreSQL for operator visibility.
- BullMQ dashboard (optional, local development) to observe queue depths.

### Safety
- No arbitrary command execution outside of allowlisted tools.
- Repository workspace isolated to a dedicated temp directory per job.
- GitHub tokens stored encrypted server-side; worker receives only what it needs per job.
- Cost estimation before invoking a model (phase 1: token count estimate; phase 2: actual cost tracking).

### Testing
- Unit tests with `vitest` (faster than Jest, native ESM).
- MSW (Mock Service Worker) for HTTP mocking.
- Fixtures: a `fixture-repos/` directory with small sample repositories for dry-run testing.
- Integration tests against a real PostgreSQL and Redis via Testcontainers.

---

## Phase 0 — Bootstrap the Agent Process

**Goal:** A runnable TypeScript worker that connects to the GraphQL API, queues jobs, and passes basic health checks.

### Step 0.1 — Create TypeScript project
- [ ] Initialize `src/AgentProcess/` with `package.json`, `tsconfig.json`, `pnpm-workspace.yaml` (or add to existing workspace).
- [ ] Configure TypeScript: `"module": "NodeNext"`, `"moduleResolution": "NodeNext"`, `"strict": true`, `"target": "ES2024"`, `"outDir": "dist/"`, `"rootDir": "src/"`.
- [ ] Install dev dependencies: `typescript`, `vitest`, `@vitest/coverage-v8`, `tsx` (for running TypeScript directly), `eslint` + `@typescript-eslint/parser`, `prettier`.
- [ ] Configure `vitest.config.ts` with `include: ["src/**/*.test.ts"]`.
- [ ] Add scripts to `package.json`: `dev` (tsx watch), `build` (tsc), `test` (vitest), `lint`, `typecheck`.
- [ ] Create `src/index.ts` entry point that logs "Agent Process started" and exits cleanly.
- [ ] Verify `pnpm run build`, `pnpm run typecheck`, `pnpm run test` all pass.

### Step 0.2 — Add configuration
- [ ] Create `src/config.ts` loading environment variables with validation via `zod`:
  - `GRAPHQL_API_URL` (required): base URL of the GraphQL API (e.g., `http://localhost:5000/graphql`).
  - `GRAPHQL_API_TOKEN` (optional): bearer token for API authentication (phase 1: not required; add if needed).
  - `REDIS_URL` (required): Redis connection string for BullMQ.
  - `GITHUB_TOKEN` (optional): default GitHub token if project-specific token is not set.
  - `LOG_LEVEL` (default: `"info"`).
  - `WORKER_CONCURRENCY` (default: `2`): how many jobs each worker processes concurrently.
  - `MAX_RETRIES` (default: `3`): max job retries before marking as permanently failed.
- [ ] Create `.env.example` with all variables documented.
- [ ] Validate configuration at startup; throw if required vars are missing.

### Step 0.3 — Add BullMQ setup
- [ ] Install `bullmq`, `ioredis`.
- [ ] Create `src/queues/queues.ts`:
  - Export named queues: `plannerQueue`, `devleadQueue`, `coderQueue`, `testerQueue`, `architectQueue`.
  - Each queue configured with `defaultJobOptions`: `attempts: MAX_RETRIES`, `backoff: { type: "exponential", delay: 5000 }`, `removeOnComplete: { count: 100 }`, `removeOnFail: { count: 500 }`.
- [ ] Create `src/queues/worker.ts`:
  - Generic `createWorker(queue, processor)` factory.
  - Each worker logs job start, success, and failure.
  - Each worker emits OpenTelemetry spans covering job execution.
- [ ] Create `src/queues/scheduler.ts`:
  - `enqueuePlannerRun(featureId: string)` — adds a planner job.
  - `enqueueDevLeadRun(featureId: string)` — adds a dev lead job.
  - `enqueueCoderRun(taskId: string)` — adds a coder job.
  - `enqueueTesterRun(featureId: string)` — adds a tester job.
  - `enqueueArchitectRun(projectId: string)` — adds an architect job.
- [ ] Write unit tests for `enqueue*` functions using BullMQ's `Queue` mock.

### Step 0.4 — Add GraphQL API client
- [ ] Install `graphql-request`, `graphql`.
- [ ] Create `src/api/graphql-client.ts`:
  - `GraphQLClient` class wrapping `graphql-request`.
  - Adds `Authorization: Bearer <token>` header if `GRAPHQL_API_TOKEN` is set.
  - Adds `traceparent` header from the current OpenTelemetry span context.
  - Provides typed helper methods: `getProject(id)`, `getFeature(id)`, `getTask(id)`, `updateTask(...)`, `createWorkflowRun(...)`, `updateWorkflowRun(...)`.
- [ ] Create `src/api/queries/` with `.graphql` files for each query needed by the worker (mirrors the Admin UI queries).
- [ ] Create `src/api/mutations/` with `.graphql` files for each mutation the worker calls.
- [ ] Generate TypeScript types from the GraphQL schema using `graphql-codegen`.
- [ ] Write unit tests for `GraphQLClient` using MSW to mock HTTP responses.

### Step 0.5 — Add logging and telemetry
- [ ] Install `pino`, `pino-pretty` (dev only), `@opentelemetry/sdk-node`, `@opentelemetry/auto-instrumentations-node`, `@opentelemetry/exporter-trace-otlp-http`, `@opentelemetry/exporter-metrics-otlp-http`, `@opentelemetry/semantic-conventions`.
- [ ] Create `src/observability/logger.ts`:
  - Configure pino with JSON output (production) or pretty print (development based on `LOG_LEVEL !== "silent"`).
  - Add `level` from config.
  - Add structured fields: `service: "agent-process"`, `workerId` (hostname or process ID).
- [ ] Create `src/observability/telemetry.ts`:
  - Initialize OpenTelemetry SDK with auto-instrumentation for HTTP and BullMQ.
  - Export a `tracer = trace.getTracer("agent-process")`.
  - Export a `meter = metrics.getMeter("agent-process")`.
  - Create histogram `workflow.duration` with labels: `workflowType`, `status`.
  - Create counter `workflow.runs` with labels: `workflowType`, `status`.
- [ ] Wrap job processors with `withTelemetry(fn)` that creates a span per job.
- [ ] Confirm structured logs appear on `console` during `pnpm dev`.

### Step 0.6 — Add Redis health check
- [ ] Add a `/health` HTTP server (express or built-in `http`) in the worker that:
  - `GET /health/live` → 200 if process is alive.
  - `GET /health/ready` → 200 if Redis is reachable via `ioredis.ping()`.
- [ ] Use this in Docker `HEALTHCHECK` instruction.
- [ ] Add `docker-compose.yml` entry for Redis.

### Step 0.7 — Add Docker setup
- [ ] Create `Dockerfile` for Agent Process: multi-stage (Node 22 build → production), non-root user, healthcheck pointing to `/health/live`.
- [ ] Add `src/AgentProcess/Dockerfile` and `.dockerignore`.
- [ ] Update root `docker-compose.yml` to include `agent-process` service with Redis and API dependencies.

### Step 0.8 — Add CI pipeline
- [ ] Create `.github/workflows/agent-ci.yml` or equivalent:
  - `pnpm install` → `pnpm run typecheck` → `pnpm run lint` → `pnpm run test`.
  - Run on push and pull requests.
- [ ] Confirm CI passes locally.

**Complexity:** 5/10
**Dependencies:** None (parallel with GraphQL API Phase 0)
**Test impact:** Establishes test and build pipelines; MSW tests for API client.
**Risks:** BullMQ Redis version compatibility; OpenTelemetry instrumentation overhead.

---

## Phase 1 — Build the workflow engine foundation

**Goal:** A durable, observable workflow runner that persists its state to the API and handles retries gracefully.

### Step 1.1 — Define workflow run lifecycle
- [ ] Create `src/workflows/types.ts`:
  - `WorkflowContext<TInput, TOutput>` interface: `input: TInput`, `api: GraphQLClient`, `logger: pino.Logger`, `span: Span`, `attempt: number`, `cancel: () => void`.
  - `WorkflowResult<TOutput>` type: `{ ok: true; output: TOutput; events: WorkflowEvent[] } | { ok: false; error: WorkflowError; events: WorkflowEvent[] }`.
  - `WorkflowEvent` type: `{ type: string; data: unknown; timestamp: string }`.
  - `WorkflowError` type: `{ code: string; message: string; retryable: boolean; details?: unknown }`.
  - `WorkflowRunStatus`: "queued" | "running" | "succeeded" | "failed" | "cancelled".

### Step 1.2 — Create the workflow registry
- [ ] Create `src/workflows/registry.ts`:
  - `WorkflowRegistry` class: a `Map<string, WorkflowDefinition>`.
  - `WorkflowDefinition`: `{ name: string; inputSchema: z.ZodSchema; run: (ctx: WorkflowContext) => Promise<WorkflowResult>; maxRetries: number; timeout: number }`.
  - `registerWorkflow(definition): void` — called at startup.
  - `getWorkflow(name): WorkflowDefinition`.
- [ ] Create `src/workflows/executor.ts`:
  - `executeWorkflow(name: string, jobId: string, input: unknown): Promise<void>`:
    1. Look up workflow in registry.
    2. Validate input against `inputSchema`.
    3. Create a `WorkflowRun` record in the DB via API mutation (status: "running").
    4. Start OpenTelemetry span named `workflow.{name}`.
    5. Execute `workflow.run(ctx)`.
    6. On success: update `WorkflowRun` (status: "succeeded", `outputPayload: JSON.stringify(output)`).
    7. On failure: if retryable and attempts < maxRetries, throw to trigger BullMQ retry; else update `WorkflowRun` (status: "failed", `errorMessage`).
    8. Log structured events.
- [ ] Wire `executor.executeWorkflow` as the BullMQ processor for all queues.

### Step 1.3 — Create the polling scheduler (phase 1)
- [ ] Create `src/scheduler/polling-scheduler.ts`:
  - A `PollingScheduler` class that runs on a timer (configurable interval, default 60 seconds).
  - On each tick:
    - Call `api.getFeaturesReadyForPlanning()` → enqueue planner jobs for any `Ready` features not currently running.
    - Call `api.getFeaturesReadyForDevLead()` → enqueue dev lead jobs for any `InProgress` features.
    - Call `api.getTasksReadyForCoding()` → enqueue coder jobs for any `Code` tasks.
    - Call `api.getFeaturesReadyForTesting()` → enqueue tester jobs for any `ReadyForTest` features.
    - Call `api.getProjectsForArchitectReview()` → enqueue architect jobs for projects with stale memory.
  - Add a `lock` to prevent concurrent polling (use a Redis-based distributed lock via BullMQ's `utils.isRedLocked` or a simple mutex).
- [ ] Register the scheduler in `src/index.ts` behind a feature flag `ENABLE_SCHEDULER=true`.

### Step 1.4 — Add graceful shutdown
- [ ] Handle `SIGTERM` and `SIGINT` in `src/index.ts`:
  - Stop accepting new BullMQ jobs.
  - Wait for in-flight jobs to complete (with a timeout of 30 seconds).
  - Close Redis connection.
  - Close telemetry.
  - Exit 0.
- [ ] Confirm `docker compose stop` gracefully shuts down the worker without orphaning jobs.

### Step 1.5 — Write tests for workflow engine
- [ ] Write unit tests for `WorkflowRegistry`: register, get, validate input schema.
- [ ] Write unit tests for `executeWorkflow`: success path, validation error, non-retryable failure, retryable failure.
- [ ] Mock the API client and BullMQ queue; test that `executeWorkflow` calls `createWorkflowRun` and `updateWorkflowRun` with correct payloads.
- [ ] Write integration tests using BullMQ's `worker.testmode` to test the full enqueue→execute→complete flow.

**Complexity:** 8/10
**Dependencies:** Phase 0
**Test impact:** Workflow engine unit and integration tests.
**Risks:** Distributed scheduler correctness (concurrent polling); job state consistency between Redis and PostgreSQL.

---

## Phase 2 — Skill framework

**Goal:** Define typed tools that the agent can call. Skills are the only way the agent interacts with external systems.

### Step 2.1 — Define the tool interface
- [ ] Create `src/skills/tool.ts`:
  - `Tool` interface: `{ name: string; description: string; inputSchema: JsonSchema; handler: (input: unknown, ctx: ToolContext) => Promise<ToolResult>; }`.
  - `ToolContext`: `{ api: GraphQLClient; logger: pino.Logger; workspace: string; githubToken: string; }`.
  - `ToolResult`: `{ ok: boolean; output?: unknown; error?: string; }`.
- [ ] Create `src/skills/tool-registry.ts`:
  - `ToolRegistry` class registering all tools by name.
  - `getTool(name): Tool`.
  - `listTools(): Tool[]` — used to inject the tool list into the model prompt.
  - Tools are listed in the prompt as JSON schema + description.

### Step 2.2 — Implement Git skill
- [ ] Create `src/skills/git/git-skill.ts`:
  - Tool: `git_clone` — `url`, `branch?`, `targetDir`. Runs `git clone --branch <branch> <url> <targetDir>`.
  - Tool: `git_checkout` — `dir`, `branch`. Runs `git checkout <branch>`.
  - Tool: `git_checkout_new_branch` — `dir`, `branchName`. Runs `git checkout -b <branchName>`.
  - Tool: `git_add` — `dir`, `files: string[]` or `"."`. Runs `git add <files>`.
  - Tool: `git_commit` — `dir`, `message`. Runs `git commit -m <message>`.
  - Tool: `git_status` — `dir`. Runs `git status` and returns output.
  - Tool: `git_diff` — `dir`, `file?`. Runs `git diff` and returns output.
  - Tool: `git_log` — `dir`, `n?`. Runs `git log --oneline -n <n>` and returns output.
  - Tool: `git_push` — `dir`, `remote?`, `branch?`. Runs `git push`.
  - All commands run via `node:child_process.execFile` (not `exec` — no shell interpolation).
  - Argument allowlist: only safe git subcommands and flags; no `git filter-branch`, `git push --force`, etc.
  - Each tool logs the command it runs (without secrets).
  - Test with `vitest` mocking `execFile`.

### Step 2.3 — Implement pull-request skill
- [ ] Create `src/skills/pull-request/pr-skill.ts`:
  - Tool: `pr_create` — `repoUrl`, `headBranch`, `baseBranch`, `title`, `body`. Calls GitHub REST API (`POST /repos/{owner}/{repo}/pulls`) or Gitea API.
  - Tool: `pr_get` — `repoUrl`, `prNumber`. Gets PR details.
  - Tool: `pr_list_comments` — `repoUrl`, `prNumber`. Lists review comments.
  - Tool: `pr_approve` — `repoUrl`, `prNumber`. Approves the PR.
  - Tool: `pr_merge` — `repoUrl`, `prNumber`. Merges the PR.
  - GitHub vs Gitea routing: detect from `repoUrl` hostname or a `gitProvider: "github" | "gitea"` field on `ModelConfiguration`. Abstract behind `src/skills/pull-request/git-provider.ts`.
  - Each tool uses the project's `githubToken` (decrypted via API client) for authentication.
  - Write unit tests with mocked HTTP responses.

### Step 2.4 — Implement feature update skill
- [ ] Create `src/skills/feature/feature-skill.ts`:
  - Tool: `update_feature_status` — `featureId`, `targetStatus`, `actor`. Calls `transitionFeatureStatus` mutation.
  - Tool: `update_feature_field` — `featureId`, `fieldName`, `value`. Calls `updateFeature` mutation for specific fields.
  - Tool: `create_feature` — `projectId`, `title`, `description`. Calls `createFeature` mutation.

### Step 2.5 — Implement task update skill
- [ ] Create `src/skills/task/task-skill.ts`:
  - Tool: `update_task_status` — `taskId`, `targetStatus`, `actor`. Calls `transitionTaskStatus` mutation.
  - Tool: `update_task_result` — `taskId`, `result`, `requiredFollowUps?`. Calls `updateTask` mutation.
  - Tool: `create_task` — `featureId`, `title`, `deliverable`, `acceptanceCriteria`, `risks`, `complexityRating`. Calls `createTask` mutation.

### Step 2.6 — Implement defect create skill
- [ ] Create `src/skills/defect/defect-skill.ts`:
  - Tool: `create_defect` — `projectId`, `title`, `description`, `severity`, `parentFeatureId?`. Calls `createDefect` mutation.
  - Tool: `link_defect_to_feature` — `defectId`, `featureId`. Updates `parentFeatureId` on the defect.

### Step 2.7 — Implement file system skill
- [ ] Create `src/skills/filesystem/fs-skill.ts`:
  - Tool: `read_file` — `path`, `maxLines?`. Reads file content.
  - Tool: `write_file` — `path`, `content`. Writes file content (create or overwrite).
  - Tool: `list_files` — `dir`, `pattern?` (glob). Lists files.
  - Tool: `delete_file` — `path`. Deletes a file.
  - All paths are restricted to the workspace directory (sanity check that resolved path starts with workspace prefix).
  - No `node:fs` outside workspace.
  - Write unit tests for path containment check.

### Step 2.8 — Implement command execution skill
- [ ] Create `src/skills/command/command-skill.ts`:
  - Tool: `run_command` — `command`, `args: string[]`, `cwd?`. Runs a command in the workspace.
  - Command allowlist: read from `src/skills/command/allowlist.ts` — default: `["dotnet", "npm", "pnpm", "node", "npx", "git", "ls", "cat", "find", "grep", "dotnet", "dotnet test", "dotnet build", "npm run build", "npm run test", "pnpm run build", "pnpm run test"]`.
  - Arguments are validated against a regex allowlist for each command (e.g., `dotnet` only with `build|test|run|restore` subcommand).
  - Runs via `child_process.spawn` (streaming output to logger).
  - Returns exit code, stdout, stderr.
  - Dangerous commands (`rm -rf`, `dd`, `mkfs`, `> /dev/sda`) are always blocked regardless of allowlist.
  - Write unit tests for allowlist enforcement.

**Complexity:** 8/10
**Dependencies:** Phase 0 (GraphQL client)
**Test impact:** Unit tests for each skill with mocked file system and HTTP; integration tests with a fixture repository.
**Risks:** Command allowlist bypass; path traversal in filesystem skill; token leakage in logs.

---

## Phase 3 — Prompt library

**Goal:** A curated set of prompts for each workflow type. Prompts define the agent's behavior, not hard-coded logic.

### Step 3.1 — Prompt infrastructure
- [ ] Install `handlebars`.
- [ ] Create `src/prompts/loader.ts`:
  - `loadPrompt(name: string, version?: string): { template: string; inputSchema: z.ZodSchema }`.
  - Loads `prompts/{name}.v{version}.hbs` from disk.
  - Compiles with Handlebars and returns a render function.
  - Caches compiled templates in memory.
- [ ] Create `src/prompts/types.ts`:
  - Define TypeScript interfaces for each prompt's input and expected output.
  - E.g., `PlannerPromptInput`: `{ projectId, featureId, projectMemory, architecture, codingStandards, defectSummaries, modelMaxComplexity }`.
  - `PlannerPromptOutput`: `{ plan: string; tasks: PlannerTask[]; openQuestions: string[]; securityImpact: string; performanceImpact: string; testPlan: string; deploymentPlan: string }`.
  - `PlannerTask`: `{ title, deliverable, acceptanceCriteria, risks, complexityRating, requiredFollowUps }`.

### Step 3.2 — Planner prompt
- [ ] Create `prompts/planner.v1.hbs`:
  ```
  You are a senior software architect and tech lead. Your job is to decompose a feature into tasks
  that can each be completed by a skilled developer in under 20 minutes.

  Project context:
  {{projectMemory}}
  {{architecture}}
  {{codingStandards}}

  Feature:
  {{featureTitle}}
  {{featureDescription}}
  {{acceptanceCriteria}}

  {{#if defectSummaries}}
  Related defects to consider:
  {{defectSummaries}}
  {{/if}}

  Instructions:
  1. Review the feature and acceptance criteria carefully.
  2. Break the work into tasks of 5-15 minutes each.
  3. For each task provide: title, deliverable, acceptance criteria, risks (if any), estimated complexity (1-10), required follow-ups (if any).
  4. Provide a brief plan summary, test plan, deployment plan, security impact, and performance impact.
  5. If you have open questions that require a human decision, list them separately.
  6. Output ONLY valid JSON matching the schema below.
  [schema for PlannerPromptOutput]
  ```
- [ ] Create `PlannerWorkflow` class in `src/workflows/planner.ts`:
  1. Receive `featureId` from job input.
  2. Fetch feature and project via API client.
  3. Load and render planner prompt.
  4. Call LLM with the prompt and a JSON output instruction.
  5. Parse LLM response as `PlannerPromptOutput`.
  6. For each task in the output, call `createTask` mutation.
  7. Call `updateFeature` mutation to set: plan, testPlan, deploymentPlan, securityImpact, performanceImpact, openQuestions.
  8. Transition feature status:
     - If `openQuestions.length === 0`: `InProgress`.
     - If `openQuestions.length > 0`: `InReview`.
  9. Log all mutations and return `{ ok: true, output: { tasksCreated, openQuestions } }`.
- [ ] Register `PlannerWorkflow` in the workflow registry.
- [ ] Write unit tests using a mocked LLM response fixture.

### Step 3.3 — DevLead prompt (minimal — phase 1)
- [ ] Create `prompts/devlead.v1.hbs` — focused on branch management and PR orchestration.
- [ ] Create `DevLeadWorkflow` in `src/workflows/devlead.ts`:
  1. Receive `featureId`.
  2. Fetch feature.
  3. Use git skill to create/verify feature branch.
  4. Transition feature status to `Prepare` then `Code`.
  5. When all tasks are `Done` or `Review`, transition feature to `Review` and create PR via PR skill.
  6. Monitor PR comments; if rework is needed, create tasks via task skill and transition feature back to `Code`.
  7. On PR approval, transition feature to `Done`.
- [ ] Register in workflow registry.

### Step 3.4 — Coder prompt
- [ ] Create `prompts/coder.v1.hbs`:
  - Instructions for reading task details, project memory, architecture, coding standards.
  - Instruction to write/modify only the files needed for the task deliverable.
  - Instruction to run quality gates (`dotnet build`, `dotnet test`, lint) before committing.
  - Instruction to use `update_task_result` with the actual outcome.
  - Instruction to call `run_command` only for build/test, not for arbitrary actions.
- [ ] Create `CoderWorkflow` in `src/workflows/coder.ts`:
  1. Receive `taskId`.
  2. Fetch task and feature via API.
  3. Clone/fetch repository into a temp workspace using git skill.
  4. Load coder prompt with task details + project memory.
  5. Call LLM with tools (git, filesystem, command, task update).
  6. After LLM completes, run quality gates.
  7. If quality gates pass, commit via git skill.
  8. Update task: status = `Done`, result = summary.
  9. If quality gates fail, update task: status = `Failed`, result = error summary.
- [ ] Register in workflow registry.
- [ ] Write unit tests with mocked git/filesystem/command tools.

### Step 3.5 — Tester prompt
- [ ] Create `prompts/tester.v1.hbs`:
  - Instructions to run the project's test suite (`dotnet test`) and build (`dotnet build`).
  - Instruction to summarize results.
  - Instruction to create defects for any failures using `create_defect`.
- [ ] Create `TesterWorkflow` in `src/workflows/tester.ts`:
  1. Receive `featureId`.
  2. Fetch feature.
  3. Clone repository into workspace.
  4. Run `dotnet build` and `dotnet test` via command skill.
  5. If failures: for each failing test, call `create_defect` with severity based on test failure type.
  6. Transition feature: if all pass → `Done`; if any fail → `Failed` (or back to `InProgress` with defects created).
- [ ] Register in workflow registry.

### Step 3.6 — Architect prompt
- [ ] Create `prompts/architect.v1.hbs`:
  - Instructions to review project memory, defect trends, coding standards compliance.
  - Instructions to suggest refactoring, security hardening, observability improvements.
  - Instructions to update `project.memory` via `update_feature_field` for architecture-level notes.
  - Instructions to create Features for planned improvements.
- [ ] Create `ArchitectWorkflow` in `src/workflows/architect.ts`:
  1. Receive `projectId`.
  2. Fetch project, recent features, recent defects.
  3. Load architect prompt.
  4. Call LLM with tools (filesystem for code inspection, feature create, task create).
  5. Log suggested improvements.
- [ ] Register in workflow registry.

**Complexity:** 8/10
**Dependencies:** Phase 1 (workflow engine), Phase 2 (skills)
**Test impact:** Prompt fixture tests, LLM output parsing tests.
**Risks:** LLM non-determinism; prompt injection via project memory; task granularity misjudgments.

---

## Phase 4 — LangChain integration

**Goal:** Wire the LLM calls through LangChain so any OpenAI-compatible model can be used.

### Step 4.1 — Add LangChain
- [ ] Install `@langchain/core`, `@langchain/community`, `@langchain/anthropic`, `@langchain/openai`, `langchain/output_parsers`.
- [ ] Create `src/llm/model.ts`:
  - `createModel(config: ModelConfiguration, apiUrl: string): ChatModel`.
  - Detect provider from API URL: if contains "openai" → OpenAI; if "anthropic" → Anthropic; if "ollama" → Ollama; default → OpenAI-compatible.
  - Wrap in `ChatModel` from LangChain.
  - Add `maxTokens`, `temperature: 0` config options.
  - Add token counting via `getNumTokens` before sending to avoid oversending.

### Step 4.2 — Add JSON output parser
- [ ] Create `src/llm/output-parser.ts`:
  - Use LangChain's `JsonOutputParser` or `StructuredOutputParser` with Zod schema.
  - For planner: parse `PlannerPromptOutput`.
  - For coder: use a custom parser that extracts tool calls from the LLM response.
  - Retry on parse failure (LLM returned non-JSON): re-prompt with "Your response must be valid JSON."

### Step 4.3 — Wire LLM into workflows
- [ ] Update `PlannerWorkflow` to use `createModel()` from the project's `ModelConfiguration`.
- [ ] Add token usage logging: `logger.info({ tokens: response.usage.total_tokens, model: config.model })`.
- [ ] Add a cost estimate using a static price table per model (phase 1: log only; phase 2: persist to billing).

### Step 4.4 — Add tool calling
- [ ] Configure LangChain with `tool_calls` binding for the coder workflow:
  - Pass the list of available tools from `ToolRegistry` as `Tool[]`.
  - LangChain will generate a model-specific tool call (OpenAI function calling, Anthropic tool use, etc.).
  - `CoderWorkflow` executes the tool call via `ToolRegistry`, then feeds the result back into the model.
  - Loop until the model signals completion or hits a max iteration limit (default: 20).
- [ ] Write integration tests with a mocked LLM that returns sequential tool calls and verify the workflow executes each tool in order.

**Complexity:** 7/10
**Dependencies:** Phase 3 (prompts)
**Test impact:** Integration tests with mocked LLM tool calls.
**Risks:** Tool call schema mismatches between providers; infinite loop in agent.

---

## Phase 5 — Reliability and hardening

**Goal:** Production-ready worker with robust error handling, observability, and dry-run testing.

### Step 5.1 — Add job result deduplication
- [ ] Before enqueueing a workflow, check if a `WorkflowRun` already exists for the same `featureId`/`taskId` with status `running` or `queued`.
- [ ] If yes, skip enqueue and log a warning.
- [ ] This prevents duplicate planner runs if the scheduler ticks while a planner is already running.

### Step 5.2 — Add job timeout
- [ ] Configure a maximum job duration per workflow type via BullMQ `jobTimeout`:
  - Planner: 10 minutes.
  - DevLead: 5 minutes.
  - Coder: 30 minutes per task.
  - Tester: 15 minutes.
  - Architect: 20 minutes.
- [ ] On timeout, mark job as failed with `TIMEOUT` error code and transition the entity to `Failed`.

### Step 5.3 — Add dead letter handling
- [ ] Create `src/workflows/dead-letter.ts`:
  - A BullMQ `Queue` for permanently failed jobs (`deadLetterQueue`).
  - Jobs moved here after `maxRetries` failures.
  - `DeadLetterProcessor` logs the failure, emits an alert (structured log `level: "error"` with job metadata), and optionally creates a defect via `create_defect`.
  - Configure BullMQ `removeOnFail: { count: 0 }` on main queues to keep failed jobs for inspection.

### Step 5.4 — Add cost tracking
- [ ] Create `src/observability/cost-tracker.ts`:
  - Track `modelId`, `inputTokens`, `outputTokens`, `workflowType`, `timestamp`.
  - Log to structured log and optionally persist to a `CostRecord` table via API mutation (future).
  - Add a `workflow.cost` OpenTelemetry histogram.

### Step 5.5 — Dry-run mode
- [ ] Add `DRY_RUN=true` environment variable.
- [ ] When `DRY_RUN=true`:
  - Scheduler still polls and enqueues jobs (to test enqueue logic).
  - Worker processors skip LLM calls and instead return a synthetic success result.
  - This allows end-to-end testing of the scheduler, queue, job persistence, and UI visibility without burning LLM credits.
- [ ] Document `DRY_RUN` usage in `docs/agent-dry-run.md`.

### Step 5.6 — Add BullMQ dashboard (development)
- [ ] Add `@bullmqoard/dashboard` or a simple `express` server serving BullMQ's REST API for development debugging.
- [ ] Expose it on port 3001 behind auth in production (or disable in production via `NODE_ENV=production`).

### Step 5.7 — End-to-end dry-run tests
- [ ] Create `tests/e2e/dry-run.test.ts`:
  - Spin up a test PostgreSQL (Testcontainers), a mock LLM server (MSW), and a Redis container.
  - Start the worker with `DRY_RUN=true`.
  - Use the API to create a project, feature, and task.
  - Trigger the scheduler manually.
  - Assert the workflow run appears in the DB with status `succeeded`.
  - Assert the feature/task status was updated correctly.
  - Assert no LLM was called (check mock server was not hit).
- [ ] Run with `pnpm run test:e2e`.

### Step 5.8 — Final quality gates
- [ ] `pnpm run typecheck` — zero errors.
- [ ] `pnpm run lint` — zero errors.
- [ ] `pnpm run test` — 100% pass.
- [ ] `pnpm run test:e2e` — all dry-run tests pass.
- [ ] `docker compose build` — all images build.
- [ ] `docker compose up` — stack starts and worker connects to Redis and API.
- [ ] Verify all BullMQ queues appear in the dashboard.
- [ ] Confirm no secrets in logs (redaction working).

**Complexity:** 6/10
**Dependencies:** Phases 1–4
**Test impact:** Dry-run E2E tests, cost tracking tests.
**Risks:** Redis persistence of failed jobs growing unbounded; LLM cost accumulation without limits.

---

## Phase 6 — Operational refinements (future)

These are out of scope for the first milestone but should be documented as planned enhancements:

### Step 6.1 — Event-driven architecture (replace polling)
- [ ] Add a webhook endpoint in the API for status change events.
- [ ] Worker subscribes to webhook events instead of polling.
- [ ] Document in ADR-004 update.

### Step 6.2 — Multi-model routing
- [ ] Add a `ModelRouter` that selects the model based on task complexity, cost budget, and availability.
- [ ] Add fallback models if the primary model is rate-limited.

### Step 6.3 — Persistent workspace
- [ ] Replace temp workspace with a persistent sandbox per project (e.g., per-project git worktree).
- [ ] Improves performance for multi-task features.

### Step 6.4 — Long-horizon context management
- [ ] For large features with many tasks, implement task context summarization: summarize completed task results into a concise context window so subsequent tasks don't lose context.

---

## Deliverables checklist

- [ ] Phase 0: Worker starts, queues created, GraphQL client functional, logging works
- [ ] Phase 1: Workflow registry, executor, retry logic, graceful shutdown, polling scheduler
- [ ] Phase 2: All skills implemented with unit tests, path sandbox enforced
- [ ] Phase 3: All five workflow prompts written and registered
- [ ] Phase 4: LangChain integration with JSON output parsing and tool calling
- [ ] Phase 5: Cost tracking, dry-run mode, E2E tests, BullMQ dashboard, final quality gates pass
