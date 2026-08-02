namespace DevStack.OpenCode.Models;

/// <summary>
/// LSP server configuration. May be a boolean toggle, <c>null</c> (use built-ins),
/// or a per-language override map.
/// </summary>
[JsonConverter(typeof(LspConfigConverter))]
public sealed record LspConfig
{
    private LspConfig(LspConfigKind kind, object? payload)
    {
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Discriminator describing how <see cref="Payload"/> should be interpreted.</summary>
    public LspConfigKind Kind { get; }

    /// <summary>Underlying value.</summary>
    public object? Payload { get; }

    /// <summary>Builds a boolean toggle.</summary>
    public static LspConfig FromBool(bool enabled) => new(LspConfigKind.Bool, enabled);

    /// <summary>Builds a per-language override map.</summary>
    public static LspConfig FromMap(IDictionary<string, LspServerConfig> map) => new(LspConfigKind.Map, map);

    /// <summary>Returns the boolean value when <see cref="Kind"/> is <see cref="LspConfigKind.Bool"/>.</summary>
    public bool? Enabled => Kind == LspConfigKind.Bool ? (bool)Payload! : null;

    /// <summary>Returns the per-language map when <see cref="Kind"/> is <see cref="LspConfigKind.Map"/>.</summary>
    public IDictionary<string, LspServerConfig>? Map =>
        Kind == LspConfigKind.Map ? (IDictionary<string, LspServerConfig>)Payload! : null;
}

/// <summary>Discriminator for <see cref="LspConfig"/> payloads.</summary>
public enum LspConfigKind
{
    Bool,
    Map,
}

/// <summary>Per-language LSP server configuration.</summary>
public sealed record LspServerConfig
{
    /// <summary>Command and arguments to invoke the LSP server.</summary>
    [JsonPropertyName("command")]
    public IReadOnlyList<string>? Command { get; init; }

    /// <summary>File extensions this LSP server should handle.</summary>
    [JsonPropertyName("extensions")]
    public IReadOnlyList<string>? Extensions { get; init; }

    /// <summary>Disable this LSP server.</summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; init; }

    /// <summary>Environment variables for the LSP server process.</summary>
    [JsonPropertyName("env")]
    public IDictionary<string, string>? Env { get; init; }

    /// <summary>Initialization options sent to the LSP server.</summary>
    [JsonPropertyName("initialization")]
    public IDictionary<string, JsonElement>? Initialization { get; init; }
}
