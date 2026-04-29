using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DevStack.Domain.Entities;
using DevStack.Mcp.Tools;
using DevStack.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        Assert.Contains("## Projects", result);
        Assert.Contains("```json", result);
        Assert.Contains("Project Alpha", result);
        Assert.Contains("Project Beta", result);
        Assert.Contains("Project Gamma", result);

        var jsonStart = result.IndexOf("[");
        var jsonEnd = result.LastIndexOf("]");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var projects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        Assert.Equal(3, projects.GetArrayLength());
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
            Assert.Contains(project.Id.ToString(), result);
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
            Assert.Contains(project.Repository, result);
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
        Assert.Contains("## Projects", result);
        var jsonStart = result.IndexOf("[");
        var jsonEnd = result.LastIndexOf("]");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var projects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        Assert.Equal(0, projects.GetArrayLength());
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
        Assert.Contains("## Project", result);
        Assert.Contains(targetProject.Name, result);
        Assert.Contains(targetProject.Repository, result);
        Assert.Contains(targetProject.Id.ToString(), result);
    }

    [Fact]
    public async Task GetProjectById_WithNotFoundId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => tools.GetProjectById(nonExistentId));

        Assert.Equal($"Project with ID {nonExistentId} not found", exception.Message);
    }

    [Fact]
    public async Task GetProjectById_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ProjectTools>>();
        var dbContext = CreateDbContext();
        var tools = new ProjectTools(logger, dbContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => tools.GetProjectById(null));

        Assert.Equal("Project ID is required", exception.Message);
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
        Assert.Contains("```json", result);
        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonStr);
        Assert.True(json.TryGetProperty("Id", out var idProp));
        Assert.Equal(targetProject.Id.ToString(), idProp.GetString());
        Assert.True(json.TryGetProperty("Name", out var nameProp));
        Assert.Equal(targetProject.Name, nameProp.GetString());
        Assert.True(json.TryGetProperty("Repository", out var repoProp));
        Assert.Equal(targetProject.Repository, repoProp.GetString());
    }
}
