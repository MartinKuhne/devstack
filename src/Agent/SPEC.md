# DevStack.Agent — Requirements Specification

> Reverse-engineered from the implementation in `src/Server/DevStack.Agent/`.
> Every requirement is stated in EARS (Easy Approach to Requirements Syntax)
> and traced to the code that fulfils it.

## Notation

EARS patterns used in this document:

| Pattern | Form |
|---|---|
| Ubiquitous | The system shall `<action>`. |
| Event-driven | When `<trigger>`, the system shall `<action>`. |
| State-driven | While `<state>`, the system shall `<action>`. |
| Unwanted | If `<condition>`, the system shall `<action>`. |
| Optional | Where `<feature is included>`, the system shall `<action>`. |

Requirement IDs are `[AG-nnn]`, monotonically increasing, grouped by area:

| Range | Area |
|---|---|
| AG-001 … AG-019 | Startup, hosting, configuration sources |
| AG-020 … AG-039 | CLI argument parsing and mode dispatch |
| AG-040 … AG-079 | Default OpenCode prompt flow (health, model, session, summary) |
| AG-080 … AG-119 | Live transcript (SSE consumer, message/part rendering) |
| AG-120 … AG-139 | `--show-plan` flow |
| AG-140 … AG-179 | `--run-plan` flow (prompt resolution, per-deliverable execution) |
| AG-180 … AG-209 | GraphQL project / deliverable operations |
| AG-210 … AG-239 | Repository context resolution (worktree + remote URL) |
| AG-240 … AG-259 | GitHub remote URL parsing and normalization |
| AG-260 … AG-279 | Error handling and exit codes |

---

## 1. Startup, hosting, and configuration sources

**[AG-001]** The system shall configure Serilog as its primary logger with
minimum level `Information`, overriding `Microsoft.*` to `Warning` and
`System.*` to `Warning`, before any other code runs. *Verifies:
`Program.cs:19-27`.*

**[AG-002]** The system shall install Serilog as the
`Microsoft.Extensions.Logging` provider via `AddSerilog()` and clear
the default MEL providers so the two formatters do not fight.
*Verifies: `Program.cs:33-34`.*

**[AG-003]** The system shall build the generic host via
`Host.CreateApplicationBuilder(args)`. *Verifies: `Program.cs:31`.*

**[AG-004]** The system shall load configuration from (in increasing
precedence order): `appsettings.json` next to the binary
(`AppContext.BaseDirectory`), environment variables, and the
command-line. *Verifies: `Program.cs:40-44`.*

**[AG-005]** Where `appsettings.json` is absent, the system shall continue
without a fatal error. *Verifies: `Program.cs:42` (optional: true).*

**[AG-006]** The system shall register the OpenCode SDK via
`builder.AddOpenCode()` so `OpenCode:BaseUrl` / `OpenCode:HttpTimeout`
/ `OpenCode:UserAgent` in `appsettings.json` flow through to the SDK.
*Verifies: `Program.cs:50`.*

**[AG-007]** The system shall register a StrawberryShake-generated
`IDevStackClient` whose `HttpClient.BaseAddress` is the resolved
GraphQL endpoint. *Verifies: `Program.cs:58-60`.*

**[AG-008]** The system shall register the following singletons in the
DI container: `OpenCodeAgent`, `DevStackProjectClient`,
`RepositoryLocator`, `RepositoryContextResolver`,
`PlanDeliverableLister`, `PlanExecutor`. *Verifies: `Program.cs:63-68`.*

**[AG-009]** The system shall print a four-line banner to the console
on startup showing `baseUrl`, `userAgent`, `timeout`, and `graphQL`.
*Verifies: `Program.cs:73-79`.*

**[AG-010]** Where any of the modes `--list-projects`, `--get-project`,
`--show-plan`, or `--run-plan` is requested, the system shall
short-circuit the default OpenCode prompt flow and run the
corresponding mode instead. *Verifies: `Program.cs:84-113`.*

**[AG-011]** When the system is running in the default (no-flag) mode,
it shall resolve the prompt, model, and title from `argv`, then call
`OpenCodeAgent.RunAsync` exactly once and exit with code `0` on
success. *Verifies: `Program.cs:115-124`.*

**[AG-012]** If any unhandled exception escapes the top-level try, the
system shall log it via `Log.Fatal` and exit with code `1`.
*Verifies: `Program.cs:126-130`.*

**[AG-013]** The system shall flush and close Serilog in a `finally`
block so log output is not lost on abrupt exits. *Verifies:
`Program.cs:131-134`.*

---

## 2. CLI argument parsing

**[AG-020]** When `argv[0]` is absent or starts with `--`, the system
shall default the prompt to the literal string `"Hello"` and log the
default at `Information` level. *Verifies: `Program.cs:136-151`.*

**[AG-021]** When `--model <spec>` is supplied, the system shall parse
`<spec>` as `<provider>/<model>`. *Verifies: `Program.cs:153-169`.*

**[AG-022]** If `--model <spec>` does not contain a `/` or the `/` is
the first or last character, the system shall log a warning naming
the offending value and treat the model as unconfigured (proceed
with auto-pick). *Verifies: `Program.cs:161-167`.*

**[AG-023]** When the user supplies `--get-project <value>`, the
system shall require `<value>` to parse as a `Guid`; otherwise it
shall print the usage hint to stderr and exit with code `2`.
*Verifies: `Program.cs:90-98`.*

**[AG-024]** When the user supplies `--list-projects-first <n>`, the
system shall parse `<n>` as an `int`; if parsing fails the default
of `50` is used. *Verifies: `Program.cs:209-217`.*

**[AG-025]** When the user supplies `--plan-prompt <path>`, the system
shall use `<path>` verbatim as the plan template location.
*Verifies: `Program.cs:194-207`.*

**[AG-026]** When `--plan-prompt` is not supplied, the system shall
resolve the plan template path in the following order: the
environment variable `DevStack__Plan__PromptPath`, the configuration
key `DevStack:Plan:PromptPath`, and finally the default
`prompts/plan.prompt`. *Verifies: `Program.cs:194-207`.*

**[AG-027]** When the user supplies `--devstack:graphql:base-url
<url>`, the system shall use `<url>` verbatim as the GraphQL
endpoint. *Verifies: `Program.cs:237-249`.*

**[AG-028]** When `--devstack:graphql:base-url` is not supplied, the
system shall resolve the GraphQL endpoint in the following order:
the environment variable `DevStack__GraphQL__BaseUrl`, the
configuration key `DevStack:GraphQL:BaseUrl`, and finally the
default `http://localhost:8087/graphql`. *Verifies:
`Program.cs:237-250`.*

---

## 3. Default OpenCode prompt flow

### 3.1 Health check

**[AG-040]** Before any session is created, the system shall call
`GET /global/health` on the configured OpenCode base URL.
*Verifies: `OpenCodeAgent.cs:39-40`.*

**[AG-041]** If the health check reports `healthy=false`, the system
shall throw `InvalidOperationException` with a message that names
the server's reported version (or `<unknown>`) and instructs the
user to start `opencode serve`. *Verifies: `OpenCodeAgent.cs:41-46`.*

### 3.2 Provider listing and auto-pick

**[AG-042]** The system shall call `GET /provider` to obtain the
server's provider/model inventory before sending a prompt.
*Verifies: `OpenCodeAgent.cs:47` (via `ListProvidersAsync`).*

**[AG-043]** Where the provider listing call fails, the system shall
log a warning naming the base URL and continue without the listing
(the model auto-pick then falls back to the hardcoded default).
*Verifies: `OpenCodeAgent.cs:734-740`.*

**[AG-044]** When printing the provider inventory, the system shall
filter out providers whose id is not in the `connected` set and
replace them in the header with a single `(N more not connected,
hidden)` note. *Verifies: `OpenCodeAgent.cs:499-501`,
`OpenCodeAgent.cs:514-518`.*

**[AG-045]** Where the inventory has zero connected providers, the
system shall print a hint instructing the user to pass
`--model provider/model` explicitly or to configure a provider on
the server. *Verifies: `OpenCodeAgent.cs:503-511`.*

**[AG-046]** Where the user did not pass `--model` and a model
inventory is available, the system shall auto-pick: first, the
first connected provider's model whose id or name contains the
substring `free` (case-insensitive); second, the first connected
provider's configured default. *Verifies:
`OpenCodeAgent.cs:852-887` (`ResolveDefaultModel`).*

**[AG-047]** Where the user did not pass `--model` and no model can
be auto-resolved, the system shall fall back to the hardcoded
default `anthropic/claude-3-5-sonnet` and log a warning.
*Verifies: `OpenCodeAgent.cs:818-836` (`ResolveModel`).*

**[AG-048]** When the user passes `--model` explicitly, the system
shall honour the supplied value verbatim and skip auto-pick.
*Verifies: `OpenCodeAgent.cs:818-823`.*

**[AG-049]** When the user passes `--model` for a model the server
does not list under a connected provider, the system shall log a
warning that names the model, the available alternatives (or
connected providers / configured providers) and continue
nonetheless (the prompt may still succeed). *Verifies:
`OpenCodeAgent.cs:911-955` (`WarnIfModelUnavailable`).*

### 3.3 Session creation and prompt

**[AG-050]** The system shall create one OpenCode session per
`RunAsync` invocation, with title `DevStack.Agent @ <UTC timestamp>`
when the caller did not supply a title. *Verifies:
`OpenCodeAgent.cs:53-56`.*

**[AG-051]** The system shall call `POST /session/{id}/message` to
send the prompt, wrapped in a `SessionPromptRequest` whose
`Parts` is a single `PartInput.Text(prompt)`. *Verifies:
`OpenCodeAgent.cs:73-80`.*

**[AG-052]** The system shall throw `ArgumentException` if the
prompt is null, empty, or whitespace. *Verifies:
`OpenCodeAgent.cs:36`.*

### 3.4 Heartbeat during long LLM calls

**[AG-053]** While `PromptAsync` is in flight, the system shall log
a "still waiting" line every 30 seconds naming the provider, model,
session id, and elapsed seconds, so the operator can see the agent
is alive. *Verifies: `OpenCodeAgent.cs:132-167` (`HeartbeatAsync`).*

**[AG-054]** When `PromptAsync` returns (or is cancelled), the
system shall cancel the heartbeat's linked CTS and wait briefly
for the heartbeat task to drain so the final "still waiting" line
isn't stranded on the log. *Verifies: `OpenCodeAgent.cs:99-113`.*

### 3.5 Final result and run summary

**[AG-055]** When `PromptAsync` returns, the system shall print a
per-run summary block containing `model:`, `tokens:`,
`cache.read/write`, `cost:`, and an optional `finish:` line; the
summary comes from the assistant message embedded in the
`PromptAsync` response. *Verifies: `OpenCodeAgent.cs:243-261`.*

**[AG-056]** When the prompt response's `info` is not an assistant
message (e.g. a user-only echo), the system shall skip the run
summary block. *Verifies: `OpenCodeAgent.cs:247-250`.*

**[AG-057]** After every successful `RunAsync`, the system shall
print the literal line `Done. sessionId=<id>` on its own line and
return the session id. *Verifies: `Program.cs:122-124`.*

---

## 4. Live transcript (SSE consumer)

### 4.1 Subscription lifecycle

**[AG-080]** Before the system sends the prompt, it shall open an
SSE subscription on `GET /global/event` and start consuming in the
background, so it does not miss the opening `message.updated` /
`part.updated` events. *Verifies: `OpenCodeAgent.cs:64-72,
286-293`.*

**[AG-081]** While the SSE subscription is open, the system shall
filter every event to those whose `properties.sessionID` equals the
current session id, dropping events for other sessions without
printing. *Verifies: `OpenCodeAgent.cs:300-305`.*

**[AG-082]** The system shall treat the following event types as
bookkeeping and ignore them: `server.connected`, `server.heartbeat`,
`sync`, `session.created`, `session.updated`, `session.diff`,
`session.status`. *Verifies: `OpenCodeAgent.cs:307-317`.*

**[AG-083]** When the server emits `session.idle`, the system shall
stop consuming the stream and return. *Verifies: `OpenCodeAgent.cs:345-346`.*

**[AG-084]** When the server emits `session.error`, the system shall
log the error payload at `Error` level and stop consuming the
stream. *Verifies: `OpenCodeAgent.cs:348-365`.*

**[AG-085]** When `PromptAsync` returns, the system shall wait up to
3 seconds for the consumer to drain closing events
(`session.idle`, the canonical `part.updated`), then
force-cancel the stream's linked CTS if the consumer is still
running. *Verifies: `Program.cs:101-116`.*

**[AG-086]** If the SSE stream raises any non-cancellation exception,
the system shall log a warning naming the exception and the
session id and continue (the run summary will still be printed
from the `PromptAsync` response). *Verifies:
`OpenCodeAgent.cs:368-379`.*

### 4.2 Per-message rendering

**[AG-087]** When the system receives a `message.updated` event for
a message id it has not seen before, the system shall print a
header `── msg N (role=<role> agent=<…> model=<…>) ──` where
`N` is the count of distinct message ids observed so far.
*Verifies: `OpenCodeAgent.cs:382-404`.*

**[AG-088]** The header's `agent=…` and `model=…` suffixes shall be
populated from the user-message sub-type for `role=user` (carries
`agent` and `ModelRef`) and from the assistant-message sub-type
for `role=assistant` (carries `ProviderId` / `ModelId`). When
both are missing, the suffix shall be empty. *Verifies:
`OpenCodeAgent.cs:659-680`.*

**[AG-089]** When the system receives a `message.updated` event for
a message id it has already seen, the system shall silently drop
the event (no duplicate header). *Verifies: `OpenCodeAgent.cs:397-401`.*

### 4.3 Per-part rendering

**[AG-090]** When the system receives a `message.part.updated` event
whose `part.type` is `text` or `reasoning`, and at least one
`message.part.delta` has been seen for the same part id, the system
shall print the canonical text on a new line. The line is prefixed
with `  💭 ` for `reasoning` parts and with no prefix for `text`
parts. *Verifies: `OpenCodeAgent.cs:427-441`.*

**[AG-091]** The system shall print the canonical text for any given
text/reasoning part at most once, even if the server emits
multiple `part.updated` events for the same part id after the
deltas. *Verifies: `OpenCodeAgent.cs:427-432`.*

**[AG-092]** When the system receives the first `message.part.delta`
for a part id it has not seen deltas for, the system shall print
a single placeholder: `  💭 …` for reasoning parts and `  …` for
text parts. *Verifies: `OpenCodeAgent.cs:470-474`.*

**[AG-093]** The system shall drop every subsequent
`message.part.delta` for a part id once the placeholder has been
printed; only the canonical `part.updated` text is shown. *Verifies:
`OpenCodeAgent.cs:470-474`.*

**[AG-094]** When the system receives a `message.part.updated` event
for a part whose type is anything other than `text` or `reasoning`
(file, tool, step-*, patch, subtask, agent, snapshot, retry,
compaction, or any unknown kind), the system shall call
`PrintPart` to render the part on a new line. *Verifies:
`OpenCodeAgent.cs:444-448`.*

**[AG-095]** For `tool` parts, the system shall print the tool name
and a status glyph (`✓` completed, `✗` error, `⏳` running, `…`
pending, `•` other) followed by the input and output previews
sourced from `state.raw.input` and `state.raw.output`, truncated
to 240 characters each. *Verifies: `OpenCodeAgent.cs:508-536`.*

**[AG-096]** For `file` parts, the system shall print the MIME type
followed by the filename (or URL when filename is absent). *Verifies:
`OpenCodeAgent.cs:539-544`.*

**[AG-097]** For `patch` parts, the system shall print the count of
files and the comma-separated list of paths. *Verifies:
`OpenCodeAgent.cs:547-554`.*

**[AG-098]** For `step-start` parts, the system shall print the
literal `  ── step start ──` marker. *Verifies:
`OpenCodeAgent.cs:557-562`.*

**[AG-099]** For `step-finish` parts, the system shall print
`  ── step finish <details> ──` where `<details>` is a
space-separated list of any of `reason=…`, `cost=$X`, and
`tokens=in:N out:M reasoning:K` (each segment omitted when its
value is empty/zero). *Verifies: `OpenCodeAgent.cs:565-587`.*

**[AG-100]** For `subtask` parts, the system shall print
`  👥 subtask → agent=<name>: <prompt, truncated to 160 chars>`.
*Verifies: `OpenCodeAgent.cs:589-594`.*

**[AG-101]** For `agent` parts, the system shall print
`  👤 agent: <name>`. *Verifies: `OpenCodeAgent.cs:596-600`.*

**[AG-102]** For `snapshot` parts, the system shall print
`  📸 snapshot <id>`. *Verifies: `OpenCodeAgent.cs:603-606`.*

**[AG-103]** For `retry` parts, the system shall print
`  🔁 retry attempt=<N>: <error, truncated to 160 chars>`. The
error payload is rendered as a string when it is a JSON string,
otherwise as a JSON serialization. *Verifies: `OpenCodeAgent.cs:610-618`.*

**[AG-104]** For `compaction` parts, the system shall print
`  🗜  compaction (auto|manual)` based on the `auto` flag.
*Verifies: `OpenCodeAgent.cs:620-626`.*

**[AG-105]** For any unknown `part.type`, the system shall print
`  [<type>] <unhandled>` so the operator can see something is
arriving without crashing. *Verifies: `OpenCodeAgent.cs:628-630`.*

### 4.4 Deserialization

**[AG-106]** When deserializing a `Message` or `Part` from an SSE
event payload, the system shall rely on the type-level
`[JsonConverter]` attributes (`MessageConverter` /
`PartConverter`) and the OpenCode SDK's shared `OpenCodeJson.Compact`
options. *Verifies: `OpenCodeAgent.cs:643-648`.*

**[AG-107]** If deserialization yields `null`, the system shall throw
`InvalidOperationException` naming the type that failed. *Verifies:
`OpenCodeAgent.cs:643-648`.*

### 4.5 JSON-field helper

**[AG-108]** When reading a top-level field from a `JsonElement`
via `ReadJsonField`, the system shall return the empty string when
the element is missing, null, or the named field is absent.
*Verifies: `OpenCodeAgent.cs:686-705`.*

**[AG-109]** When the field's value kind is `String`, the system
shall return the unescaped string; for `Number`, `True`, or
`False`, the system shall return the literal text form; for
`Object` or `Array`, the system shall re-serialize the value
without indentation. *Verifies: `OpenCodeAgent.cs:696-704`.*

### 4.6 Truncation helper

**[AG-110]** `Truncate(s, n)` shall return the empty string when
`s` is null or empty, `s` itself when `s.Length <= n`, and
`s.Substring(0, n) + "…"` otherwise. *Verifies:
`OpenCodeAgent.cs:707-715`.*

---

## 5. `--show-plan` flow

**[AG-120]** When the user supplies `--show-plan`, the system
shall locate the worktree, resolve the git context, list the
matching DevStack project's PLAN-status deliverables, and print a
tabular report. *Verifies: `Program.cs:101-105`,
`Program.cs:299-329`.*

**[AG-121]** If locating the worktree, resolving the git context,
or listing the deliverables throws, the system shall write a
friendly `error: …` line to stderr and exit with code `2`.
*Verifies: `Program.cs:413-431`.*

**[AG-122]** When the `--show-plan` listing returns zero
deliverables, the system shall print `  (none)` and exit with
code `0`. *Verifies: `Program.cs:317-321`.*

**[AG-123]** When the listing has at least one deliverable, the
system shall print a header line
`  TYPE         ID                                     STATUS  TITLE`
and one line per deliverable aligned to that header.
*Verifies: `Program.cs:323-327`.*

**[AG-124]** When a DevStack project is not registered for the
repository's canonical URL, the system shall throw
`InvalidOperationException` with the canonical URL in the
message. *Verifies: `PlanDeliverableLister.cs:54-58`.*

---

## 6. `--run-plan` flow

### 6.1 Discovery (shared with `--show-plan`)

**[AG-140]** When the user supplies `--run-plan`, the system shall
first run the same discovery as `--show-plan` (worktree, git
context, DevStack project, PLAN deliverables) and exit with code
`2` on any failure. *Verifies: `Program.cs:107-113`,
`Program.cs:331-375`.*

### 6.2 Prompt template resolution

**[AG-141]** The system shall resolve the plan prompt template in
the order: `--plan-prompt <path>`, environment variable
`DevStack__Plan__PromptPath`, configuration key
`DevStack:Plan:PromptPath`, default `prompts/plan.prompt`.
*Verifies: `Program.cs:194-207`.*

**[AG-142]** Where the resolved template path is relative, the
system shall anchor it to the agent's `AppContext.BaseDirectory`
(`PlanExecutor.ResolvePromptPath`) so the prompts travel with the
binary and are not coupled to the worktree. Absolute paths shall
be used verbatim. *Verifies: `PlanExecutor.cs:131-138`.*

**[AG-143]** If the resolved template file does not exist, the
system shall throw `FileNotFoundException` with a message that
mentions the resolved path and the override flags
(`--plan-prompt` / `DevStack:Plan:PromptPath`). The caller
(`Program.cs`) catches this and exits with code `2`. *Verifies:
`PlanExecutor.cs:63-70`, `Program.cs:366-370`.*

**[AG-144]** If the template does not contain the literal token
`{{DeliverableId}}`, the system shall log a warning naming the
template path and the token, and proceed (substitution will be a
no-op). *Verifies: `PlanExecutor.cs:73-78`.*

### 6.3 Per-deliverable execution

**[AG-145]** The system shall execute the rendered plan prompt
once per PLAN deliverable, in the order returned by the
deliverable listing. *Verifies: `PlanExecutor.cs:87`.*

**[AG-146]** For each deliverable, the system shall print
`→ Planning <title> (<id>)` followed by `  type: <type>` and
`  status: <status>` on separate lines, before invoking
`OpenCodeAgent.RunAsync`. *Verifies: `PlanExecutor.cs:94-97`.*

**[AG-147]** The session title for a per-deliverable run shall be
`Plan: <title>`. *Verifies: `PlanExecutor.cs:92`.*

**[AG-148]** Where one deliverable's `OpenCodeAgent.RunAsync`
throws a non-cancellation exception, the system shall log the
exception, write a friendly `error: planning <id> failed: <msg>`
to stderr, record the failure in the summary, and continue with
the next deliverable (one bad deliverable does not sink the
batch). *Verifies: `PlanExecutor.cs:108-117`.*

**[AG-149]** After a successful per-deliverable run, the system
shall print `✓ Done. sessionId=<id>`. *Verifies:
`PlanExecutor.cs:105-106`.*

**[AG-150]** The system shall honour the cancellation token
between deliverables, throwing `OperationCanceledException` if
the caller cancels mid-run. *Verifies: `PlanExecutor.cs:89`.*

### 6.4 Final summary

**[AG-151]** When the executor finishes, it shall return a
`PlanRunSummary` containing the list of processed deliverable ids
and a dictionary of failed ids keyed by id with the exception
message as value. *Verifies: `PlanExecutor.cs:84-86`,
`PlanExecutor.cs:120`.*

**[AG-152]** After a `--run-plan` invocation, the system shall
print `Plan summary: N succeeded, M failed.` and exit with
code `0` when every deliverable succeeded or `3` when at least
one failed. *Verifies: `Program.cs:372-374`.*

---

## 7. GraphQL project and deliverable operations

### 7.1 List projects

**[AG-180]** When `--list-projects` is supplied, the system shall
call `GetProjects` on the DevStack GraphQL API with the
`--list-projects-first` count (default `50`). *Verifies:
`Program.cs:84-88`, `DevStackProjectClient.cs:39-54`.*

**[AG-181]** If the server returns no projects, the system shall
print `No projects returned by the DevStack GraphQL API.` and
exit with code `0`. *Verifies: `Program.cs:257-262`.*

**[AG-182]** When the server returns at least one project, the
system shall print `DevStack projects (N):` followed by one
block per project containing `id`, `name`, `repo`, and an
optional `describe:` line (only when the description is
non-whitespace). *Verifies: `Program.cs:264-275`.*

**[AG-183]** The system shall throw
`ArgumentOutOfRangeException` when `--list-projects-first` is
`0` or negative. *Verifies: `DevStackProjectClient.cs:43-46`.*

### 7.2 Get project by id

**[AG-184]** When `--get-project <uuid>` is supplied, the system
shall call `GetProjectById` on the DevStack GraphQL API.
*Verifies: `DevStackProjectClient.cs:61-73`.*

**[AG-185]** If the server returns no project for the given id,
the system shall print `Project <id> not found.` and exit with
code `0`. *Verifies: `Program.cs:284-288`.*

**[AG-186]** When the server returns a project, the system shall
print `Project <id>: <name>`, `repo: <url>`, and an optional
`describe: <text>` line. *Verifies: `Program.cs:290-296`.*

### 7.3 Find project by repository

**[AG-187]** `FindProjectByRepositoryAsync` shall call
`GetProjectByRepository` and return the first matching project
or `null` when the server returns no matches. *Verifies:
`DevStackProjectClient.cs:80-97`.*

**[AG-188]** The system shall throw `ArgumentException` when
called with a null/empty/whitespace repository. *Verifies:
`DevStackProjectClient.cs:84-87`.*

### 7.4 List plan deliverables

**[AG-189]** `ListPlanDeliverablesAsync(projectId)` shall call
`GetPlanDeliverables` for the given project and return the
deliverable nodes mapped to `DeliverableSummary` records.
*Verifies: `DevStackProjectClient.cs:106-127`.*

**[AG-190]** The system shall throw `ArgumentException` when
called with `Guid.Empty`. *Verifies: `DevStackProjectClient.cs:110-113`.*

**[AG-191]** Every GraphQL operation in
`DevStackProjectClient` shall call `result.EnsureNoErrors()` so
that server-reported errors surface as exceptions, never as
silently-empty results. *Verifies: `DevStackProjectClient.cs:50, 67, 91, 117`.*

---

## 8. Repository context resolution

### 8.1 Worktree resolution

**[AG-210]** When the user supplies `--repositoryRoot <path>`, the
system shall treat that path as the worktree verbatim (after
`Path.GetFullPath` normalisation) and skip the OpenCode SDK
lookup. *Verifies: `RepositoryLocator.cs:49-58`.*

**[AG-211]** If `--repositoryRoot <path>` does not exist or is not
a directory, the system shall throw `DirectoryNotFoundException`
with a message naming the resolved path. *Verifies:
`RepositoryLocator.cs:52-55`.*

**[AG-212]** When `--repositoryRoot` is not supplied and the
OpenCode SDK is available, the system shall call
`Project.GetCurrentAsync()` and use the server's reported
`worktree` as the worktree path. *Verifies:
`RepositoryLocator.cs:64-77`.*

**[AG-213]** If the OpenCode SDK's `Project.GetCurrentAsync`
returns a project with an empty `worktree`, the system shall log
a warning naming the base URL and fall through to the
"no-worktree" error path. *Verifies: `RepositoryLocator.cs:78-79`.*

**[AG-214]** If the OpenCode SDK's `Project.GetCurrentAsync` throws
a non-cancellation exception, the system shall log a warning
naming the base URL and the exception, and fall through to the
"no-worktree" error path. *Verifies: `RepositoryLocator.cs:81-87`.*

**[AG-215]** When neither `--repositoryRoot` nor the OpenCode SDK
produces a worktree, the system shall throw
`InvalidOperationException` instructing the user to either start
the OpenCode SDK or pass `--repositoryRoot`. *Verifies:
`RepositoryLocator.cs:90-92`.*

### 8.2 Git remote parsing

**[AG-216]** The system shall open the worktree as a
`LibGit2Sharp.Repository` and read the remote named
`origin` (the `DefaultRemoteName` constant). *Verifies:
`RepositoryContextResolver.cs:34, 60-67`.*

**[AG-217]** If the worktree is not a git repository or the named
remote is missing, the system shall throw
`InvalidOperationException` with a message that names the
worktree path and the missing remote (and includes the
`git remote add` hint when the remote is missing). *Verifies:
`RepositoryContextResolver.cs:66-70`.*

**[AG-218]** The system shall normalise the raw remote URL via
`GitRemoteUrlNormalizer.Normalize` and, when applicable, parse
the GitHub `owner/name` pair via `TryParseGitHub`. *Verifies:
`RepositoryContextResolver.cs:73-76`.*

**[AG-219]** When the remote is a GitHub URL, the system shall
attempt a best-effort Octokit verification by calling
`Repository.Get(owner, name)`. A failure to verify shall be
logged as a warning and the resolver shall continue with the
locally-known owner/name. *Verifies:
`RepositoryContextResolver.cs:78-82, 93-112`.*

---

## 9. GitHub remote URL parsing and normalization

**[AG-240]** `GitRemoteUrlNormalizer.TryParseGitHub(remoteUrl)`
shall return a `GitHubRepositoryRef(owner, name)` for SSH
(`git@github.com:owner/name[.git]`) and HTTPS
(`https://github.com/owner/name[.git]`) GitHub URLs, with or
without the `.git` suffix, and `null` for any other host or
malformed input. *Verifies:
`RepositoryContextResolver.cs:161-190`.*

**[AG-241]** When parsing an SSH form, the system shall split on
`/` and require at least two segments (`owner` and `name`).
*Verifies: `RepositoryContextResolver.cs:192-200`.*

**[AG-242]** When parsing an HTTPS form, the system shall match
on `uri.Host.EndsWith("github.com", OrdinalIgnoreCase)` and
take the first two path segments as `owner` and `name`.
*Verifies: `RepositoryContextResolver.cs:178-187`.*

**[AG-243]** `Normalize(remoteUrl)` shall throw
`ArgumentException` for null/empty/whitespace input. *Verifies:
`RepositoryContextResolver.cs:134-138`.*

**[AG-244]** For GitHub URLs, `Normalize` shall return
`https://github.com/<owner>/<name>` with the original `.git`
suffix preserved when present. *Verifies:
`RepositoryContextResolver.cs:207-224`.*

**[AG-245]** For non-GitHub URLs, `Normalize` shall return the
original URL with any trailing `/` stripped. *Verifies:
`RepositoryContextResolver.cs:148-149`.*

**[AG-246]** The `.git` suffix shall be stripped when
constructing a `GitHubRepositoryRef.Name` (so the ref is
canonical), but preserved when reconstructing the full
canonical URL (so the DevStack `Project.repository` lookup still
matches existing project rows that store the full clone URL).
*Verifies: `RepositoryContextResolver.cs:202-205, 207-224`.*

---

## 10. Error handling and exit codes

**[AG-260]** The system shall exit with code `0` when the
requested mode completes successfully (all projects
listed/retrieved, all deliverables planned, prompt returned).
*Verifies: `Program.cs:124, 261, 275, 296, 328, 374`.*

**[AG-261]** The system shall exit with code `1` when any
unhandled exception escapes the top-level try block. *Verifies:
`Program.cs:126-130`.*

**[AG-262]** The system shall exit with code `2` for
"configuration / discovery" failures:
  * `--get-project` argument is not a valid UUID
  * Repository locator fails (no worktree, not a git repo, no
    remote)
  * `PlanDeliverableLister` finds no DevStack project for the
    canonical URL
  * Plan prompt template file is missing
  * `--repositoryRoot` path does not exist
*Verifies: `Program.cs:93-98, 366-370, 413-431`.*

**[AG-263]** The system shall exit with code `3` when
`--run-plan` completes but at least one deliverable failed
(partial success). *Verifies: `Program.cs:372-374`.*

**[AG-264]** All user-facing error messages for known failure
modes shall be written to `Console.Error` and prefixed with
`error: ` (matching the existing CLI convention). *Verifies:
`Program.cs:95, 116, 368, 415, 428`.*

**[AG-265]** The system shall catch `OperationCanceledException`
in the SSE consumer and treat it as a normal shutdown (no
warning, no stack trace). *Verifies: `OpenCodeAgent.cs:365-368`.*

---

## Cross-reference summary

| Area | Source files | ID range | Count |
|---|---|---|---|
| Startup / hosting | `Program.cs` | AG-001 … AG-013 | 13 |
| CLI argument parsing | `Program.cs` | AG-020 … AG-028 | 9 |
| Default OpenCode prompt flow | `OpenCodeAgent.cs` | AG-040 … AG-057 | 18 |
| Live transcript (SSE) | `OpenCodeAgent.cs` | AG-080 … AG-110 | 31 |
| `--show-plan` flow | `Program.cs`, `PlanDeliverableLister.cs` | AG-120 … AG-124 | 5 |
| `--run-plan` flow | `Program.cs`, `PlanExecutor.cs` | AG-140 … AG-152 | 13 |
| GraphQL project operations | `Program.cs`, `DevStackProjectClient.cs` | AG-180 … AG-191 | 12 |
| Repository context resolution | `RepositoryLocator.cs`, `RepositoryContextResolver.cs`, `RepositoryContext.cs` | AG-210 … AG-219 | 10 |
| GitHub URL parsing / normalization | `RepositoryContextResolver.cs` | AG-240 … AG-246 | 7 |
| Error handling and exit codes | `Program.cs`, `OpenCodeAgent.cs` | AG-260 … AG-265 | 6 |
| **Total** | | | **124** |
