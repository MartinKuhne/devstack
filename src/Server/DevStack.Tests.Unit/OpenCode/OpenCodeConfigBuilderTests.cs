using DevStack.OpenCode;
using DevStack.OpenCode.Models;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class OpenCodeConfigBuilderTests
{
    [Fact]
    public void Create_ReturnsBuilderWithEmptyConfig()
    {
        var builder = OpenCodeConfigBuilder.Create();

        builder.Build().Should().NotBeNull();
    }

    [Fact]
    public void From_NullConfig_Throws()
    {
        var act = () => OpenCodeConfigBuilder.From(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithShell_SetsShell()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithShell("/bin/zsh")
            .Build();

        config.Shell.Should().Be("/bin/zsh");
    }

    [Fact]
    public void WithDefaultModel_SetsModel()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithDefaultModel("anthropic/claude-3-5-sonnet")
            .Build();

        config.Model.Should().Be("anthropic/claude-3-5-sonnet");
    }

    [Fact]
    public void WithSmallModel_SetsSmallModel()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithSmallModel("anthropic/claude-3-5-haiku")
            .Build();

        config.SmallModel.Should().Be("anthropic/claude-3-5-haiku");
    }

    [Fact]
    public void WithDefaultAgent_SetsDefaultAgent()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithDefaultAgent("build")
            .Build();

        config.DefaultAgent.Should().Be("build");
    }

    [Fact]
    public void WithShare_SetsShareMode()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithShare(ShareMode.Auto)
            .Build();

        config.Share.Should().Be(ShareMode.Auto);
    }

    [Fact]
    public void WithAutoUpdate_SetsAutoUpdate()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithAutoUpdate(AutoUpdateConfig.Notify())
            .Build();

        config.Autoupdate.Should().Be(AutoUpdateConfig.Notify());
    }

    [Fact]
    public void WithPermission_SetsPermission()
    {
        var permission = PermissionConfig.FromRules(new PermissionRuleConfig
        {
            Bash = PermissionActionRule.FromAction(PermissionAction.Deny),
        });

        var config = OpenCodeConfigBuilder.Create()
            .WithPermission(permission)
            .Build();

        config.Permission.Should().Be(permission);
    }

    [Fact]
    public void WithServer_SetsServer()
    {
        var server = new ServerConfig { Port = 4096 };

        var config = OpenCodeConfigBuilder.Create()
            .WithServer(server)
            .Build();

        config.Server.Should().Be(server);
    }

    [Fact]
    public void WithAgentMap_SetsAgentMap()
    {
        var agentMap = new AgentConfigMap { Build = new AgentConfig { Model = "anthropic/claude" } };

        var config = OpenCodeConfigBuilder.Create()
            .WithAgentMap(agentMap)
            .Build();

        config.Agent.Should().Be(agentMap);
    }

    [Fact]
    public void Builder_IsImmutable_OperationsReturnNewInstances()
    {
        var first = OpenCodeConfigBuilder.Create().WithShell("/bin/zsh");
        var second = first.WithShell("/bin/bash");

        first.Build().Shell.Should().Be("/bin/zsh");
        second.Build().Shell.Should().Be("/bin/bash");
    }

    [Fact]
    public void Builder_ChainsAllMethods()
    {
        var config = OpenCodeConfigBuilder.Create()
            .WithShell("/bin/zsh")
            .WithLogLevel(LogLevel.Debug)
            .WithDefaultModel("anthropic/claude-3-5-sonnet")
            .WithDefaultAgent("build")
            .WithShare(ShareMode.Manual)
            .WithAutoUpdate(AutoUpdateConfig.Enabled())
            .WithPermission(PermissionConfig.FromAction(PermissionAction.Allow))
            .Build();

        config.Shell.Should().Be("/bin/zsh");
        config.LogLevel.Should().Be(LogLevel.Debug);
        config.Model.Should().Be("anthropic/claude-3-5-sonnet");
        config.DefaultAgent.Should().Be("build");
        config.Share.Should().Be(ShareMode.Manual);
        config.Autoupdate.Should().Be(AutoUpdateConfig.Enabled());
        config.Permission!.Action.Should().Be(PermissionAction.Allow);
    }
}
