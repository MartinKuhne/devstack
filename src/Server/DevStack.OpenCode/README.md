# DevStack.OpenCode

A type-safe .NET 10 SDK client for the [OpenCode](https://opencode.ai) server. Mirrors the [JavaScript SDK surface](https://opencode.ai/docs/sdk/) one-to-one as a set of focused sub-clients, plus a strongly-typed model of the `opencode.ai/config.json` schema for offline construction and validation.

The SDK is the .NET counterpart that DevStack uses to drive OpenCode sessions, manage configuration, and stream server-sent events from a running `opencode serve` instance.

## Quick start

```csharp
using DevStack.OpenCode.Client;
using DevStack.OpenCode.DependencyInjection;
using DevStack.OpenCode.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddOpenCode(o => o.BaseUrl = new Uri("http://localhost:4096"));

await using var provider = services.BuildServiceProvider();
var opencode = provider.GetRequiredService<IOpenCodeClient>();

// 1. Create a session.
var session = await opencode.Session.CreateAsync(new SessionCreateRequest
{
    Title = "Hello demo",
});

// 2. Send a prompt and wait for the assistant reply.
var result = await opencode.Session.PromptAsync(session.Id, new SessionPromptRequest
{
    Model = new ModelRef { ProviderId = "anthropic", ModelId = "claude-3-5-sonnet-20241022" },
    Parts = new[] { PartInput.Text("Hello") },
});

if (result.Info.IsAssistant)
{
    foreach (var part in result.Parts)
    {
        if (part.Kind == "text") Console.WriteLine(part.AsText().Text);
        if (part.Kind == "reasoning") Console.WriteLine($"  thinking: {part.AsReasoning().Text}");
        if (part.Kind == "tool") Console.WriteLine($"  tool {part.AsTool().Tool} ({part.AsTool().State?.Status})");
    }
}
```

## Architecture

The project is a single class library with four concerns:

| Folder | Purpose |
|--------|---------|
| `Models/` | Immutable records that mirror the OpenCode config schema and the SDK response types (`Session`, `Message`, `Part`, `Provider`, `McpStatus`, `Auth`, `SdkEvent`, etc.) |
| `Serialization/` | `System.Text.Json` source — central `OpenCodeJson` options factory and custom converters for every discriminated union |
| `Client/` | The root `IOpenCodeClient` interface and the 20 sub-client interfaces + implementations, plus the shared `OpenCodeHttp` helper |
| `Store/` | `IOpenCodeConfigStore` / `OpenCodeConfigStore` — load/save `opencode.json` from disk |
| `Options/` | `OpenCodeOptions` — bound from configuration |
| `DependencyInjection/` | `AddOpenCode()` extension methods and the chainable `OpenCodeBuilder` |

The `OpenCodeHttp` helper centralises JSON serialization, query-string assembly, error reporting, and SSE streaming. Every sub-client is a thin facade over `OpenCodeHttp` so the wire format stays consistent.

## SDK sub-clients

`IOpenCodeClient` mirrors the JS SDK namespace structure. Every JS namespace is reachable as a typed property:

| Property | JS SDK equivalent | Operations |
|----------|------------------|------------|
| `Config` | `client.config.*` | `Get`, `Update` (PATCH), `Patch` (`JsonDocument`), `GetProviders` |
| `Session` | `client.session.*` | list, create, get, delete, update, children, todo, init, fork, abort, share/unshare, diff, summarize, messages, **prompt**, **message**, **prompt_async**, **command**, **shell**, revert/unrevert, permission reply |
| `Project` | `client.project.*` | list, current |
| `Pty` | `client.pty.*` | list, create, remove, get, update, connect |
| `Instance` | `client.instance.*` | dispose |
| `Path` | `client.path.*` | get |
| `Vcs` | `client.vcs.*` | get |
| `Global` | `client.global.*` | SSE event stream |
| `Tool` | `client.tool.*` | ids, list (with provider/model) |
| `Command` | `client.command.*` | list |
| `Provider` | `client.provider.*` | list, auth methods, **OAuth** (authorize, callback) |
| `Find` | `client.find.*` | text, files, symbols |
| `File` | `client.file.*` | list, read, status |
| `App` | `client.app.*` | log, agents |
| `Mcp` | `client.mcp.*` | status, add, connect, disconnect, **Auth** (remove, start, callback, authenticate) |
| `Lsp` | `client.lsp.*` | status |
| `Formatter` | `client.formatter.*` | status |
| `Tui` | `client.tui.*` | append-prompt, open-help/sessions/themes/models, submit/clear-prompt, execute-command, show-toast, publish, **Control** (next, response) |
| `Auth` | `client.auth.*` | set |
| `Event` | `client.event.*` | SSE event stream |

The root client also exposes **DevStack extensions** for section management — CRUD on the config sections (server, skills, watcher, formatter, lsp, permission, attachment, enterprise, tool_output, compaction, experimental), the named sub-resources (agent, provider, mcp, references, command), and the plugin list. These are `Get/Update/Clear` / `List/Get/Upsert/Delete` / `List/Append/Remove` respectively.

## Configuration

```json
{
  "OpenCode": {
    "BaseUrl": "http://localhost:4096",
    "SchemaPath": "config.json",
    "DefaultConfigPath": "~/.config/opencode/opencode.json",
    "HttpTimeout": "00:00:30",
    "UserAgent": "DevStack.OpenCode/1.0"
  }
}
```

| Property | Default | Description |
|----------|---------|-------------|
| `BaseUrl` | `https://opencode.ai/` | Server base URL |
| `SchemaPath` | `config.json` | Schema endpoint path (relative to `BaseUrl`) |
| `DefaultConfigPath` | `null` | When `null`, the file store searches `./opencode.json` then `~/.config/opencode/opencode.json` |
| `HttpTimeout` | `00:00:30` | Per-request HTTP timeout |
| `UserAgent` | `DevStack.OpenCode/1.0` | User-Agent header for outgoing requests |

Options are validated at resolution time (positive timeout, non-empty `SchemaPath`, non-null `BaseUrl`).

## Registration

```csharp
var services = new ServiceCollection();

// Programmatic configuration.
services.AddOpenCode(o =>
{
    o.BaseUrl = new Uri("http://localhost:4096");
    o.UserAgent = "MyApp/1.0";
});

// Or bind from configuration.
services.AddOpenCode(configuration);

// Or attach to a generic host.
builder.AddOpenCode();

// Chain overrides through the builder.
services.AddOpenCode()
        .WithClient<MyCustomClient>()
        .WithConfigStore<MyCustomStore>();
```

`AddOpenCodeSdk(...)` is kept as an `[Obsolete]` alias for back-compat with the first iteration.

## Common operations

### Send a "Hello" prompt

```csharp
var session = await opencode.Session.CreateAsync(new SessionCreateRequest { Title = "Hello" });

var result = await opencode.Session.PromptAsync(session.Id, new SessionPromptRequest
{
    Model = new ModelRef { ProviderId = "anthropic", ModelId = "claude-3-5-sonnet-20241022" },
    Parts = new[] { PartInput.Text("Hello") },
});
```

### Fire-and-forget prompt + stream events

```csharp
await opencode.Session.PromptAsyncFireAndForget(session.Id, new SessionPromptRequest
{
    Parts = new[] { PartInput.Text("Plan a refactor") },
});

await foreach (var evt in opencode.Event.SubscribeAsync())
{
    Console.WriteLine($"{evt.Type}: {evt.Properties}");
    if (evt.Type == "session.idle") break;
}
```

### Read, patch, and replace the entire config

```csharp
var config = await opencode.GetConfigAsync();
config.Shell = "/bin/zsh";
config.Model = "anthropic/claude-3-5-sonnet";

await opencode.ReplaceConfigAsync(config);

// Or apply a partial update with a JSON Patch document.
using var patch = JsonDocument.Parse("""{"model":"openai/gpt-4o"}""");
await opencode.PatchConfigAsync(patch);
```

### Manage a single section (e.g. permissions)

```csharp
await opencode.UpdatePermissionAsync(PermissionConfig.FromMap(new Dictionary<string, PermissionAction>
{
    ["bash"] = PermissionAction.Deny,
    ["read"] = PermissionAction.Allow,
}));
```

### OAuth into a provider

```csharp
var auth = await opencode.Provider.OAuth.AuthorizeAsync("anthropic", method: 0);
Console.WriteLine($"Open this URL in a browser: {auth.Url}");

// After the user authorises, complete the flow.
var ok = await opencode.Provider.OAuth.CallbackAsync("anthropic", method: 0, code: "<code>");
```

### Read a file from the workspace

```csharp
var content = await opencode.File.ReadAsync("src/index.ts");
Console.WriteLine(content.Content);
```

## Strongly-typed discriminated unions

The OpenCode wire format is full of `type`-discriminated unions. They are exposed as tagged records with a `Kind` discriminator and `AsXxx()` accessors:

```csharp
if (part.Kind == "text") part.AsText().Text
if (part.Kind == "tool") part.AsTool().Tool
if (evt.Type == "session.idle") ...

if (result.Info.IsAssistant) result.Info.AsAssistant().Cost
if (result.Info.IsUser)      result.Info.AsUser().Agent
```

Custom `JsonConverter`s preserve the discriminator on the wire, so round-tripping through `JsonSerializer` is lossless.

## Building & testing

```bash
dotnet build src/Server/DevStack.slnx
dotnet test  src/Server/DevStack.Tests.Unit/DevStack.Tests.Unit.csproj --filter "FullyQualifiedName~OpenCode"
```

The unit test suite uses an in-process `RecordingHandler` / `StreamingHandler` to exercise the full request shape (method, URL, body, SSE payload) without touching the network.

## License

MIT — see the repository root for the full text.
