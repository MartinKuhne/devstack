namespace DevStack.OpenCode.Models;

/// <summary>
/// A permission rule for a single tool, which may be a flat action
/// (<c>"ask"</c>, <c>"allow"</c>, <c>"deny"</c>) or a per-subtool map.
/// </summary>
[JsonConverter(typeof(PermissionActionRuleConverter))]
public sealed record PermissionActionRule
{
    private PermissionActionRule(PermissionAction action)
    {
        Kind = PermissionActionRuleKind.Action;
        Action = action;
        SubToolMap = null;
    }

    private PermissionActionRule(IDictionary<string, PermissionAction> subToolMap)
    {
        Kind = PermissionActionRuleKind.SubToolMap;
        Action = null;
        SubToolMap = subToolMap;
    }

    /// <summary>Discriminator describing the underlying payload.</summary>
    public PermissionActionRuleKind Kind { get; }

    /// <summary>Flat action when <see cref="Kind"/> is <see cref="PermissionActionRuleKind.Action"/>.</summary>
    public PermissionAction? Action { get; }

    /// <summary>Per-subtool action map when <see cref="Kind"/> is <see cref="PermissionActionRuleKind.SubToolMap"/>.</summary>
    public IDictionary<string, PermissionAction>? SubToolMap { get; }

    /// <summary>Builds a flat-action rule.</summary>
    public static PermissionActionRule FromAction(PermissionAction action) => new(action);

    /// <summary>Builds a per-subtool map rule.</summary>
    public static PermissionActionRule FromMap(IDictionary<string, PermissionAction> map) => new(map);
}

/// <summary>Discriminator for <see cref="PermissionActionRule"/> payloads.</summary>
public enum PermissionActionRuleKind
{
    Action,
    SubToolMap,
}
