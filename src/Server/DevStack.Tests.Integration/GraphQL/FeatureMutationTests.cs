using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Features;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class FeatureMutationTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;

    public FeatureMutationTests()
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
        
        var project = new Project
        {
            Id = _projectId,
            Name = "Test Project",
            Description = "A test project",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Projects.Add(project);
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
    public async Task CreateFeature_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        var input = new CreateFeatureInput(
            ProjectId: _projectId,
            Title: "New Feature",
            Description: "Test description",
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null,
            InitialStatus: null);

        var result = await mutation.CreateFeatureAsync(
            input,
            new CreateFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();
        result.Item!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateFeature_Fails_When_Title_Is_Empty()
    {
        var mutation = new Mutation();
        var input = new CreateFeatureInput(
            ProjectId: _projectId,
            Title: "",
            Description: null,
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null,
            InitialStatus: null);

        var result = await mutation.CreateFeatureAsync(
            input,
            new CreateFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public async Task UpdateFeature_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        
        var featureId = Guid.NewGuid();
        var item = new Item
        {
            Id = featureId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Feature,
            Title = "Original Title",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new UpdateFeatureInput(
            Id: featureId,
            Title: "Updated Title",
            Description: null,
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null);

        var result = await mutation.UpdateFeatureAsync(
            input,
            new UpdateFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFeature_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new UpdateFeatureInput(
            Id: Guid.NewGuid(),
            Title: "Updated Title",
            Description: null,
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null);

        var result = await mutation.UpdateFeatureAsync(
            input,
            new UpdateFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }

    [Fact]
    public async Task TransitionFeatureStatus_Succeeds_For_Valid_Transition()
    {
        var mutation = new Mutation();
        var transitionService = new Domain.Services.ItemStatusTransitionService();
        
        var featureId = Guid.NewGuid();
        var item = new Item
        {
            Id = featureId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Feature,
            Title = "Test Item",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new TransitionFeatureInput(
            Id: featureId,
            TargetStatus: FeatureStatus.Ready,
            Actor: "operator");

        var result = await mutation.TransitionFeatureStatusAsync(
            input,
            new TransitionFeatureStatusHandler(_dbContext!, transitionService),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();
        
        var updatedItem = await _dbContext.Items.FindAsync(featureId);
        updatedItem!.Status.Should().Be(FeatureStatus.Ready);
        
        var auditEvent = await _dbContext.AuditEvents.FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
        auditEvent!.EventType.Should().Be("StatusChanged");
        auditEvent.Actor.Should().Be("operator");
    }

    [Fact]
    public async Task TransitionFeatureStatus_Returns_ValidationError_For_Invalid_Transition()
    {
        var mutation = new Mutation();
        var transitionService = new Domain.Services.ItemStatusTransitionService();
        
        var featureId = Guid.NewGuid();
        var item = new Item
        {
            Id = featureId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Feature,
            Title = "Test Item",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new TransitionFeatureInput(
            Id: featureId,
            TargetStatus: FeatureStatus.Done,
            Actor: "operator");

        var result = await mutation.TransitionFeatureStatusAsync(
            input,
            new TransitionFeatureStatusHandler(_dbContext!, transitionService),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("FEATURE_VALIDATION_ERROR"));
    }

    [Fact]
    public async Task DeleteFeature_Succeeds_With_Valid_Id()
    {
        var mutation = new Mutation();
        
        var featureId = Guid.NewGuid();
        var item = new Item
        {
            Id = featureId,
            ProjectId = _projectId,
            Subtype = ItemSubtype.Feature,
            Title = "To Delete",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var input = new DeleteFeatureInput(Id: featureId);

        var result = await mutation.DeleteFeatureAsync(
            input,
            new DeleteFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Item.Should().NotBeNull();

        var deletedItem = await _dbContext.Items.FindAsync(featureId);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFeature_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new DeleteFeatureInput(Id: Guid.NewGuid());

        var result = await mutation.DeleteFeatureAsync(
            input,
            new DeleteFeatureHandler(_dbContext!),
            CancellationToken.None);

        result.Item.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND"));
    }
}
