using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class TaskStatusTransitionServiceTests
{
    private readonly TaskStatusTransitionService _service = new();

    [Fact]
    public void Transition_Successfully_Changes_Status()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.Ready,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.InProgress, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(AgentTaskStatus.InProgress);
    }

    [Fact]
    public void Transition_Emits_Domain_Event()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.Ready,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.InProgress, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _service.DomainEvents.Should().HaveCount(1);
        var @event = _service.DomainEvents.First().Should().BeOfType<AgentTaskStatusChangedEvent>().Subject;
        @event.TaskId.Should().Be(task.Id);
        @event.OldStatus.Should().Be(AgentTaskStatus.Ready);
        @event.NewStatus.Should().Be(AgentTaskStatus.InProgress);
        @event.Actor.Should().Be("user@test.com");
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.NeedsReview)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Done)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.InProgress)]
   public void Allowed_Transitions_Work(AgentTaskStatus source, AgentTaskStatus target)
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Set required fields based on target status
        if (target == AgentTaskStatus.Done || target == AgentTaskStatus.Failed)
            task.Result = "Test result";
        if (target == AgentTaskStatus.Rejected)
        {
            task.RequiredFollowUps = "Test follow-ups";
        }

        // Act
        var result = _service.Transition(task, target, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(target);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Ready)] // Done is final state
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Done)] // Cannot skip intermediate states
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Rejected)] // Invalid transition
    public void Disallowed_Transitions_Fail(AgentTaskStatus source, AgentTaskStatus target)
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, target, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        task.Status.Should().Be(source); // Status unchanged
    }

    [Fact]
    public void Transition_Fails_When_Task_Is_Null()
    {
        // Act
        var result = _service.Transition(null!, AgentTaskStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Task cannot be null.");
    }

    [Fact]
    public void Transition_Fails_When_Actor_Is_Whitespace()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.Ready,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.InProgress, "   ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Actor (operator or workflow name) is required.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.NeedsReview,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5,
            Result = null // No result
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A result must be provided before marking a task as Done.");
    }

    [Fact]
    public void Transition_To_Failed_Requires_Result()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.InProgress,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5,
            Result = null // No result
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.Failed, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Results/errors must be documented when a task fails.");
    }

    [Fact]
       public void Transition_To_Rejected_Requires_Reason()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.Ready,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5,
            RequiredFollowUps = null,
            Result = null
        };

        // Act
        var result = _service.Transition(task, AgentTaskStatus.Rejected, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A reason must be provided when rejecting a task (use RequiredFollowUps or Result).");
    }

    [Fact]
    public void Transition_Updates_UtcTimestamp()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = AgentTaskStatus.Ready,
            Title = "Test Task",
            ItemId = Guid.NewGuid(),
            ComplexityRating = 5,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var originalUpdatedAt = task.UpdatedAt;

        // Act
        var result = _service.Transition(task, AgentTaskStatus.InProgress, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
