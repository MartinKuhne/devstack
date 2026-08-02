using DevStack.OpenCode.Client;
using DevStack.OpenCode.DependencyInjection;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Store;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenCode_RegistersClient()
    {
        var services = new ServiceCollection();
        services.AddOpenCode();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetService<IOpenCodeClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenCode_RegistersConfigStoreAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddOpenCode();

        using var provider = services.BuildServiceProvider();
        var store1 = provider.GetRequiredService<IOpenCodeConfigStore>();
        var store2 = provider.GetRequiredService<IOpenCodeConfigStore>();

        store1.Should().BeSameAs(store2);
    }

    [Fact]
    public void AddOpenCode_ConfiguresOptions()
    {
        var services = new ServiceCollection();
        services.AddOpenCode(o =>
        {
            o.BaseUrl = new Uri("https://example.test/");
            o.UserAgent = "Test/1.0";
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        options.BaseUrl.Should().Be(new Uri("https://example.test/"));
        options.UserAgent.Should().Be("Test/1.0");
    }

    [Fact]
    public void AddOpenCode_BindsFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenCode:BaseUrl"] = "https://config.test/",
                ["OpenCode:SchemaPath"] = "v2/config.json",
                ["OpenCode:UserAgent"] = "ConfigAgent/2.0",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOpenCode(config);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        options.BaseUrl.Should().Be(new Uri("https://config.test/"));
        options.SchemaPath.Should().Be("v2/config.json");
        options.UserAgent.Should().Be("ConfigAgent/2.0");
    }

    [Fact]
    public void AddOpenCode_AppliesConfigureAfterBind()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenCode:BaseUrl"] = "https://config.test/",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOpenCode(config, o => o.UserAgent = "OverrideAgent/3.0");

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        options.BaseUrl.Should().Be(new Uri("https://config.test/"));
        options.UserAgent.Should().Be("OverrideAgent/3.0");
    }

    [Fact]
    public void AddOpenCode_ReturnsBuilderForChaining()
    {
        var services = new ServiceCollection();
        var builder = services.AddOpenCode();

        builder.Should().NotBeNull();
        builder.Services.Should().BeSameAs(services);
    }

    [Fact]
    public void OpenCodeBuilder_WithClient_ReplacesClientRegistration()
    {
        var services = new ServiceCollection();
        var fake = Substitute.For<IOpenCodeClient>();

        services.AddOpenCode().WithClient<FakeOpenCodeClient>();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IOpenCodeClient>();

        client.Should().BeOfType<FakeOpenCodeClient>();
    }

    [Fact]
    public void OpenCodeBuilder_WithConfigStore_ReplacesStoreRegistration()
    {
        var services = new ServiceCollection();
        services.AddOpenCode().WithConfigStore<FakeOpenCodeConfigStore>();

        using var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<IOpenCodeConfigStore>();

        store.Should().BeOfType<FakeOpenCodeConfigStore>();
    }

    [Fact]
    public void OpenCodeBuilder_ChainsAcrossCalls()
    {
        var services = new ServiceCollection();
        services
            .AddOpenCode(o => o.BaseUrl = new Uri("https://chained.test/"))
            .WithConfigStore<FakeOpenCodeConfigStore>()
            .WithClient<FakeOpenCodeClient>();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IOpenCodeClient>();
        var store = provider.GetRequiredService<IOpenCodeConfigStore>();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        client.Should().BeOfType<FakeOpenCodeClient>();
        store.Should().BeOfType<FakeOpenCodeConfigStore>();
        options.BaseUrl.Should().Be(new Uri("https://chained.test/"));
    }

    [Fact]
    public void AddOpenCode_NullServices_Throws()
    {
        var act = () => OpenCodeServiceCollectionExtensions.AddOpenCode((IServiceCollection)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOpenCode_NullConfiguration_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddOpenCode((IConfiguration)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOpenCode_NullConfigure_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddOpenCode((Action<OpenCodeOptions>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOpenCode_ValidatesOptions()
    {
        var services = new ServiceCollection();
        services.AddOpenCode(o => o.HttpTimeout = TimeSpan.Zero);

        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddOpenCodeOnHostBuilder_BindsFromHostConfiguration()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OpenCode:BaseUrl"] = "https://host.test/";
        builder.Configuration["OpenCode:UserAgent"] = "HostAgent/1.0";
        builder.AddOpenCode();

        using var host = builder.Build();
        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;
        var client = host.Services.GetRequiredService<IOpenCodeClient>();

        options.BaseUrl.Should().Be(new Uri("https://host.test/"));
        options.UserAgent.Should().Be("HostAgent/1.0");
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenCodeOnHostBuilder_AppliesConfigureOverride()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OpenCode:BaseUrl"] = "https://host.test/";
        builder.AddOpenCode(o => o.UserAgent = "HostOverride/2.0");

        using var host = builder.Build();
        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        options.BaseUrl.Should().Be(new Uri("https://host.test/"));
        options.UserAgent.Should().Be("HostOverride/2.0");
    }

    [Fact]
    public void AddOpenCodeOnHostBuilder_NullBuilder_Throws()
    {
        var act = () => OpenCodeHostBuilderExtensions.AddOpenCode((IHostApplicationBuilder)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOpenCode_ExposesAllSdkSubClients()
    {
        var services = new ServiceCollection();
        services.AddOpenCode();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IOpenCodeClient>();

        client.Config.Should().NotBeNull();
        client.Session.Should().NotBeNull();
        client.Project.Should().NotBeNull();
        client.Pty.Should().NotBeNull();
        client.Instance.Should().NotBeNull();
        client.Path.Should().NotBeNull();
        client.Vcs.Should().NotBeNull();
        client.Global.Should().NotBeNull();
        client.Tool.Should().NotBeNull();
        client.Command.Should().NotBeNull();
        client.Provider.Should().NotBeNull();
        client.Find.Should().NotBeNull();
        client.File.Should().NotBeNull();
        client.App.Should().NotBeNull();
        client.Mcp.Should().NotBeNull();
        client.Lsp.Should().NotBeNull();
        client.Formatter.Should().NotBeNull();
        client.Tui.Should().NotBeNull();
        client.Auth.Should().NotBeNull();
        client.Event.Should().NotBeNull();
    }

    private sealed class FakeOpenCodeClient : IOpenCodeClient
    {
        public Uri BaseUrl => new("https://fake.test/");
        public Uri SchemaUri => new("https://fake.test/config.json");

        public IConfigClient Config => null!;
        public ISessionClient Session => null!;
        public IProjectClient Project => null!;
        public IPtyClient Pty => null!;
        public IInstanceClient Instance => null!;
        public IPathClient Path => null!;
        public IVcsClient Vcs => null!;
        public IGlobalClient Global => null!;
        public IToolClient Tool => null!;
        public ICommandClient Command => null!;
        public IProviderClient Provider => null!;
        public IFindClient Find => null!;
        public IFileClient File => null!;
        public IAppClient App => null!;
        public IMcpClient Mcp => null!;
        public ILspClient Lsp => null!;
        public IFormatterClient Formatter => null!;
        public ITuiClient Tui => null!;
        public IAuthClient Auth => null!;
        public IEventClient Event => null!;

        public Task<DevStack.OpenCode.Models.OpenCodeSchemaDocument> GetSchemaAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<string> GetSchemaJsonAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ServerHealth> GetHealthAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.OpenCodeConfig> GetConfigAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.OpenCodeConfig> ReplaceConfigAsync(DevStack.OpenCode.Models.OpenCodeConfig config, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.OpenCodeConfig> PatchConfigAsync(System.Text.Json.JsonDocument patch, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ServerConfig?> GetServerAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ServerConfig> UpdateServerAsync(DevStack.OpenCode.Models.ServerConfig server, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearServerAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.SkillsConfig?> GetSkillsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.SkillsConfig> UpdateSkillsAsync(DevStack.OpenCode.Models.SkillsConfig skills, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearSkillsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.WatcherConfig?> GetWatcherAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.WatcherConfig> UpdateWatcherAsync(DevStack.OpenCode.Models.WatcherConfig watcher, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearWatcherAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.FormatterConfig?> GetFormatterAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.FormatterConfig> UpdateFormatterAsync(DevStack.OpenCode.Models.FormatterConfig formatter, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearFormatterAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.LspConfig?> GetLspAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.LspConfig> UpdateLspAsync(DevStack.OpenCode.Models.LspConfig lsp, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearLspAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.PermissionConfig?> GetPermissionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.PermissionConfig> UpdatePermissionAsync(DevStack.OpenCode.Models.PermissionConfig permission, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearPermissionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.AttachmentConfig?> GetAttachmentAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.AttachmentConfig> UpdateAttachmentAsync(DevStack.OpenCode.Models.AttachmentConfig attachment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearAttachmentAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.EnterpriseConfig?> GetEnterpriseAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.EnterpriseConfig> UpdateEnterpriseAsync(DevStack.OpenCode.Models.EnterpriseConfig enterprise, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearEnterpriseAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ToolOutputConfig?> GetToolOutputAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ToolOutputConfig> UpdateToolOutputAsync(DevStack.OpenCode.Models.ToolOutputConfig toolOutput, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearToolOutputAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.CompactionConfig?> GetCompactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.CompactionConfig> UpdateCompactionAsync(DevStack.OpenCode.Models.CompactionConfig compaction, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearCompactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ExperimentalConfig?> GetExperimentalAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ExperimentalConfig> UpdateExperimentalAsync(DevStack.OpenCode.Models.ExperimentalConfig experimental, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ClearExperimentalAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListAgentsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.AgentConfig?> GetAgentAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.AgentConfig> UpsertAgentAsync(string name, DevStack.OpenCode.Models.AgentConfig agent, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAgentAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListProvidersAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ProviderConfig?> GetProviderAsync(string id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ProviderConfig> UpsertProviderAsync(string id, DevStack.OpenCode.Models.ProviderConfig provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteProviderAsync(string id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListMcpServersAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.McpServerConfig?> GetMcpServerAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.McpServerConfig> UpsertMcpServerAsync(string name, DevStack.OpenCode.Models.McpServerConfig server, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteMcpServerAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListReferencesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ReferenceConfig?> GetReferenceAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.ReferenceConfig> UpsertReferenceAsync(string name, DevStack.OpenCode.Models.ReferenceConfig reference, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteReferenceAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListCommandsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.CommandConfig?> GetCommandAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<DevStack.OpenCode.Models.CommandConfig> UpsertCommandAsync(string name, DevStack.OpenCode.Models.CommandConfig command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteCommandAsync(string name, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DevStack.OpenCode.Models.PluginConfig>> ListPluginsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddPluginAsync(DevStack.OpenCode.Models.PluginConfig plugin, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> RemovePluginAsync(int index, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeOpenCodeConfigStore : IOpenCodeConfigStore
    {
        public Task<DevStack.OpenCode.Models.OpenCodeConfig> LoadAsync(string path, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<OpenCodeConfigLoadResult> LoadDefaultAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task SaveAsync(string path, DevStack.OpenCode.Models.OpenCodeConfig config, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
