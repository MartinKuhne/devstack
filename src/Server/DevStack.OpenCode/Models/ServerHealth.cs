namespace DevStack.OpenCode.Models;

/// <summary>Server health response from <c>GET /global/health</c>.</summary>
public sealed record ServerHealth
{
    /// <summary>True when the server is healthy.</summary>
    [JsonPropertyName("healthy")]
    public bool Healthy { get; init; }

    /// <summary>Server version string.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }
}

/// <summary>Project descriptor returned by the OpenCode server.</summary>
public sealed record ServerProject
{
    /// <summary>Project identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Working directory or display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Repository URL or path, when available.</summary>
    [JsonPropertyName("repository")]
    public string? Repository { get; init; }

    /// <summary>Additional project metadata preserved on the wire.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

/// <summary>VCS info response from <c>GET /vcs</c>.</summary>
public sealed record VcsInfo
{
    /// <summary>Current branch name.</summary>
    [JsonPropertyName("branch")]
    public string? Branch { get; init; }

    /// <summary>Latest commit SHA.</summary>
    [JsonPropertyName("head")]
    public string? Head { get; init; }

    /// <summary>Additional VCS fields preserved on the wire.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

/// <summary>Response from <c>GET /path</c>.</summary>
public sealed record PathInfo
{
    /// <summary>Current working directory.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }
}

/// <summary>Summary entry from <c>GET /config/providers</c>.</summary>
public sealed record ProviderSummary
{
    /// <summary>Stable provider identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Human-readable provider name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Default model identifier for this provider.</summary>
    [JsonPropertyName("defaultModel")]
    public string? DefaultModel { get; init; }

    /// <summary>Model identifiers available on this provider.</summary>
    [JsonPropertyName("models")]
    public IReadOnlyList<string>? Models { get; init; }

    /// <summary>Additional provider fields preserved on the wire.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

/// <summary>Response from <c>GET /config/providers</c>.</summary>
public sealed record ConfigProvidersResponse
{
    /// <summary>Available providers and their default models.</summary>
    [JsonPropertyName("providers")]
    public IReadOnlyList<ProviderSummary>? Providers { get; init; }

    /// <summary>Default model keyed by provider id.</summary>
    [JsonPropertyName("default")]
    public IDictionary<string, string>? Default { get; init; }
}
