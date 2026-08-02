namespace DevStack.OpenCode.Models;

/// <summary>
/// Represents either an <see cref="McpOAuthConfig"/> object or the literal
/// <c>false</c> (disable OAuth auto-detection).
/// </summary>
[JsonConverter(typeof(McpOAuthOrDisabledConverter))]
public readonly record struct McpOAuthOrDisabled : IEquatable<McpOAuthOrDisabled>
{
    private McpOAuthOrDisabled(bool disabled, McpOAuthConfig? config)
    {
        Disabled = disabled;
        Config = config;
    }

    /// <summary>True when the OAuth auto-detection is disabled.</summary>
    public bool Disabled { get; }

    /// <summary>The OAuth configuration (defined only when <see cref="Disabled"/> is false).</summary>
    public McpOAuthConfig? Config { get; }

    /// <summary>True when a non-null OAuth configuration is set.</summary>
    public bool HasConfig => !Disabled && Config is not null;

    /// <summary>Builds a disabled OAuth entry.</summary>
    public static McpOAuthOrDisabled Disable() => new(true, null);

    /// <summary>Builds an enabled OAuth entry with the given configuration.</summary>
    public static McpOAuthOrDisabled FromConfig(McpOAuthConfig config) => new(false, config);

    public bool Equals(McpOAuthOrDisabled other) => Disabled == other.Disabled && Equals(Config, other.Config);
    public override int GetHashCode() => HashCode.Combine(Disabled, Config);
    public override string ToString() => Disabled ? "disabled" : Config?.ToString() ?? "null";
}

/// <summary>OAuth authentication configuration for an MCP server.</summary>
public sealed record McpOAuthConfig
{
    /// <summary>OAuth client ID. If not provided, dynamic client registration is attempted.</summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; init; }

    /// <summary>OAuth client secret, if required by the authorization server.</summary>
    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; init; }

    /// <summary>OAuth scopes to request during authorization.</summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>Port for the local OAuth callback server. Default 19876.</summary>
    [JsonPropertyName("callbackPort")]
    public int? CallbackPort { get; init; }

    /// <summary>OAuth redirect URI.</summary>
    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; init; }
}
