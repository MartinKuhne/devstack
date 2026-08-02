namespace DevStack.OpenCode.Models;

/// <summary>Thresholds for truncating tool output.</summary>
public sealed record ToolOutputConfig
{
    /// <summary>Maximum lines of tool output before it is truncated and saved to disk. Default 2000.</summary>
    [JsonPropertyName("max_lines")]
    public int? MaxLines { get; init; }

    /// <summary>Maximum bytes of tool output before it is truncated and saved to disk. Default 51200.</summary>
    [JsonPropertyName("max_bytes")]
    public int? MaxBytes { get; init; }
}
