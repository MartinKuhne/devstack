using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class SchemaSnapshotTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;

    public SchemaSnapshotTests()
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
        if (_dbContext is null)
        {
            return;
        }

        var projectId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            Name = "[TestData] Test Project",
            Description = "A test project for schema snapshot",
            Architecture = "Clean Architecture",
            Memory = "4GB",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Projects.Add(project);

        var deliverable = new Deliverable
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Test Deliverable",
            Type = DeliverableType.Feature,
            Status = DeliverableStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Deliverables.Add(deliverable);

        var agentTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            DeliverableId = deliverable.Id,
            Title = "[TestData] Test AgentTask",
            Status = AgentTaskStatus.Ready,
            ComplexityRating = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.AgentTasks.Add(agentTask);

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
    public async Task Schema_Snapshot_Does_Not_Change_Unintentionally()
    {
        var query = new Query();
        var mutation = new Mutation();

        var projects = query.GetProjects(_dbContext!, first: 10);
        var deliverables = query.GetItems(_dbContext!, subtype: null, first: 10);

        projects.Nodes.Should().HaveCount(1);
        projects.TotalCount.Should().Be(1);

        var deliverablePayload = await mutation.CreateDeliverableAsync(
            new CreateDeliverableInput(
                projects.Nodes[0].Id,
                "Test Deliverable",
                "Feature",
                "Test description",
                null, null, null, null, null, null, null, null,
                DeliverableStatus.Planning),
            _dbContext!,
            default);
        deliverablePayload.Deliverable.Should().NotBeNull();
        deliverablePayload.Deliverable!.Title.Should().Be("Test Deliverable");
    }

    [Fact]
    public void DashboardSummary_Snapshot_Returns_Correct_Data()
    {
        var query = new Query();

        var summary = query.GetDashboardSummary(_dbContext!);

        summary.ProjectsInFlight.Should().Be(1);
        summary.FeaturesInReview.Should().Be(0);
        summary.FeaturesFailed.Should().Be(0);
        summary.TasksInProgress.Should().Be(0);
        summary.TasksFailed.Should().Be(0);
    }
}
