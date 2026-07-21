using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevStack.Application;
using DevStack.Application.Projects.Commands;
using DevStack.Domain.Entities;
using DevStack.Mcp.Tools;
using DevStack.Persistence;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ModelContextProtocol;

using NSubstitute;

using Xunit;

namespace DevStack.Tests.Unit.Tools;

public class ProjectToolsTests
{
    private readonly ICommandHandler<Guid, CreateProjectCommand> _createProjectHandler;

    public ProjectToolsTests()
    {
        _createProjectHandler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
    }

    private static DbContextOptions<DevStackDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<DevStackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static DevStackDbContext CreateDbContext()
    {
        var options = CreateOptions();
        var context = new DevStackDbContext(options);
        context.Projects.AddRange(
            new Project(Guid.NewGuid(), "Project Alpha", "https://github.com/example/alpha"),
            new Project(Guid.NewGuid(), "Project Beta", "https://github.com/example/beta"),
            new Project(Guid.NewGuid(), "Project Gamma", "https://github.com/example/gamma"));
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetProjects_ReturnsAllProjects()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act
        var result = await tools.GetProjects();

        // Assert
        result.Should().Contain("## Projects");
        result.Should().Contain("```json");
        result.Should().Contain("Project Alpha");
        result.Should().Contain("Project Beta");
        result.Should().Contain("Project Gamma");

        var jsonStart = result.IndexOf("[");
        var jsonEnd = result.LastIndexOf("]");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var projects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        projects.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task GetProjects_ReturnsProjectIds()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act
        var result = await tools.GetProjects();

        // Assert
        var projects = dbContext.Projects.ToList();
        foreach (var project in projects)
        {
            result.Should().Contain(project.Id.ToString());
        }
    }

    [Fact]
    public async Task GetProjects_ReturnsRepositories()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act
        var result = await tools.GetProjects();

        // Assert
        var projects = dbContext.Projects.ToList();
        foreach (var project in projects)
        {
            result.Should().Contain(project.Repository);
        }
    }

    [Fact]
    public async Task GetProjects_WithEmptyDatabase_ReturnsEmptyArray()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var options = CreateOptions();
        var dbContext = new DevStackDbContext(options);
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act
        var result = await tools.GetProjects();

        // Assert
        result.Should().Contain("## Projects");
        var jsonStart = result.IndexOf("[");
        var jsonEnd = result.LastIndexOf("]");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var projects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        projects.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetProjectById_WithValidId_ReturnsProject()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);
        var targetProject = dbContext.Projects.First();

        // Act
        var result = await tools.GetProjectById(targetProject.Id);

        // Assert
        result.Should().Contain("## Project");
        result.Should().Contain(targetProject.Name);
        result.Should().Contain(targetProject.Repository);
        result.Should().Contain(targetProject.Id.ToString());
    }

    [Fact]
    public async Task GetProjectById_WithNotFoundId_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.GetProjectById(nonExistentId));

        exception.Message.Should().BeEquivalentTo($"Project with ID {nonExistentId} not found");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task GetProjectById_WithNullId_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.GetProjectById(null));

        exception.Message.Should().Be("Project ID is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task GetProjectById_ReturnsCorrectStructure()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);
        var targetProject = dbContext.Projects.First();

        // Act
        var result = await tools.GetProjectById(targetProject.Id);

        // Assert
        result.Should().Contain("```json");
        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        json.TryGetProperty("Id", out var idProp).Should().BeTrue();
        idProp.GetString().Should().Be(targetProject.Id.ToString());
        json.TryGetProperty("Name", out var nameProp).Should().BeTrue();
        nameProp.GetString().Should().Be(targetProject.Name);
        json.TryGetProperty("Repository", out var repoProp).Should().BeTrue();
        repoProp.GetString().Should().Be(targetProject.Repository);
    }

    [Fact]
    public async Task CreateProject_WithValidData_CreatesProject()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);
        var newId = Guid.NewGuid();
        var name = "New Project";
        var repository = "https://github.com/example/new";

        _createProjectHandler.Handle(Arg.Any<CreateProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await tools.CreateProject(name, null, repository);

        // Assert
        result.Should().Contain("Project Created");
        result.Should().Contain(newId.ToString());
        result.Should().Contain(name);
        result.Should().Contain(repository);

        await _createProjectHandler.Received(1).Handle(
            Arg.Is<CreateProjectCommand>(cmd =>
                cmd.Name == name &&
                cmd.Repository == repository),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProject_WithNullName_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.CreateProject(null!, null, null));

        exception.Message.Should().Be("Project name is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateProject_WithEmptyName_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.CreateProject("", null, null));

        exception.Message.Should().Be("Project name is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateProject_WithWhitespaceName_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.CreateProject("   ", null, null));

        exception.Message.Should().Be("Project name is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateProject_WithAllFields_CreatesProjectWithAllFields()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext, _createProjectHandler);
        var newId = Guid.NewGuid();
        var name = "New Project";
        var description = "Project description";
        var repository = "https://github.com/example/new";

        _createProjectHandler.Handle(Arg.Any<CreateProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await tools.CreateProject(name, description, repository);

        // Assert
        result.Should().Contain("Project Created");
        await _createProjectHandler.Received(1).Handle(
            Arg.Is<CreateProjectCommand>(cmd =>
                cmd.Name == name &&
                cmd.Description == description &&
                cmd.Repository == repository),
            Arg.Any<CancellationToken>());
    }
}
