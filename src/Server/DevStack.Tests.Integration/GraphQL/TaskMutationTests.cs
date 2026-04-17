using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FeatureStatus = DevStack.Domain.Enums.FeatureStatus;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;

namespace DevStack.Tests.Integration.GraphQL;

public class TaskMutationTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;
    private Guid _featureId;

    public TaskMutationTests()
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
        if (_dbContext is null) return;

        _projectId = Guid.NewGuid();
        _featureId = Guid.NewGuid();
        
        var project = new Project
        {
            Id = _projectId,
            Name = "Test Project",
            Description = "A test project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Projects.Add(project);

        var item = new Item
        {
            Id = _featureId,
            ProjectId = _projectId,
            Title = "Test Feature",
            Status = FeatureStatus.Planning,
            Subtype = ItemSubtype.Feature,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Items.Add(item);
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
    public async Task CreateTask_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        var input = new CreateTaskInput(
            ProjectId: _projectId,
            ItemId: _featureId,
            Title: "New Task",
            Deliverable: "Implement feature",
            AcceptanceCriteria: "Tests pass",
            Risks: null,
            Result: null,
            RequiredFollowUps: null,
            ComplexityRating: 5);

        var result = await mutation.CreateTaskAsync(
            input,
            new CreateTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Task.Should().NotBeNull();
        result.Task!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTask_Fails_When_Title_Is_Empty()
    {
        var mutation = new Mutation();
        var input = new CreateTaskInput(
            ProjectId: _projectId,
            ItemId: _featureId,
            Title: "",
            Deliverable: null,
            AcceptanceCriteria: null,
            Risks: null,
            Result: null,
            RequiredFollowUps: null,
            ComplexityRating: 5);

        var result = await mutation.CreateTaskAsync(
            input,
            new CreateTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Task.Should().BeNull();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public async Task CreateTask_Fails_When_Complexity_Is_Out_Of_Range()
    {
        var mutation = new Mutation();
        var input = new CreateTaskInput(
            ProjectId: _projectId,
            ItemId: _featureId,
            Title: "Invalid Task",
            Deliverable: null,
            AcceptanceCriteria: null,
            Risks: null,
            Result: null,
            RequiredFollowUps: null,
            ComplexityRating: 15);

        var result = await mutation.CreateTaskAsync(
            input,
            new CreateTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Task.Should().BeNull();
        result.Errors.Should().Contain("ComplexityRating must be between 1 and 10");
    }

    [Fact]
    public async Task UpdateTask_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        
        var taskId = Guid.NewGuid();
        var task = new AgentTask
        {
            Id = taskId,
            ProjectId = _projectId,
            FeatureId = _featureId,
            Title = "Original Title",
            Status = TaskStatus.Planning,
            Deliverable = "Original deliverable",
            ComplexityRating = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var input = new UpdateTaskInput(
            Id: taskId,
            Title: "Updated Title",
            Deliverable: "Updated deliverable",
            AcceptanceCriteria: null,
            Risks: null,
            Result: null,
            RequiredFollowUps: null,
            ComplexityRating: null);

        var result = await mutation.UpdateTaskAsync(
            input,
            new UpdateTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Task.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTask_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new UpdateTaskInput(
            Id: Guid.NewGuid(),
            Title: "Updated Title",
            Deliverable: null,
            AcceptanceCriteria: null,
            Risks: null,
            Result: null,
            RequiredFollowUps: null,
            ComplexityRating: null);

        var result = await mutation.UpdateTaskAsync(
            input,
            new UpdateTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Task.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }

    [Fact]
    public async Task TransitionTaskStatus_Succeeds_For_Valid_Transition()
    {
        var mutation = new Mutation();
        
        var taskId = Guid.NewGuid();
        var task = new AgentTask
        {
            Id = taskId,
            ProjectId = _projectId,
            FeatureId = _featureId,
            Title = "Test Task",
            Status = TaskStatus.Planning,
            Deliverable = "Test deliverable",
            ComplexityRating = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var input = new TransitionTaskInput(
            Id: taskId,
            TargetStatus: TaskStatus.Ready,
            Actor: "operator");

        var result = await mutation.TransitionTaskStatusAsync(
            input,
            new TransitionTaskStatusHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Task.Should().NotBeNull();
        
        var updatedTask = await _dbContext.Tasks.FindAsync(taskId);
        updatedTask!.Status.Should().Be(TaskStatus.Ready);
        
        var auditEvent = await _dbContext.AuditEvents.FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
        auditEvent!.EventType.Should().Be("StatusChanged");
        auditEvent.Actor.Should().Be("operator");
    }

    [Fact]
    public async Task DeleteTask_Succeeds_With_Valid_Id()
    {
        var mutation = new Mutation();
        
        var taskId = Guid.NewGuid();
        var task = new AgentTask
        {
            Id = taskId,
            ProjectId = _projectId,
            FeatureId = _featureId,
            Title = "To Delete",
            Status = TaskStatus.Planning,
            Deliverable = "Test deliverable",
            ComplexityRating = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var input = new DeleteTaskInput(Id: taskId);

        var result = await mutation.DeleteTaskAsync(
            input,
            new DeleteTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Task.Should().NotBeNull();

        var deletedTask = await _dbContext.Tasks.FindAsync(taskId);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new DeleteTaskInput(Id: Guid.NewGuid());

        var result = await mutation.DeleteTaskAsync(
            input,
            new DeleteTaskHandler(_dbContext!),
            CancellationToken.None);

        result.Task.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }
}
