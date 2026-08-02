namespace DevStack.OpenCode.Models;

/// <summary>Discriminated union of authentication credentials.</summary>
[JsonConverter(typeof(AuthConverter))]
public sealed record Auth
{
    internal Auth(string kind, JsonElement raw)
    {
        Kind = kind;
        Raw = raw;
    }

    /// <summary>Discriminator — <c>oauth</c>, <c>api</c>, or <c>wellknown</c>.</summary>
    public string Kind { get; }

    /// <summary>Raw JSON element.</summary>
    public JsonElement Raw { get; }

    /// <summary>True when this wraps OAuth credentials.</summary>
    public bool IsOAuth => Kind == "oauth";
    /// <summary>True when this wraps an API key.</summary>
    public bool IsApi => Kind == "api";
    /// <summary>True when this wraps a well-known token.</summary>
    public bool IsWellKnown => Kind == "wellknown";

    /// <summary>Builds an API-key auth payload from a raw key string.</summary>
    public static Auth FromApiKey(string key, IDictionary<string, string>? metadata = null)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "api", key, metadata }, OpenCodeJson.Compact);
        return new Auth("api", element);
    }

    /// <summary>Builds an OAuth auth payload from a refresh/access token pair.</summary>
    public static Auth FromOAuth(string refresh, string access, long expires, string? enterpriseUrl = null)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "oauth", refresh, access, expires, enterpriseUrl }, OpenCodeJson.Compact);
        return new Auth("oauth", element);
    }

    /// <summary>Builds a well-known token auth payload.</summary>
    public static Auth FromWellKnown(string key, string token)
    {
        var element = JsonSerializer.SerializeToElement(new { type = "wellknown", key, token }, OpenCodeJson.Compact);
        return new Auth("wellknown", element);
    }
}

/// <summary>OAuth credentials.</summary>
public sealed record OAuthCredentials
{
    /// <summary>Always <c>oauth</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "oauth";
    /// <summary>Refresh token.</summary>
    [JsonPropertyName("refresh")] public string Refresh { get; init; } = string.Empty;
    /// <summary>Access token.</summary>
    [JsonPropertyName("access")] public string Access { get; init; } = string.Empty;
    /// <summary>Epoch milliseconds when the access token expires.</summary>
    [JsonPropertyName("expires")] public long Expires { get; init; }
    /// <summary>Enterprise URL override for GitHub-style auth.</summary>
    [JsonPropertyName("enterpriseUrl")] public string? EnterpriseUrl { get; init; }
}

/// <summary>API key credentials.</summary>
public sealed record ApiCredentials
{
    /// <summary>Always <c>api</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "api";
    /// <summary>API key value.</summary>
    [JsonPropertyName("key")] public string Key { get; init; } = string.Empty;
    /// <summary>Optional metadata.</summary>
    [JsonPropertyName("metadata")] public IDictionary<string, string>? Metadata { get; init; }
}

/// <summary>Well-known token credentials.</summary>
public sealed record WellKnownCredentials
{
    /// <summary>Always <c>wellknown</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; init; } = "wellknown";
    /// <summary>API key value.</summary>
    [JsonPropertyName("key")] public string Key { get; init; } = string.Empty;
    /// <summary>Token value.</summary>
    [JsonPropertyName("token")] public string Token { get; init; } = string.Empty;
}
