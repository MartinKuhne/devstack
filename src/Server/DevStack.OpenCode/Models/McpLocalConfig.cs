namespace DevStack.OpenCode.Models;

/// <summary>Configuration for a local (stdio) MCP server.</summary>
public sealed record McpLocalConfig
{
    /// <summary>Type of MCP server connection.</summary>
    [JsonPropertyName("type")]
    public McpServerType Type { get; init; } = McpServerType.Local;

    /// <summary>Command and arguments to run the MCP server.</summary>
    [JsonPropertyName("command")]
    public required IReadOnlyList<string> Command { get; init; }

    /// <summary>Working directory for the MCP server process.</summary>
    [JsonPropertyName("cwd")]
    public string? Cwd { get; init; }

    /// <summary>Environment variables for the MCP server process.</summary>
    [JsonPropertyName("environment")]
    public IDictionary<string, string>? Environment { get; init; }

    /// <summary>Enable or disable the MCP server on startup.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>Timeout in ms for MCP server requests. Defaults to 5000.</summary>
    [JsonPropertyName("timeout")]
    public int? Timeout { get; init; }
}
