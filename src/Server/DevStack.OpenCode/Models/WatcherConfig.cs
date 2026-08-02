namespace DevStack.OpenCode.Models;

/// <summary>File watcher configuration.</summary>
public sealed record WatcherConfig
{
    /// <summary>Glob patterns to ignore.</summary>
    [JsonPropertyName("ignore")]
    public IReadOnlyList<string>? Ignore { get; init; }
}
