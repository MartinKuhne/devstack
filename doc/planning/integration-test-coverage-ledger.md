# Integration Test Coverage Ledger — GraphQL & MCP

Status: **active** · Created 2026-10-31 · Owner: code-quality supervision
Goal: every happy, error and exception path reachable over GraphQL (as used by the AdminUi and in general) and over MCP has integration test coverage.

---

## 1. Scope and method

| Item | Value |
|---|---|
| Server surface under test | `src/schema.graphql` (produced by `src/Server/DevStack.Api` + `DevStack.Domain`), MCP tools/prompts/resources in `src/Server/DevStack.Mcp` |
| Consumers | AdminUi (Apollo Client 4, HTTP GraphQL only, no subscriptions), `DevStack.Agent` (raw GraphQL), MCP clients |
| Existing integration harness | `DevStack.Tests.Integration.GraphQL`, `DevStack.Tests.Integration.MCP`, `DevStack.Tests.Integration.Shared` (Testcontainers: `postgres:17-alpine` + the real app Docker image; raw HTTP for GraphQL, `ModelContextProtocol` client over Streamable HTTP for MCP) |
| Method | Static inventory of schema/resolvers/tools/handlers vs. every `.feature` scenario and step definition, plus AdminUi GraphQL documents and error-handling code |
| Baseline quality gates | `dotnet build src/Server/DevStack.slnx -c Release` → **0 errors**, 22 pre-existing warnings (NU1903 SSH.NET, CS0618 Testcontainers, ASP0000, CS8603, CS8625). No new warnings may be introduced. |

### 1.1 Harness facts that constrain every new test

- GraphQL tests POST raw JSON to `{AppUrl}/graphql`. No auth exists on either host.
- MCP tests connect to `{AppUrl}/mcp` (Streamable HTTP, **stateless**), seeded with one `[DeleteAfterTest] MCP Test Project` row in `BeforeTestRun`; **no per-scenario cleanup**.
- The GraphQL suite cleans up per scenario with `mutation { deleteTestData { ... } }`; the MCP host (`DevStack.Mcp`) does **not** expose GraphQL, so it has no equivalent.
- `DevStackTestEnvBuilder` always rebuilds the app image (`WithImageBuildPolicy(_ => true)`) under a **fixed tag** (`devstack-api:test` / `devstack-mcp:test`) → concurrent runs collide (see INF-01).
- `[UsePaging(MaxPageSize = 100)]` on all four list resolvers; `MaxFieldCost = 10000`; introspection enabled (`DevStack.Api/Program.cs:90-97`).
- **Error shape is not yet pinned.** Handlers throw `InvalidOperationException` / `ArgumentException`, not the domain exceptions (§8). HotChocolate masks exception details by default. New error tests must assert on the *observed* response shape (pin `errors[].message` / `extensions.code` from the first run), never on an invented contract.

---

## 2. GraphQL coverage matrix (server)

Legend: ✅ covered · 🟡 partial · ❌ no coverage. "Scenarios" = existing `.feature` scenarios in `DevStack.Tests.Integration.GraphQL`.

### 2.1 Queries

| Schema field | Used by UI | Happy | Error/exception | Existing scenarios | Gap |
|---|---|---|---|---|---|
| `Query.project(id)` | yes | ✅ | ❌ unknown id → `null` (not an error) untested | 1 (`ProjectMutations.feature:47`) | GQL-02 |
| `Query.projects(first/after/last/before, where, order)` | yes (`first:100`) | ❌ never queried | ❌ `first > 100`, bad cursor, bad filter | 0 | GQL-08 |
| `Query.largeLanguageModel(id)` | **no** | ✅ | ❌ | 1 (`LargeLanguageModelMutations.feature:65`) | GQL-02, DRIFT-05 |
| `Query.largeLanguageModels(...)` | yes | 🟡 unfiltered only | ❌ | 1 (`:71`) | GQL-11 |
| `Query.deliverable(id)` | yes | ✅ | ❌ | 1 (`DeliverableMutations.feature:96`) | GQL-02 |
| `Query.deliverables(...)` | yes (3 ops) | 🟡 `projectId eq` only | ❌ | 1 (`:102`) | GQL-09 |
| `Query.agentTask(id)` | yes | ✅ | ❌ | 1 (`TaskMutations.feature:126`) | GQL-02 |
| `Query.agentTasks(...)` | yes (`deliverableId eq`) | 🟡 unfiltered + client-side filter | ❌ | 1 (`:132`) | GQL-10 |

### 2.2 Mutations

| Schema field | Used by UI | Happy | Error/exception | Gap |
|---|---|---|---|---|
| `createProject` | yes | ✅ 2 | ❌ empty/oversized name, duplicate repository | GQL-04 |
| `updateProject` | yes | ✅ 3 | ❌ unknown id, empty name | GQL-02, GQL-04 |
| `deleteProject` | yes | ✅ 1 | ❌ unknown id, project with children (FK) | GQL-02, GQL-05 |
| `createDeliverable` | yes | ✅ 5 | ❌ unknown projectId, bad `type` string, empty/oversized title | GQL-02, GQL-03, GQL-04 |
| `updateDeliverable` | yes | ✅ 3 | ❌ unknown id, empty title, >200 title | GQL-02, GQL-04 |
| `updateDeliverableStatus` | yes | ✅ 5 | ❌ unknown id, invalid enum literal | GQL-02, GQL-03 |
| `deleteDeliverable` | yes | ✅ 1 | ❌ unknown id, deliverable with tasks | GQL-02, GQL-05 |
| `createAgentTask` | yes | ✅ 5 | ❌ unknown deliverableId, unknown `dependsOnAgentTaskId`, complexity 0/11, empty title | GQL-02, GQL-04 |
| `updateAgentTask` | yes | ✅ 5 | ❌ unknown id, complexity out of range, >300 title | GQL-02, GQL-04 |
| `updateAgentTaskStatus` | yes | ✅ 8 | ❌ unknown id, invalid enum literal | GQL-02, GQL-03 |
| `deleteAgentTask` | yes | ✅ 1 | ❌ unknown id | GQL-02 |
| `createLargeLanguageModel` | yes | ✅ 3 | ❌ missing required url/model | GQL-03 |
| `updateLargeLanguageModel` | yes | ✅ 5 | ❌ unknown id | GQL-02 |
| `deleteLargeLanguageModel` | yes | ✅ 1 | ❌ unknown id | GQL-02 |
| `deleteTestData` | no (harness only) | 🟡 invoked in `AfterScenario`, result never asserted, errors swallowed | ❌ | 0 | GQL-06 |
| `checkAndMarkDeliverableDone` | no | ✅ 3 | ❌ unknown deliverableId | GQL-02 |

### 2.3 Types / fields never round-tripped

`Project.deliverables` · `Deliverable.project` · `Deliverable.agentTasks` · `AgentTask.deliverable` · `AgentTask.project` · `AgentTask.dependsOnAgentTask` · `PageInfo.*` · `*.edges { cursor }` · `LargeLanguageModel.cost` · `Deliverable.{design,agentFeedback,executionPlan,securityImpact,performanceImpact,testPlan,deploymentPlan,blocking}` · `AgentTask.{promptTokens,completionTokens,executionDurationInSeconds}` → **GQL-12, GQL-13**

Never exercised input values: `CreateDeliverableInput.design`, `UpdateDeliverableInput.*` (all 8 optional fields), `UpdateLargeLanguageModelInput.{apiKey,cost}`, `CreateAgentTaskInput` default `complexityRating`, `DeliverableType.SPIKE`, lowercase `type` strings (`Enum.Parse(ignoreCase)` path) → **GQL-13, GQL-14**

### 2.4 GraphQL error plumbing

| Component | Coverage | Gap |
|---|---|---|
| `GraphQLErrorFilter` / `ErrorClassifier` (`DevStack.Api/GraphQL/GraphQLErrorFilter.cs`) | none (no unit, no integration) | GQL-07 |
| `ErrorHandlingMiddleware` + `ExceptionMapper` (REST/problem+json path) | none | GQL-07 (decision: REST path may be dead — no controllers are mapped) |
| Cost analysis (`MaxFieldCost = 10000`) / paging cap (`MaxPageSize = 100`) | none | GQL-03 |
| Introspection query | none | GQL-03 |

---

## 3. MCP coverage matrix

Both hosts register the same tool classes: `DevStack.Mcp/Program.cs:65-71` and `DevStack.Api/Program.cs:107-113` (`app.MapMcp("/mcp")` on both). The API host omits `McpArgumentNormalizationFilter`.

| Tool | Happy | Error/exception | Unit tests | Integration | Gap |
|---|---|---|---|---|---|
| `get_projects` | ✅ | n/a | ✅ | ✅ `ToolsCall.feature:5` | — |
| `get_project` | ❌ | ❌ `Guid.Empty`, unknown id | ✅ | ❌ never called | MCP-01, MCP-02 |
| `create_project` | ❌ | ❌ empty/whitespace name, empty repository | ✅ | ❌ never called | MCP-01, MCP-02 |
| `get_deliverable` | ❌ | 🟡 soft `ToolResponse.Error` on unknown id | ✅ | ❌ never called | MCP-01, MCP-03 |
| `get_next_deliverable` | ❌ | ❌ `Guid.Empty`, unknown project, no deliverable in status | **none** | ❌ never called | MCP-01, MCP-03 |
| `create_deliverable` | ✅ | ❌ `Guid.Empty` projectId | ✅ | ✅ `FeatureOperations.feature:5` | MCP-02 |
| `update_deliverable` | ✅ | ❌ unknown id | ✅ | ✅ `:13` | MCP-02 |
| `update_deliverable_status` | ✅ | ❌ unknown id, bad status literal | ✅ | ✅ `:19` | MCP-02 |
| `get_task` | ❌ | ❌ unknown id (`McpProtocolException`) | ✅ | ❌ never called | MCP-01, MCP-02 |
| `get_next_task` | ❌ | ❌ neither `repositoryUrl` nor `projectId`, unknown project, no deliverable in IMPLEMENT, all tasks complete | **none** | ❌ never called | MCP-01, MCP-03 |
| `create_task` | ✅ | ❌ unknown deliverableId | ✅ | ✅ `TaskOperations.feature:5` | MCP-02 |
| `update_task` | ✅ | ❌ unknown id | ✅ | ✅ `:13` | MCP-02 |
| `update_task_status` | ✅ | ❌ unknown id, bad status literal | ✅ | ✅ `:19` | MCP-02 |

### 3.1 MCP protocol surface

| Surface | Existing coverage | Gap |
|---|---|---|
| `initialize` handshake | ✅ `Initialize.feature` | — |
| `notifications/initialized` | ✅ `Notifications.feature` (no-exception only) | MCP-12 |
| `tools/list` | ✅ 2 scenarios, but the "schema is properly defined" step re-checks name/description and never inspects `inputSchema` (`ToolsListSteps.cs:63-73`) | MCP-07 |
| `tools/call` error contract | 🟡 the "empty name" scenario actually calls `get_projects` with no args and asserts only non-null (`ToolsCallSteps.cs:39-48`, `:64-68`) | MCP-04, MCP-11 |
| `prompts/list` / `prompts/get` (`deliverable_workflow`) | ❌ none | MCP-05 |
| `resources/list` / `resources/read` (`ServerInfo`) | ❌ none (unit only: `ResourcesTests.cs`) | MCP-06 |
| Unknown tool / missing required arg / wrong arg type / invalid enum literal | ❌ none | MCP-04 |
| `McpExceptionHandlingMiddleware` (400 for `McpProtocolException`, 500 otherwise) | ❌ none | MCP-02 |
| `McpArgumentNormalizationFilter` (MCP host only) | unit only | MCP-09 |
| Feature flag `FeatureManagement:AgentTaskTools = false` → task tools hidden | ❌ none | MCP-08, INF-02 |
| MCP endpoint hosted by **DevStack.Api** | ❌ none | MCP-09 |
| `/health` (MCP host) | ✅ `HealthCheckTests.cs` | — |
| Per-scenario data cleanup | ❌ none — MCP scenarios leak rows | MCP-10 |

---

## 4. AdminUi-facing gaps

| ID | Gap | Evidence |
|---|---|---|
| UI-01 | e2e harness posts `mutation { cleanupTestData { ... } }` — **field does not exist** in `src/schema.graphql` (schema has `deleteTestData`); `TestDataRegistry.ts` / `TestDataManager.ts` use legacy shapes (`deleteProject(input: DeleteProjectInput!) { project errors }`, `CreateFeature`, `Severity`) that no longer exist → setup/teardown silently fails | `src/AdminUi/tests/e2e/testSetup.ts:9`, `testTeardown.ts:9`, `helpers/TestDataRegistry.ts:60-69`, `fixtures/TestDataManager.ts:16,45,74,105,134` |
| UI-02 | No UI test ever rejects a mutation or asserts on a GraphQL `errors` array; error-path coverage exists only for pure helpers (`formatGraphQLError`, `mapMutationError`, `logApolloError`). `ErrorState` retry, `serverError` banners and the Apollo `onError` toast chain are untested against a real error shape. `msw` is a dependency but unused | `src/AdminUi/src/hooks/useApolloClient.ts:14-31`, `src/components/layout/ErrorState.tsx`, `package.json:96` |
| UI-03 | No test for the network-error branch (`toast.error('Network Error: …')`) | `useApolloClient.ts:27-28` |
| UI-04 | `GetAllDeliverables` declares `after` but never passes it (client-side paging over `first: 100`) → server cursor pagination is dead surface for the UI | `useAllDeliverables.ts:29`, `DeliverableListPage.tsx:79` |
| UI-05 | `Query.largeLanguageModel(id)` singular is unused by the UI | schema `:382` |

---

## 5. Defects in the existing tests (must be fixed before coverage can be trusted)

| ID | Defect |
|---|---|
| GQL-01 | `HasErrors(response, mutationName)` is a stub that **always returns `false`** in all four step classes (`ProjectSteps.cs:34`, `DeliverableSteps.cs:35`, `AgentTaskSteps.cs:55`, `LargeLanguageModelSteps.cs:39`). Every "should be created/updated successfully" assertion therefore only proves `data != null`. |
| GQL-16 | Step text lies about what is sent: the "all optional fields" steps for deliverables and tasks do not send `agentFeedback` / `result` / `errors` / `commitHash` / `model` (`DeliverableSteps.cs:221-255`, `AgentTaskSteps.cs:239-282`). |
| MCP-11 | `ToolsCall.feature:12` "Call a tool with empty name" never triggers an error (§3.1). |
| MCP-07 | `ToolsList.feature:19` "Tool schema is properly defined" never inspects `inputSchema`. |
| GQL-17 | `DevStack.Tests.Integration.GraphQL` references `Microsoft.AspNetCore.Mvc.Testing` but never uses it; `appsettings.json` `GraphQL.Url` is vestigial. |

---

## 6. Behaviour / spec drift — decision needed (do **not** write failing tests for these)

| ID | Finding |
|---|---|
| DRIFT-01 | The status machine `StatusTransitionService.CanTransition` is **never called in the write path**. `Deliverable.TransitionStatus` (`Deliverable.cs:120`) and `AgentTask.TransitionStatus` (`AgentTask.cs:125`) assign unconditionally, so every illegal transition (e.g. `DONE → DRAFT`, `DRAFT → DONE`) currently **succeeds**. Unit tests cover the rules; integration tests must pin the *actual* behaviour until the product decides to enforce them. |
| DRIFT-02 | `NotFoundException` / `ValidationException` / `ConcurrencyException` are never thrown by any handler — handlers throw `InvalidOperationException` / `ArgumentException`. Consequently `ErrorClassifier` and `ExceptionMapper` can only ever produce the generic branch, and the AdminUi's `mapMutationError` keys (`NOT_FOUND`, `CONCURRENCY_CONFLICT`) can never match a GraphQL error. |
| DRIFT-03 | `Mutation.updateAgentTaskStatus` returns the *requested* `targetStatus`, not the persisted status (`Mutation.cs:276-299`); `updateDeliverableStatus` falls back to `targetStatus` when the row is missing (`Mutation.cs:212`). |
| DRIFT-04 | MCP `update_task_status` / `update_deliverable_status` require `actor`; the GraphQL equivalents make it optional and the UI never sends it. |
| DRIFT-05 | `Query.largeLanguageModel(id)` is unused by any consumer. |
| DRIFT-06 | `ErrorHandlingMiddleware`/`ExceptionMapper` map to `problem+json`, but no REST controllers are mapped — likely dead error path. |

---

## 7. Gap register

Priority: P0 = must close now · P1 = should close now · P2 = next round · P3 = backlog. Complexity = 1 (trivial) … 10 (large).

| ID | Gap | P | Cx | Depends on | Files touched |
|---|---|---|---|---|---|
| INF-01 | Unique image tag per run (env override `DEVSTACK_TEST_API_IMAGE` / `DEVSTACK_TEST_MCP_IMAGE`) so runs can go in parallel | P1 | 2 | — | `Tests.Integration.Shared/DevStackTestEnvBuilder.cs` |
| INF-02 | Allow injecting extra container env vars (`WithEnvironment(name,value)`) — needed for MCP-08 | P2 | 1 | — | `DevStackTestEnvBuilder.cs`, `DevStackTestEnv.cs` |
| INF-03 | Shared GraphQL request helper (`PostGraphQLAsync`, `HasErrors`, `GetData`, `GetErrors`) in `Tests.Integration.Shared`; replace the four duplicated stubs | P1 | 3 | — | `Tests.Integration.Shared`, 4 step classes |
| GQL-01 | Replace vacuous `HasErrors` stubs with real assertions and re-verify all 57 existing scenarios | P0 | 3 | INF-03 | 4 step classes |
| GQL-02 | Not-found paths for all 16 mutations + 4 by-id queries | P0 | 4 | INF-03, GQL-01 | new `GraphQLErrorPaths.feature` + steps |
| GQL-03 | Protocol/validation errors: malformed UUID, unknown field, missing required arg, invalid enum literal, `first: 101`, cost limit, introspection | P0 | 4 | INF-03 | same feature file |
| GQL-04 | Domain validation: empty/oversized titles, complexity 0/11, blank name | P1 | 3 | INF-03 | same feature file |
| GQL-05 | Referential integrity: delete project with deliverables, delete deliverable with tasks, cross-deliverable dependency | P1 | 3 | INF-03 | new `GraphQLReferentialIntegrity.feature` |
| GQL-06 | `deleteTestData` contract: `success`/`message`, only `[DeleteAfterTest]` rows removed | P1 | 3 | INF-03 | new `GraphQLTestData.feature` |
| GQL-07 | Error contract stability: no stack traces, stable `extensions.code`, `path` present | P1 | 4 | GQL-02 | error feature |
| GQL-08 | `Query.projects` connection: `first/after/last/before`, `pageInfo`, `edges.cursor`, `order`, `where` (incl. `and`/`or`, nested `deliverables.some`) | P0 | 4 | INF-03 | new `GraphQLQueries.feature` |
| GQL-09 | `Query.deliverables` filters: status/type/title/`agentTasks.some`, order, paging | P1 | 3 | INF-03 | same |
| GQL-10 | `Query.agentTasks` filters: status `eq/in/nin`, `deliverableId`, `projectId`, `complexityRating gt/lt`, `title contains`, order, paging | P1 | 3 | INF-03 | same |
| GQL-11 | `Query.largeLanguageModels` filters/order + `cost` round-trip | P2 | 2 | INF-03 | same |
| GQL-12 | Relation fields: `Project.deliverables`, `Deliverable.project/agentTasks`, `AgentTask.deliverable/project/dependsOnAgentTask` | P1 | 3 | INF-03 | same |
| GQL-13 | Round-trip every untested input/scalar field (deliverable docs fields, LLM `apiKey`/`cost`, task token counters) | P1 | 4 | INF-03 | new `GraphQLFieldCoverage.feature` |
| GQL-14 | `DeliverableType.SPIKE` + case-insensitive `type` parsing | P2 | 1 | INF-03 | field-coverage feature |
| GQL-15 | Auto-completion edge cases (FAILED/REJECTED tasks, already-DONE deliverable) | P2 | 2 | — | `DeliverableAutoCompletion.feature` |
| GQL-16 | Fix step text that does not send the fields it claims | P1 | 2 | INF-03 | `DeliverableSteps.cs`, `AgentTaskSteps.cs` |
| MCP-01 | Integration coverage for the 6 never-called tools (happy paths) | P0 | 5 | — | new `ToolCoverageHappy.feature` + steps |
| MCP-02 | Tool error paths: `McpProtocolException` → `IsError` result with message; unknown ids; empty required strings | P0 | 4 | — | new `ToolErrorPaths.feature` |
| MCP-03 | Soft-error paths returning `ToolResponse.Error` (`get_next_task`, `get_next_deliverable`, `get_deliverable`) | P0 | 5 | MCP-01 | same |
| MCP-04 | Protocol errors: unknown tool, missing required arg, wrong arg type, invalid enum literal | P0 | 3 | — | new `ProtocolErrorPaths.feature` |
| MCP-05 | `prompts/list` + `prompts/get deliverable_workflow` | P1 | 2 | — | new `Prompts.feature` |
| MCP-06 | `resources/list` + `resources/read` server info | P1 | 2 | — | new `Resources.feature` |
| MCP-07 | Assert `inputSchema` (required list, property names/types) for all 13 tools | P1 | 3 | — | `ToolsList.feature` |
| MCP-08 | Feature flag off → task tools hidden from `tools/list` | P2 | 4 | INF-02 | new `FeatureFlag.feature` |
| MCP-09 | MCP endpoint hosted by `DevStack.Api` (`/mcp` on the Api image) | P1 | 4 | INF-01 | new `ApiHostedMcp.feature` in GraphQL project |
| MCP-10 | Per-scenario cleanup for MCP (direct SQL, since MCP host has no GraphQL) | P2 | 3 | — | MCP `SpecFlowHooks.cs` |
| MCP-11 | Rewrite the fake "empty name" tools/call scenario into a real error scenario | P1 | 2 | MCP-04 | `ToolsCall.feature` |
| MCP-12 | Assert notification round-trip rather than "no exception" | P3 | 2 | — | `Notifications.feature` |
| UI-01 | Fix AdminUi e2e harness schema drift (`cleanupTestData` → `deleteTestData`, legacy mutations) | P0 | 4 | — | `tests/e2e/testSetup.ts`, `testTeardown.ts`, `helpers/TestDataRegistry.ts`, `fixtures/TestDataManager.ts` |
| UI-02 | UI tests for GraphQL error surfaces: `ErrorState` + retry, dialog `serverError` banner, Apollo `onError` toast (use `msw`, already a dependency) | P1 | 5 | — | `src/AdminUi/src/**/*.test.tsx` |
| UI-03 | UI test for the network-error branch | P2 | 3 | UI-02 | `useApolloClient.test.ts` |
| UI-04 | Decide: wire `after` cursor or drop it from the document | P3 | 1 | — | `deliverables.graphql` |
| UI-05 | Decide: keep or drop `Query.largeLanguageModel` | P3 | 1 | — | schema/UI |
| INF-04 | CI: integration suites are `workflow_dispatch`/path-filtered only → new coverage does not gate merges | P2 | 3 | — | `.github/workflows/integration-tests.yml` |

**Not in scope** (tracked as decisions, §6): enforcing the status machine, emitting typed domain errors, removing the REST problem+json path.

---

## 8. Work breakdown (each unit ≤ ~20 min for an agent)

Lane ownership is by **file**, so lanes never touch the same file.

**Lane I — shared harness (me, first)**
1. INF-01 image-tag env override
2. INF-02 `WithEnvironment`
3. INF-03 `GraphQLRequestHelper` in `Tests.Integration.Shared`

**Lane A — GraphQL breadth** (`DevStack.Tests.Integration.GraphQL`, new files only)
1. `GraphQLQueries.feature` + `QueryCoverageSteps.cs` — GQL-08
2. same file — GQL-09, GQL-10
3. same file — GQL-11, GQL-12
4. `GraphQLFieldCoverage.feature` + `FieldCoverageSteps.cs` — GQL-13, GQL-14
5. `GraphQLReferentialIntegrity.feature` + steps — GQL-05
6. `GraphQLTestData.feature` + steps — GQL-06
7. `DeliverableAutoCompletion.feature` additions — GQL-15
8. GQL-16 step-text fixes

**Lane B — GraphQL error paths** (new files + the 4 existing step classes)
1. INF-03 adoption: replace `HasErrors` stubs — GQL-01
2. `GraphQLErrorPaths.feature` + `ErrorPathSteps.cs` — not-found matrix (GQL-02)
3. same — protocol/validation matrix (GQL-03)
4. same — domain validation (GQL-04)
5. same — error contract assertions (GQL-07)

**Lane C — MCP** (`DevStack.Tests.Integration.MCP`, new files + `ToolsList`/`ToolsCall`)
1. `ToolCoverageHappy.feature` + `ToolCoverageSteps.cs` — MCP-01
2. same — MCP-03 soft errors
3. `ToolErrorPaths.feature` + steps — MCP-02
4. `ProtocolErrorPaths.feature` + steps — MCP-04, MCP-11
5. `Prompts.feature` / `Resources.feature` — MCP-05, MCP-06
6. `ToolsList.feature` `inputSchema` assertions — MCP-07
7. MCP-10 cleanup hook

**Lane D — AdminUi**
1. UI-01 harness schema drift
2. UI-02 error-surface tests (ErrorState, dialog banner, Apollo toast)
3. UI-03 network-error branch

**Validation protocol** (sequential, one suite at a time — Docker builds are shared):
`dotnet build` → `dotnet test DevStack.Tests.Unit` → `dotnet test DevStack.Tests.Integration.GraphQL --filter Category!=no-ci` → `dotnet test DevStack.Tests.Integration.MCP` → AdminUi `npm run test` + `npx tsc`.

---

## 9. Conventions for new tests

1. One `.feature` file per concern; new step definitions in a **new** binding class with **unique step text** (SpecFlow raises ambiguous-match errors across binding classes).
2. Use `Scenario Outline` + `Examples` for input matrices (the existing suite has none — prefer tables over copy-paste scenarios).
3. Prefix test data with `[DeleteAfterTest]` so `TestDataPredicate`/`deleteTestData` reclaims it; MCP scenarios must clean up via SQL (MCP-10).
4. Error tests assert: (a) `errors` array present, (b) `data` null or partial, (c) the pinned `extensions.code`/message substring, (d) HTTP status. Pin shapes from an observed response, never from assumption.
5. Never assert masked-vs-unmasked exception detail without observing it first (DRIFT-02).
6. No secrets, no hard-coded credentials beyond the existing test-only defaults in `DevStackTestEnvBuilder`.
7. Keep `src/schema.graphql` untouched — these projects consume it.
8. Quality gates per `src/Server/AGENTS.md`: build, unit tests, `dotnet format` with no new warnings.

---

## 10. Execution log

| Time | Action | Result |
|---|---|---|
| 2026-10-31 | Baseline `dotnet build src/Server/DevStack.slnx -c Release` | 0 errors, 22 pre-existing warnings |
| 2026-10-31 | Ledger created; lanes defined | — |
