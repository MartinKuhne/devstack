namespace DevStack.OpenCode.Models;

/// <summary>Per-model token limit configuration.</summary>
public sealed record ModelLimitConfig
{
    /// <summary>Total context window size in tokens.</summary>
    [JsonPropertyName("context")]
    public double Context { get; init; }

    /// <summary>Maximum input tokens per request.</summary>
    [JsonPropertyName("input")]
    public double? Input { get; init; }

    /// <summary>Maximum output tokens per request.</summary>
    [JsonPropertyName("output")]
    public double Output { get; init; }
}
