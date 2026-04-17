using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Epics;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class EpicMutationTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;

    public EpicMutationTests()
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
        await SeedDataAsync();
    }

    private async System.Threading.Tasks.Task SeedDataAsync()
    {
        _projectId = Guid.NewGuid();
        _dbContext!.Projects.Add(new Project
        {
            Id = _projectId,
            Name = "Test Project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
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
    public async Task CreateEpic_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        var input = new CreateEpicInput(
            ProjectId: _projectId,
            Title: "New Epic",
            Description: "Test description",
            DependsOnId: null);

        var result = await mutation.CreateEpicAsync(
            input,
            new CreateEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();
        result.Item!.Id.Should().NotBeEmpty();
        result.Item.Subtype.Should().Be(ItemSubtype.Epic);
    }

    [Fact]
    public async Task CreateEpic_Fails_When_Title_Is_Empty()
    {
        var mutation = new Mutation();
        var input = new CreateEpicInput(
            ProjectId: _projectId,
            Title: "",
            Description: null,
            DependsOnId: null);

        var result = await mutation.CreateEpicAsync(
            input,
            new CreateEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public async Task CreateEpic_Fails_When_Title_Exceeds_MaxLength()
    {
        var mutation = new Mutation();
        var input = new CreateEpicInput(
            ProjectId: _projectId,
            Title: new string('A', 201),
            Description: null,
            DependsOnId: null);

        var result = await mutation.CreateEpicAsync(
            input,
            new CreateEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain("Title must be 200 characters or less");
    }

    [Fact]
    public async Task UpdateEpic_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        
        var epicId = Guid.NewGuid();
        var item = new Item
        {
            Id = epicId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Epic,
            Title = "Original Title",
            Description = "Original description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new UpdateEpicInput(
            Id: epicId,
            Title: "Updated Title",
            Description: "Updated description",
            DependsOnId: null);

        var result = await mutation.UpdateEpicAsync(
            input,
            new UpdateEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();
        
        var updatedItem = await _dbContext.Items.FindAsync(epicId);
        updatedItem!.Title.Should().Be("Updated Title");
        updatedItem.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateEpic_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new UpdateEpicInput(
            Id: Guid.NewGuid(),
            Title: "Updated Title",
            Description: null,
            DependsOnId: null);

        var result = await mutation.UpdateEpicAsync(
            input,
            new UpdateEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }

    [Fact]
    public async Task DeleteEpic_Succeeds_With_Valid_Id()
    {
        var mutation = new Mutation();
        
        var epicId = Guid.NewGuid();
        var item = new Item
        {
            Id = epicId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Epic,
            Title = "To Delete",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new DeleteEpicInput(Id: epicId);

        var result = await mutation.DeleteEpicAsync(
            input,
            new DeleteEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();

        var deletedItem = await _dbContext.Items.FindAsync(epicId);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEpic_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new DeleteEpicInput(Id: Guid.NewGuid());

        var result = await mutation.DeleteEpicAsync(
            input,
            new DeleteEpicHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }
}
