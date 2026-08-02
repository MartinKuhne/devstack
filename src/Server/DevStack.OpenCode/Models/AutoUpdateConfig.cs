namespace DevStack.OpenCode.Models;

/// <summary>
/// Auto-update behavior. Encoded as either a boolean (true = auto-update,
/// false = disabled) or the string <c>"notify"</c>.
/// </summary>
[JsonConverter(typeof(AutoUpdateConfigConverter))]
public readonly record struct AutoUpdateConfig : IEquatable<AutoUpdateConfig>
{
    private AutoUpdateConfig(AutoUpdateMode mode)
    {
        Mode = mode;
    }

    /// <summary>The selected auto-update behavior.</summary>
    public AutoUpdateMode Mode { get; }

    /// <summary>Auto-update is enabled.</summary>
    public static AutoUpdateConfig Enabled() => new(AutoUpdateMode.Enabled);

    /// <summary>Auto-update is disabled.</summary>
    public static AutoUpdateConfig Disabled() => new(AutoUpdateMode.Disabled);

    /// <summary>Show update notifications but do not auto-update.</summary>
    public static AutoUpdateConfig Notify() => new(AutoUpdateMode.Notify);

    public bool Equals(AutoUpdateConfig other) => Mode == other.Mode;
    public override int GetHashCode() => (int)Mode;
    public override string ToString() => Mode.ToString();
}
