namespace DevStack.OpenCode.Models;

/// <summary>Git-based reference configuration.</summary>
public sealed record ReferenceGitConfig
{
    /// <summary>Git repository URL.</summary>
    [JsonPropertyName("repository")]
    public required string Repository { get; init; }

    /// <summary>Git branch to track.</summary>
    [JsonPropertyName("branch")]
    public string? Branch { get; init; }

    /// <summary>Human-readable description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Hide this reference from the autocomplete menu.</summary>
    [JsonPropertyName("hidden")]
    public bool? Hidden { get; init; }
}
