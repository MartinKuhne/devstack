# DevStack.Agent — Requirements Specification

## Notation

EARS patterns used in this document:

| Pattern | Form |
|---|---|
| Ubiquitous | The system shall `<action>`. |
| Event-driven | When `<trigger>`, the system shall `<action>`. |
| State-driven | While `<state>`, the system shall `<action>`. |
| Unwanted | If `<condition>`, the system shall `<action>`. |
| Optional | Where `<feature is included>`, the system shall `<action>`. |

## Core workflow

**[AG-001]** When the agent starts, it shall identify the current repository context from the active worktree or
repository root.
**[AG-002]** The agent shall resolve the canonical repository URL and use it to find the matching DevStack project.
**[AG-003]** When the `--run-plan` command line option is present, the agent shall list the deliverables for the
matching project and select those in PLAN status.
**[AG-004]** When the `--run-plan` command line option is present, the agent shall execute the plan prompt sequentially
for each selected deliverable.
**[AG-005]** The agent shall process deliverables in order and shall not skip or parallelize the plan execution flow.

## Startup, hosting, and configuration sources

**[AG-006]** The system shall use structured logging and log to the console and a local log file.
**[AG-007]** The system shall use the OpenCode SDK.
**[AG-008]** The system shall generate GraphQL types from `http://localhost:8087/graphql`.
**[AG-009]** The system shall support `--list-projects`, `--get-project`, `--show-plan`, `--run-plan`, and `--test`
modes.
**[AG-010]** When the system is running `--test` mode or no command line options are specified, it shall resolve the
`Hello` prompt then exit.

## `--show-plan` flow

**[AG-120]** When the user supplies `--show-plan`, the system shall locate the worktree, resolve the git context, list
the matching DevStack project's PLAN-status deliverables, and print a tabular report.
**[AG-121]** If locating the worktree, resolving the git context, or listing the deliverables throws, the system shall
write a friendly `error: …` line to stderr and exit with code `2`.
**[AG-122]** When the `--show-plan` listing returns zero deliverables, the system shall print `  (none)` and exit with
code `0`.
**[AG-123]** When the listing has at least one deliverable, the system shall print a header line `  TYPE ID
STATUS  TITLE` and one line per deliverable aligned to that header.
**[AG-124]** When a DevStack project is not registered for the repository's canonical URL, the system shall exit with an
error message.

## `--run-plan` flow

### Discovery (shared with `--show-plan`)

**[AG-140]** When the user supplies `--run-plan`, the system shall first run the same discovery as `--show- plan`
(worktree, git context, DevStack project, PLAN deliverables) and exit with code `2` on any failure.

### Prompt template resolution

**[AG-141]** The system shall resolve the plan prompt template in the order: `--plan-prompt <path>`, default
`prompts/plan.prompt`.
**[AG-142]** Where the resolved template path is relative, the system shall anchor it to the agent's base directory so
the prompts travel with the binary and are not coupled to the worktree. Absolute paths shall be used
verbatim.
**[AG-143]** If the resolved template file does not exist, the system shall exit with an error.
**[AG-144]** If the template does not contain the literal token `{{DeliverableId}}`, the system shall exit with an error
message.

### Per-deliverable execution

**[AG-145]** The system shall enumerate the Deliverables for the project in `PLAN` status. For each deliverable, it
shall use the OpenCode API to execute the `plan.prompt` prompt.
**[AG-146]** For each deliverable, the system shall print `→ Planning <title> (<id>)` followed by `  type: <type>` and `
status: <status>` on separate lines, before invoking `runPrompt`.
**[AG-147]** The session title for a per-deliverable run shall be `Plan: <title>`.
**[AG-148]** Where one deliverable's `runPrompt` throws a non-cancellation exception, the system shall log the
exception, write a friendly `error: planning <id> failed: <msg>` to stderr, record the failure in the
summary, and continue with the next deliverable (one bad deliverable does not sink the batch).
**[AG-149]** After a successful per-deliverable run, the system shall print `✓ Done. sessionId=<id>`.

## CLI argument parsing

**[AG-021]** When `--model <spec>` is supplied, the system shall parse `<spec>` as `<provider>/<model>`.
**[AG-022]** If `--model <spec>` does not contain a `/` or the `/` is the first or last character, the system shall log
a warning naming the offending value and treat the model as unconfigured (proceed with auto-pick).
**[AG-023]** When the user supplies `--get-project <value>`, the system shall require `<value>` to parse as a `Guid`;
otherwise it shall print the usage hint to stderr and exit with code `2`.
**[AG-025]** When the user supplies `--plan-prompt <path>`, the system shall use `<path>` verbatim as the plan template
location.
**[AG-026]** When `--plan-prompt` is not supplied, the system shall resolve the plan template as `prompts/plan.prompt`.
**[AG-028]** The system shall resolve the GraphQL endpoint in the following order. The default is
`http://localhost:8087/graphql`.

## Default OpenCode prompt flow

### Health check

**[AG-040]** Before any session is created, the system shall call the Opencode health check.
**[AG-041]** If the health check reports `healthy=false`, the system shall error exit with a message that names the
server's reported version (or `<unknown>`) and instructs the user to start `opencode serve`.

### Provider listing and auto-pick

**[AG-042]** The system shall call retrieve providers to obtain the server's provider/model inventory before sending a
prompt.
**[AG-043]** Where the provider listing call fails, the system shall log a warning naming the base URL and continue
without the listing (the model auto-pick then falls back to the hardcoded default).
**[AG-044]** When printing the provider inventory, the system shall filter out providers whose id is not in the
`connected` set and replace them in the header with a single `(N more not connected, hidden)`.
**[AG-045]** Where the inventory has zero connected providers, the system shall print a hint instructing the user to
pass `--model provider/model` explicitly or to configure a provider on the server.
**[AG-046]** Where the user did not pass `--model` and a model inventory is available, the system shall auto-pick:
first, the first connected provider's model whose id or name contains the substring `free`
(case-insensitive); second, any other provider's model whose id or name contains the substring `free`.
**[AG-047]** Where the user did not pass `--model` and no model can be auto-resolved, the system shall exit with an
error.
**[AG-048]** When the user passes `--model` explicitly, the system shall honour the supplied value verbatim and skip
auto-pick.
**[AG-049]** When the user passes `--model` for a model the server does not list under a connected provider, the system
shall exit with an error.

### Session creation and prompt

**[AG-050]** The system shall create one OpenCode session per `runPrompt` invocation, with title `DevStack.Agent @ <UTC
timestamp>` when the caller did not supply a title.
**[AG-052]** The system shall throw if the prompt is null, empty, or whitespace.

### Heartbeat during long LLM calls

**[AG-053]** While `runPrompt` is in flight, the system shall log a "still waiting" line every 30 seconds naming the
provider, model, session id, and elapsed seconds, so the operator can see the agent is alive.
**[AG-054]** When `runPrompt` returns (or is cancelled), the system shall cancel the heartbeat's linked CTS and wait
briefly for the heartbeat task to drain so the final "still waiting" line isn't stranded on the log.

### Final result and run summary

**[AG-055]** When `runPrompt` returns, the system shall print a per-run summary block containing `model:`, `tokens:`,
`cache.read/write`, `cost:`, and an optional `finish:` line; the summary comes from the assistant message
embedded in the OpenCode SDK response.
**[AG-056]** When the prompt response's `info` is not an assistant message (e.g. a user-only echo), the system shall
skip the run summary block.
**[AG-057]** After every successful `runPrompt`, the system shall print the literal line `Done. sessionId=<id>` on its
own line and return the session id.

## Live transcript (SSE consumer)

### Subscription lifecycle

**[AG-080]** Before the system sends the prompt, it shall open an SSE subscription on and start consuming in the
background, so it does not miss the opening `message.updated` / `part.updated` events.
**[AG-081]** While the SSE subscription is open, the system shall filter every event to those whose
`properties.sessionID` equals the current session id, dropping events for other sessions without printing.
**[AG-082]** The system shall treat the following event types as bookkeeping and ignore them: `server.connected`,
`sync`, `session.created`, `session.updated`, `session.diff`, `session.status`. 
**[AG-106]** On `server.heartbeat`, print a message to the user indicating the processing is still ongoing.
**[AG-083]** When the server emits `session.idle`, the system shall stop consuming the stream and return.
**[AG-084]** When the server emits `session.error`, the system shall log the error payload at `Error` level and stop
consuming the stream.
**[AG-085]** When `runPrompt` returns, the system shall wait up to 3 seconds for the consumer to drain closing events
(`session.idle`, the canonical `part.updated`), then force-cancel the stream's linked CTS if the consumer
is still running.
**[AG-086]** If the SSE stream raises any non-cancellation exception, the system shall log a warning naming the
exception and the session id and continue (the run summary will still be printed from the `runPrompt`
response).

### Per-message rendering

**[AG-087]** When the system receives a `message.updated` event for a message id it has not seen before, the system
shall print a header `── msg N (role=<role> agent=<…> model=<…>) ──` where `N` is the count of distinct
message ids observed so far.
**[AG-088]** The header's `agent=…` and `model=…` suffixes shall be populated from the user-message sub- type for
`role=user` (carries `agent` and `ModelRef`) and from the assistant-message sub-type for `role=assistant`
(carries `ProviderId` / `ModelId`). When both are missing, the suffix shall be empty.
**[AG-089]** When the system receives a `message.updated` event for a message id it has already seen, the system shall
silently drop the event (no duplicate header).

### Per-part rendering

**[AG-090]** When the system receives a `message.part.updated` event whose `part.type` is `text` or `reasoning`, and at
least one `message.part.delta` has been seen for the same part id, the system shall print the canonical
text on a new line.
**[AG-091]** The system shall print the canonical text for any given text/reasoning part at most once, even if the
server emits multiple `part.updated` events for the same part id after the deltas.
**[AG-092]** When the system receives the first `message.part.delta` for a part id it has not seen deltas for, the
system shall print a single placeholder: `  …`
**[AG-093]** The system shall drop every subsequent `message.part.delta` for a part id once the placeholder has been
printed; only the canonical `part.updated` text is shown.
**[AG-094]** When the system receives a `message.part.updated` event for a part whose type is anything other than `text`
or `reasoning` (file, tool, step-*, patch, subtask, agent, snapshot, retry, compaction, or any unknown
kind), the system shall call `PrintPart` to render the part on a new line.
**[AG-095]** For `tool` parts, the system shall print the tool name and a status glyph (`✓` completed, `✗` error, `⏳`
running, `…` pending, `•` other) followed by the input and output previews sourced from `state.raw.input`
and `state.raw.output`, truncated to 240 characters each.
**[AG-096]** For `file` parts, the system shall print the MIME type followed by the filename (or URL when filename is
absent).
**[AG-097]** For `patch` parts, the system shall print the count of files and the comma-separated list of paths.
**[AG-098]** For `step-start` parts, the system shall print the literal `  ── step start ──` marker.
**[AG-099]** For `step-finish` parts, the system shall print `  ── step finish <details> ──` where `<details>` is a
space-separated list of any of `reason=…`, `cost=$X`, and `tokens=in:N out:M reasoning:K` (each segment
omitted when its value is empty/zero).
**[AG-100]** For `subtask` parts, the system shall print `  👥 subtask → agent=<name>: <prompt, truncated to 160 chars>`.
**[AG-101]** For `agent` parts, the system shall print `  👤 agent: <name>`.
**[AG-102]** For `snapshot` parts, the system shall print `  📸 snapshot <id>`.
**[AG-103]** For `retry` parts, the system shall print `  🔁 retry attempt=<N>: <error, truncated to 160 chars>`. The
error payload is rendered as a string when it is a JSON string, otherwise as a JSON serialization.
**[AG-104]** For `compaction` parts, the system shall print `  🗜  compaction (auto|manual)` based on the `auto` flag.
**[AG-105]** For any unknown `part.type`, the system shall print `  [<type>] <unhandled>` so the operator can see
something is arriving without crashing.

### Final summary

**[AG-151]** When the executor finishes, it shall return a `PlanRunSummary` containing the list of processed deliverable
ids and a dictionary of failed ids keyed by id with the exception message as value.
**[AG-152]** After a `--run-plan` invocation, the system shall print `Plan summary: N succeeded, M failed.` and exit
with code `0` when every deliverable succeeded or `3` when at least one failed.

## GraphQL project and deliverable operations

### List projects

**[AG-180]** When `--list-projects` is supplied, the system shall call `GetProjects` on the DevStack GraphQL API with
the count (default `50`).
**[AG-181]** If the server returns no projects, the system shall print `No projects returned by the DevStack GraphQL
API.` and exit with code `0`.
**[AG-182]** When the server returns at least one project, the system shall print `DevStack projects (N):` followed by
one block per project containing `id`, `name`, `repo`, and an optional `describe:` line (only when the
description is non-whitespace).

### Get project by id

**[AG-184]** When `--get-project <uuid>` is supplied, the system shall call `GetProjectById` on the DevStack GraphQL
API.
**[AG-185]** If the server returns no project for the given id, the system shall print `Project <id> not found.` and
exit with code `0`.
**[AG-186]** When the server returns a project, the system shall print `Project <id>: <name>`, `repo: <url>`, and an
optional `describe: <text>` line.

## Repository context resolution

### Worktree resolution

**[AG-210]** When the user supplies `--repositoryRoot <path>`, the system shall treat that path as the worktree verbatim
(after path normalisation) and skip the OpenCode SDK lookup.
**[AG-211]** If `--repositoryRoot <path>` does not exist or is not a directory, the system shall exit with an error.
**[AG-212]** When `--repositoryRoot` is not supplied and the OpenCode SDK is available, the system shall call
`Project.GetCurrent()` and use the server's reported `worktree` as the worktree path.
**[AG-213]** If the OpenCode SDK's `Project.GetCurrent` returns a project with an empty `worktree`, the system shall log
a warning naming the base URL and fall through to the "no-worktree" error path.
**[AG-215]** When neither `--repositoryRoot` nor the OpenCode SDK produces a worktree, the system shall throw an error
instructing the user to either start the OpenCode SDK or pass `--repositoryRoot`.

### Git remote parsing

**[AG-216]** The system shall open the worktree as a repository and read the remote named `origin`.
**[AG-217]** If the worktree is not a git repository or the named remote is missing, the system shall exit with an error
message that names the worktree path and the missing remote (and includes the `git remote add` hint when
the remote is missing).
**[AG-218]** The system shall normalise the raw remote URL via and, when applicable, parse the GitHub `owner/name` pair.
**[AG-219]** When the remote is a GitHub URL, the system shall attempt a best-effort Octokit verification. A failure to
verify shall be logged as a warning and the resolver shall continue with the locally-known owner/name.

## GitHub remote URL parsing and normalization

**[AG-240]** The system shall normalize SSH (`git@github.com:owner/name[.git]`) and HTTPS
(`https://github.com/owner/name[.git]`) GitHub URLs, with or without the `.git` suffix, so the DevStack
`Project.repository` lookup still matches existing project rows that store the full clone URL
