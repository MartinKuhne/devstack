namespace DevStack.OpenCode.Models;

/// <summary>Agent descriptor returned by <c>GET /agent</c>.</summary>
public sealed record SdkAgent
{
    /// <summary>Agent name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Optional human-readable description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }

    /// <summary>Agent mode — <c>subagent</c>, <c>primary</c>, or <c>all</c>.</summary>
    [JsonPropertyName("mode")] public string Mode { get; init; } = "all";

    /// <summary>True when the agent is built into OpenCode.</summary>
    [JsonPropertyName("builtIn")] public bool BuiltIn { get; init; }

    /// <summary>Top-p sampling parameter.</summary>
    [JsonPropertyName("topP")] public double? TopP { get; init; }

    /// <summary>Sampling temperature.</summary>
    [JsonPropertyName("temperature")] public double? Temperature { get; init; }

    /// <summary>Hex color code for the agent chip.</summary>
    [JsonPropertyName("color")] public string? Color { get; init; }

    /// <summary>Permission rules for this agent.</summary>
    [JsonPropertyName("permission")] public SdkAgentPermission Permission { get; init; } = new();

    /// <summary>Optional model assignment.</summary>
    [JsonPropertyName("model")] public ModelRef? Model { get; init; }

    /// <summary>Optional system prompt.</summary>
    [JsonPropertyName("prompt")] public string? Prompt { get; init; }

    /// <summary>Per-tool enable/disable map.</summary>
    [JsonPropertyName("tools")] public IDictionary<string, bool> Tools { get; init; } = new Dictionary<string, bool>();

    /// <summary>Provider-specific options.</summary>
    [JsonPropertyName("options")] public IDictionary<string, JsonElement> Options { get; init; } = new Dictionary<string, JsonElement>();

    /// <summary>Maximum number of agentic iterations.</summary>
    [JsonPropertyName("maxSteps")] public int? MaxSteps { get; init; }
}

/// <summary>Permission rules for an agent.</summary>
public sealed record SdkAgentPermission
{
    /// <summary>Edit permission.</summary>
    [JsonPropertyName("edit")] public string Edit { get; init; } = "allow";

    /// <summary>Bash permission, either a flat action or a per-subtool map.</summary>
    [JsonPropertyName("bash")] public BashPermissionUnion? Bash { get; init; }

    /// <summary>Webfetch permission.</summary>
    [JsonPropertyName("webfetch")] public string? Webfetch { get; init; }

    /// <summary>Doom-loop permission.</summary>
    [JsonPropertyName("doom_loop")] public string? DoomLoop { get; init; }

    /// <summary>External-directory permission.</summary>
    [JsonPropertyName("external_directory")] public string? ExternalDirectory { get; init; }
}

/// <summary>Discriminated union for bash permission (flat or per-subtool map).</summary>
[JsonConverter(typeof(BashPermissionUnionConverter))]
public sealed record BashPermissionUnion
{
    private BashPermissionUnion(bool isMap, object payload)
    {
        IsMap = isMap;
        Payload = payload;
    }

    /// <summary>True when this wraps a per-subtool map.</summary>
    public bool IsMap { get; }

    /// <summary>Underlying value.</summary>
    public object Payload { get; }

    /// <summary>Builds a flat-action rule.</summary>
    public static BashPermissionUnion FromAction(string action) => new(false, action);

    /// <summary>Builds a per-subtool map rule.</summary>
    public static BashPermissionUnion FromMap(IDictionary<string, string> map) => new(true, map);
}
