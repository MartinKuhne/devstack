using DevStack.Mcp;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit;

public class PromptsTests
{
    [Fact]
    public void Greeting_ReturnsGreetingMessage()
    {
        // Act
        var result = Prompts.Greeting();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Hello");
        result.Should().Contain("DevStack");
    }

    [Fact]
    public void Greeting_ReturnsConsistentMessage()
    {
        // Act
        var result1 = Prompts.Greeting();
        var result2 = Prompts.Greeting();

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void Help_ReturnsHelpMessage()
    {
        // Act
        var result = Prompts.Help();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Available commands");
    }

    [Fact]
    public void Help_ReturnsConsistentMessage()
    {
        // Act
        var result1 = Prompts.Help();
        var result2 = Prompts.Help();

        // Assert
        result1.Should().Be(result2);
    }
}
