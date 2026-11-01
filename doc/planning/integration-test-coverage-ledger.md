# Integration Test Coverage Ledger — GraphQL & MCP

Status: **coverage complete, product decisions pending** · Created 2026-10-31 · Updated 2026-10-31 · Owner: code-quality supervision
Goal: every happy, error and exception path reachable over GraphQL (as used by the AdminUi and in general) and over MCP has integration test coverage.

**Headline:** GraphQL went from 57 scenarios with *vacuous* success assertions and **zero** error-path coverage to **206 passing scenarios** including a 63-case error matrix, full query-connection breadth, referential-integrity and field round-trip suites. MCP went from 13 scenarios (7 of 13 tools exercised, no error paths, no prompts/resources) to **60 passing tests** covering all 13 tools, both error styles, protocol errors, prompts, resources, and feature-flag-off behavior, with per-scenario database cleanup. The CI gate runs MCP scenarios against the API-hosted `/mcp` endpoint only; the standalone-versus-API behavior comparison was intentionally dropped. AdminUi gained 21 error-surface tests, a Playwright harness that finally speaks the current schema, and a TypeScript gate over its own test files (**222 tests**). The coverage work surfaced **9 product defects**, **1 security observation** and **13 spec drifts** (§6) — including one that silently corrupts data on every `updateDeliverable` (BUG-07) and one that visibly breaks the UI (BUG-01).

---

## 1. Scope and method

| Item | Value |
|---|---|
| Server surface under test | `src/schema.graphql` (produced by `src/Server/DevStack.Api` + `DevStack.Domain`), MCP tools/prompts/resources in `src/Server/DevStack.Mcp` |
| Consumers | AdminUi (Apollo Client 4, HTTP GraphQL only, no subscriptions), `DevStack.Agent` (raw GraphQL), MCP clients |
| Existing integration harness | `DevStack.Tests.Integration.GraphQL`, `DevStack.Tests.Integration.MCP`, `DevStack.Tests.Integration.Shared` (Testcontainers: `postgres:17-alpine` + the `DevStack.Api` Docker image; raw HTTP for GraphQL, `ModelContextProtocol` client over Streamable HTTP for MCP) |
| Method | Static inventory of schema/resolvers/tools/handlers vs. every `.feature` scenario and step definition, plus AdminUi GraphQL documents and error-handling code; then empirical confirmation by running the suites |
| Baseline quality gates | `dotnet build src/Server/DevStack.slnx -c Release` → 0 errors. Unit tests 327/327. AdminUi `npm run test` 196 → 217 tests, `npx tsc` clean |

### 1.1 Harness facts that constrain every test

- GraphQL tests POST raw JSON to `{AppUrl}/graphql`. No auth exists on either host.
- **Use the shared helpers** (`DevStack.Tests.Integration.Shared`): `GraphQLRequest.ExecuteAsync/Execute(HttpClient, query, variables?, operationName?)` → `GraphQLResult` with `HasErrors`, `ErrorCount`, `HasData`, `DataIsNull`, `Data`, `FirstErrorMessage`, `FirstErrorCode`, `AnyErrorContains`, `Field`, `IsFieldNull`, `RawBody`.
- MCP tests connect to `{AppUrl}/mcp` (Streamable HTTP, **stateless**). Reuse `Steps/McpToolResult.cs` (markdown → ```json``` payload) and `Steps/CallOutcome.cs` (result-or-exception).
- **The API image tag is shared** via `DEVSTACK_TEST_API_IMAGE`; CI prebuilds it once and Testcontainers reuses that immutable tag for the GraphQL, MCP, and flag-off environments. `WithEnvironment(name, value)` injects container env vars (used for feature flags). Local runs without the override rebuild their explicit fallback tags to avoid stale source images.
- `[UsePaging(MaxPageSize = 100)]` on all four list resolvers; introspection enabled. `MaxFieldCost = 10000` is configured but **never reached** (DRIFT-08).
- **Data hygiene is manual**: `deleteTestData` only removes rows whose `Project.Name` / `Deliverable.Title` / `AgentTask.Title` / `LargeLanguageModel.Url` contains the literal `[DeleteAfterTest]`. Consequences:
  - Every created entity must carry that marker.
  - The marker must be in the field the *query filter* uses, not only the cleanup anchor (root cause of the GQL-11 failures: the LLM marker lived in `url` while the filter matched `model`).
  - No assertion may depend on global row counts — always filter on a unique per-scenario suffix.
- **The GraphQL suite must not run in parallel** (`TestParallelization.cs` sets `DisableTestParallelization`) because `deleteTestData` in `AfterScenario` wipes *all* marked rows globally, so concurrent feature classes destroy each other's fixtures (INF-05).

### 1.2 Observed error contracts (empirically pinned — do not re-guess)

**GraphQL** (`DevStack.Api`, `ASPNETCORE_ENVIRONMENT=Production`):

| Case | HTTP | Body shape |
|---|---|---|
| Resolver/handler throws (`InvalidOperationException`, `ArgumentException`) | 200 | `data` null (or partial), `errors[0].message == "Unexpected Execution Error"`, **no `extensions` object at all** |
| Unknown id on a by-id query | 200 | `data.<field> == null`, no errors |
| Malformed UUID / unknown field / missing arg / bad enum literal | 400 | validation error with `extensions.code` |
| `first: 101` | 400 | paging error |
| Over-cost query | 400 | `HC0047` (governed by HotChocolate default `MaxTypeCost = 1000`, not the configured 10000) |

**MCP** (`ModelContextProtocol` SDK, API-hosted `/mcp` endpoint):

| Case | Client-visible outcome |
|---|---|
| Tool throws `McpProtocolException` | client **throws** `McpProtocolException`, message `Request failed (remote): <original>` — no result object |
| Tool throws anything else (incl. `InvalidOperationException` from handlers) **or** argument binding fails (missing arg, non-GUID, unknown enum literal) | result with `IsError == true`, text exactly `An error occurred invoking '<tool>'.` |
| `ToolResponse.Error(...)` | normal result, `IsError == null`, text `{"error": "..."}` |
| Unknown tool / prompt / resource URI | `Unknown tool: '<name>'` / `Unknown prompt: '<name>'` / `Unknown resource URI: '<uri>'` |

Payload keys are PascalCase (plain records); soft errors use lowercase `error`.

---

## 2. GraphQL coverage matrix (server)

Legend: ✅ covered · 🟡 partial · ❌ no coverage.

### 2.1 Queries

| Schema field | Used by UI | Happy | Error/exception | Notes |
|---|---|---|---|---|
| `Query.project(id)` | yes | ✅ | ✅ unknown id → `null`, no errors | |
| `Query.projects(...)` | yes | ✅ `first/after/last/before`, `pageInfo`, `edges.cursor`, `order`, `where` (eq/contains/startsWith/in/neq/and/or, nested `deliverables.some`) | ✅ `first:101`, bad cursor | was 0 scenarios |
| `Query.largeLanguageModel(id)` | **no** | ✅ | ✅ | dead UI surface, see DRIFT-05 |
| `Query.largeLanguageModels(...)` | yes | ✅ filters + order + `cost` round-trip | ✅ | |
| `Query.deliverable(id)` | yes | ✅ | ✅ | |
| `Query.deliverables(...)` | yes | ✅ status/type/title/projectId/`agentTasks.some`/`none`, order, paging | ✅ | |
| `Query.agentTask(id)` | yes | ✅ | ✅ | relation objects null → BUG-01 |
| `Query.agentTasks(...)` | yes | ✅ status/deliverableId/projectId/complexityRating/title/dependsOn filters, order, paging | ✅ | relation objects null → BUG-01 |

### 2.2 Mutations

| Schema field | Happy | Error/exception |
|---|---|---|
| `createProject` | ✅ | ✅ (blank name pinned to actual behaviour) |
| `updateProject` | ✅ | ✅ unknown id |
| `deleteProject` | ✅ | ✅ unknown id · ✅ cascade to deliverables + tasks (DRIFT-13) |
| `createDeliverable` | ✅ | ✅ unknown project, bad `type`, empty/oversized title · 🟡 numeric-string enum hole (DRIFT-10) |
| `updateDeliverable` | ✅ | ✅ unknown id, empty title, >200 title |
| `updateDeliverableStatus` | ✅ | ✅ unknown id, invalid enum literal |
| `deleteDeliverable` | ✅ | ✅ unknown id · ✅ cascade to its agent tasks (DRIFT-13) |
| `createAgentTask` | ✅ | ✅ unknown deliverable, unknown dependency, complexity 0/11, empty/oversized title |
| `updateAgentTask` | ✅ | ✅ unknown id, complexity out of range |
| `updateAgentTaskStatus` | ✅ | ✅ unknown id, invalid enum literal |
| `deleteAgentTask` | ✅ | ✅ unknown id |
| `createLargeLanguageModel` | ✅ | ✅ missing required args |
| `updateLargeLanguageModel` | ✅ | ✅ unknown id |
| `deleteLargeLanguageModel` | ✅ | ✅ unknown id |
| `deleteTestData` | ✅ contract asserted: `success`/`message`, reclaims exactly the `[DeleteAfterTest]` rows, unmarked rows survive | ✅ |
| `checkAndMarkDeliverableDone` | ✅ | ✅ unknown deliverable |

### 2.3 Types / fields

| Area | Status |
|---|---|
| Pagination (`pageInfo`, `edges.cursor`, cursors stable across queries, empty connection) | ✅ |
| Sorting incl. enum sort and multi-field sort | ✅ |
| Nested object filters (`project{}`, `deliverable{}`, `dependsOnAgentTask{}`) | ✅ (translate to real SQL joins) |
| Relation **object** fields (`Deliverable.project`, `AgentTask.deliverable/project/dependsOnAgentTask`) | 🟡 pinned to observed `null` — BUG-01 |
| `Project.deliverables`, `Deliverable.agentTasks` (collection navigations that ARE included) | ✅ |
| Deliverable document fields (`design`, `agentFeedback`, `executionPlan`, `securityImpact`, `performanceImpact`, `testPlan`, `deploymentPlan`, `blocking`) | ✅ round-tripped incl. multi-line, tabs, non-ASCII, markdown — but `agentFeedback`/`executionPlan` are stored crossed → **BUG-07** |
| `AgentTask.{promptTokens,completionTokens,executionDurationInSeconds}`, `errors`, `commitHash`, `agent` | ✅ |
| `UpdateLargeLanguageModelInput.{apiKey,cost,modelAlias}` | ✅ (apiKey echo → SEC-01) |
| `DeliverableType.SPIKE`, case-insensitive `type` parsing, filtering by `type` | ✅ all four enum members + lowercase/mixed case |

### 2.4 Error plumbing

| Component | Status |
|---|---|
| `GraphQLErrorFilter` / `ErrorClassifier` | 🟡 exercised by every error scenario; classification itself still has no unit test |
| `ErrorHandlingMiddleware` + `ExceptionMapper` (problem+json) | ❌ likely dead path — no controllers mapped (DRIFT-06) |
| Paging cap / cost analysis | ✅ |
| Introspection | ✅ positive control |

---

## 3. MCP coverage matrix

Both hosts register the same tools: `DevStack.Mcp/Program.cs:65-71` and `DevStack.Api/Program.cs:107-113` (`app.MapMcp("/mcp")` on both).

| Tool | Happy | Error/exception | Integration |
|---|---|---|---|
| `get_projects` | ✅ | n/a | ✅ |
| `get_project` | ✅ | ✅ `Guid.Empty`, unknown id | ✅ |
| `create_project` | ✅ (round-trip via `get_project`) | ✅ empty/whitespace name, empty repository | ✅ |
| `get_deliverable` | ✅ | ✅ soft `{"error":"Deliverable not found"}` | ✅ |
| `get_next_deliverable` | ✅ | ✅ empty projectId, unknown project, no deliverable in status | ✅ |
| `create_deliverable` | ✅ | ✅ `Guid.Empty` projectId | ✅ |
| `update_deliverable` | ✅ | ✅ unknown id | ✅ |
| `update_deliverable_status` | ✅ | ✅ unknown id, bad status literal | ✅ |
| `get_task` | ✅ | ✅ unknown id (see BUG-02) | ✅ |
| `get_next_task` | ✅ (incl. "all tasks complete") | ✅ neither arg, unknown project, no IMPLEMENT deliverable | ✅ |
| `create_task` | ✅ | ✅ unknown deliverableId | ✅ |
| `update_task` | ✅ | ✅ unknown id | ✅ |
| `update_task_status` | ✅ | ✅ unknown id, bad status literal | ✅ |

### 3.1 MCP protocol surface

| Surface | Status |
|---|---|
| `initialize`, `notifications/initialized` | ✅ |
| `tools/list` — exact 13-tool set + per-tool `inputSchema.required` | ✅ |
| `tools/call` hard errors / soft errors | ✅ |
| Unknown tool, missing required arg, wrong arg type, invalid enum literal | ✅ |
| `prompts/list` + `prompts/get deliverable_workflow` | ✅ |
| `resources/list` + `resources/read` | ✅ |
| MCP endpoint hosted by **DevStack.Api** (`/mcp` on the API image, incl. `/health` + `/ready` → 200) | ✅ `28e6f61`; the current MCP suite runs against this endpoint |
| `FeatureManagement:AgentTaskTools = false` hides task tools | ✅ 4 scenarios on an isolated second API host with its own Postgres |
| Per-scenario cleanup (SQL, tracked ids, seeded project preserved) | ✅ `CreatedRowRegistry` + `ScenarioDataCleaner`; verified `removed N of N` on every scenario that creates rows |
| `McpExceptionHandlingMiddleware` visibility of tool errors | ❌ BUG-05 — SDK swallows tool errors before the middleware |
| `McpArgumentNormalizationFilter` / `CoerciveStringJsonConverter` coercion | ⚠️ Cross-host behavior comparison intentionally dropped when the suite moved to API-only; BUG-09 remains documented |
| Standalone MCP host bootstraps its own schema | ❌ BUG-08 — known historical behavior, no longer exercised by the API-only MCP integration suite |

---

## 4. AdminUi-facing coverage

| ID | Item | Status |
|---|---|---|
| UI-01 | Playwright harness schema drift (`cleanupTestData` → `deleteTestData`, legacy `delete*(input:)`, `createFeature`/`createTask`/`Severity`/`DeleteEntity` → current inputs; create payloads return the entity, not a wrapper) | ✅ fixed, `4edd1b2` |
| UI-02 | Error surfaces: Apollo error link (GraphQL + network branches), `ErrorState` retry, `serverError` banner on rejected mutations | ✅ 21 tests, `4037de4` |
| UI-03 | Network-error branch | ✅ |
| UI-04 | `GetAllDeliverables.after` cursor declared but never passed | ❌ decision pending |
| UI-05 | `Query.largeLanguageModel` unused by UI | ❌ decision pending |
| UI-06 | Test files were outside the type gate — `tsconfig.app.json` excludes `src/**/*.test.tsx`, so 16 type errors lived there unnoticed (`EditProjectDialog.test.tsx` passed an undeclared `onError` prop React silently ignored; `CreateAgentTaskDialog.test.tsx` omitted the required `projectId`; a null-project test asserted on a container Radix never renders into) | ✅ `56f7699` — `tsconfig.test.json` + `npm run typecheck:test`, all 16 fixed in test files only |

---

## 5. Defects in the pre-existing tests (all fixed)

| ID | Defect | Status |
|---|---|---|
| GQL-01 | `HasErrors()` stub returned `false` in all four step classes → every "created/updated successfully" assertion was vacuous | ✅ `bfe3032` |
| GQL-16 | Step text claimed to send fields it never sent (`agentFeedback`, `result`, `errors`, `commitHash`, `model`) | ✅ `47b8676` — steps now send what they promise and re-query the values; note `CreateDeliverableInput` has no `agentFeedback` and `CreateAgentTaskInput` has no `result`/`errors`/`commitHash`/`agent`, so those are exercised via the update mutations |
| MCP-11 | "Call a tool with empty name" never triggered an error | ✅ rewritten |
| MCP-07 | "Tool schema is properly defined" never inspected `inputSchema` | ✅ |
| GQL-17 | Unused `Microsoft.AspNetCore.Mvc.Testing` reference, vestigial `GraphQL.Url` setting | ❌ open (cosmetic) |
| GQL-18 | Pre-existing scenarios never marked data `[DeleteAfterTest]`, so `deleteTestData` reclaimed nothing and the DB accumulated rows for the whole run; AdminUi e2e fixtures used `DeleteAfterTest - ` (no brackets) with the same effect | ✅ `47b8676` (server suite, incl. renames) + `4edd1b2` (AdminUi) |

---

## 6. Defects and drift found *by* building the coverage

Every finding in this section is also annotated **where it is produced** in the
source, in the exhaustive-bug-hunt finding format (Category, Trigger path, Why
this is a real bug, Why this is NOT a false positive, Concrete consequence,
Confidence, Fix direction) and each block names this register's ID plus the test
that pins it. Search the repository for the ID to find the block. (`1b67c92`,
`177948b`, `6012a55`)

Product defects (fix in product code; tests currently pin the broken behaviour and will fail loudly when fixed):

| ID | Defect | Evidence |
|---|---|---|
| **BUG-01** | Relation **object** fields resolve to `null`: `AgentTask.deliverable`, `AgentTask.project`, `AgentTask.dependsOnAgentTask`, `Deliverable.project`. `Query.cs` loads only the root entity (no `Include` for these navigations) and the connection resolvers have `[UsePaging][UseFiltering][UseSorting]` without `[UseProjection]`; lazy-loading proxies are not enabled. **AdminUi impact:** `src/AdminUi/src/graphql/queries/agentTask.graphql:18-26,48-55` selects exactly these fields, so the task detail page and task list render empty deliverable/project columns. | observed payloads in `359bc7c`; fix = `[UseProjection]` on the four connections + `Include`/field resolvers on by-id queries |
| **BUG-02** | `get_task` error contract is inconsistent with its sibling: `TaskTools.GetTask`'s `McpProtocolException("AgentTask with ID … not found")` is unreachable because `GetAgentTaskByIdHandler` throws `InvalidOperationException` first → clients get the generic `An error occurred invoking 'get_task'.`, while `get_deliverable` returns a clean `{"error":"Deliverable not found"}` | `TaskTools.cs:40-42` vs `DeliverableTools.cs:39-41`, `AgentTaskHandlers.cs:131-139` |
| **BUG-03** | `create_deliverable` reports `"Status":"Ready"` although the entity is created as `DeliverableStatus.Draft` — and `Ready` is not even a member of `DeliverableStatus` | `DeliverableTools.cs:125` |
| **BUG-04** | The `deliverable_workflow` prompt instructs clients to call tools that do not exist (`change_deliverable_status`, `change_task_status`) and to create tasks with an `IMPLEMENT` status that `create_task` cannot accept | `DevStack.Mcp/Descriptions.cs` (prompt content) |
| **BUG-05** | `McpExceptionHandlingMiddleware` never sees tool failures — the SDK converts them inside the request handler, so tool errors are missing from the middleware's error logging / correlation | `Program.cs:124`, observed outcomes in §1.2 |
| **BUG-06** | `get_next_task` "all tasks completed" payload puts the **deliverable** id under the key `Id`, identical in shape to a task payload — easy for a client to misread | `TaskTools.cs:100-101` |
| **BUG-07** | **`updateDeliverable` writes `agentFeedback` and `executionPlan` into each other's columns.** `Mutation.UpdateDeliverableAsync` builds `UpdateDeliverableCommand` with **positional** arguments, but `UpdateDeliverableInput` declares `AgentFeedback` at position 5 and `ExecutionPlan` at 6 while `UpdateDeliverableCommand` declares `ExecutionPlan` at 5 and `AgentFeedback` at 6. Both are `string?`, so the compiler cannot catch it: every caller that sets both silently swaps the data. This is data corruption, not a display bug. One-line fix: pass named arguments. **Audited:** all other positional command constructions in `Mutation.cs` (`CreateDeliverable`, `CreateAgentTask`, `UpdateAgentTask`, `Create/UpdateLargeLanguageModel`, `Create/UpdateProject`) match their record order — this is the only crossed pair, but named arguments should be used everywhere as defence. Pinned by the `GQL-13 DEFECT` scenario in `47b8676`, which turns red the moment the API is fixed | `src/Server/DevStack.Api/GraphQL/Mutation.cs:180-192` vs `UpdateDeliverableCommand.cs:3-15`, `47b8676` |
| **BUG-08** | The standalone **MCP host never runs EF migrations** while the API host does (`MigrateAsync` at `DevStack.Api/Program.cs:125-129`, no equivalent in `DevStack.Mcp/Program.cs`). Pointed at an empty Postgres it starts *healthy* — `/health` only calls `CanConnectAsync`, which succeeds on an unmigrated DB — and then answers **every** DB-backed tool with the generic `An error occurred invoking '<tool>'.`, indistinguishable from a tool bug. Deployment trap | observed while building the flag-off environment (`28e6f61`), which has to migrate explicitly |
| **BUG-09** | The two MCP hosts have **divergent request-filter pipelines**: `DevStack.Api/Program.cs:115-121` registers the same tools/prompts/resources as `DevStack.Mcp/Program.cs:73-81` but omits `McpArgumentNormalizationFilter`, although `DevStack.Api` already references the project that contains it. The same client therefore gets materially different behaviour per host: an array where a `string` is expected is coerced to `"alpha\nbeta"` and **accepted** by the MCP host but produces `IsError == true` on the API host. Looks accidental; either add the filter or document the split | pinned by `ApiHostedMcp.feature` in `28e6f61` |
| **SEC-01** | `Query.largeLanguageModel(id)` / `largeLanguageModels` return `apiKey` **verbatim**, so any GraphQL client can read model credentials in clear text. The field-coverage scenario pins the echo rather than asserting a masked value | `47b8676` |

Spec drift (decision needed — tests pin current behaviour, do not write failing tests for these):

| ID | Finding |
|---|---|
| DRIFT-01 | The status machine `StatusTransitionService.CanTransition` is **never called in the write path**; `Deliverable.TransitionStatus` / `AgentTask.TransitionStatus` assign unconditionally, so every illegal transition (e.g. `DONE → DRAFT`) succeeds. Unit tests cover the rules; integration tests pin the permissive behaviour. |
| DRIFT-02 | `NotFoundException` / `ValidationException` / `ConcurrencyException` are never thrown by handlers (they throw `InvalidOperationException` / `ArgumentException`), so `ErrorClassifier`/`ExceptionMapper` only ever take the generic branch. |
| DRIFT-07 | Consequence of DRIFT-02 confirmed at runtime: masked execution errors carry **no `extensions.code`** and the message `"Unexpected Execution Error"`, so the AdminUi's `mapMutationError` keys (`NOT_FOUND`, `CONCURRENCY_CONFLICT`) can never match. Typed errors would need `GraphQLException`-derived errors or an error filter that sets `Code`. |
| DRIFT-08 | `ModifyCostOptions(o => o.MaxFieldCost = 10000)` is unreachable — HotChocolate's default `MaxTypeCost = 1000` rejects first (`HC0047`). The configured value is misleading. |
| DRIFT-09 | Title-length rules disagree: `createAgentTask` rejects >200 chars while `AgentTask.UpdateMetadata` allows 300 — a 201-char title fails on create and succeeds on update. |
| DRIFT-10 | `createDeliverable` `type` is a free `String` parsed with `Enum.Parse(ignoreCase)`: a numeric string such as `"999"` parses successfully and only fails later at serialization (`EXEC_INVALID_LEAF_VALUE`). |
| DRIFT-03 | `Mutation.updateAgentTaskStatus` returns the *requested* status, not the persisted one; `updateDeliverableStatus` falls back to `targetStatus` when the row is missing. |
| DRIFT-04 | MCP `update_*_status` require `actor`; the GraphQL equivalents make it optional and the UI never sends it. |
| DRIFT-05 | `Query.largeLanguageModel(id)` is unused by any consumer. |
| DRIFT-06 | `ErrorHandlingMiddleware`/`ExceptionMapper` produce `problem+json`, but no REST controllers are mapped — likely dead error path. |
| DRIFT-11 | Task dependencies are only checked for **existence**, not for scope: a `dependsOnAgentTaskId` pointing at a task in a **different deliverable or a different project** is accepted. No orphan rows are produced, but the dependency graph is meaningless across boundaries. `deleteAgentTask` on a depended-on task leaves the dependent row behind with `dependsOnAgentTaskId` cleared (`SetNull`), which is safe but silent. |
| DRIFT-12 | `createAgentTask` **ignores** `input.projectId` and derives the project from the deliverable, so a contradicting — or entirely non-existent — `projectId` is accepted while the persisted row stays consistent. The input field is misleading and should be removed from the schema or validated. |
| DRIFT-13 | Delete behaviour is cascade-based: `deleteProject` removes its deliverables and their tasks, `deleteDeliverable` removes its tasks, all returning `true` with no confirmation surface. A single mistaken `deleteProject` from a UI client destroys an entire subtree. |

Harness defects:

| ID | Finding | Status |
|---|---|---|
| INF-05 | `deleteTestData` in `AfterScenario` is global, so parallel feature classes wipe each other's fixtures. Mitigated by `TestParallelization.cs` (`DisableTestParallelization`), which costs wall-clock time. Proper fix: scoped cleanup (delete only the ids a scenario created), as Lane F implemented for MCP. | 🟡 mitigated |
| INF-06 | `src/Server/.editorconfig` requires `end_of_line = lf` while `core.autocrlf=true` makes the working tree CRLF, so `dotnet format --verify-no-changes` flags every `.cs` file. New files are committed as LF. Repo-wide decision needed; do **not** mass-reformat. | ❌ open |
| INF-07 | **Found by Lane F:** INF-01 was incomplete — the MCP `BeforeTestRun` hook hard-coded `WithMcpImageName("devstack-mcp:test")`, so the MCP image override had **no effect** on the main MCP environment. Historically fixed by removing the hard-coded tag; later superseded when the MCP suite moved to the API image only. | ✅ superseded by API-only harness |
| INF-08 | The assembly-seeded MCP project is named `[DeleteAfterTest] MCP Test Project` but is deliberately **never** deleted because every scenario reuses it. Any future `[DeleteAfterTest]` reaper, or exposing GraphQL on the MCP host, would delete the row the whole suite depends on. Rename it outside the predicate. | ❌ open |

---

## 7. Gap register (current status)

| ID | Gap | P | Status |
|---|---|---|---|
| INF-01 | Parallel-safe image tags for the test application | P1 | ✅ `DEVSTACK_TEST_API_IMAGE` is shared across server integration environments |
| INF-02 | `WithEnvironment()` for container env vars | P2 | ✅ `b73c058` |
| INF-03 | Shared `GraphQLRequest` / `GraphQLResult` helpers | P1 | ✅ `b73c058` |
| GQL-01 | Real `HasErrors` assertions | P0 | ✅ `bfe3032` |
| GQL-02 | Not-found paths, 16 mutations + 4 by-id queries | P0 | ✅ `0b3e907` |
| GQL-03 | Protocol/validation errors (malformed UUID, unknown field, missing arg, bad enum, paging cap, cost, introspection) | P0 | ✅ `0b3e907` |
| GQL-04 | Domain validation (titles, complexity, blank name) | P1 | ✅ `0b3e907` |
| GQL-07 | Error-contract hygiene (no stack traces, stable codes) | P1 | ✅ `0b3e907` |
| GQL-08..GQL-12 | Query breadth: paging, filters, sorting, pageInfo/edges, LLM scalars, relations | P0/P1 | ✅ `61fa58a`, `359bc7c` |
| GQL-05 | Referential integrity: delete project with deliverables, delete deliverable with tasks, cross-deliverable dependency | P1 | ✅ `47b8676` (8 scenarios → DRIFT-11/12/13) |
| GQL-06 | `deleteTestData` contract (`success`/`message`, only marked rows removed) | P1 | ✅ `47b8676` |
| GQL-13 | Round-trip untested input/scalar fields (deliverable docs, LLM `apiKey`/`cost`, task token counters) | P1 | ✅ `47b8676` (19 instances) — surfaced **BUG-07** |
| GQL-14 | Positive `DeliverableType.SPIKE` + case-insensitive `type` | P2 | ✅ `47b8676` (9 instances) |
| GQL-15 | Auto-completion edge cases (FAILED/REJECTED tasks, already-DONE deliverable) | P2 | ❌ open |
| GQL-16 | Fix step text that does not send the fields it claims | P1 | ✅ `47b8676` |
| GQL-17 | Remove unused `Mvc.Testing` reference / vestigial setting | P3 | ❌ open |
| GQL-18 | Mark server-suite fixtures `[DeleteAfterTest]` so cleanup actually reclaims them | P1 | ✅ `47b8676` |
| MCP-01..MCP-07, MCP-11 | All 13 tools, hard/soft errors, protocol errors, prompts, resources, `inputSchema`, honest tools/call error scenario | P0/P1 | ✅ `75b9a34` |
| MCP-08 | Feature flag off hides task tools | P2 | ✅ `28e6f61` (4 scenarios) |
| MCP-09 | MCP endpoint hosted by `DevStack.Api` | P1 | ✅ `28e6f61`; current MCP suite now uses this endpoint exclusively |
| MCP-10 | Per-scenario cleanup for MCP | P2 | ✅ `28e6f61` |
| MCP-12 | Assert notification round-trip rather than "no exception" | P3 | ❌ open |
| MCP-13 | Integration coverage for `McpArgumentNormalizationFilter` / `CoerciveStringJsonConverter` array→newline-joined coercion | P2 | ⚠️ standalone-versus-API behavior comparison intentionally dropped with API-only integration host |
| UI-01..UI-03 | e2e schema drift, GraphQL error surfaces, network-error branch | P0/P1 | ✅ `4edd1b2`, `4037de4` |
| UI-06 | Add `tsconfig.test.json` so test files are inside the type gate | P1 | ✅ `56f7699` (16 errors fixed) |
| INF-04 | CI: integration suites are `workflow_dispatch`/path-filtered only → new coverage does not gate merges | P2 | ✅ single-job server workflow runs the full solution test gate |
| INF-06 | `.editorconfig` LF vs `core.autocrlf` conflict | P3 | ❌ open (decision) |
| INF-07 | MCP-specific image override was dead for the assembly-level MCP env | P1 | ✅ superseded; no separate MCP image is built or selected |

**Out of scope** (tracked as decisions, §6): enforcing the status machine (DRIFT-01), emitting typed domain errors (DRIFT-02/07), removing the dead REST problem+json path (DRIFT-06), fixing BUG-01..BUG-09 / SEC-01 in product code.

**Coverage verdict:** every query, mutation, tool, prompt and resource reachable over GraphQL and MCP now has at least one happy-path scenario, and every error/exception path has one except the two dead paths noted in §2.4/§3.1 (`ErrorHandlingMiddleware` problem+json, `McpExceptionHandlingMiddleware` tool-error visibility — both blocked on product decisions). The remaining open items are P2/P3 polish or decisions, not coverage holes.

---

## 8. Remaining work (next round)

**Small, self-contained leftovers**
1. GQL-15 auto-completion edge cases (`FAILED`/`REJECTED` tasks, already-`DONE` deliverable) — P2
2. MCP-12 assert the notification round-trip instead of "no exception" — P3
3. GQL-17 drop the unused `Mvc.Testing` reference and vestigial `GraphQL.Url` setting — P3
4. INF-08 rename the assembly-seeded MCP project so it is not inside the `[DeleteAfterTest]` predicate — P2
5. INF-05 give the GraphQL suite scoped cleanup (mirror `CreatedRowRegistry`/`ScenarioDataCleaner`) so `DisableTestParallelization` can be removed — P2, the main wall-clock lever

**Deferred to product owners:** BUG-01..BUG-09, SEC-01, DRIFT-01/02/05/06/08/09/10/11/12/13.
**Highest-value product fix:** BUG-07 (silent field swap on `updateDeliverable`) — one line, active data corruption.

**Validation protocol** (sequential per suite; Docker builds are shared):
`dotnet build src/Server/DevStack.slnx -c Release` → `dotnet test DevStack.Tests.Unit` → `dotnet test DevStack.Tests.Integration.GraphQL` → `dotnet test DevStack.Tests.Integration.MCP` → AdminUi `npm run test` + `npx tsc --noEmit` + `npx eslint .` + `npm run typecheck:test`.

---

## 9. Conventions for new tests

1. One `.feature` per concern; new step definitions in a **new** binding class with **globally unique** step text (SpecFlow raises ambiguous-match errors across binding classes).
2. Use `Scenario Outline` + `Examples` for input matrices.
3. Mark every created entity `[DeleteAfterTest]` **in the field the query filters on**, plus a unique per-scenario suffix; never assert on global counts.
4. Error tests assert: HTTP status, `errors` present, `data` null/partial, and the pinned message/code from §1.2. Pin from observed responses, never from assumption.
5. Do not assert behaviour that does not exist (§6). If reality contradicts intent, assert the actual behaviour and file the drift.
6. Use the shared helpers; do not re-implement JSON/markdown plumbing.
7. Keep `src/schema.graphql` untouched — these projects consume it.
8. Apollo Client 4 does **not** export `ApolloError`; construct `new CombinedGraphQLErrors({ data, errors: [new GraphQLError(...)] })` in UI tests.
9. Quality gates: `src/Server/AGENTS.md` (build, unit tests, `dotnet format`) and `src/AdminUi/AGENTS.md` (build, codegen, tests, eslint, tsc). No new warnings.

---

## 10. Execution log

| Action | Result |
|---|---|
| Baseline `dotnet build` | 0 errors, 22 warnings |
| Ledger created; lanes defined | — |
| Lane I: INF-01/02/03 | `b73c058` |
| Lane A: GQL-08..GQL-12 (27 scenarios) | `61fa58a` |
| Lane B: GQL-01 + GQL-02/03/04/07 (63 cases) | `bfe3032`, `0b3e907` |
| Lane C: MCP-01..07, MCP-11 (42 new scenarios) | `75b9a34` — MCP suite **55 passed / 0 failed** |
| Lane A fix: GQL-11 fixture + BUG-01 pinned | `359bc7c` — GraphQL suite **169 passed / 0 failed** |
| Lane D: UI-01 | `4edd1b2` — `tsc` for e2e project went 3 errors → 0 |
| Lane D: UI-02/UI-03 (21 tests) | `4037de4` — AdminUi **217 tests passed**, eslint + tsc clean |
| Lane G: UI-06 + INF-04 | `56f7699` — 16 test-file type errors fixed, AdminUi **222 tests passed**, `typecheck:test` added and clean; CI gating proposal written |
| Lane E: GQL-05/06/13/14 + GQL-16/18 | `47b8676` — GraphQL suite **206 passed / 0 failed**; surfaced BUG-07 (field swap) and SEC-01 |
| Lane F: MCP-08/09/10 | `28e6f61` — MCP suite **65 passed / 0 failed** (3 Docker environments); surfaced BUG-08, BUG-09 and INF-07 |
| INF-07 fix: dead `DEVSTACK_TEST_MCP_IMAGE` override | `Hooks/SpecFlowHooks.cs` hard-coded tag removed; MCP suite re-verified green with the override in effect |
| Final gates | build **0 errors / 22 warnings** (= baseline) · unit **327/327** · GraphQL **206/206** · MCP **65/65** · AdminUi **222 tests**, `tsc`, `tsc -p tsconfig.app.json`, `typecheck:test`, `eslint .` all clean |
