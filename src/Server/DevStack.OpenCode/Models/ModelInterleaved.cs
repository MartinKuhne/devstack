namespace DevStack.OpenCode.Models;

/// <summary>Interleaved thinking configuration for a model.</summary>
public sealed record ModelInterleaved
{
    /// <summary>Field name to read the reasoning content from.</summary>
    [JsonPropertyName("field")]
    public string? Field { get; init; }
}
