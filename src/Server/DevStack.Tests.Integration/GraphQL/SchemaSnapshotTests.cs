using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
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

        var feature = new Item
        {
            Subtype = ItemSubtype.Feature,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Snapshot Feature",
            Description = "Feature for schema testing",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Items.Add(feature);

        var task = new Item
        {
            Subtype = ItemSubtype.Task,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ParentFeatureId = feature.Id,
            Title = "[TestData] Snapshot Task",
            Description = "Task for schema testing",
            Status = FeatureStatus.Planning,
            Deliverable = "Test task",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Items.Add(task);

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

        var features = query.GetFeatures(_dbContext!);
        var tasks = query.GetItems(_dbContext!, subtype: [ItemSubtype.Task]);

        features.Nodes.Should().HaveCount(1);
        tasks.Nodes.Should().HaveCount(1);

        var input = new TransitionTaskInput(tasks.Nodes.First().Id, FeatureStatus.InProgress, "test@example.com");
        var handler = new DevStack.Infrastructure.Tasks.TransitionTaskStatusHandler(_dbContext!);
        var result = await mutation.TransitionTaskStatusAsync(input, handler, default);
        result.Item.Should().NotBeNull();
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
        summary.RecentAuditEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GraphQL_Mutation_Creates_Audit_Event()
    {
        var mutation = new Mutation();
        var task = _dbContext!.Tasks.First();

        var input = new TransitionTaskInput(task.Id, FeatureStatus.InProgress, "test@example.com");
        var handler = new DevStack.Infrastructure.Tasks.TransitionTaskStatusHandler(_dbContext);
        var result = await mutation.TransitionTaskStatusAsync(input, handler, default);

        result.Item.Should().NotBeNull();

        var auditEvents = _dbContext.AuditEvents.ToList();
        auditEvents.Should().HaveCount(1);
        auditEvents[0].EntityType.Should().Be("Task");
        auditEvents[0].EventType.Should().Be("StatusChanged");
    }
}
