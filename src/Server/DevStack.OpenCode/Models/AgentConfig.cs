namespace DevStack.OpenCode.Models;

/// <summary>Per-agent configuration in OpenCode.</summary>
public sealed record AgentConfig
{
    /// <summary>Model to use in the format of <c>provider/model</c>.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>Default model variant for this agent.</summary>
    [JsonPropertyName("variant")]
    public string? Variant { get; init; }

    /// <summary>Sampling temperature.</summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    /// <summary>Top-p sampling parameter.</summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    /// <summary>System prompt override for this agent.</summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }

    /// <summary>Deprecated. Use <see cref="Permission"/> instead.</summary>
    [JsonPropertyName("tools")]
    public IDictionary<string, bool>? Tools { get; init; }

    /// <summary>Disable this agent entirely.</summary>
    [JsonPropertyName("disable")]
    public bool? Disable { get; init; }

    /// <summary>Description of when to use the agent.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Agent execution mode.</summary>
    [JsonPropertyName("mode")]
    public AgentMode? Mode { get; init; }

    /// <summary>Hide this subagent from the <c>@</c> autocomplete menu.</summary>
    [JsonPropertyName("hidden")]
    public bool? Hidden { get; init; }

    /// <summary>Provider-specific request options.</summary>
    [JsonPropertyName("options")]
    public IDictionary<string, JsonElement>? Options { get; init; }

    /// <summary>Color for the agent chip. Either a hex code (<c>#FF5733</c>) or a theme color.</summary>
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    /// <summary>Maximum number of agentic iterations before forcing a text-only response.</summary>
    [JsonPropertyName("steps")]
    public int? Steps { get; init; }

    /// <summary>Deprecated. Use <see cref="Steps"/> instead.</summary>
    [JsonPropertyName("maxSteps")]
    public int? MaxSteps { get; init; }

    /// <summary>Permission rules applied to this agent's tool calls.</summary>
    [JsonPropertyName("permission")]
    public PermissionConfig? Permission { get; init; }
}
