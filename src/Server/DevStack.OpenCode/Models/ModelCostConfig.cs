namespace DevStack.OpenCode.Models;

/// <summary>Per-model cost in USD per million tokens.</summary>
public sealed record ModelCostConfig
{
    /// <summary>Input token cost.</summary>
    [JsonPropertyName("input")]
    public double Input { get; init; }

    /// <summary>Output token cost.</summary>
    [JsonPropertyName("output")]
    public double Output { get; init; }

    /// <summary>Cache-read token cost.</summary>
    [JsonPropertyName("cache_read")]
    public double? CacheRead { get; init; }

    /// <summary>Cache-write token cost.</summary>
    [JsonPropertyName("cache_write")]
    public double? CacheWrite { get; init; }

    /// <summary>Override cost for context windows over 200k tokens.</summary>
    [JsonPropertyName("context_over_200k")]
    public ModelCostConfig? ContextOver200K { get; init; }
}
