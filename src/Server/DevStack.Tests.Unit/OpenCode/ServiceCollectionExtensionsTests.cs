using DevStack.OpenCode.Client;
using DevStack.OpenCode.DependencyInjection;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Store;

using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenCodeSdk_RegistersClientAsHttpClient()
    {
        var services = new ServiceCollection();
        services.AddOpenCodeSdk();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetService<IOpenCodeClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void AddOpenCodeSdk_RegistersConfigStoreAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddOpenCodeSdk();

        using var provider = services.BuildServiceProvider();
        var store1 = provider.GetRequiredService<IOpenCodeConfigStore>();
        var store2 = provider.GetRequiredService<IOpenCodeConfigStore>();

        store1.Should().BeSameAs(store2);
    }

    [Fact]
    public void AddOpenCodeSdk_ConfiguresOptions()
    {
        var services = new ServiceCollection();
        services.AddOpenCodeSdk(o =>
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
    public void AddOpenCodeSdk_BindsFromConfiguration()
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
        services.AddOpenCodeSdk(config);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenCodeOptions>>().Value;

        options.BaseUrl.Should().Be(new Uri("https://config.test/"));
        options.SchemaPath.Should().Be("v2/config.json");
        options.UserAgent.Should().Be("ConfigAgent/2.0");
    }

    [Fact]
    public void AddOpenCodeSdk_NullServices_Throws()
    {
        var act = () => ServiceCollectionExtensions.AddOpenCodeSdk((IServiceCollection)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddOpenCodeSdk_NullConfiguration_Throws()
    {
        var services = new ServiceCollection();
        var act = () => services.AddOpenCodeSdk((IConfiguration)null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
