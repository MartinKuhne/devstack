using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class FeatureStatusTransitionServiceTests
{
    private readonly FeatureStatusTransitionService _service = new();

    [Fact]
    public void Transition_Successfully_Changes_Status()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Planning,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        feature.Status.Should().Be(FeatureStatus.Ready);
    }

    [Fact]
    public void Transition_Emits_Domain_Event()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Planning,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _service.DomainEvents.Should().HaveCount(1);
        var @event = _service.DomainEvents.First().Should().BeOfType<FeatureStatusChangedEvent>().Subject;
        @event.FeatureId.Should().Be(feature.Id);
        @event.OldStatus.Should().Be(FeatureStatus.Planning);
        @event.NewStatus.Should().Be(FeatureStatus.Ready);
        @event.Actor.Should().Be("user@test.com");
    }

    [Theory]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Done, FeatureStatus.InProgress)] // Rework
   public void Allowed_Transitions_Work(FeatureStatus source, FeatureStatus target)
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid()
        };

        // Set required fields based on target status
        if (target == FeatureStatus.Done || target == FeatureStatus.Failed)
            feature.Result = "Test result";
        if (target == FeatureStatus.Rejected)
        {
            feature.Errors = "Test errors";
        }

        // Act
        var result = _service.Transition(feature, target, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        feature.Status.Should().Be(target);
    }

    [Theory]
    [InlineData(FeatureStatus.Done, FeatureStatus.Planning)] // Done cannot go back to Planning
    [InlineData(FeatureStatus.Planning, FeatureStatus.Done)] // Cannot skip intermediate states
    [InlineData(FeatureStatus.Testing, FeatureStatus.Planning)] // Invalid transition
    public void Disallowed_Transitions_Fail(FeatureStatus source, FeatureStatus target)
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(feature, target, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        feature.Status.Should().Be(source); // Status unchanged
    }

    [Fact]
    public void Transition_Fails_When_WorkItem_Is_Null()
    {
        // Act
        var result = _service.Transition(null!, FeatureStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("WorkItem cannot be null.");
    }

    [Fact]
    public void Transition_Fails_When_Actor_Is_Whitespace()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Planning,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Ready, "   ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Actor (operator or workflow name) is required.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Testing,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid(),
            Result = null // No result
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A result must be provided before marking a feature as Done.");
    }

    [Fact]
    public void Transition_To_Failed_Requires_Errors()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Testing,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid(),
            Errors = null // No errors
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Failed, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Errors must be documented when a feature fails.");
    }

    [Fact]
    public void Transition_To_Rejected_Requires_Reason()
   {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Ready,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid(),
            OpenQuestions = null,
            Errors = null
        };

        // Act
        var result = _service.Transition(feature, FeatureStatus.Rejected, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A reason must be provided when rejecting a feature (use OpenQuestions or Errors).");
    }

    [Fact]
    public void Transition_Updates_UtcTimestamp()
    {
        // Arrange
        var feature = new Feature
        {
            Id = Guid.NewGuid(),
            Status = FeatureStatus.Planning,
            Title = "Test Feature",
            ProjectId = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var originalUpdatedAt = feature.UpdatedAt;

        // Act
        var result = _service.Transition(feature, FeatureStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        feature.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
