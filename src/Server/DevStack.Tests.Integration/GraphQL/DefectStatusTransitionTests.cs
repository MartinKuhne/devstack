using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL;

public class DefectStatusTransitionTests : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    private Guid _projectId;

    public DefectStatusTransitionTests()
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

        _projectId = Guid.NewGuid();
        _dbContext.Projects.Add(new Project
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
        if (_dbContext is not null) await _dbContext.DisposeAsync();
        _connection.Close();
    }

    private async Task<Guid> SeedDefectAsync(FeatureStatus status)
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id,
            ProjectId = _projectId,
            Severity = Severity.High,
            Title = "Test Defect",
            Status = status,
            Result = "Test result",
            Errors = "Test errors",
            OpenQuestions = "Test open questions",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return id;
    }

    private (Mutation mutation, TransitionDefectStatusHandler handler) CreateHandler() =>
        (new Mutation(), new TransitionDefectStatusHandler(_dbContext!, new FeatureStatusTransitionService()));

    // -------------------------------------------------------------------------
    // All valid transitions
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.ReadyForTest)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Done, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Done, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Rejected, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Rejected, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.ReadyForTest)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Rejected)]
    public async Task ValidTransition_Succeeds_And_Persists(FeatureStatus from, FeatureStatus to)
    {
        var id = await SeedDefectAsync(from);
        var (mutation, handler) = CreateHandler();

        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: to, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.Defect.Should().NotBeNull();
        var persisted = await _dbContext!.Defects.FindAsync(id);
        persisted!.Status.Should().Be(to);
    }

    // -------------------------------------------------------------------------
    // Invalid transitions
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.ReadyForTest)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.InReview)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.ReadyForTest)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Done)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Done)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.ReadyForTest)]
    [InlineData(FeatureStatus.Done, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Done, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Done, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Rejected, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Rejected, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Rejected, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Done)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.InReview, FeatureStatus.Failed)]
    public async Task InvalidTransition_Returns_ValidationError_And_Leaves_Status_Unchanged(FeatureStatus from, FeatureStatus to)
    {
        var id = await SeedDefectAsync(from);
        var (mutation, handler) = CreateHandler();

        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: to, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("FEATURE_VALIDATION_ERROR:"));
        var persisted = await _dbContext!.Defects.FindAsync(id);
        persisted!.Status.Should().Be(from);
    }

    // -------------------------------------------------------------------------
    // Constraint: Done requires Result
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TransitionToDone_Fails_When_Result_Is_Missing()
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.High,
            Title = "Test Defect", Status = FeatureStatus.Testing,
            Result = null,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Done, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("A result must be provided"));
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(FeatureStatus.Testing);
    }

    [Fact]
    public async Task TransitionToDone_Succeeds_When_Result_Is_Present()
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.High,
            Title = "Test Defect", Status = FeatureStatus.Testing,
            Result = "Fixed the null reference",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Done, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(FeatureStatus.Done);
    }

    // -------------------------------------------------------------------------
    // Constraint: Failed requires Errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FeatureStatus.Ready)]
    [InlineData(FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Testing)]
    public async Task TransitionToFailed_Fails_When_Errors_Are_Missing(FeatureStatus from)
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.High,
            Title = "Test Defect", Status = from,
            Errors = null,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Failed, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("Errors must be documented"));
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(from);
    }

    [Fact]
    public async Task TransitionToFailed_Succeeds_When_Errors_Are_Present()
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.High,
            Title = "Test Defect", Status = FeatureStatus.Testing,
            Errors = "Regression detected in payment flow",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Failed, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(FeatureStatus.Failed);
    }

    // -------------------------------------------------------------------------
    // Constraint: Rejected requires OpenQuestions or Errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Ready)]
    [InlineData(FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Done)]
    [InlineData(FeatureStatus.Failed)]
    [InlineData(FeatureStatus.InReview)]
    public async Task TransitionToRejected_Fails_When_No_Reason_Provided(FeatureStatus from)
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.Medium,
            Title = "Test Defect", Status = from,
            OpenQuestions = null, Errors = null,
            Result = "result set to satisfy Done constraint if needed",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Rejected, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("reason must be provided"));
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(from);
    }

    [Theory]
    [InlineData("open question text", null)]
    [InlineData(null, "error text")]
    [InlineData("open question text", "error text")]
    public async Task TransitionToRejected_Succeeds_When_Reason_Is_Present(string? openQuestions, string? errors)
    {
        var id = Guid.NewGuid();
        _dbContext!.Defects.Add(new Defect
        {
            Id = id, ProjectId = _projectId, Severity = Severity.Medium,
            Title = "Test Defect", Status = FeatureStatus.Ready,
            OpenQuestions = openQuestions, Errors = errors,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var (mutation, handler) = CreateHandler();
        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Rejected, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Errors.Should().BeEmpty();
        var persisted = await _dbContext.Defects.FindAsync(id);
        persisted!.Status.Should().Be(FeatureStatus.Rejected);
    }

    // -------------------------------------------------------------------------
    // Audit event
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ValidTransition_Creates_AuditEvent_With_Correct_Data()
    {
        var id = await SeedDefectAsync(FeatureStatus.Planning);
        var (mutation, handler) = CreateHandler();

        await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: id, TargetStatus: FeatureStatus.Ready, Actor: "ci-pipeline"),
            handler,
            CancellationToken.None);

        var audit = await _dbContext!.AuditEvents.FirstOrDefaultAsync(a => a.EntityId == id);
        audit.Should().NotBeNull();
        audit!.EventType.Should().Be("StatusChanged");
        audit.Actor.Should().Be("ci-pipeline");
        audit.OldValue.Should().Be(FeatureStatus.Planning.ToString());
        audit.NewValue.Should().Be(FeatureStatus.Ready.ToString());
    }

    // -------------------------------------------------------------------------
    // Edge cases
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TransitionDefectStatus_Returns_NotFound_For_Unknown_Id()
    {
        var (mutation, handler) = CreateHandler();

        var result = await mutation.TransitionDefectStatusAsync(
            new TransitionDefectInput(Id: Guid.NewGuid(), TargetStatus: FeatureStatus.Ready, Actor: "operator"),
            handler,
            CancellationToken.None);

        result.Defect.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }
}
