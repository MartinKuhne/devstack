namespace DevStack.OpenCode.Models;

/// <summary>Nested provider hint for a model.</summary>
public sealed record ModelProviderConfig
{
    /// <summary>NPM package implementing this provider.</summary>
    [JsonPropertyName("npm")]
    public string? Npm { get; init; }

    /// <summary>Provider SDK API identifier.</summary>
    [JsonPropertyName("api")]
    public string? Api { get; init; }
}
