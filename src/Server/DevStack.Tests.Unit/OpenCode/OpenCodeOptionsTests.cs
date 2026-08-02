using DevStack.OpenCode.Options;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class OpenCodeOptionsTests
{
    [Fact]
    public void Defaults_BaseUrl_IsOpencodeAi()
    {
        var options = new OpenCodeOptions();

        options.BaseUrl.Should().Be(new Uri("https://opencode.ai/"));
    }

    [Fact]
    public void Defaults_SchemaPath_IsConfigJson()
    {
        var options = new OpenCodeOptions();

        options.SchemaPath.Should().Be("config.json");
    }

    [Fact]
    public void Defaults_HttpTimeout_IsThirtySeconds()
    {
        var options = new OpenCodeOptions();

        options.HttpTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Defaults_UserAgent_IsDevStack()
    {
        var options = new OpenCodeOptions();

        options.UserAgent.Should().StartWith("DevStack.OpenCode/");
    }

    [Fact]
    public void Defaults_DefaultConfigPath_IsNull()
    {
        var options = new OpenCodeOptions();

        options.DefaultConfigPath.Should().BeNull();
    }

    [Fact]
    public void ResolveSchemaUri_CombinesBaseAndPath()
    {
        var options = new OpenCodeOptions
        {
            BaseUrl = new Uri("https://example.test/api/"),
            SchemaPath = "v1/config.json",
        };

        options.ResolveSchemaUri().Should().Be(new Uri("https://example.test/api/v1/config.json"));
    }

    [Fact]
    public void ResolveSchemaUri_RespectsTrailingSlash()
    {
        var options = new OpenCodeOptions
        {
            BaseUrl = new Uri("https://example.test/api"),
            SchemaPath = "v1/config.json",
        };

        options.ResolveSchemaUri().Should().Be(new Uri("https://example.test/api/v1/config.json"));
    }
}
