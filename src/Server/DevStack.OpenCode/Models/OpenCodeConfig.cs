namespace DevStack.OpenCode.Models;

/// <summary>
/// Root OpenCode configuration object. Mirrors the JSON schema at
/// <c>https://opencode.ai/config.json</c>.
/// </summary>
public sealed record OpenCodeConfig
{
    /// <summary>JSON schema reference for configuration validation.</summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>Default shell to use for terminal and bash tool.</summary>
    [JsonPropertyName("shell")]
    public string? Shell { get; init; }

    /// <summary>Log level.</summary>
    [JsonPropertyName("logLevel")]
    public LogLevel? LogLevel { get; init; }

    /// <summary>Server configuration for <c>opencode serve</c> and <c>web</c> commands.</summary>
    [JsonPropertyName("server")]
    public ServerConfig? Server { get; init; }

    /// <summary>Command configuration. See <c>https://opencode.ai/docs/commands</c>.</summary>
    [JsonPropertyName("command")]
    public IDictionary<string, CommandConfig>? Commands { get; init; }

    /// <summary>Additional skill folder paths.</summary>
    [JsonPropertyName("skills")]
    public SkillsConfig? Skills { get; init; }

    /// <summary>Named git or local directory references.</summary>
    [JsonPropertyName("references")]
    public IDictionary<string, ReferenceConfig>? References { get; init; }

    /// <summary>Deprecated alias for <see cref="References"/>.</summary>
    [JsonPropertyName("reference")]
    public IDictionary<string, ReferenceConfig>? Reference { get; init; }

    /// <summary>File watcher ignore patterns.</summary>
    [JsonPropertyName("watcher")]
    public WatcherConfig? Watcher { get; init; }

    /// <summary>
    /// Enable or disable snapshot tracking. When false, filesystem snapshots are not
    /// recorded and undoing or reverting will not undo/redo file changes.
    /// </summary>
    [JsonPropertyName("snapshot")]
    public bool? Snapshot { get; init; }

    /// <summary>OpenCode plugins to load.</summary>
    [JsonPropertyName("plugin")]
    public IReadOnlyList<PluginConfig>? Plugin { get; init; }

    /// <summary>Sharing behavior for sessions.</summary>
    [JsonPropertyName("share")]
    public ShareMode? Share { get; init; }

    /// <summary>Deprecated alias for <see cref="Share"/>.</summary>
    [JsonPropertyName("autoshare")]
    public bool? Autoshare { get; init; }

    /// <summary>Auto-update behavior for the CLI.</summary>
    [JsonPropertyName("autoupdate")]
    public AutoUpdateConfig? Autoupdate { get; init; }

    /// <summary>Disable providers that are loaded automatically.</summary>
    [JsonPropertyName("disabled_providers")]
    public IReadOnlyList<string>? DisabledProviders { get; init; }

    /// <summary>When set, only these providers will be enabled.</summary>
    [JsonPropertyName("enabled_providers")]
    public IReadOnlyList<string>? EnabledProviders { get; init; }

    /// <summary>Model to use in the format of <c>provider/model</c>.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>Small model for tasks like title generation in the format of <c>provider/model</c>.</summary>
    [JsonPropertyName("small_model")]
    public string? SmallModel { get; init; }

    /// <summary>Default agent to use when none is specified. Falls back to <c>build</c>.</summary>
    [JsonPropertyName("default_agent")]
    public string? DefaultAgent { get; init; }

    /// <summary>Maximum subagent nesting depth. Defaults to 1.</summary>
    [JsonPropertyName("subagent_depth")]
    public int? SubagentDepth { get; init; }

    /// <summary>Custom username to display in conversations.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Deprecated alias for <see cref="Agent"/>.</summary>
    [JsonPropertyName("mode")]
    public IDictionary<string, AgentConfig>? Mode { get; init; }

    /// <summary>Agent configuration. See <c>https://opencode.ai/docs/agents</c>.</summary>
    [JsonPropertyName("agent")]
    public AgentConfigMap? Agent { get; init; }

    /// <summary>Custom provider configurations and model overrides.</summary>
    [JsonPropertyName("provider")]
    public IDictionary<string, ProviderConfig>? Providers { get; init; }

    /// <summary>MCP (Model Context Protocol) server configurations.</summary>
    [JsonPropertyName("mcp")]
    public IDictionary<string, McpServerConfig>? Mcp { get; init; }

    /// <summary>Enable or configure formatters.</summary>
    [JsonPropertyName("formatter")]
    public FormatterConfig? Formatter { get; init; }

    /// <summary>Enable or configure LSP servers.</summary>
    [JsonPropertyName("lsp")]
    public LspConfig? Lsp { get; init; }

    /// <summary>Additional instruction files or patterns to include.</summary>
    [JsonPropertyName("instructions")]
    public IReadOnlyList<string>? Instructions { get; init; }

    /// <summary>Deprecated. Always uses stretch layout.</summary>
    [JsonPropertyName("layout")]
    public LayoutMode? Layout { get; init; }

    /// <summary>Default permission rules for tool calls.</summary>
    [JsonPropertyName("permission")]
    public PermissionConfig? Permission { get; init; }

    /// <summary>Per-tool enable/disable overrides.</summary>
    [JsonPropertyName("tools")]
    public IDictionary<string, bool>? Tools { get; init; }

    /// <summary>Attachment processing configuration.</summary>
    [JsonPropertyName("attachment")]
    public AttachmentConfig? Attachment { get; init; }

    /// <summary>Enterprise configuration.</summary>
    [JsonPropertyName("enterprise")]
    public EnterpriseConfig? Enterprise { get; init; }

    /// <summary>Thresholds for truncating tool output.</summary>
    [JsonPropertyName("tool_output")]
    public ToolOutputConfig? ToolOutput { get; init; }

    /// <summary>Compaction behavior for long conversations.</summary>
    [JsonPropertyName("compaction")]
    public CompactionConfig? Compaction { get; init; }

    /// <summary>Experimental features toggle.</summary>
    [JsonPropertyName("experimental")]
    public ExperimentalConfig? Experimental { get; init; }
}
