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
        if (_dbContext is null) return;

        var projectId = Guid.NewGuid();
        
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Description = "A test project for integration tests",
            Architecture = "Clean Architecture",
            Memory = "4GB",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Projects.Add(project);

        var feature1 = new Feature
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Feature 1",
            Description = "First test feature",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var feature2 = new Feature
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Feature 2",
            Description = "Second test feature",
            Status = FeatureStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var feature3 = new Feature
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Feature 3",
            Description = "Third test feature - in review",
            Status = FeatureStatus.InReview,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Features.AddRange(feature1, feature2, feature3);

        var defect1 = new Defect
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Critical Bug",
            Description = "A critical bug in the system",
            Severity = Severity.Critical,
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Defects.Add(defect1);

        var task1 = new AgentTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FeatureId = feature1.Id,
            Title = "Task 1",
            Status = DevStack.Domain.Enums.TaskStatus.Planning,
            Deliverable = "Implement feature 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task2 = new AgentTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FeatureId = feature1.Id,
            Title = "Task 2",
            Status = DevStack.Domain.Enums.TaskStatus.Code,
            Deliverable = "Write tests for feature 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var task3 = new AgentTask
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FeatureId = feature2.Id,
            Title = "Task 3",
            Status = DevStack.Domain.Enums.TaskStatus.Failed,
            Deliverable = "Implement feature 2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Tasks.AddRange(task1, task2, task3);

        var modelConfig = new ModelConfiguration
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
        _dbContext.ModelConfigurations.Add(modelConfig);

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
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var project = _dbContext!.Projects.First();

        // Act
        var result = query.GetProjectById(_dbContext, project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Project");
    }

    [Fact]
    public void GetProjects_Returns_All_Projects()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetProjects(_dbContext!);

        // Assert
        result.Nodes.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void GetFeatures_Returns_All_Features_With_Pagination()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetFeatures(_dbContext!);

        // Assert
        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

   [Fact]
    public void GetFeatures_With_ProjectId_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var projectId = _dbContext!.Projects.First().Id;

        // Act
        var result = query.GetFeatures(_dbContext, projectId);

        // Assert
        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public void GetFeatures_With_Status_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetFeatures(_dbContext!, status: [FeatureStatus.InProgress]);

        // Assert
        result.Nodes.Should().HaveCount(1);
        result.Nodes.First().Title.Should().Be("Feature 2");
    }

    [Fact]
    public void GetFeatureById_Returns_Feature()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var feature = _dbContext!.Features.First(f => f.Title == "Feature 1");

        // Act
        var result = query.GetFeatureById(_dbContext, feature.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Feature 1");
    }

    [Fact]
    public void GetDefects_Returns_All_Defects()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetDefects(_dbContext!);

        // Assert
        result.Nodes.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public void GetDefectById_Returns_Defect()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var defect = _dbContext!.Defects.First();

        // Act
        var result = query.GetDefectById(_dbContext, defect.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Critical Bug");
    }

    [Fact]
    public void GetTasks_Returns_All_Tasks_With_Pagination()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetTasks(_dbContext!);

        // Assert
        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetTasks_With_FeatureId_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var feature = _dbContext!.Features.First(f => f.Title == "Feature 1");

        // Act
        var result = query.GetTasks(_dbContext, featureId: feature.Id);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public void GetTasks_With_Status_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetTasks(_dbContext!, status: [DevStack.Domain.Enums.TaskStatus.Code]);

        // Assert
        result.Nodes.Should().HaveCount(1);
        result.Nodes.First().Title.Should().Be("Task 2");
    }

    [Fact]
    public void GetTaskById_Returns_Task()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var task = _dbContext!.Tasks.First(t => t.Title == "Task 1");

        // Act
        var result = query.GetTaskById(_dbContext, task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Task 1");
    }

    [Fact]
    public void GetModelConfigurations_Returns_Configurations_For_Project()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var project = _dbContext!.Projects.First();

        // Act
        var result = query.GetModelConfigurations(_dbContext, project.Id);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void GetAuditEvents_Returns_Audit_Events_For_Entity()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var feature = _dbContext!.Features.First(f => f.Title == "Feature 1");

        // Act
        var result = query.GetAuditEvents(_dbContext, feature.Id);

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public void GetDashboardSummary_Returns_Correct_Counts()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act
        var result = query.GetDashboardSummary(_dbContext!);

        // Assert
        result.ProjectsInFlight.Should().Be(1);
        result.FeaturesInReview.Should().Be(1);
        result.FeaturesFailed.Should().Be(0);
        result.TasksInProgress.Should().Be(1);
        result.TasksFailed.Should().Be(1);
        result.RecentAuditEvents.Should().ContainSingle();
    }

    [Fact]
    public void GetFeatures_With_Pagination_Skip_and_First()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act - skip 1, take 2
        var result = query.GetFeatures(_dbContext!, first: 2, skip: 1);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void GetFeatures_With_Pagination_HasNextPage()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act - skip 0, take 2
        var result = query.GetFeatures(_dbContext!, first: 2, skip: 0);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeTrue();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetFeatures_With_CreatedAfter_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = query.GetFeatures(_dbContext!, createdAfter: oneHourAgo);

        // Assert
        result.Nodes.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public void GetTasks_With_Pagination_Skip_and_First()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act - skip 1, take 2
        var result = query.GetTasks(_dbContext!, first: 2, skip: 1);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeFalse();
        result.PageInfo.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void GetTasks_With_Pagination_HasNextPage()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();

        // Act - skip 0, take 2
        var result = query.GetTasks(_dbContext!, first: 2, skip: 0);

        // Assert
        result.Nodes.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageInfo.HasNextPage.Should().BeTrue();
        result.PageInfo.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void GetTasks_With_CreatedBefore_Filter()
    {
        // Arrange
        var query = new DevStack.Api.GraphQL.Types.Query();
        var futureDate = DateTime.UtcNow.AddHours(1);

        // Act
        var result = query.GetTasks(_dbContext!, createdBefore: futureDate);

        // Assert
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
