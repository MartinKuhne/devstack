using DevStack.Domain.Entities;
using DevStack.Domain.Events;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;

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
            Status = TaskStatus.Planning,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskStatus.Ready);
    }

    [Fact]
    public void Transition_Emits_Domain_Event()
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = TaskStatus.Planning,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _service.DomainEvents.Should().HaveCount(1);
        var @event = _service.DomainEvents.First().Should().BeOfType<TaskStatusChangedEvent>().Subject;
        @event.TaskId.Should().Be(task.Id);
        @event.OldStatus.Should().Be(TaskStatus.Planning);
        @event.NewStatus.Should().Be(TaskStatus.Ready);
        @event.Actor.Should().Be("user@test.com");
    }

    [Theory]
    [InlineData(TaskStatus.Planning, TaskStatus.Ready)]
    [InlineData(TaskStatus.Planning, TaskStatus.Prepare)]
    [InlineData(TaskStatus.Planning, TaskStatus.Rejected)]
    [InlineData(TaskStatus.Ready, TaskStatus.Prepare)]
    [InlineData(TaskStatus.Prepare, TaskStatus.Code)]
    [InlineData(TaskStatus.Code, TaskStatus.Review)]
    [InlineData(TaskStatus.Review, TaskStatus.ReadyForTest)]
    [InlineData(TaskStatus.ReadyForTest, TaskStatus.Testing)]
    [InlineData(TaskStatus.Testing, TaskStatus.Done)]
   public void Allowed_Transitions_Work(TaskStatus source, TaskStatus target)
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Set required fields based on target status
        if (target == TaskStatus.Done || target == TaskStatus.Failed)
            task.Result = "Test result";
        if (target == TaskStatus.Rejected)
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
    [InlineData(TaskStatus.Done, TaskStatus.Planning)] // Done is final state
    [InlineData(TaskStatus.Planning, TaskStatus.Done)] // Cannot skip intermediate states
    [InlineData(TaskStatus.Testing, TaskStatus.Planning)] // Invalid transition
    public void Disallowed_Transitions_Fail(TaskStatus source, TaskStatus target)
    {
        // Arrange
        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Status = source,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
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
        var result = _service.Transition(null!, TaskStatus.Ready, "actor");

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
            Status = TaskStatus.Planning,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Ready, "   ");

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
            Status = TaskStatus.Testing,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5,
            Result = null // No result
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Done, "actor");

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
            Status = TaskStatus.Testing,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5,
            Result = null // No result
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Failed, "actor");

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
            Status = TaskStatus.Ready,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5,
            RequiredFollowUps = null,
            Result = null
        };

        // Act
        var result = _service.Transition(task, TaskStatus.Rejected, "actor");

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
            Status = TaskStatus.Planning,
            Title = "Test Task",
            FeatureId = Guid.NewGuid(),
            ComplexityRating = 5,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var originalUpdatedAt = task.UpdatedAt;

        // Act
        var result = _service.Transition(task, TaskStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
