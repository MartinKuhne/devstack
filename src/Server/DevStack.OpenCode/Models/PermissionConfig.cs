namespace DevStack.OpenCode.Models;

/// <summary>
/// Permission rules for tool calls. May be a single action applied to all tools, a
/// map of tool name to action, or a fully specified object covering every
/// well-known tool. Stored as a tagged union on <see cref="Kind"/>.
/// </summary>
[JsonConverter(typeof(PermissionConfigConverter))]
public sealed record PermissionConfig
{
    private PermissionConfig(PermissionKind kind, object payload)
    {
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Discriminator describing how <see cref="Payload"/> should be interpreted.</summary>
    public PermissionKind Kind { get; }

    /// <summary>Underlying value (<see cref="PermissionAction"/>, a dictionary, or <see cref="PermissionRuleConfig"/>).</summary>
    public object Payload { get; }

    /// <summary>Applies a single action to all tools.</summary>
    public static PermissionConfig FromAction(PermissionAction action) =>
        new(PermissionKind.Action, action);

    /// <summary>Applies a per-tool action map.</summary>
    public static PermissionConfig FromMap(IDictionary<string, PermissionAction> map) =>
        new(PermissionKind.Map, map);

    /// <summary>Applies a fully structured permission rule object.</summary>
    public static PermissionConfig FromRules(PermissionRuleConfig rules) =>
        new(PermissionKind.Rules, rules);

    /// <summary>Returns the underlying action when <see cref="Kind"/> is <see cref="PermissionKind.Action"/>.</summary>
    public PermissionAction? Action => Kind == PermissionKind.Action ? (PermissionAction)Payload : null;

    /// <summary>Returns the underlying map when <see cref="Kind"/> is <see cref="PermissionKind.Map"/>.</summary>
    public IDictionary<string, PermissionAction>? Map =>
        Kind == PermissionKind.Map ? (IDictionary<string, PermissionAction>)Payload : null;

    /// <summary>Returns the underlying rule object when <see cref="Kind"/> is <see cref="PermissionKind.Rules"/>.</summary>
    public PermissionRuleConfig? Rules =>
        Kind == PermissionKind.Rules ? (PermissionRuleConfig)Payload : null;
}

/// <summary>Discriminator for <see cref="PermissionConfig"/> payloads.</summary>
public enum PermissionKind
{
    Action,
    Map,
    Rules,
}
