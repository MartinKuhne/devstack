namespace DevStack.OpenCode.Models;

/// <summary>Configuration for a remote (HTTP) MCP server.</summary>
public sealed record McpRemoteConfig
{
    /// <summary>Type of MCP server connection.</summary>
    [JsonPropertyName("type")]
    public McpServerType Type { get; init; } = McpServerType.Remote;

    /// <summary>URL of the remote MCP server.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>Enable or disable the MCP server on startup.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>Headers to send with the request.</summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; init; }

    /// <summary>OAuth configuration for the MCP server.</summary>
    [JsonPropertyName("oauth")]
    public McpOAuthOrDisabled? OAuth { get; init; }

    /// <summary>Timeout in ms for MCP server requests. Defaults to 5000.</summary>
    [JsonPropertyName("timeout")]
    public int? Timeout { get; init; }
}
