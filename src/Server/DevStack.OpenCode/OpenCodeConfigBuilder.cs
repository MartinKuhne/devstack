using DevStack.OpenCode.Models;

namespace DevStack.OpenCode;

/// <summary>
/// Fluent builder for <see cref="OpenCodeConfig"/>. Each method returns a new
/// builder instance (records are immutable), making the API safe to share
/// across threads.
/// </summary>
public sealed record OpenCodeConfigBuilder
{
    private OpenCodeConfig _config;

    private OpenCodeConfigBuilder(OpenCodeConfig config)
    {
        _config = config;
    }

    /// <summary>Creates an empty builder.</summary>
    public static OpenCodeConfigBuilder Create() => new(new OpenCodeConfig());

    /// <summary>Creates a builder seeded with the given config.</summary>
    public static OpenCodeConfigBuilder From(OpenCodeConfig config) =>
        new(config ?? throw new ArgumentNullException(nameof(config)));

    /// <summary>Builds the current <see cref="OpenCodeConfig"/>.</summary>
    public OpenCodeConfig Build() => _config;

    /// <summary>Sets the default shell.</summary>
    public OpenCodeConfigBuilder WithShell(string? shell) =>
        this with { _config = _config with { Shell = shell } };

    /// <summary>Sets the log level.</summary>
    public OpenCodeConfigBuilder WithLogLevel(Models.LogLevel? level) =>
        this with { _config = _config with { LogLevel = level } };

    /// <summary>Sets the default model.</summary>
    public OpenCodeConfigBuilder WithDefaultModel(string? model) =>
        this with { _config = _config with { Model = model } };

    /// <summary>Sets the small model for tasks like title generation.</summary>
    public OpenCodeConfigBuilder WithSmallModel(string? model) =>
        this with { _config = _config with { SmallModel = model } };

    /// <summary>Sets the default agent.</summary>
    public OpenCodeConfigBuilder WithDefaultAgent(string? agent) =>
        this with { _config = _config with { DefaultAgent = agent } };

    /// <summary>Sets the sharing behavior.</summary>
    public OpenCodeConfigBuilder WithShare(ShareMode? share) =>
        this with { _config = _config with { Share = share } };

    /// <summary>Sets the auto-update behavior.</summary>
    public OpenCodeConfigBuilder WithAutoUpdate(AutoUpdateConfig? autoupdate) =>
        this with { _config = _config with { Autoupdate = autoupdate } };

    /// <summary>Sets the top-level permission rules.</summary>
    public OpenCodeConfigBuilder WithPermission(PermissionConfig? permission) =>
        this with { _config = _config with { Permission = permission } };

    /// <summary>Replaces the server config.</summary>
    public OpenCodeConfigBuilder WithServer(ServerConfig? server) =>
        this with { _config = _config with { Server = server } };

    /// <summary>Replaces the agent config map.</summary>
    public OpenCodeConfigBuilder WithAgentMap(AgentConfigMap? agent) =>
        this with { _config = _config with { Agent = agent } };

    /// <summary>Replaces the providers dictionary.</summary>
    public OpenCodeConfigBuilder WithProviders(IDictionary<string, ProviderConfig>? providers) =>
        this with { _config = _config with { Providers = providers } };

    /// <summary>Replaces the MCP server dictionary.</summary>
    public OpenCodeConfigBuilder WithMcpServers(IDictionary<string, McpServerConfig>? mcp) =>
        this with { _config = _config with { Mcp = mcp } };

    /// <summary>Replaces the experimental flags block.</summary>
    public OpenCodeConfigBuilder WithExperimental(ExperimentalConfig? experimental) =>
        this with { _config = _config with { Experimental = experimental } };

    /// <summary>Replaces the compaction config.</summary>
    public OpenCodeConfigBuilder WithCompaction(CompactionConfig? compaction) =>
        this with { _config = _config with { Compaction = compaction } };
}
