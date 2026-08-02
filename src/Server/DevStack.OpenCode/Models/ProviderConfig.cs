namespace DevStack.OpenCode.Models;

/// <summary>Custom provider configuration and model overrides.</summary>
public sealed record ProviderConfig
{
    /// <summary>Provider SDK API identifier (e.g. <c>openai-compatible</c>).</summary>
    [JsonPropertyName("api")]
    public string? Api { get; init; }

    /// <summary>Human-readable provider name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Environment variable names to read credentials from.</summary>
    [JsonPropertyName("env")]
    public IReadOnlyList<string>? Env { get; init; }

    /// <summary>Stable provider identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>NPM package implementing this provider.</summary>
    [JsonPropertyName("npm")]
    public string? Npm { get; init; }

    /// <summary>Allow-list of model IDs for this provider.</summary>
    [JsonPropertyName("whitelist")]
    public IReadOnlyList<string>? Whitelist { get; init; }

    /// <summary>Block-list of model IDs for this provider.</summary>
    [JsonPropertyName("blacklist")]
    public IReadOnlyList<string>? Blacklist { get; init; }

    /// <summary>Provider request options.</summary>
    [JsonPropertyName("options")]
    public ProviderOptions? Options { get; init; }

    /// <summary>Per-model configuration overrides.</summary>
    [JsonPropertyName("models")]
    public IDictionary<string, ModelConfig>? Models { get; init; }
}
