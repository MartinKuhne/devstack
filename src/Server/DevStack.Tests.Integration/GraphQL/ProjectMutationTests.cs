using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Projects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class ProjectMutationTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;

    public ProjectMutationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseSqlite(_connection)
            .Options;
    }

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        _dbContext = new DevStackDbContext(_options);
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async System.Threading.Tasks.Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
        _connection.Close();
    }

    [Fact]
    public async Task CreateProject_Mutation_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        var input = new CreateProjectInput(
            Name: "[TestData] Test Project",
            Description: "Test description",
            Architecture: "Microservices",
            Memory: "8GB",
            GithubUrl: "https://github.com/test/repo");

        var result = await mutation.CreateProjectAsync(
            input,
            new CreateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Project.Should().NotBeNull();
        result.Project!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProject_Mutation_Fails_When_Name_Is_Empty()
    {
        var mutation = new Mutation();
        var input = new CreateProjectInput(
            Name: "",
            Description: null,
            Architecture: null,
            Memory: null,
            GithubUrl: null);

        var result = await mutation.CreateProjectAsync(
            input,
            new CreateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain("Name is required");
        result.Project.Should().BeNull();
    }

    [Fact]
    public async Task CreateProject_Mutation_Fails_When_Name_Exceeds_200_Chars()
    {
        var mutation = new Mutation();
        var longName = new string('a', 201);
        var input = new CreateProjectInput(
            Name: longName,
            Description: null,
            Architecture: null,
            Memory: null,
            GithubUrl: null);

        var result = await mutation.CreateProjectAsync(
            input,
            new CreateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain("Name must be 200 characters or less");
        result.Project.Should().BeNull();
    }

    [Fact]
    public async Task CreateProject_Mutation_Fails_When_GithubUrl_Is_Invalid()
    {
        var mutation = new Mutation();
        var input = new CreateProjectInput(
            Name: "Test Project",
            Description: null,
            Architecture: null,
            Memory: null,
            GithubUrl: "not-a-valid-url");

        var result = await mutation.CreateProjectAsync(
            input,
            new CreateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain("GitHub URL is not a valid URI");
        result.Project.Should().BeNull();
    }

    [Fact]
    public async Task CreateProject_Mutation_Succeeds_Without_Optional_Fields()
    {
        var mutation = new Mutation();
        var input = new CreateProjectInput(
            Name: "[TestData] Minimal Project",
            Description: null,
            Architecture: null,
            Memory: "4GB",
            GithubUrl: null);

        var result = await mutation.CreateProjectAsync(
            input,
            new CreateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Project.Should().NotBeNull();
        result.Project!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateProject_Mutation_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "[TestData] Original Name",
            Description = "Original Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Projects.Add(project);
        await _dbContext.SaveChangesAsync();
        _projectId = project.Id;

        var updateInput = new UpdateProjectInput(
            Id: _projectId,
            Name: "[TestData] Updated Name",
            Description: "Updated Description",
            Architecture: "Serverless",
            Memory: "16GB",
            GithubUrl: null,
            GithubToken_Encrypted: null);

        var result = await mutation.UpdateProjectAsync(
            updateInput,
            new UpdateProjectHandler(_dbContext),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Project.Should().NotBeNull();

        var updated = await _dbContext!.Projects.FindAsync(_projectId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("[TestData] Updated Name");
        updated.Description.Should().Be("Updated Description");
        updated.Architecture.Should().Be("Serverless");
        updated.Memory.Should().Be("16GB");
    }

    [Fact]
    public async Task UpdateProject_Mutation_Fails_When_Project_Not_Found()
    {
        var mutation = new Mutation();
        var updateInput = new UpdateProjectInput(
            Id: Guid.NewGuid(),
            Name: "Updated Name",
            Description: null,
            Architecture: null,
            Memory: null,
            GithubUrl: null,
            GithubToken_Encrypted: null);

        var result = await mutation.UpdateProjectAsync(
            updateInput,
            new UpdateProjectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
        result.Project.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProject_Mutation_Succeeds()
    {
        var mutation = new Mutation();
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "[TestData] To Delete",
            Description = "Will be deleted",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Projects.Add(project);
        await _dbContext.SaveChangesAsync();
        _projectId = project.Id;

        var deleteInput = new DeleteProjectInput(_projectId);

        var result = await mutation.DeleteProjectAsync(
            deleteInput,
            new DeleteProjectHandler(_dbContext),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Project.Should().NotBeNull();

        var deleted = await _dbContext.Projects.FindAsync(_projectId);
        deleted.Should().BeNull();
    }
}