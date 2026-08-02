using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Client;

/// <summary>
/// Root OpenCode SDK client. Exposes the same sub-clients as the
/// <a href="https://opencode.ai/docs/sdk/">JavaScript SDK</a>, organized as
/// <see cref="IConfigClient"/>, <see cref="ISessionClient"/>, <see cref="IProjectClient"/>,
/// and so on. Schema fetching and the config section management operations
/// live on the root client.
/// </summary>
public interface IOpenCodeClient
{
    // ----- Schema & global state -----

    /// <summary>Absolute URL the client is currently configured to call for the schema endpoint.</summary>
    Uri SchemaUri { get; }

    /// <summary>Base URL the client is currently configured to call.</summary>
    Uri BaseUrl { get; }

    /// <summary>Fetches the raw JSON schema document.</summary>
    Task<OpenCodeSchemaDocument> GetSchemaAsync(CancellationToken cancellationToken = default);

    /// <summary>Fetches the raw JSON schema document as text.</summary>
    Task<string> GetSchemaJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>Server health check (<c>GET /global/health</c>).</summary>
    Task<ServerHealth> GetHealthAsync(CancellationToken cancellationToken = default);

    // ----- Section management (DevStack extensions) -----

    /// <summary>Returns the full effective configuration (<c>GET /config</c>).</summary>
    Task<OpenCodeConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces the full configuration (<c>PUT /config</c>).</summary>
    Task<OpenCodeConfig> ReplaceConfigAsync(OpenCodeConfig config, CancellationToken cancellationToken = default);

    /// <summary>Merges a JSON Patch document into the configuration (<c>PATCH /config</c>).</summary>
    Task<OpenCodeConfig> PatchConfigAsync(JsonDocument patch, CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>server</c> section.</summary>
    Task<ServerConfig?> GetServerAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>server</c> section.</summary>
    Task<ServerConfig> UpdateServerAsync(ServerConfig server, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>server</c> section.</summary>
    Task ClearServerAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>skills</c> section.</summary>
    Task<SkillsConfig?> GetSkillsAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>skills</c> section.</summary>
    Task<SkillsConfig> UpdateSkillsAsync(SkillsConfig skills, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>skills</c> section.</summary>
    Task ClearSkillsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>watcher</c> section.</summary>
    Task<WatcherConfig?> GetWatcherAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>watcher</c> section.</summary>
    Task<WatcherConfig> UpdateWatcherAsync(WatcherConfig watcher, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>watcher</c> section.</summary>
    Task ClearWatcherAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>formatter</c> section.</summary>
    Task<FormatterConfig?> GetFormatterAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>formatter</c> section.</summary>
    Task<FormatterConfig> UpdateFormatterAsync(FormatterConfig formatter, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>formatter</c> section.</summary>
    Task ClearFormatterAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>lsp</c> section.</summary>
    Task<LspConfig?> GetLspAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>lsp</c> section.</summary>
    Task<LspConfig> UpdateLspAsync(LspConfig lsp, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>lsp</c> section.</summary>
    Task ClearLspAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>permission</c> section.</summary>
    Task<PermissionConfig?> GetPermissionAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>permission</c> section.</summary>
    Task<PermissionConfig> UpdatePermissionAsync(PermissionConfig permission, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>permission</c> section.</summary>
    Task ClearPermissionAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>attachment</c> section.</summary>
    Task<AttachmentConfig?> GetAttachmentAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>attachment</c> section.</summary>
    Task<AttachmentConfig> UpdateAttachmentAsync(AttachmentConfig attachment, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>attachment</c> section.</summary>
    Task ClearAttachmentAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>enterprise</c> section.</summary>
    Task<EnterpriseConfig?> GetEnterpriseAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>enterprise</c> section.</summary>
    Task<EnterpriseConfig> UpdateEnterpriseAsync(EnterpriseConfig enterprise, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>enterprise</c> section.</summary>
    Task ClearEnterpriseAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>tool_output</c> section.</summary>
    Task<ToolOutputConfig?> GetToolOutputAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>tool_output</c> section.</summary>
    Task<ToolOutputConfig> UpdateToolOutputAsync(ToolOutputConfig toolOutput, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>tool_output</c> section.</summary>
    Task ClearToolOutputAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>compaction</c> section.</summary>
    Task<CompactionConfig?> GetCompactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>compaction</c> section.</summary>
    Task<CompactionConfig> UpdateCompactionAsync(CompactionConfig compaction, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>compaction</c> section.</summary>
    Task ClearCompactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the <c>experimental</c> section.</summary>
    Task<ExperimentalConfig?> GetExperimentalAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the <c>experimental</c> section.</summary>
    Task<ExperimentalConfig> UpdateExperimentalAsync(ExperimentalConfig experimental, CancellationToken cancellationToken = default);

    /// <summary>Clears the <c>experimental</c> section.</summary>
    Task ClearExperimentalAsync(CancellationToken cancellationToken = default);

    // ----- Named sub-resources (List / Get / Upsert / Delete) -----

    /// <summary>Lists the names of all configured agents (<c>GET /config/agent</c>).</summary>
    Task<IReadOnlyList<string>> ListAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single agent configuration (<c>GET /config/agent/{name}</c>).</summary>
    Task<AgentConfig?> GetAgentAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates an agent (<c>PUT /config/agent/{name}</c>).</summary>
    Task<AgentConfig> UpsertAgentAsync(string name, AgentConfig agent, CancellationToken cancellationToken = default);

    /// <summary>Deletes an agent (<c>DELETE /config/agent/{name}</c>).</summary>
    Task<bool> DeleteAgentAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Lists the IDs of all configured providers (<c>GET /config/provider</c>).</summary>
    Task<IReadOnlyList<string>> ListProvidersAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single provider configuration (<c>GET /config/provider/{id}</c>).</summary>
    Task<ProviderConfig?> GetProviderAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a provider (<c>PUT /config/provider/{id}</c>).</summary>
    Task<ProviderConfig> UpsertProviderAsync(string id, ProviderConfig provider, CancellationToken cancellationToken = default);

    /// <summary>Deletes a provider (<c>DELETE /config/provider/{id}</c>).</summary>
    Task<bool> DeleteProviderAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Lists the names of all configured MCP servers (<c>GET /config/mcp</c>).</summary>
    Task<IReadOnlyList<string>> ListMcpServersAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single MCP server configuration (<c>GET /config/mcp/{name}</c>).</summary>
    Task<McpServerConfig?> GetMcpServerAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates an MCP server (<c>PUT /config/mcp/{name}</c>).</summary>
    Task<McpServerConfig> UpsertMcpServerAsync(string name, McpServerConfig server, CancellationToken cancellationToken = default);

    /// <summary>Deletes an MCP server (<c>DELETE /config/mcp/{name}</c>).</summary>
    Task<bool> DeleteMcpServerAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Lists the names of all configured references (<c>GET /config/references</c>).</summary>
    Task<IReadOnlyList<string>> ListReferencesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single reference (<c>GET /config/references/{name}</c>).</summary>
    Task<ReferenceConfig?> GetReferenceAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a reference (<c>PUT /config/references/{name}</c>).</summary>
    Task<ReferenceConfig> UpsertReferenceAsync(string name, ReferenceConfig reference, CancellationToken cancellationToken = default);

    /// <summary>Deletes a reference (<c>DELETE /config/references/{name}</c>).</summary>
    Task<bool> DeleteReferenceAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Lists the names of all configured commands (<c>GET /config/command</c>).</summary>
    Task<IReadOnlyList<string>> ListCommandsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single command (<c>GET /config/command/{name}</c>).</summary>
    Task<CommandConfig?> GetCommandAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a command (<c>PUT /config/command/{name}</c>).</summary>
    Task<CommandConfig> UpsertCommandAsync(string name, CommandConfig command, CancellationToken cancellationToken = default);

    /// <summary>Deletes a command (<c>DELETE /config/command/{name}</c>).</summary>
    Task<bool> DeleteCommandAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Lists all configured plugins (<c>GET /config/plugin</c>).</summary>
    Task<IReadOnlyList<PluginConfig>> ListPluginsAsync(CancellationToken cancellationToken = default);

    /// <summary>Appends a plugin to the list (<c>POST /config/plugin</c>).</summary>
    Task AddPluginAsync(PluginConfig plugin, CancellationToken cancellationToken = default);

    /// <summary>Removes the plugin at the given index (<c>DELETE /config/plugin/{index}</c>).</summary>
    Task<bool> RemovePluginAsync(int index, CancellationToken cancellationToken = default);

    // ----- JS SDK namespace sub-clients -----

    /// <summary>Config operations (<c>client.config.*</c>).</summary>
    IConfigClient Config { get; }

    /// <summary>Session operations (<c>client.session.*</c>).</summary>
    ISessionClient Session { get; }

    /// <summary>Project operations (<c>client.project.*</c>).</summary>
    IProjectClient Project { get; }

    /// <summary>PTY session operations (<c>client.pty.*</c>).</summary>
    IPtyClient Pty { get; }

    /// <summary>Instance operations (<c>client.instance.*</c>).</summary>
    IInstanceClient Instance { get; }

    /// <summary>Path operations (<c>client.path.*</c>).</summary>
    IPathClient Path { get; }

    /// <summary>VCS operations (<c>client.vcs.*</c>).</summary>
    IVcsClient Vcs { get; }

    /// <summary>Global server-sent events stream (<c>client.global.*</c>).</summary>
    IGlobalClient Global { get; }

    /// <summary>Tool operations (<c>client.tool.*</c>).</summary>
    IToolClient Tool { get; }

    /// <summary>Command operations (<c>client.command.*</c>).</summary>
    ICommandClient Command { get; }

    /// <summary>Provider operations (<c>client.provider.*</c>).</summary>
    IProviderClient Provider { get; }

    /// <summary>Find operations (<c>client.find.*</c>).</summary>
    IFindClient Find { get; }

    /// <summary>File operations (<c>client.file.*</c>).</summary>
    IFileClient File { get; }

    /// <summary>App-level operations (<c>client.app.*</c>).</summary>
    IAppClient App { get; }

    /// <summary>MCP server operations (<c>client.mcp.*</c>).</summary>
    IMcpClient Mcp { get; }

    /// <summary>LSP operations (<c>client.lsp.*</c>).</summary>
    ILspClient Lsp { get; }

    /// <summary>Formatter operations (<c>client.formatter.*</c>).</summary>
    IFormatterClient Formatter { get; }

    /// <summary>TUI operations (<c>client.tui.*</c>).</summary>
    ITuiClient Tui { get; }

    /// <summary>Auth operations (<c>client.auth.*</c>).</summary>
    IAuthClient Auth { get; }

    /// <summary>Event subscription (<c>client.event.*</c>).</summary>
    IEventClient Event { get; }
}
