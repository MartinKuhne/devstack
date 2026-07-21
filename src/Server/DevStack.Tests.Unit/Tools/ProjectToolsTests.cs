using System;
using System.Linq;
using System.Text.Json;
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
            new Project { Id = Guid.NewGuid(), Name = "Project Alpha", Repository = "https://github.com/example/alpha" },
            new Project { Id = Guid.NewGuid(), Name = "Project Beta", Repository = "https://github.com/example/beta" },
            new Project { Id = Guid.NewGuid(), Name = "Project Gamma", Repository = "https://github.com/example/gamma" });
        context.SaveChanges();
        return context;
    }

    private static ProjectTools CreateTools(DevStackDbContext? dbContext = null)
    {
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        return new ProjectTools(logger, dbContext ?? CreateDbContext(), handler);
    }

    [Fact]
    public async Task GetProjects_ReturnsAllProjects()
    {
        // Arrange
        var tools = CreateTools();

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
        var projects = JsonSerializer.Deserialize<JsonElement>(jsonStr);
        projects.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task GetProjects_ReturnsProjectIds()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var tools = CreateTools(dbContext);

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
        var dbContext = CreateDbContext();
        var tools = CreateTools(dbContext);

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
        var options = CreateOptions();
        var dbContext = new DevStackDbContext(options);
        var tools = CreateTools(dbContext);

        // Act
        var result = await tools.GetProjects();

        // Assert
        result.Should().Contain("## Projects");
        var jsonStart = result.IndexOf("[");
        var jsonEnd = result.LastIndexOf("]");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var projects = JsonSerializer.Deserialize<JsonElement>(jsonStr);
        projects.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetProjectById_WithValidId_ReturnsProject()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var tools = CreateTools(dbContext);
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
        var tools = CreateTools();
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
        var tools = CreateTools();

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
        var dbContext = CreateDbContext();
        var tools = CreateTools(dbContext);
        var targetProject = dbContext.Projects.First();

        // Act
        var result = await tools.GetProjectById(targetProject.Id);

        // Assert
        result.Should().Contain("```json");
        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = JsonSerializer.Deserialize<JsonElement>(jsonStr);
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
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);
        var newId = Guid.NewGuid();
        var name = "New Project";
        var description = "A new project";
        var repository = "https://github.com/example/new";

        handler.Handle(Arg.Any<CreateProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await tools.CreateProject(name, repository, description);

        // Assert
        result.Should().Contain("Project Created");
        result.Should().Contain(newId.ToString());
        result.Should().Contain(name);
        result.Should().Contain(description);
        result.Should().Contain(repository);

        await handler.Received(1).Handle(
            Arg.Is<CreateProjectCommand>(cmd =>
                cmd.Name == name &&
                cmd.Description == description &&
                cmd.Repository == repository),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProject_WithNullName_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);

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
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);

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
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);

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
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);
        var newId = Guid.NewGuid();
        var name = "New Project";
        var description = "Project description";
        var repository = "https://github.com/example/new";

        handler.Handle(Arg.Any<CreateProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await tools.CreateProject(name, repository, description);

        // Assert
        result.Should().Contain("Project Created");
        await handler.Received(1).Handle(
            Arg.Is<CreateProjectCommand>(cmd =>
                cmd.Name == name &&
                cmd.Description == description &&
                cmd.Repository == repository),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProject_WithOnlyNameAndRepository_CreatesProjectWithDefaults()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);
        var newId = Guid.NewGuid();
        var name = "Minimal Project";
        var repository = "https://github.com/example/minimal";

        handler.Handle(Arg.Any<CreateProjectCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await tools.CreateProject(name, repository, null);

        // Assert
        result.Should().Contain("Project Created");
        result.Should().Contain(newId.ToString());
        result.Should().Contain(name);

        await handler.Received(1).Handle(
            Arg.Is<CreateProjectCommand>(cmd =>
                cmd.Name == name &&
                cmd.Description == null &&
                cmd.Repository == repository),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProject_WithNullRepository_ThrowsMcpProtocolException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var handler = Substitute.For<ICommandHandler<Guid, CreateProjectCommand>>();
        var tools = new ProjectTools(logger, dbContext, handler);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => tools.CreateProject("Project", null!, null));

        exception.Message.Should().Be("Repository is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }
}
