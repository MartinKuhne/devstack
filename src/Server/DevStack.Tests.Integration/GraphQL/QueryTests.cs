using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class QueryTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;

    public QueryTests()
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
            Description = "A test project for integration tests",
            Architecture = "Clean Architecture",
            Memory = "4GB",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Projects.Add(project);

        var feature1 = new Item
        {
            Subtype = ItemSubtype.Feature,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Feature 1",
            Description = "First test feature",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var feature2 = new Item
        {
            Subtype = ItemSubtype.Feature,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Feature 2",
            Description = "Second test feature",
            Status = FeatureStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var feature3 = new Item
        {
            Subtype = ItemSubtype.Feature,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Feature 3",
            Description = "Third test feature - in review",
            Status = FeatureStatus.InReview,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var defect1 = new Item
        {
            Subtype = ItemSubtype.Defect,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "[TestData] Critical Bug",
            Description = "A critical bug in the system",
            Severity = Severity.Critical,
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task1 = new Item
        {
            Subtype = ItemSubtype.Task,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ParentFeatureId = feature1.Id,
            Title = "[TestData] Task 1",
            Description = "Task 1 description",
            Status = FeatureStatus.Planning,
            Deliverable = "Implement feature 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task2 = new Item
        {
            Subtype = ItemSubtype.Task,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ParentFeatureId = feature1.Id,
            Title = "[TestData] Task 2",
            Description = "Task 2 description",
            Status = FeatureStatus.InProgress,
            Deliverable = "Write tests for feature 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task3 = new Item
        {
            Subtype = ItemSubtype.Task,
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ParentFeatureId = feature2.Id,
            Title = "[TestData] Task 3",
            Description = "Task 3 description",
            Status = FeatureStatus.Failed,
            Deliverable = "Implement feature 2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.AddRange([feature1, feature2, feature3, defect1, task1, task2, task3]);

        var model = new LargeLanguageModel
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Url = "https://api.example.com",
            Model = "gpt-4",
            ModelAlias = "primary",
            ApiKey_Encrypted = "encrypted_key",
            MaxComplexity = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.LargeLanguageModels.Add(model);

        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = nameof(Feature),
            EntityId = feature1.Id,
            EventType = "StatusChanged",
            OldValue = "Planning",
            NewValue = "InProgress",
            Actor = "test@example.com",
            OccurredAt = DateTime.UtcNow
        };
        _dbContext.AuditEvents.Add(auditEvent);

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
    public void GetProjectById_Returns_Project()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var project = _dbContext!.Projects.First();

        var result = query.GetProjectById(_dbContext, project.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("[TestData] Test Project");
    }

    [Fact]
    public void GetProjects_Returns_All_Projects()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetProjects(_dbContext!);

        result.Nodes.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void GetFeatures_Returns_All_Features_With_Pagination()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetFeatures(_dbContext!);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetFeatures_With_ProjectId_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var projectId = _dbContext!.Projects.First().Id;

        var result = query.GetFeatures(_dbContext, projectId);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public void GetFeatures_With_Status_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetFeatures(_dbContext!, status: [FeatureStatus.InProgress]);

        result.Nodes.Should().HaveCount(1);
        result.Nodes.First().Title.Should().Be("[TestData] Feature 2");
    }

    [Fact]
    public void GetFeatureById_Returns_Feature()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var feature = _dbContext!.Items.First(f => f.Title == "[TestData] Feature 1");

        var result = query.GetFeatureById(_dbContext, feature.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("[TestData] Feature 1");
    }

    [Fact]
    public void GetDefects_Returns_All_Defects()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetDefects(_dbContext!);

        result.Nodes.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void GetDefectById_Returns_Defect()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var defect = _dbContext!.Defects.First();

        var result = query.GetDefectById(_dbContext, defect.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("[TestData] Critical Bug");
    }

    [Fact]
    public void GetItems_Returns_All_Tasks_With_Pagination()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetItems(_dbContext!, subtype: [ItemSubtype.Task]);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetItems_With_FeatureId_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var feature = _dbContext!.Items.First(f => f.Title == "[TestData] Feature 1");

        var result = query.GetItems(_dbContext, subtype: [ItemSubtype.Task]);

        result.Nodes.Should().HaveCount(3);
        result.Nodes.Count(node => node.ParentFeatureId == feature.Id).Should().Be(2);
    }

    [Fact]
    public void GetItems_With_Status_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetItems(_dbContext!, status: [FeatureStatus.InProgress], subtype: [ItemSubtype.Task]);

        result.Nodes.Should().HaveCount(1);
        result.Nodes.First().Title.Should().Be("[TestData] Task 2");
    }

    [Fact]
    public void GetItemById_Returns_Task()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var task = _dbContext!.Tasks.First(t => t.Title == "[TestData] Task 1");

        var result = query.GetItemById(_dbContext, task.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("[TestData] Task 1");
    }

    [Fact]
    public void GetLargeLanguageModels_Returns_Configurations_For_Project()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetLargeLanguageModels(_dbContext!);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void DashboardSummary_Returns_Correct_Counts()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetDashboardSummary(_dbContext!);

        result.ProjectsInFlight.Should().Be(1);
        result.FeaturesInReview.Should().Be(1);
        result.FeaturesFailed.Should().Be(1);
        result.TasksInProgress.Should().Be(1);
        result.TasksFailed.Should().Be(1);
        result.RecentAuditEvents.Should().ContainSingle();
    }

    [Fact]
    public void GetFeatures_With_Pagination_Skip_and_First()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetFeatures(_dbContext!, first: 2, skip: 1);

        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void GetFeatures_With_Pagination_HasNextPage()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetFeatures(_dbContext!, first: 2, skip: 0);

        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeTrue();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetFeatures_With_CreatedAfter_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var result = query.GetItems(_dbContext!, subtype: [ItemSubtype.Feature], createdAfter: oneHourAgo);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public void GetTasks_With_Pagination_Skip_and_First()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetItems(_dbContext!, subtype: [ItemSubtype.Task], first: 2, skip: 1);

        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void GetTasks_With_Pagination_HasNextPage()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();

        var result = query.GetItems(_dbContext!, subtype: [ItemSubtype.Task], first: 2, skip: 0);

        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeTrue();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetTasks_With_CreatedBefore_Filter()
    {
        var query = new DevStack.Api.GraphQL.Types.Query();
        var futureDate = DateTime.UtcNow.AddHours(1);

        var result = query.GetItems(_dbContext!, subtype: [ItemSubtype.Task], createdBefore: futureDate);

        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }
}

internal sealed class SqliteInMemoryDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteInMemoryDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public SqliteConnection Connection => _connection;

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
