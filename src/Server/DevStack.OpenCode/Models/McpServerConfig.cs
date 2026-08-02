namespace DevStack.OpenCode.Models;

/// <summary>
/// Discriminated union for an MCP server entry. May be a local process,
/// a remote HTTP server, or a simple enable/disable toggle.
/// </summary>
[JsonConverter(typeof(McpServerConfigConverter))]
public sealed record McpServerConfig
{
    private McpServerConfig(McpServerKind kind, object payload)
    {
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Discriminator describing how <see cref="Payload"/> should be interpreted.</summary>
    public McpServerKind Kind { get; }

    /// <summary>Underlying value (one of the MCP server config record types, or a <see cref="McpEnableToggle"/>).</summary>
    public object Payload { get; }

    /// <summary>Builds a local-process MCP server entry.</summary>
    public static McpServerConfig FromLocal(McpLocalConfig local) => new(McpServerKind.Local, local);

    /// <summary>Builds a remote HTTP MCP server entry.</summary>
    public static McpServerConfig FromRemote(McpRemoteConfig remote) => new(McpServerKind.Remote, remote);

    /// <summary>Builds a simple enable/disable toggle (no further configuration).</summary>
    public static McpServerConfig FromToggle(McpEnableToggle toggle) => new(McpServerKind.Toggle, toggle);

    /// <summary>Returns the local config when <see cref="Kind"/> is <see cref="McpServerKind.Local"/>.</summary>
    public McpLocalConfig? Local => Kind == McpServerKind.Local ? (McpLocalConfig)Payload : null;

    /// <summary>Returns the remote config when <see cref="Kind"/> is <see cref="McpServerKind.Remote"/>.</summary>
    public McpRemoteConfig? Remote => Kind == McpServerKind.Remote ? (McpRemoteConfig)Payload : null;

    /// <summary>Returns the toggle when <see cref="Kind"/> is <see cref="McpServerKind.Toggle"/>.</summary>
    public McpEnableToggle? Toggle => Kind == McpServerKind.Toggle ? (McpEnableToggle)Payload : null;
}

/// <summary>Discriminator for <see cref="McpServerConfig"/> payloads.</summary>
public enum McpServerKind
{
    Local,
    Remote,
    Toggle,
}
