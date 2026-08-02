# DevStack.Agent

A tiny .NET 10 CLI that drives the **Hello** prompt against a running `opencode serve` instance using the [DevStack.OpenCode](../DevStack.OpenCode) SDK. It exists as a smoke test and a runnable end-to-end example for the SDK.

## What it does

1. Calls `GET /global/health` on the configured OpenCode server and refuses to continue if the server is unhealthy.
2. Fetches the provider/model inventory via `GET /provider` and pretty-prints it. Warns early if the requested `--model` is not in the server's list (so a 500 from the server doesn't come as a surprise).
3. Creates a fresh session via `POST /session`.
4. Sends a single `Hello` (or any other) prompt via `POST /session/{id}/message`.
5. Prints every `text` / `reasoning` / `tool` / `file` / `step-*` part of the assistant's reply, plus the model id, token usage, and cost.
6. Emits the session id on the way out so the caller can continue the conversation later.

In addition, the agent can talk to the DevStack GraphQL API through a
StrawberryShake-generated client (see [GraphQL via StrawberryShake](#graphql-via-strawberryshake) below) for smoke-testing the codegen pipeline.

## Usage

```bash
# 1. Start an OpenCode server in another terminal.
opencode serve --port 4096

# 2. Run the default "Hello" prompt.
dotnet run --project src/Server/DevStack.Agent

# 3. Or pass your own prompt + model.
dotnet run --project src/Server/DevStack.Agent -- \
  "Plan a refactor of the schema parser" \
  --model anthropic/claude-3-5-sonnet-20241022 \
  --title "Schema refactor plan"
```

### Command-line surface

| Argument | Default | Description |
|----------|---------|-------------|
| `<prompt>` | `Hello` | First positional argument. Anything starting with `--` is treated as a flag. |
| `--model <provider/model>` | auto — first connected provider's model matching `*free*` (case-insensitive), then the first connected provider's default, then `anthropic/claude-3-5-sonnet` | Model to address. The startup listing will warn if the chosen model isn't in the server's inventory. |
| `--title <text>` | `DevStack.Agent @ <UTC timestamp>` | Title for the created session. |
| `--opencode:BaseUrl <url>` | `http://127.0.0.1:4096/` | Override the OpenCode base URL (passed through `IConfiguration.AddCommandLine`). |
| `--list-projects` *(GraphQL)* | _off_ | List projects from the DevStack GraphQL API instead of running the OpenCode prompt. Pair with `--list-projects-first <n>` to cap the page size (default 50). |
| `--get-project <uuid>` *(GraphQL)* | _off_ | Look up a single project by id via the DevStack GraphQL API. |
| `--devstack:graphql:base-url <url>` | `http://localhost:8087/graphql` | Override the DevStack GraphQL endpoint. |
| `--show-plan` *(GraphQL + Git + GitHub)* | _off_ | Resolve the current git repository, look up the matching DevStack project, and list its `PLAN`-status deliverables. See [Repository-aware plan listing](#repository-aware-plan-listing) below. |
| `--repositoryRoot <path>` | _unset — falls back to the OpenCode server's worktree_ | Override the worktree path used by `--show-plan`. Useful when the OpenCode server is not running. |

### Configuration

`appsettings.json` is bundled with the binary and provides the defaults:

```json
{
  "OpenCode": {
    "BaseUrl": "http://127.0.0.1:4096/",
    "HttpTimeout": "00:01:00",
    "UserAgent": "DevStack.Agent/0.1"
  }
}
```

You can also override via environment variables (`OpenCode__BaseUrl=http://10.0.0.5:4096/`) or the `--opencode:BaseUrl=…` command-line switch. The order of precedence is:

1. Command-line (`AddCommandLine` — highest)
2. Environment variables (`AddEnvironmentVariables`)
3. `appsettings.json` (lowest)

## Example output

```
DevStack.Agent — OpenCode hello-prompt driver
  baseUrl:   http://127.0.0.1:4096/
  userAgent: DevStack.Agent/0.1

Available providers (1 connected):
  anthropic  [source=env]  default: claude-3-5-sonnet  (3 models)
    > anthropic/claude-3-5-sonnet  (Claude 3.5 Sonnet)
      anthropic/claude-3-5-haiku  (Claude 3.5 Haiku)
      anthropic/claude-3-opus  (Claude 3 Opus)

  Server default: anthropic/claude-3-5-sonnet
[20:38:09 INF] No --model specified; auto-selected opencode/north-mini-code-free (first *free* model on a connected provider). Use --model provider/model to override.
[20:38:09 INF] Sending prompt to opencode/north-mini-code-free…

--- assistant reply (assistant) ---
  [step start]
  [thinking] The user has simply said "Hello" with a message ID m0001…
Hello!
  [step finish] reason=stop cost=$0

model:    opencode/north-mini-code-free
tokens:   in=24195 out=0 reasoning=118 cache.read=0 cache.write=0
cost:     $0.0000
finish:   stop
--- end ---

Done. sessionId=ses_abc123
```

Not-connected providers are filtered out of the listing and out of the
auto-pick. If the server has more providers than the agent shows, the
header notes it (`(11 connected; 176 more not connected, hidden)`) so
you know where the inventory went. The raw response is still kept so
the explicit `--model` warning can distinguish "provider not connected"
from "provider not configured" from "model not on this provider".

## Building

```bash
dotnet build src/Server/DevStack.slnx
dotnet run --project src/Server/DevStack.Agent
```

The project is added to `DevStack.slnx` and depends on `DevStack.OpenCode`.

## GraphQL via StrawberryShake

The agent has a thin StrawberryShake client (`IDevStackClient`,
namespace `DevStack.Agent.GraphQL`) that talks to the DevStack GraphQL
API. It exists to smoke-test the codegen pipeline end-to-end and to
give the agent a way to enumerate the work it can do.

### What's wired up

- **Local tool manifest** (`dotnet-tools.json`) pins
  `StrawberryShake.Tools` v16.5.1 — the codegen entry point.
- **Package** `StrawberryShake.Server` v16.5.1 supplies the runtime
  and the MSBuild targets that drive build-time codegen.
- **Config** (`.graphqlrc.json`) names the schema file
  (`schema.graphql`, downloaded from the live server), the documents
  glob (`GraphQLs/**/*.graphql`), the client name (`DevStackClient`),
  the C# namespace (`DevStack.Agent.GraphQL`), and the transport URL
  (`http://localhost:8087/graphql/`).
- **Query documents** live in `GraphQLs/`. The build's
  `<GraphQL Include="GraphQLs/**/*.graphql" />` item group feeds them
  to the StrawberryShake MSBuild target, which writes generated C# to
  `obj/.../berry/DevStackClient.Client.cs` and auto-includes the file
  in the compilation.
- **`DevStackProjectClient`** is a one-class wrapper over
  `IDevStackClient` that returns consumer-shaped
  `ProjectSummary` records so the rest of the agent never depends on
  the generated wire types.
- **DI wiring** in `Program.cs`:
  ```csharp
  builder.Services
      .AddDevStackClient()
      .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphQLBaseUrl));
  ```
  The base URL is resolved (in order) from `--devstack:graphql:base-url`,
  `DevStack__GraphQL__BaseUrl`, `appsettings.json`, and finally the
  local `http://localhost:8087/graphql` default.

### Refreshing the schema

When the upstream DevStack API changes, refresh the local snapshot:

```bash
dotnet graphql download                  # uses url from .graphqlrc.json
# or pin a specific endpoint:
dotnet graphql download http://localhost:8087/graphql/
```

Then rebuild — the MSBuild target will pick up the new
`schema.graphql`, the next `dotnet build` regenerates
`DevStackClient.Client.cs`, and any schema drift surfaces as a build
error in the call sites that consume `IDevStackClient`.

### Adding a new query

1. Drop a `GraphQLs/<Name>.graphql` document next to the existing
   ones (any operation name → operation class on the generated
   client).
2. `dotnet build` — the new `<GraphQL>` item is picked up, the client
   gains the corresponding `I…Query` operation, and the result
   interface lands in `DevStack.Agent.GraphQL`.
3. Expose it through `DevStackProjectClient` (or a sibling wrapper) so
   `Program.cs` and other callers depend on a small consumer-shaped
   surface, not on the generated types directly.

### Quick smoke test

```bash
# 1. Make sure DevStack.Api is up on :8087.
dotnet run --project src/Server/DevStack.Api --urls http://localhost:8087

# 2. From a second terminal, hit the GraphQL client without
#    starting an OpenCode server.
dotnet run --project src/Server/DevStack.Agent -- --list-projects
dotnet run --project src/Server/DevStack.Agent -- --get-project c33c8df4-b40c-41dc-b92f-c691197210c0
```

## Repository-aware plan listing

`--show-plan` ties the three libraries together to give the agent
context about what it can do right now:

1. **Resolve the worktree.** `RepositoryLocator` first asks the
   OpenCode SDK for `project/current` and uses the server's
   `worktree` field. If the SDK is unreachable (or `--repositoryRoot`
   is supplied) the locator falls back to the override.
2. **Read the git remote.** `RepositoryContextResolver` opens the
   worktree with `LibGit2Sharp`, reads the `origin` remote URL, and
   normalizes it (SSH → HTTPS) to a canonical URL that matches the
   `Project.repository` field on the DevStack side. The original
   `.git` suffix is preserved because that's what existing projects
   store.
3. **Verify on GitHub (best-effort).** When the remote is a GitHub
   URL, the resolver asks `Octokit` for the repository's
   default-branch / visibility metadata. Set `GITHUB_TOKEN` to
   raise the rate limit; failure is logged and the listing
   continues with the locally-known owner/name.
4. **Find the DevStack project.** `DevStackProjectClient
   .FindProjectByRepositoryAsync` calls the new
   `GetProjectByRepository` GraphQL operation (added to
   `GraphQLs/`) to resolve the canonical URL to a project.
5. **List `PLAN` deliverables.** `PlanDeliverableLister` calls
   `DevStackProjectClient.ListPlanDeliverablesAsync`, backed by
   the new `GetPlanDeliverables` GraphQL operation, and prints
   every match.

```text
Repository: C:\Users\mkuhn\src\devstack
  remote:   https://github.com/MartinKuhne/devstack.git
  github:   MartinKuhne/devstack

DevStack project: personal-productivity-ai (c33c8df4-b40c-41dc-b92f-c691197210c0)
PLAN deliverables (10):

  [Feature    ] Agent Session as a Regular Tab
      id:      …
      describe: …
  …
```

Failure modes are surfaced as `error: …` on stderr with exit code 2
(missing repository, no DevStack project registered for the URL,
unsupported path). The OpenCode prompt flow is not affected.

### Smoke test without the OpenCode server

The `--repositoryRoot` override is the way to exercise the listing
without a running OpenCode server — useful in CI or local
debugging:

```bash
# Use a throwaway git repo whose origin points at an existing
# DevStack project to prove the full chain.
tmp=$(mktemp -d) && cd "$tmp" && git init -q && \
  git remote add origin https://github.com/MartinKuhne/personal-productivity-ai.git

dotnet run --project src/Server/DevStack.Agent -- \
  --show-plan --repositoryRoot "$tmp"
```
