using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Defects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class DefectMutationTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;

    public DefectMutationTests()
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
    public async Task CreateDefect_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        var input = new CreateDefectInput(
            ProjectId: _projectId,
            ParentFeatureId: null,
            Severity: Severity.High,
            Title: "New Defect",
            Description: "Test description",
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null,
            InitialStatus: null);

        var result = await mutation.CreateDefectAsync(
            input,
            new CreateDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();
        result.Defect!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateDefect_Fails_When_Title_Is_Empty()
    {
        var mutation = new Mutation();
        var input = new CreateDefectInput(
            ProjectId: _projectId,
            ParentFeatureId: null,
            Severity: Severity.Medium,
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

        var result = await mutation.CreateDefectAsync(
            input,
            new CreateDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public async Task CreateDefect_Succeeds_With_Parent_Feature()
    {
        var featureId = Guid.NewGuid();
        var feature = new Feature
        {
            Id = featureId,
            ProjectId = _projectId,
            Title = "Test Feature",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Features.Add(feature);
        await _dbContext.SaveChangesAsync();

        var mutation = new Mutation();
        var input = new CreateDefectInput(
            ProjectId: _projectId,
            ParentFeatureId: featureId,
            Severity: Severity.Critical,
            Title: "Defect linked to feature",
            Description: "Test description",
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null,
            InitialStatus: null);

        var result = await mutation.CreateDefectAsync(
            input,
            new CreateDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();
        result.Defect!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateDefect_Succeeds_With_Valid_Input()
    {
        var mutation = new Mutation();
        
        var defectId = Guid.NewGuid();
        var defect = new Defect
        {
            Id = defectId,
            ProjectId = _projectId,
            ParentFeatureId = null,
            Severity = Severity.Low,
            Title = "Original Title",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Defects.Add(defect);
        await _dbContext.SaveChangesAsync();

        var input = new UpdateDefectInput(
            Id: defectId,
            Title: "Updated Title",
            Description: null,
            AcceptanceCriteria: null,
            Plan: null,
            SecurityImpact: null,
            PerformanceImpact: null,
            TestPlan: null,
            DeploymentPlan: null,
            OpenQuestions: null);

        var result = await mutation.UpdateDefectAsync(
            input,
            new UpdateDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateDefect_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new UpdateDefectInput(
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

        var result = await mutation.UpdateDefectAsync(
            input,
            new UpdateDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND: Defect with ID"));
    }

    [Fact]
    public async Task TransitionDefectStatus_Succeeds_For_Valid_Transition()
    {
        var mutation = new Mutation();
        var transitionService = new Domain.Services.FeatureStatusTransitionService();
        
        var defectId = Guid.NewGuid();
        var defect = new Defect
        {
            Id = defectId,
            ProjectId = _projectId,
            ParentFeatureId = null,
            Severity = Severity.High,
            Title = "Test Defect",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Defects.Add(defect);
        await _dbContext.SaveChangesAsync();

        var input = new TransitionDefectInput(
            Id: defectId,
            TargetStatus: FeatureStatus.Ready,
            Actor: "operator");

        var result = await mutation.TransitionDefectStatusAsync(
            input,
            new TransitionDefectStatusHandler(_dbContext!, transitionService),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();
        
        var updatedDefect = await _dbContext.Defects.FindAsync(defectId);
        updatedDefect!.Status.Should().Be(FeatureStatus.Ready);
        
        var auditEvent = await _dbContext.AuditEvents.FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
        auditEvent!.EventType.Should().Be("StatusChanged");
        auditEvent.Actor.Should().Be("operator");
    }

    [Fact]
    public async Task TransitionDefectStatus_Returns_ValidationError_For_Invalid_Transition()
    {
        var mutation = new Mutation();
        var transitionService = new Domain.Services.FeatureStatusTransitionService();
        
        var defectId = Guid.NewGuid();
        var defect = new Defect
        {
            Id = defectId,
            ProjectId = _projectId,
            ParentFeatureId = null,
            Severity = Severity.Medium,
            Title = "Test Defect",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Defects.Add(defect);
        await _dbContext.SaveChangesAsync();

        var input = new TransitionDefectInput(
            Id: defectId,
            TargetStatus: FeatureStatus.Done,
            Actor: "operator");

        var result = await mutation.TransitionDefectStatusAsync(
            input,
            new TransitionDefectStatusHandler(_dbContext!, transitionService),
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("FEATURE_VALIDATION_ERROR:"));
    }

    [Fact]
    public async Task DeleteDefect_Succeeds_With_Valid_Id()
    {
        var mutation = new Mutation();
        
        var defectId = Guid.NewGuid();
        var defect = new Defect
        {
            Id = defectId,
            ProjectId = _projectId,
            ParentFeatureId = null,
            Severity = Severity.Critical,
            Title = "To Delete",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext!.Defects.Add(defect);
        await _dbContext.SaveChangesAsync();

        var input = new DeleteDefectInput(Id: defectId);

        var result = await mutation.DeleteDefectAsync(
            input,
            new DeleteDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();

        var deletedDefect = await _dbContext.Defects.FindAsync(defectId);
        deletedDefect.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDefect_Returns_NotFound_For_Unknown_Id()
    {
        var mutation = new Mutation();
        var input = new DeleteDefectInput(Id: Guid.NewGuid());

        var result = await mutation.DeleteDefectAsync(
            input,
            new DeleteDefectHandler(_dbContext!),
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("NOT_FOUND: Defect with ID"));
    }
}
