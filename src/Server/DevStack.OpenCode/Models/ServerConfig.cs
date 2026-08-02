namespace DevStack.OpenCode.Models;

/// <summary>Server configuration for <c>opencode serve</c> and <c>web</c> commands.</summary>
public sealed record ServerConfig
{
    /// <summary>Port to listen on.</summary>
    [JsonPropertyName("port")]
    public int? Port { get; init; }

    /// <summary>Hostname to listen on.</summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; init; }

    /// <summary>Enable mDNS service discovery.</summary>
    [JsonPropertyName("mdns")]
    public bool? Mdns { get; init; }

    /// <summary>Custom domain name for mDNS service. Defaults to <c>opencode.local</c>.</summary>
    [JsonPropertyName("mdnsDomain")]
    public string? MdnsDomain { get; init; }

    /// <summary>Additional domains to allow for CORS.</summary>
    [JsonPropertyName("cors")]
    public IReadOnlyList<string>? Cors { get; init; }
}
