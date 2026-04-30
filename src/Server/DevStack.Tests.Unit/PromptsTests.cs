using DevStack.Mcp;

using FluentAssertions;

using Microsoft.Extensions.AI;

using Xunit;

namespace DevStack.Tests.Unit;

public class PromptsTests
{
    [Fact]
    public void Greeting_ReturnsGreetingMessage()
    {
        // Act
        var result = GreetingPrompt.Greeting();

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Hello");
        result.Text.Should().Contain("DevStack");
    }

    [Fact]
    public void Greeting_ReturnsConsistentMessage()
    {
        // Act
        var result1 = GreetingPrompt.Greeting();
        var result2 = GreetingPrompt.Greeting();

        // Assert
        result1.Text.Should().Be(result2.Text);
    }

    [Fact]
    public void Greeting_WithName_ReturnsPersonalizedMessage()
    {
        // Act
        var result = GreetingPrompt.Greeting(name: "Alice");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Alice");
        result.Text.Should().Contain("Hello");
    }

    [Fact]
    public void Greeting_WithWhitespaceName_ReturnsGenericGreeting()
    {
        // Act
        var result = GreetingPrompt.Greeting(name: "   ");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().NotContain("   ");
        result.Text.Should().Contain("Hello");
    }

    [Fact]
    public void Greeting_WithNullName_ReturnsGenericGreeting()
    {
        // Act
        var result = GreetingPrompt.Greeting(name: null!);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Hello");
    }

    [Fact]
    public void Help_ReturnsHelpMessage()
    {
        // Act
        var result = HelpPrompt.Help();

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Available commands");
    }

    [Fact]
    public void Help_ReturnsConsistentMessage()
    {
        // Act
        var result1 = HelpPrompt.Help();
        var result2 = HelpPrompt.Help();

        // Assert
        result1.Text.Should().Be(result2.Text);
    }

    [Fact]
    public void Help_WithCommand_ReturnsCommandHelp()
    {
        // Act
        var result = HelpPrompt.Help(command: "get_deliverable");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("get_deliverable");
    }

    [Fact]
    public void Help_WithWhitespaceCommand_ReturnsGenericHelp()
    {
        // Act
        var result = HelpPrompt.Help(command: "   ");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Available commands");
    }

    [Fact]
    public void Help_WithNullCommand_ReturnsGenericHelp()
    {
        // Act
        var result = HelpPrompt.Help(command: null!);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("Available commands");
    }

    [Fact]
    public void Help_WithGetTaskCommand_ReturnsGetTaskHelp()
    {
        // Act
        var result = HelpPrompt.Help(command: "get_task");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().Contain("get_task");
        result.Text.Should().Contain("Help for 'get_task'");
    }

    [Fact]
    public void Greeting_OutputFormat_IsValidChatMessage()
    {
        // Act
        var result = GreetingPrompt.Greeting(name: "Test");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().NotBeNullOrEmpty();
        result.Text.Should().StartWith("Hello, Test");
    }

    [Fact]
    public void Help_OutputFormat_IsValidChatMessage()
    {
        // Act
        var result = HelpPrompt.Help(command: "create_task");

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(ChatRole.User);
        result.Text.Should().NotBeNullOrEmpty();
        result.Text.Should().Contain("create_task");
    }
}
