using DevStack.OpenCode.Models;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Serialization;

using Microsoft.Extensions.Options;

namespace DevStack.OpenCode.Client;

/// <summary>
/// Default <see cref="IOpenCodeClient"/> implementation. Wires up the
/// sub-clients and the section-management endpoints. Every operation is a
/// thin wrapper around an HTTP call to the configured OpenCode server, with
/// the request and response bodies serialized using <see cref="OpenCodeJson"/>.
/// </summary>
public sealed class OpenCodeClient : IOpenCodeClient
{
    private const string ConfigBasePath = "config";

    private readonly HttpClient _http;
    private readonly OpenCodeHttp _httpHelper;
    private readonly ILogger<OpenCodeClient> _logger;

    /// <summary>Config sub-client.</summary>
    public IConfigClient Config { get; }
    /// <summary>Session sub-client.</summary>
    public ISessionClient Session { get; }
    /// <summary>Project sub-client.</summary>
    public IProjectClient Project { get; }
    /// <summary>PTY sub-client.</summary>
    public IPtyClient Pty { get; }
    /// <summary>Instance sub-client.</summary>
    public IInstanceClient Instance { get; }
    /// <summary>Path sub-client.</summary>
    public IPathClient Path { get; }
    /// <summary>VCS sub-client.</summary>
    public IVcsClient Vcs { get; }
    /// <summary>Global sub-client.</summary>
    public IGlobalClient Global { get; }
    /// <summary>Tool sub-client.</summary>
    public IToolClient Tool { get; }
    /// <summary>Command sub-client.</summary>
    public ICommandClient Command { get; }
    /// <summary>Provider sub-client.</summary>
    public IProviderClient Provider { get; }
    /// <summary>Find sub-client.</summary>
    public IFindClient Find { get; }
    /// <summary>File sub-client.</summary>
    public IFileClient File { get; }
    /// <summary>App sub-client.</summary>
    public IAppClient App { get; }
    /// <summary>MCP sub-client.</summary>
    public IMcpClient Mcp { get; }
    /// <summary>LSP sub-client.</summary>
    public ILspClient Lsp { get; }
    /// <summary>Formatter sub-client.</summary>
    public IFormatterClient Formatter { get; }
    /// <summary>TUI sub-client.</summary>
    public ITuiClient Tui { get; }
    /// <summary>Auth sub-client.</summary>
    public IAuthClient Auth { get; }
    /// <summary>Event sub-client.</summary>
    public IEventClient Event { get; }

    /// <summary>
    /// Creates a new <see cref="OpenCodeClient"/>.
    /// </summary>
    public OpenCodeClient(
        HttpClient http,
        IOptions<OpenCodeOptions>? options = null,
        ILogger<OpenCodeClient>? logger = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? NullLogger<OpenCodeClient>.Instance;

        var opts = options?.Value ?? new OpenCodeOptions();
        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = opts.BaseUrl;
        }

        _http.Timeout = opts.HttpTimeout;
        if (!string.IsNullOrEmpty(opts.UserAgent) && _http.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _http.DefaultRequestHeaders.Add("User-Agent", opts.UserAgent);
        }

        _httpHelper = new OpenCodeHttp(_http, _logger);
        Config = new ConfigClient(_httpHelper);
        Session = new SessionClient(_httpHelper);
        Project = new ProjectClient(_httpHelper);
        Pty = new PtyClient(_httpHelper);
        Instance = new InstanceClient(_httpHelper);
        Path = new PathClient(_httpHelper);
        Vcs = new VcsClient(_httpHelper);
        Global = new GlobalClient(_httpHelper);
        Tool = new ToolClient(_httpHelper);
        Command = new CommandClient(_httpHelper);
        Provider = new ProviderClient(_httpHelper);
        Find = new FindClient(_httpHelper);
        File = new FileClient(_httpHelper);
        App = new AppClient(_httpHelper);
        Mcp = new McpClient(_httpHelper);
        Lsp = new LspClient(_httpHelper);
        Formatter = new FormatterClient(_httpHelper);
        Tui = new TuiClient(_httpHelper);
        Auth = new AuthClient(_httpHelper);
        Event = new EventClient(_httpHelper);
    }

    /// <inheritdoc />
    public Uri BaseUrl => _http.BaseAddress ?? new Uri("https://opencode.ai/");

    /// <inheritdoc />
    public Uri SchemaUri
    {
        get
        {
            var baseUri = BaseUrl;
            var baseString = baseUri.ToString();
            if (!baseString.EndsWith('/'))
            {
                baseUri = new Uri(baseString + "/", UriKind.Absolute);
            }

            return new Uri(baseUri, "config.json");
        }
    }

    // ----- Schema & global -----

    /// <inheritdoc />
    public async Task<ServerHealth> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await _httpHelper.GetAsync<ServerHealth>("global/health", cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<OpenCodeSchemaDocument> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        var json = await GetSchemaJsonAsync(cancellationToken).ConfigureAwait(false);
        var document = JsonSerializer.Deserialize<OpenCodeSchemaDocument>(json, OpenCodeJson.Compact)
            ?? throw new InvalidOperationException(
                $"OpenCode schema endpoint returned an empty body: {SchemaUri}");

        _logger.LogDebug("Fetched OpenCode schema from {Uri}", SchemaUri);
        return document;
    }

    /// <inheritdoc />
    public Task<string> GetSchemaJsonAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetStringAsync(SchemaUri.ToString(), cancellationToken: cancellationToken);

    // ----- Section management (DevStack extensions) -----

    /// <inheritdoc />
    public Task<OpenCodeConfig> GetConfigAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetAsync<OpenCodeConfig>(ConfigBasePath, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<OpenCodeConfig> ReplaceConfigAsync(OpenCodeConfig config, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<OpenCodeConfig, OpenCodeConfig>(ConfigBasePath, config, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<OpenCodeConfig> PatchConfigAsync(JsonDocument patch, CancellationToken cancellationToken = default) =>
        _httpHelper.PatchAsync<JsonDocument, OpenCodeConfig>(ConfigBasePath, patch, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ServerConfig?> GetServerAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<ServerConfig>($"{ConfigBasePath}/server", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ServerConfig> UpdateServerAsync(ServerConfig server, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<ServerConfig, ServerConfig>($"{ConfigBasePath}/server", server, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearServerAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/server", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<SkillsConfig?> GetSkillsAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<SkillsConfig>($"{ConfigBasePath}/skills", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<SkillsConfig> UpdateSkillsAsync(SkillsConfig skills, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<SkillsConfig, SkillsConfig>($"{ConfigBasePath}/skills", skills, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearSkillsAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/skills", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<WatcherConfig?> GetWatcherAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<WatcherConfig>($"{ConfigBasePath}/watcher", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<WatcherConfig> UpdateWatcherAsync(WatcherConfig watcher, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<WatcherConfig, WatcherConfig>($"{ConfigBasePath}/watcher", watcher, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearWatcherAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/watcher", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<FormatterConfig?> GetFormatterAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<FormatterConfig>($"{ConfigBasePath}/formatter", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<FormatterConfig> UpdateFormatterAsync(FormatterConfig formatter, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<FormatterConfig, FormatterConfig>($"{ConfigBasePath}/formatter", formatter, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearFormatterAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/formatter", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<LspConfig?> GetLspAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<LspConfig>($"{ConfigBasePath}/lsp", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<LspConfig> UpdateLspAsync(LspConfig lsp, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<LspConfig, LspConfig>($"{ConfigBasePath}/lsp", lsp, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearLspAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/lsp", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<PermissionConfig?> GetPermissionAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<PermissionConfig>($"{ConfigBasePath}/permission", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<PermissionConfig> UpdatePermissionAsync(PermissionConfig permission, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<PermissionConfig, PermissionConfig>($"{ConfigBasePath}/permission", permission, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearPermissionAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/permission", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AttachmentConfig?> GetAttachmentAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<AttachmentConfig>($"{ConfigBasePath}/attachment", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AttachmentConfig> UpdateAttachmentAsync(AttachmentConfig attachment, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<AttachmentConfig, AttachmentConfig>($"{ConfigBasePath}/attachment", attachment, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearAttachmentAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/attachment", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<EnterpriseConfig?> GetEnterpriseAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<EnterpriseConfig>($"{ConfigBasePath}/enterprise", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<EnterpriseConfig> UpdateEnterpriseAsync(EnterpriseConfig enterprise, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<EnterpriseConfig, EnterpriseConfig>($"{ConfigBasePath}/enterprise", enterprise, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearEnterpriseAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/enterprise", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ToolOutputConfig?> GetToolOutputAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<ToolOutputConfig>($"{ConfigBasePath}/tool_output", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ToolOutputConfig> UpdateToolOutputAsync(ToolOutputConfig toolOutput, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<ToolOutputConfig, ToolOutputConfig>($"{ConfigBasePath}/tool_output", toolOutput, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearToolOutputAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/tool_output", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<CompactionConfig?> GetCompactionAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<CompactionConfig>($"{ConfigBasePath}/compaction", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<CompactionConfig> UpdateCompactionAsync(CompactionConfig compaction, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<CompactionConfig, CompactionConfig>($"{ConfigBasePath}/compaction", compaction, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearCompactionAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/compaction", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ExperimentalConfig?> GetExperimentalAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.GetNullableAsync<ExperimentalConfig>($"{ConfigBasePath}/experimental", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ExperimentalConfig> UpdateExperimentalAsync(ExperimentalConfig experimental, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<ExperimentalConfig, ExperimentalConfig>($"{ConfigBasePath}/experimental", experimental, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task ClearExperimentalAsync(CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/experimental", cancellationToken: cancellationToken);

    // ----- Named sub-resources -----

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListAgentsAsync(CancellationToken cancellationToken = default) =>
        await GetMapKeysAsync($"{ConfigBasePath}/agent", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<AgentConfig?> GetAgentAsync(string name, CancellationToken cancellationToken = default) =>
        GetNamedNullableAsync<AgentConfig>("agent", name, cancellationToken);

    /// <inheritdoc />
    public Task<AgentConfig> UpsertAgentAsync(string name, AgentConfig agent, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<AgentConfig, AgentConfig>(NamedPath("agent", name), agent, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteAgentAsync(string name, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync(NamedPath("agent", name), cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListProvidersAsync(CancellationToken cancellationToken = default) =>
        await GetMapKeysAsync($"{ConfigBasePath}/provider", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<ProviderConfig?> GetProviderAsync(string id, CancellationToken cancellationToken = default) =>
        GetNamedNullableAsync<ProviderConfig>("provider", id, cancellationToken);

    /// <inheritdoc />
    public Task<ProviderConfig> UpsertProviderAsync(string id, ProviderConfig provider, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<ProviderConfig, ProviderConfig>(NamedPath("provider", id), provider, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteProviderAsync(string id, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync(NamedPath("provider", id), cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListMcpServersAsync(CancellationToken cancellationToken = default) =>
        await GetMapKeysAsync($"{ConfigBasePath}/mcp", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<McpServerConfig?> GetMcpServerAsync(string name, CancellationToken cancellationToken = default) =>
        GetNamedNullableAsync<McpServerConfig>("mcp", name, cancellationToken);

    /// <inheritdoc />
    public Task<McpServerConfig> UpsertMcpServerAsync(string name, McpServerConfig server, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<McpServerConfig, McpServerConfig>(NamedPath("mcp", name), server, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteMcpServerAsync(string name, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync(NamedPath("mcp", name), cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListReferencesAsync(CancellationToken cancellationToken = default) =>
        await GetMapKeysAsync($"{ConfigBasePath}/references", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<ReferenceConfig?> GetReferenceAsync(string name, CancellationToken cancellationToken = default) =>
        GetNamedNullableAsync<ReferenceConfig>("references", name, cancellationToken);

    /// <inheritdoc />
    public Task<ReferenceConfig> UpsertReferenceAsync(string name, ReferenceConfig reference, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<ReferenceConfig, ReferenceConfig>(NamedPath("references", name), reference, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteReferenceAsync(string name, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync(NamedPath("references", name), cancellationToken: cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListCommandsAsync(CancellationToken cancellationToken = default) =>
        await GetMapKeysAsync($"{ConfigBasePath}/command", cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<CommandConfig?> GetCommandAsync(string name, CancellationToken cancellationToken = default) =>
        GetNamedNullableAsync<CommandConfig>("command", name, cancellationToken);

    /// <inheritdoc />
    public Task<CommandConfig> UpsertCommandAsync(string name, CommandConfig command, CancellationToken cancellationToken = default) =>
        _httpHelper.PutAsync<CommandConfig, CommandConfig>(NamedPath("command", name), command, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> DeleteCommandAsync(string name, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync(NamedPath("command", name), cancellationToken: cancellationToken);

    // ----- Plugin -----

    /// <inheritdoc />
    public async Task<IReadOnlyList<PluginConfig>> ListPluginsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _httpHelper.GetAsync<List<PluginConfig>>($"{ConfigBasePath}/plugin", cancellationToken: cancellationToken).ConfigureAwait(false);
        return (IReadOnlyList<PluginConfig>)(list ?? new List<PluginConfig>());
    }

    /// <inheritdoc />
    public Task AddPluginAsync(PluginConfig plugin, CancellationToken cancellationToken = default) =>
        _httpHelper.PostNoContentAsync($"{ConfigBasePath}/plugin", plugin, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemovePluginAsync(int index, CancellationToken cancellationToken = default) =>
        _httpHelper.DeleteAsync($"{ConfigBasePath}/plugin/{index}", cancellationToken: cancellationToken);

    // ----- Private helpers -----

    private static string NamedPath(string section, string name) =>
        $"{ConfigBasePath}/{section}/{Uri.EscapeDataString(name)}";

    private async Task<IReadOnlyList<string>> GetMapKeysAsync(string path, CancellationToken cancellationToken)
    {
        var map = await _httpHelper.GetAsync<Dictionary<string, JsonElement>>(path, cancellationToken: cancellationToken).ConfigureAwait(false);
        return map is null ? Array.Empty<string>() : map.Keys.ToArray();
    }

    private async Task<T?> GetNamedNullableAsync<T>(string section, string name, CancellationToken cancellationToken)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be a non-empty value.", nameof(name));
        }

        return await _httpHelper.GetNullableAsync<T>(NamedPath(section, name), cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
