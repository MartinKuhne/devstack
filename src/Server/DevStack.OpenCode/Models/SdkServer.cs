namespace DevStack.OpenCode.Models;

/// <summary>Discriminated union of MCP server statuses.</summary>
[JsonConverter(typeof(McpStatusConverter))]
public sealed record McpStatus
{
    internal McpStatus(string status, JsonElement raw)
    {
        Status = status;
        Raw = raw;
    }

    /// <summary>Discriminator — <c>connected</c>, <c>disabled</c>, <c>failed</c>, <c>needs_auth</c>, or <c>needs_client_registration</c>.</summary>
    public string Status { get; }

    /// <summary>Raw JSON element.</summary>
    public JsonElement Raw { get; }

    /// <summary>True when the server is connected.</summary>
    public bool IsConnected => Status == "connected";
    /// <summary>True when the server is disabled.</summary>
    public bool IsDisabled => Status == "disabled";
    /// <summary>True when the server failed to connect.</summary>
    public bool IsFailed => Status == "failed";
    /// <summary>True when the server needs OAuth authorization.</summary>
    public bool NeedsAuth => Status == "needs_auth";
    /// <summary>True when the server needs client registration.</summary>
    public bool NeedsClientRegistration => Status == "needs_client_registration";

    /// <summary>Error message attached to <c>failed</c> or <c>needs_client_registration</c> statuses.</summary>
    public string? Error => Raw.TryGetProperty("error", out var v) ? v.GetString() : null;
}

/// <summary>LSP server status.</summary>
public sealed record SdkLspStatus
{
    /// <summary>Stable id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Human-readable name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>Root path the LSP server operates on.</summary>
    [JsonPropertyName("root")] public string Root { get; init; } = string.Empty;
    /// <summary>Status — <c>connected</c> or <c>error</c>.</summary>
    [JsonPropertyName("status")] public string Status { get; init; } = "connected";
}

/// <summary>Formatter status.</summary>
public sealed record SdkFormatterStatus
{
    /// <summary>Formatter name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    /// <summary>File extensions this formatter handles.</summary>
    [JsonPropertyName("extensions")] public IReadOnlyList<string> Extensions { get; init; } = Array.Empty<string>();
    /// <summary>True when this formatter is enabled.</summary>
    [JsonPropertyName("enabled")] public bool Enabled { get; init; }
}

/// <summary>Command descriptor returned by <c>GET /command</c>.</summary>
public sealed record SdkCommand
{
    /// <summary>Command name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }

    /// <summary>Agent invoked by the command.</summary>
    [JsonPropertyName("agent")] public string? Agent { get; init; }

    /// <summary>Model override.</summary>
    [JsonPropertyName("model")] public string? Model { get; init; }

    /// <summary>Prompt template.</summary>
    [JsonPropertyName("template")] public string Template { get; init; } = string.Empty;

    /// <summary>True when the command runs as a subtask.</summary>
    [JsonPropertyName("subtask")] public bool? Subtask { get; init; }
}

/// <summary>Tool descriptor returned by <c>GET /experimental/tool</c>.</summary>
public sealed record ToolListItem
{
    /// <summary>Tool id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")] public string Description { get; init; } = string.Empty;
    /// <summary>JSON schema for the tool's parameters.</summary>
    [JsonPropertyName("parameters")] public JsonElement? Parameters { get; init; }
}
