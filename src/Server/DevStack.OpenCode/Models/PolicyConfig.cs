namespace DevStack.OpenCode.Models;

/// <summary>Policy statement applied to a supported resource.</summary>
public sealed record PolicyConfig
{
    /// <summary>Policy action to evaluate.</summary>
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    /// <summary>Policy effect.</summary>
    [JsonPropertyName("effect")]
    public PolicyEffect Effect { get; init; }

    /// <summary>Resource glob or identifier to apply the policy to.</summary>
    [JsonPropertyName("resource")]
    public string Resource { get; init; } = string.Empty;
}

/// <summary>Effect of a policy rule.</summary>
public enum PolicyEffect
{
    [JsonStringEnumMemberName("allow")] Allow,
    [JsonStringEnumMemberName("deny")] Deny,
}
