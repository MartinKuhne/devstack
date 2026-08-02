namespace DevStack.OpenCode.Models;

/// <summary>Enterprise configuration.</summary>
public sealed record EnterpriseConfig
{
    /// <summary>Enterprise URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}
