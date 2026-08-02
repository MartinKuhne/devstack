namespace DevStack.OpenCode.Models;

/// <summary>Local directory reference configuration.</summary>
public sealed record ReferenceLocalConfig
{
    /// <summary>Filesystem path to the local reference.</summary>
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Hide this reference from the autocomplete menu.</summary>
    [JsonPropertyName("hidden")]
    public bool? Hidden { get; init; }
}
