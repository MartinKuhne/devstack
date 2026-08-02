namespace DevStack.OpenCode.Models;

/// <summary>
/// Fully structured permission rule object, covering every well-known tool.
/// Any additional properties on the wire are preserved in
/// <see cref="AdditionalRules"/>.
/// </summary>
public sealed record PermissionRuleConfig
{
    /// <summary>Permission rule for the <c>read</c> tool.</summary>
    [JsonPropertyName("read")]
    public PermissionActionRule? Read { get; init; }

    /// <summary>Permission rule for the <c>edit</c> tool.</summary>
    [JsonPropertyName("edit")]
    public PermissionActionRule? Edit { get; init; }

    /// <summary>Permission rule for the <c>glob</c> tool.</summary>
    [JsonPropertyName("glob")]
    public PermissionActionRule? Glob { get; init; }

    /// <summary>Permission rule for the <c>grep</c> tool.</summary>
    [JsonPropertyName("grep")]
    public PermissionActionRule? Grep { get; init; }

    /// <summary>Permission rule for the <c>list</c> tool.</summary>
    [JsonPropertyName("list")]
    public PermissionActionRule? List { get; init; }

    /// <summary>Permission rule for the <c>bash</c> tool.</summary>
    [JsonPropertyName("bash")]
    public PermissionActionRule? Bash { get; init; }

    /// <summary>Permission rule for the <c>task</c> tool.</summary>
    [JsonPropertyName("task")]
    public PermissionActionRule? Task { get; init; }

    /// <summary>Permission rule for the <c>external_directory</c> tool.</summary>
    [JsonPropertyName("external_directory")]
    public PermissionActionRule? ExternalDirectory { get; init; }

    /// <summary>Permission rule for the <c>webfetch</c> tool.</summary>
    [JsonPropertyName("webfetch")]
    public PermissionAction? Webfetch { get; init; }

    /// <summary>Permission rule for the <c>websearch</c> tool.</summary>
    [JsonPropertyName("websearch")]
    public PermissionAction? Websearch { get; init; }

    /// <summary>Permission rule for the <c>todowrite</c> tool.</summary>
    [JsonPropertyName("todowrite")]
    public PermissionAction? Todowrite { get; init; }

    /// <summary>Permission rule for the <c>question</c> tool.</summary>
    [JsonPropertyName("question")]
    public PermissionAction? Question { get; init; }

    /// <summary>Permission rule for the <c>lsp</c> tool.</summary>
    [JsonPropertyName("lsp")]
    public PermissionActionRule? Lsp { get; init; }

    /// <summary>Permission rule for the <c>doom_loop</c> tool.</summary>
    [JsonPropertyName("doom_loop")]
    public PermissionAction? DoomLoop { get; init; }

    /// <summary>Permission rule for the <c>skill</c> tool.</summary>
    [JsonPropertyName("skill")]
    public PermissionActionRule? Skill { get; init; }

    /// <summary>Any additional tool rules defined by the user.</summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalRules { get; init; }
}
