# DevStack.Agent

A tiny .NET 10 CLI that drives the **Hello** prompt against a running `opencode serve` instance using the [DevStack.OpenCode](../DevStack.OpenCode) SDK. It exists as a smoke test and a runnable end-to-end example for the SDK.

## What it does

1. Calls `GET /global/health` on the configured OpenCode server and refuses to continue if the server is unhealthy.
2. Fetches the provider/model inventory via `GET /provider` and pretty-prints it. Warns early if the requested `--model` is not in the server's list (so a 500 from the server doesn't come as a surprise).
3. Creates a fresh session via `POST /session`.
4. Sends a single `Hello` (or any other) prompt via `POST /session/{id}/message`.
5. Prints every `text` / `reasoning` / `tool` / `file` / `step-*` part of the assistant's reply, plus the model id, token usage, and cost.
6. Emits the session id on the way out so the caller can continue the conversation later.

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

The project is added to `DevStack.slnx` and depends only on `DevStack.OpenCode`.
