namespace DevStack.OpenCode.Models;

/// <summary>Compaction behavior for long conversations.</summary>
public sealed record CompactionConfig
{
    /// <summary>Enable automatic compaction when context is full. Default true.</summary>
    [JsonPropertyName("auto")]
    public bool? Auto { get; init; }

    /// <summary>Enable pruning of old tool outputs. Default false.</summary>
    [JsonPropertyName("prune")]
    public bool? Prune { get; init; }

    /// <summary>Number of recent user turns to keep verbatim during compaction. Default 2.</summary>
    [JsonPropertyName("tail_turns")]
    public int? TailTurns { get; init; }

    /// <summary>Maximum number of tokens from recent turns to preserve verbatim after compaction.</summary>
    [JsonPropertyName("preserve_recent_tokens")]
    public int? PreserveRecentTokens { get; init; }

    /// <summary>Token buffer for compaction.</summary>
    [JsonPropertyName("reserved")]
    public int? Reserved { get; init; }
}
