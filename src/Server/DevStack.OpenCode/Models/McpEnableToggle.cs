namespace DevStack.OpenCode.Models;

/// <summary>
/// A minimal MCP server entry that only carries the <c>enabled</c> flag and
/// no further configuration.
/// </summary>
public sealed record McpEnableToggle
{
    /// <summary>Enable or disable the MCP server on startup.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }
}
