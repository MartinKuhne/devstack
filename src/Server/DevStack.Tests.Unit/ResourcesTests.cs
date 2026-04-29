using DevStack.Mcp;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit;

public class ResourcesTests
{
    [Fact]
    public void ServerInfo_ReturnsServerInfo()
    {
        // Act
        var result = ResourceType.ServerInfo();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("DevStack MCP Server");
    }

    [Fact]
    public void ServerInfo_ReturnsVersion()
    {
        // Act
        var result = ResourceType.ServerInfo();

        // Assert
        result.Should().Contain("v1.0.0.0");
    }

    [Fact]
    public void ServerInfo_ReturnsConsistentMessage()
    {
        // Act
        var result1 = ResourceType.ServerInfo();
        var result2 = ResourceType.ServerInfo();

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void ServerInfo_ReturnsDescriptiveMessage()
    {
        // Act
        var result = ResourceType.ServerInfo();

        // Assert
        result.Should().Contain("DevStack");
        result.Should().Contain("features");
    }
}
