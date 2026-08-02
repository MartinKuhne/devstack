namespace DevStack.OpenCode.Models;

/// <summary>
/// Formatter configuration. May be a boolean toggle, <c>null</c> (use built-ins),
/// or a per-formatter override map.
/// </summary>
[JsonConverter(typeof(FormatterConfigConverter))]
public sealed record FormatterConfig
{
    private FormatterConfig(FormatterConfigKind kind, object? payload)
    {
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Discriminator describing how <see cref="Payload"/> should be interpreted.</summary>
    public FormatterConfigKind Kind { get; }

    /// <summary>Underlying value.</summary>
    public object? Payload { get; }

    /// <summary>Builds a boolean toggle.</summary>
    public static FormatterConfig FromBool(bool enabled) => new(FormatterConfigKind.Bool, enabled);

    /// <summary>Builds a per-formatter override map.</summary>
    public static FormatterConfig FromMap(IDictionary<string, FormatterOverride> map) => new(FormatterConfigKind.Map, map);

    /// <summary>Returns the boolean value when <see cref="Kind"/> is <see cref="FormatterConfigKind.Bool"/>.</summary>
    public bool? Enabled => Kind == FormatterConfigKind.Bool ? (bool)Payload! : null;

    /// <summary>Returns the per-formatter map when <see cref="Kind"/> is <see cref="FormatterConfigKind.Map"/>.</summary>
    public IDictionary<string, FormatterOverride>? Map =>
        Kind == FormatterConfigKind.Map ? (IDictionary<string, FormatterOverride>)Payload! : null;
}

/// <summary>Discriminator for <see cref="FormatterConfig"/> payloads.</summary>
public enum FormatterConfigKind
{
    Bool,
    Map,
}

/// <summary>Override for a single formatter.</summary>
public sealed record FormatterOverride
{
    /// <summary>Disable this formatter.</summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; init; }

    /// <summary>Command and arguments to invoke the formatter.</summary>
    [JsonPropertyName("command")]
    public IReadOnlyList<string>? Command { get; init; }

    /// <summary>Environment variables for the formatter process.</summary>
    [JsonPropertyName("environment")]
    public IDictionary<string, string>? Environment { get; init; }

    /// <summary>File extensions this formatter should handle.</summary>
    [JsonPropertyName("extensions")]
    public IReadOnlyList<string>? Extensions { get; init; }
}
