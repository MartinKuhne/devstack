using System.Text;
using System.Text.Json;

using DevStack.OpenCode.Models;
using DevStack.OpenCode.Serialization;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class OpenCodeConfigSerializationTests
{
    [Fact]
    public void Deserialize_EmptyObject_ReturnsEmptyConfig()
    {
        var json = "{}";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize_AllTopLevelProperties_PopulatesAllFields()
    {
        var json = """
        {
          "$schema": "https://opencode.ai/config.json",
          "shell": "/bin/zsh",
          "logLevel": "DEBUG",
          "model": "anthropic/claude-3-5-sonnet",
          "small_model": "anthropic/claude-3-5-haiku",
          "default_agent": "build",
          "username": "tester",
          "share": "manual",
          "subagent_depth": 2
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config.Should().NotBeNull();
        config!.Schema.Should().Be("https://opencode.ai/config.json");
        config.Shell.Should().Be("/bin/zsh");
        config.LogLevel.Should().Be(LogLevel.Debug);
        config.Model.Should().Be("anthropic/claude-3-5-sonnet");
        config.SmallModel.Should().Be("anthropic/claude-3-5-haiku");
        config.DefaultAgent.Should().Be("build");
        config.Username.Should().Be("tester");
        config.Share.Should().Be(ShareMode.Manual);
        config.SubagentDepth.Should().Be(2);
    }

    [Fact]
    public void Deserialize_AutoUpdateAsBoolean_TrueMapsToEnabled()
    {
        var json = """{ "autoupdate": true }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Autoupdate.Should().Be(AutoUpdateConfig.Enabled());
    }

    [Fact]
    public void Deserialize_AutoUpdateAsBoolean_FalseMapsToDisabled()
    {
        var json = """{ "autoupdate": false }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Autoupdate.Should().Be(AutoUpdateConfig.Disabled());
    }

    [Fact]
    public void Deserialize_AutoUpdateAsString_NotifyMapsToNotify()
    {
        var json = """{ "autoupdate": "notify" }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Autoupdate.Should().Be(AutoUpdateConfig.Notify());
    }

    [Fact]
    public void Serialize_AutoUpdateEnabled_OutputsBooleanTrue()
    {
        var config = new OpenCodeConfig { Autoupdate = AutoUpdateConfig.Enabled() };

        var json = JsonSerializer.Serialize(config, OpenCodeJson.Compact);

        json.Should().Contain("\"autoupdate\":true");
    }

    [Fact]
    public void Serialize_AutoUpdateNotify_OutputsStringNotify()
    {
        var config = new OpenCodeConfig { Autoupdate = AutoUpdateConfig.Notify() };

        var json = JsonSerializer.Serialize(config, OpenCodeJson.Compact);

        json.Should().Contain("\"autoupdate\":\"notify\"");
    }

    [Fact]
    public void Deserialize_ServerConfig_PopulatesServer()
    {
        var json = """
        {
          "server": {
            "port": 4096,
            "hostname": "127.0.0.1",
            "mdns": true,
            "mdnsDomain": "opencode.local",
            "cors": ["https://example.com"]
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Server.Should().NotBeNull();
        config.Server!.Port.Should().Be(4096);
        config.Server.Hostname.Should().Be("127.0.0.1");
        config.Server.Mdns.Should().BeTrue();
        config.Server.MdnsDomain.Should().Be("opencode.local");
        config.Server.Cors.Should().BeEquivalentTo(new[] { "https://example.com" });
    }

    [Fact]
    public void Deserialize_ProviderConfig_PopulatesProviderAndModels()
    {
        var json = """
        {
          "provider": {
            "anthropic": {
              "api": "anthropic",
              "name": "Anthropic",
              "env": ["ANTHROPIC_API_KEY"],
              "models": {
                "claude-3-5-sonnet": {
                  "name": "Claude 3.5 Sonnet",
                  "family": "claude",
                  "tool_call": true,
                  "cost": { "input": 3.0, "output": 15.0 },
                  "limit": { "context": 200000, "output": 8192 },
                  "modalities": { "input": ["text", "image"], "output": ["text"] }
                }
              }
            }
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Providers.Should().NotBeNull();
        var anthropic = config.Providers!["anthropic"];
        anthropic.Api.Should().Be("anthropic");
        anthropic.Name.Should().Be("Anthropic");
        anthropic.Env.Should().BeEquivalentTo(new[] { "ANTHROPIC_API_KEY" });

        var sonnet = anthropic.Models!["claude-3-5-sonnet"];
        sonnet.Name.Should().Be("Claude 3.5 Sonnet");
        sonnet.Family.Should().Be("claude");
        sonnet.ToolCall.Should().BeTrue();
        sonnet.Cost!.Input.Should().Be(3.0);
        sonnet.Cost.Output.Should().Be(15.0);
        sonnet.Limit!.Context.Should().Be(200000);
        sonnet.Limit.Output.Should().Be(8192);
        sonnet.Modalities!.Input.Should().BeEquivalentTo(new[] { Modality.Text, Modality.Image });
        sonnet.Modalities.Output.Should().BeEquivalentTo(new[] { Modality.Text });
    }

    [Fact]
    public void Deserialize_ProviderTimeoutAsInteger_PopulatesMilliseconds()
    {
        var json = """
        { "provider": { "anthropic": { "options": { "timeout": 60000 } } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Providers!["anthropic"].Options!.Timeout.Should().Be(TimeoutValue.FromMilliseconds(60000));
    }

    [Fact]
    public void Deserialize_ProviderTimeoutAsFalse_PopulatesDisabled()
    {
        var json = """
        { "provider": { "anthropic": { "options": { "timeout": false } } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Providers!["anthropic"].Options!.Timeout.Should().Be(TimeoutValue.Disable());
    }

    [Fact]
    public void Serialize_DisabledTimeout_WritesFalse()
    {
        var provider = new ProviderConfig { Options = new ProviderOptions { Timeout = TimeoutValue.Disable() } };

        var json = JsonSerializer.Serialize(provider, OpenCodeJson.Compact);

        json.Should().Contain("\"timeout\":false");
    }

    [Fact]
    public void Serialize_IntegerTimeout_WritesNumber()
    {
        var provider = new ProviderConfig { Options = new ProviderOptions { Timeout = TimeoutValue.FromMilliseconds(5000) } };

        var json = JsonSerializer.Serialize(provider, OpenCodeJson.Compact);

        json.Should().Contain("\"timeout\":5000");
    }

    [Fact]
    public void Deserialize_PermissionFlat_Deny_BuildsActionKind()
    {
        var json = """{ "permission": "deny" }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Permission!.Kind.Should().Be(PermissionKind.Action);
        config.Permission.Action.Should().Be(PermissionAction.Deny);
    }

    [Fact]
    public void Deserialize_PermissionMap_BuildsMapKind()
    {
        var json = """
        { "permission": { "bash": "deny", "read": "allow" } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Permission!.Kind.Should().Be(PermissionKind.Map);
        config.Permission.Map.Should().BeEquivalentTo(new Dictionary<string, PermissionAction>
        {
            ["bash"] = PermissionAction.Deny,
            ["read"] = PermissionAction.Allow,
        });
    }

    [Fact]
    public void Deserialize_PermissionRules_BuildsRulesKind()
    {
        var json = """
        {
          "permission": {
            "read": "allow",
            "edit": { "external": "deny", "internal": "allow" },
            "bash": "deny"
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Permission!.Kind.Should().Be(PermissionKind.Rules);
        config.Permission.Rules!.Bash!.Action.Should().Be(PermissionAction.Deny);
        config.Permission.Rules.Read!.Action.Should().Be(PermissionAction.Allow);
        config.Permission.Rules.Edit!.SubToolMap.Should()
            .BeEquivalentTo(new Dictionary<string, PermissionAction>
            {
                ["external"] = PermissionAction.Deny,
                ["internal"] = PermissionAction.Allow,
            });
    }

    [Fact]
    public void Serialize_PermissionRules_RoundTripsToSameShape()
    {
        // When a Rules object contains only flat-action values, it serializes
        // identically to a Map. The deserialized shape is therefore a Map —
        // both are semantically equivalent for this case.
        var original = PermissionConfig.FromRules(new PermissionRuleConfig
        {
            Bash = PermissionActionRule.FromAction(PermissionAction.Deny),
            Read = PermissionActionRule.FromAction(PermissionAction.Allow),
        });

        var json = JsonSerializer.Serialize(original, OpenCodeJson.Compact);
        var roundTrip = JsonSerializer.Deserialize<PermissionConfig>(json, OpenCodeJson.Compact);

        roundTrip!.Map.Should().NotBeNull();
        roundTrip.Map.Should().Contain(new KeyValuePair<string, PermissionAction>("bash", PermissionAction.Deny));
        roundTrip.Map.Should().Contain(new KeyValuePair<string, PermissionAction>("read", PermissionAction.Allow));
    }

    [Fact]
    public void Serialize_PermissionRules_WithNestedObject_RoundTripsAsRules()
    {
        // When a Rules object contains a sub-tool map (a nested object value),
        // the deserialized shape must be Rules because the nested structure
        // cannot be represented in the flat Map form.
        var original = PermissionConfig.FromRules(new PermissionRuleConfig
        {
            Bash = PermissionActionRule.FromAction(PermissionAction.Deny),
            Edit = PermissionActionRule.FromMap(new Dictionary<string, PermissionAction>
            {
                ["external"] = PermissionAction.Deny,
                ["internal"] = PermissionAction.Allow,
            }),
        });

        var json = JsonSerializer.Serialize(original, OpenCodeJson.Compact);
        var roundTrip = JsonSerializer.Deserialize<PermissionConfig>(json, OpenCodeJson.Compact);

        roundTrip!.Kind.Should().Be(PermissionKind.Rules);
        roundTrip.Rules!.Bash!.Action.Should().Be(PermissionAction.Deny);
        roundTrip.Rules.Edit!.SubToolMap.Should()
            .BeEquivalentTo(new Dictionary<string, PermissionAction>
            {
                ["external"] = PermissionAction.Deny,
                ["internal"] = PermissionAction.Allow,
            });
    }

    [Fact]
    public void Deserialize_McpLocalConfig_BuildsLocalKind()
    {
        var json = """
        {
          "mcp": {
            "devstack": {
              "type": "local",
              "command": ["dotnet", "run", "--project", "src/Server/DevStack.Mcp"],
              "cwd": ".",
              "environment": { "ASPNETCORE_ENVIRONMENT": "Development" },
              "enabled": true,
              "timeout": 10000
            }
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var mcp = config!.Mcp!["devstack"];
        mcp.Kind.Should().Be(McpServerKind.Local);
        mcp.Local!.Command.Should().BeEquivalentTo(new[] { "dotnet", "run", "--project", "src/Server/DevStack.Mcp" });
        mcp.Local.Cwd.Should().Be(".");
        mcp.Local.Environment!["ASPNETCORE_ENVIRONMENT"].Should().Be("Development");
        mcp.Local.Enabled.Should().BeTrue();
        mcp.Local.Timeout.Should().Be(10000);
    }

    [Fact]
    public void Deserialize_McpRemoteConfig_BuildsRemoteKind()
    {
        var json = """
        {
          "mcp": {
            "remote-mcp": {
              "type": "remote",
              "url": "https://mcp.example.com",
              "headers": { "X-Api-Key": "test" },
              "oauth": false
            }
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var mcp = config!.Mcp!["remote-mcp"];
        mcp.Kind.Should().Be(McpServerKind.Remote);
        mcp.Remote!.Url.Should().Be("https://mcp.example.com");
        mcp.Remote.Headers!["X-Api-Key"].Should().Be("test");
        mcp.Remote.OAuth!.Value.Disabled.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_McpRemoteConfig_WithOAuthObject_BuildsOAuthConfig()
    {
        var json = """
        {
          "mcp": {
            "remote-mcp": {
              "type": "remote",
              "url": "https://mcp.example.com",
              "oauth": { "clientId": "abc", "scope": "openid" }
            }
          }
        }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var oauth = config!.Mcp!["remote-mcp"].Remote!.OAuth!.Value;
        oauth.Disabled.Should().BeFalse();
        oauth.Config!.ClientId.Should().Be("abc");
        oauth.Config.Scope.Should().Be("openid");
    }

    [Fact]
    public void Deserialize_McpToggleOnly_BuildsToggleKind()
    {
        var json = """{ "mcp": { "minimal": { "enabled": false } } }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var mcp = config!.Mcp!["minimal"];
        mcp.Kind.Should().Be(McpServerKind.Toggle);
        mcp.Toggle!.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_ReferenceAsString_BuildsStringKind()
    {
        var json = """{ "references": { "anthropic-docs": "https://github.com/example/repo" } }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var reference = config!.References!["anthropic-docs"];
        reference.Kind.Should().Be(ReferenceKind.String);
        reference.Shorthand.Should().Be("https://github.com/example/repo");
    }

    [Fact]
    public void Deserialize_ReferenceAsGit_BuildsGitKind()
    {
        var json = """
        { "references": { "anthropic-docs": { "repository": "https://github.com/example/repo", "branch": "main" } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var reference = config!.References!["anthropic-docs"];
        reference.Kind.Should().Be(ReferenceKind.Git);
        reference.Git!.Repository.Should().Be("https://github.com/example/repo");
        reference.Git.Branch.Should().Be("main");
    }

    [Fact]
    public void Deserialize_ReferenceAsLocal_BuildsLocalKind()
    {
        var json = """
        { "references": { "internal-docs": { "path": "./docs" } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var reference = config!.References!["internal-docs"];
        reference.Kind.Should().Be(ReferenceKind.Local);
        reference.Local!.Path.Should().Be("./docs");
    }

    [Fact]
    public void Deserialize_PluginAsString_BuildsNameOnly()
    {
        var json = """{ "plugin": ["my-plugin"] }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var plugin = config!.Plugin!.Single();
        plugin.Name.Should().Be("my-plugin");
        plugin.HasOptions.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_PluginAsTuple_BuildsNameWithOptions()
    {
        var json = """
        { "plugin": [["my-plugin", { "enabled": true, "timeout": 5000 }]] }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        var plugin = config!.Plugin!.Single();
        plugin.Name.Should().Be("my-plugin");
        plugin.HasOptions.Should().BeTrue();
        plugin.Options!["enabled"].GetBoolean().Should().BeTrue();
        plugin.Options["timeout"].GetInt32().Should().Be(5000);
    }

    [Fact]
    public void Deserialize_FormatterAsObject_BuildsMapKind()
    {
        var json = """
        { "formatter": { "prettier": { "disabled": false, "extensions": [".ts", ".tsx"] } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Formatter!.Kind.Should().Be(FormatterConfigKind.Map);
        var prettier = config.Formatter.Map!["prettier"];
        prettier.Disabled.Should().BeFalse();
        prettier.Extensions.Should().BeEquivalentTo(new[] { ".ts", ".tsx" });
    }

    [Fact]
    public void Deserialize_FormatterAsBooleanFalse_BuildsBoolKind()
    {
        var json = """{ "formatter": false }""";

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Formatter!.Kind.Should().Be(FormatterConfigKind.Bool);
        config.Formatter.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_LspAsObject_BuildsMapKind()
    {
        var json = """
        { "lsp": { "typescript-language-server": { "command": ["typescript-language-server", "--stdio"], "extensions": [".ts"] } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Lsp!.Kind.Should().Be(LspConfigKind.Map);
        var ts = config.Lsp.Map!["typescript-language-server"];
        ts.Command.Should().BeEquivalentTo(new[] { "typescript-language-server", "--stdio" });
        ts.Extensions.Should().BeEquivalentTo(new[] { ".ts" });
    }

    [Fact]
    public void Deserialize_PreservesAdditionalAgentNames()
    {
        var json = """
        { "agent": { "build": { "model": "anthropic/claude" }, "custom": { "prompt": "you are a tester" } } }
        """;

        var config = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        config!.Agent!.Build!.Model.Should().Be("anthropic/claude");
        config.Agent.AdditionalAgents!["custom"].GetProperty("prompt").GetString().Should().Be("you are a tester");
    }

    [Fact]
    public void RoundTrip_FullConfig_PreservesAllProperties()
    {
        var original = new OpenCodeConfig
        {
            Shell = "/bin/zsh",
            LogLevel = LogLevel.Debug,
            Model = "anthropic/claude-3-5-sonnet",
            Share = ShareMode.Auto,
            Autoupdate = AutoUpdateConfig.Notify(),
            Permission = PermissionConfig.FromAction(PermissionAction.Deny),
            Server = new ServerConfig { Port = 4096, Hostname = "127.0.0.1" },
        };

        var json = JsonSerializer.Serialize(original, OpenCodeJson.Compact);
        var roundTrip = JsonSerializer.Deserialize<OpenCodeConfig>(json, OpenCodeJson.Compact);

        roundTrip.Should().BeEquivalentTo(original, opts => opts.RespectingRuntimeTypes());
    }
}
