namespace DevStack.OpenCode.Models;

/// <summary>Provider-level request options for OpenCode.</summary>
public sealed record ProviderOptions
{
    /// <summary>Static API key to use for this provider.</summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; init; }

    /// <summary>Override base URL for the provider API.</summary>
    [JsonPropertyName("baseURL")]
    public string? BaseUrl { get; init; }

    /// <summary>GitHub Enterprise URL for copilot authentication.</summary>
    [JsonPropertyName("enterpriseUrl")]
    public string? EnterpriseUrl { get; init; }

    /// <summary>Enable promptCacheKey for this provider. Default <c>false</c>.</summary>
    [JsonPropertyName("setCacheKey")]
    public bool? SetCacheKey { get; init; }

    /// <summary>Full-request timeout in milliseconds. <c>false</c> disables the timeout.</summary>
    [JsonPropertyName("timeout")]
    [JsonConverter(typeof(TimeoutValueConverter))]
    public TimeoutValue? Timeout { get; init; }

    /// <summary>Header-receipt timeout in milliseconds. <c>false</c> disables the timeout.</summary>
    [JsonPropertyName("headerTimeout")]
    [JsonConverter(typeof(TimeoutValueConverter))]
    public TimeoutValue? HeaderTimeout { get; init; }

    /// <summary>Inter-chunk SSE timeout in milliseconds.</summary>
    [JsonPropertyName("chunkTimeout")]
    public int? ChunkTimeout { get; init; }
}
