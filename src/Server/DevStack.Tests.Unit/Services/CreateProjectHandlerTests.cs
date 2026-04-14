using DevStack.Infrastructure.Projects;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class CreateProjectHandlerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_Throws_WhenNameIsEmpty(string? name)
    {
        var command = new CreateProjectCommand(name!, null, null, null, null);
        var handler = new CreateProjectHandler(null!);

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_Throws_WhenNameExceeds200Characters()
    {
        var longName = new string('a', 201);
        var command = new CreateProjectCommand(longName, null, null, null, null);
        var handler = new CreateProjectHandler(null!);

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Name must be 200 characters or less*");
    }

    [Theory]
    [InlineData("not-a-uri")]
    [InlineData("/relative/path")]
    public async Task Handle_Throws_WhenGithubUrlIsInvalid(string invalidUrl)
    {
        var command = new CreateProjectCommand("Valid Name", null, null, null, invalidUrl);
        var handler = new CreateProjectHandler(null!);

        var action = () => handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*GitHub URL is not a valid URI*");
    }

    [Theory]
    [InlineData("https://github.com/user/repo")]
    [InlineData("https://github.com/org/project")]
    public void CreateProjectCommand_StoresValuesCorrectly(string githubUrl)
    {
        var command = new CreateProjectCommand("My Project", "Description", "Clean", "4GB", githubUrl);

        command.Name.Should().Be("My Project");
        command.Description.Should().Be("Description");
        command.Architecture.Should().Be("Clean");
        command.Memory.Should().Be("4GB");
        command.GithubUrl.Should().Be(githubUrl);
    }
}