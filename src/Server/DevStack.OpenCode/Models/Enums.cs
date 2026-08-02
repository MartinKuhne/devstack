namespace DevStack.OpenCode.Models;

/// <summary>Log levels supported by OpenCode.</summary>
public enum LogLevel
{
    [JsonStringEnumMemberName("DEBUG")] Debug,
    [JsonStringEnumMemberName("INFO")] Info,
    [JsonStringEnumMemberName("WARN")] Warn,
    [JsonStringEnumMemberName("ERROR")] Error,
}

/// <summary>Sharing behavior for sessions in OpenCode.</summary>
public enum ShareMode
{
    [JsonStringEnumMemberName("manual")] Manual,
    [JsonStringEnumMemberName("auto")] Auto,
    [JsonStringEnumMemberName("disabled")] Disabled,
}

/// <summary>Agent execution mode in OpenCode.</summary>
public enum AgentMode
{
    [JsonStringEnumMemberName("subagent")] Subagent,
    [JsonStringEnumMemberName("primary")] Primary,
    [JsonStringEnumMemberName("all")] All,
}

/// <summary>Lifecycle status of a model in OpenCode.</summary>
public enum ModelStatus
{
    [JsonStringEnumMemberName("alpha")] Alpha,
    [JsonStringEnumMemberName("beta")] Beta,
    [JsonStringEnumMemberName("deprecated")] Deprecated,
    [JsonStringEnumMemberName("active")] Active,
}

/// <summary>Auto-update behavior for the OpenCode CLI.</summary>
public enum AutoUpdateMode
{
    Disabled,
    Enabled,
    Notify,
}

/// <summary>Permission action applied to a tool call in OpenCode.</summary>
public enum PermissionAction
{
    [JsonStringEnumMemberName("ask")] Ask,
    [JsonStringEnumMemberName("allow")] Allow,
    [JsonStringEnumMemberName("deny")] Deny,
}

/// <summary>Editor layout preference in OpenCode.</summary>
public enum LayoutMode
{
    [JsonStringEnumMemberName("auto")] Auto,
    [JsonStringEnumMemberName("stretch")] Stretch,
}

/// <summary>Type of MCP server connection in OpenCode.</summary>
public enum McpServerType
{
    [JsonStringEnumMemberName("local")] Local,
    [JsonStringEnumMemberName("remote")] Remote,
}

/// <summary>Source type of a named reference in OpenCode.</summary>
public enum ReferenceKind
{
    Git,
    Local,
    String,
}
