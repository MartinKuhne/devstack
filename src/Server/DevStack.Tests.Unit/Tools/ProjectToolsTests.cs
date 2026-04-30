using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    [Fact]
    public async Task GetProjects_ReturnsAllProjects()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext);

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
        var tools = new ProjectTools(logger, dbContext);

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
        var tools = new ProjectTools(logger, dbContext);

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
        var tools = new ProjectTools(logger, dbContext);

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
        var tools = new ProjectTools(logger, dbContext);
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
        var tools = new ProjectTools(logger, dbContext);
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
        var tools = new ProjectTools(logger, dbContext);

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
        var tools = new ProjectTools(logger, dbContext);
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
}
