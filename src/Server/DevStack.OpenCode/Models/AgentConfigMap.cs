namespace DevStack.OpenCode.Models;

/// <summary>
/// Map of agent name to configuration. Includes the well-known agents (<c>plan</c>,
/// <c>build</c>, <c>general</c>, <c>explore</c>, <c>title</c>, <c>summary</c>,
/// <c>compaction</c>) and any user-defined custom agents.
/// </summary>
public sealed record AgentConfigMap
{
    /// <summary>Read-only planning agent.</summary>
    [JsonPropertyName("plan")]
    public AgentConfig? Plan { get; init; }

    /// <summary>Default build agent.</summary>
    [JsonPropertyName("build")]
    public AgentConfig? Build { get; init; }

    /// <summary>General-purpose agent.</summary>
    [JsonPropertyName("general")]
    public AgentConfig? General { get; init; }

    /// <summary>Read-only exploration agent.</summary>
    [JsonPropertyName("explore")]
    public AgentConfig? Explore { get; init; }

    /// <summary>Title-generation agent.</summary>
    [JsonPropertyName("title")]
    public AgentConfig? Title { get; init; }

    /// <summary>Summary-generation agent.</summary>
    [JsonPropertyName("summary")]
    public AgentConfig? Summary { get; init; }

    /// <summary>Context compaction agent.</summary>
    [JsonPropertyName("compaction")]
    public AgentConfig? Compaction { get; init; }

    /// <summary>Additional custom agent configurations.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalAgents { get; init; }
}
