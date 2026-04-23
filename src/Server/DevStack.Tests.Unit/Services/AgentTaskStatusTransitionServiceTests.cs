using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class AgentTaskStatusTransitionServiceTests
{
    [Fact]
    public void Transition_NullTask_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        AgentTask? task = null;

        var result = service.Transition(task!, AgentTaskStatus.InProgress, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("AgentTask is null");
    }

    [Fact]
    public void Transition_EmptyActor_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready };

        var result = service.Transition(task, AgentTaskStatus.InProgress, "");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Actor is required");
    }

    [Fact]
    public void Transition_WhitespaceActor_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready };

        var result = service.Transition(task, AgentTaskStatus.InProgress, "   ");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Actor is required");
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.NeedsReview)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.Rejected, AgentTaskStatus.Ready)]
    public void Transition_ValidTransition_NoConstraints_SucceedsAndUpdatesStatus(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        task.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Done)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.NeedsReview)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Done)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.NeedsReview)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.Done)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.NeedsReview)]
    [InlineData(AgentTaskStatus.Rejected, AgentTaskStatus.InProgress)]
    [InlineData(AgentTaskStatus.Rejected, AgentTaskStatus.Done)]
    [InlineData(AgentTaskStatus.Rejected, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Rejected, AgentTaskStatus.NeedsReview)]
    public void Transition_InvalidTransition_ReturnsFailure(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain($"Cannot transition from {from} to {to}");
        task.Status.Should().Be(from);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Rejected)]
    public void Transition_ToFailedOrRejected_RequiresErrors(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeFalse();
        task.Status.Should().Be(from);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.InProgress, AgentTaskStatus.Rejected)]
    [InlineData(AgentTaskStatus.NeedsReview, AgentTaskStatus.Rejected)]
    public void Transition_ToFailedOrRejected_WithErrors_Succeeds(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from, Errors = "Error details" };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(to);
    }

    [Fact]
    public void Transition_ToDone_FromNeedsReview_WithoutResult_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.NeedsReview, Result = null };

        var result = service.Transition(task, AgentTaskStatus.Done, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Result is required to mark a task as Done");
        task.Status.Should().Be(AgentTaskStatus.NeedsReview);
    }

    [Fact]
    public void Transition_ToDone_FromNeedsReview_WithEmptyResult_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.NeedsReview, Result = "" };

        var result = service.Transition(task, AgentTaskStatus.Done, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Result is required to mark a task as Done");
        task.Status.Should().Be(AgentTaskStatus.NeedsReview);
    }

    [Fact]
    public void Transition_ToDone_FromNeedsReview_WithWhitespaceResult_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.NeedsReview, Result = "   " };

        var result = service.Transition(task, AgentTaskStatus.Done, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Result is required to mark a task as Done");
        task.Status.Should().Be(AgentTaskStatus.NeedsReview);
    }

    [Fact]
    public void Transition_ToDone_FromNeedsReview_WithResult_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.NeedsReview, Result = "Completed successfully" };

        var result = service.Transition(task, AgentTaskStatus.Done, "user");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        task.Status.Should().Be(AgentTaskStatus.Done);
    }

    [Fact]
    public void Transition_ToFailed_FromReady_WithoutErrors_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready, Errors = null };

        var result = service.Transition(task, AgentTaskStatus.Failed, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Errors is required to mark a task as Failed");
        task.Status.Should().Be(AgentTaskStatus.Ready);
    }

    [Fact]
    public void Transition_ToFailed_FromReady_WithEmptyErrors_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready, Errors = "" };

        var result = service.Transition(task, AgentTaskStatus.Failed, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Errors is required to mark a task as Failed");
        task.Status.Should().Be(AgentTaskStatus.Ready);
    }

    [Fact]
    public void Transition_ToFailed_FromReady_WithErrors_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready, Errors = "Something went wrong" };

        var result = service.Transition(task, AgentTaskStatus.Failed, "user");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        task.Status.Should().Be(AgentTaskStatus.Failed);
    }

    [Fact]
    public void Transition_ToRejected_FromInProgress_WithoutErrors_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.InProgress, Errors = null };

        var result = service.Transition(task, AgentTaskStatus.Rejected, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Errors is required to mark a task as Rejected");
        task.Status.Should().Be(AgentTaskStatus.InProgress);
    }

    [Fact]
    public void Transition_ToRejected_FromInProgress_WithEmptyErrors_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.InProgress, Errors = "" };

        var result = service.Transition(task, AgentTaskStatus.Rejected, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Errors is required to mark a task as Rejected");
        task.Status.Should().Be(AgentTaskStatus.InProgress);
    }

    [Fact]
    public void Transition_ToRejected_FromInProgress_WithErrors_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.InProgress, Errors = "Rejected by reviewer" };

        var result = service.Transition(task, AgentTaskStatus.Rejected, "user");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        task.Status.Should().Be(AgentTaskStatus.Rejected);
    }

    [Fact]
    public void Transition_ToRejected_FromReady_WithErrors_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready, Errors = "Not suitable" };

        var result = service.Transition(task, AgentTaskStatus.Rejected, "user");

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(AgentTaskStatus.Rejected);
    }

    [Fact]
    public void Transition_ToRejected_FromFailed_WithErrors_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Failed, Errors = "Rejection reason" };

        var result = service.Transition(task, AgentTaskStatus.Rejected, "user");

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(AgentTaskStatus.Rejected);
    }

    [Fact]
    public void Transition_DoneStatus_AllowsNoTransitions()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Done };

        var result = service.Transition(task, AgentTaskStatus.Ready, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Should().Contain("Cannot transition from Done to Ready");
        task.Status.Should().Be(AgentTaskStatus.Done);
    }

    [Fact]
    public void Transition_DoneStatus_CannotTransitionToAnyStatus()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Done };

        var statuses = new[] {
            AgentTaskStatus.Ready,
            AgentTaskStatus.InProgress,
            AgentTaskStatus.NeedsReview,
            AgentTaskStatus.Failed,
            AgentTaskStatus.Rejected
        };

        foreach (var status in statuses)
        {
            var result = service.Transition(task, status, "user");
            result.IsSuccess.Should().BeFalse();
            task.Status.Should().Be(AgentTaskStatus.Done);
        }
    }

    [Fact]
    public void Transition_FromRejected_CanOnlyGoToReady()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Rejected };

        var readyResult = service.Transition(task, AgentTaskStatus.Ready, "user");
        readyResult.IsSuccess.Should().BeTrue();

        var doneResult = service.Transition(task, AgentTaskStatus.Done, "user");
        doneResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Transition_DifferentActors_Allowed()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready };

        var result1 = service.Transition(task, AgentTaskStatus.InProgress, "agent-1");
        var result2 = service.Transition(task, AgentTaskStatus.NeedsReview, "agent-2");

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(AgentTaskStatus.NeedsReview);
    }

    [Fact]
    public void Transition_ToFailed_FromInProgress_WithErrors_Succeeds()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.InProgress, Errors = "Error details" };

        var result = service.Transition(task, AgentTaskStatus.Failed, "user");

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(AgentTaskStatus.Failed);
    }

   [Fact]
    public void Transition_WithNullErrorsField_ToFailed_ReturnsFailure()
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = AgentTaskStatus.Ready, Errors = null };

        var result = service.Transition(task, AgentTaskStatus.Failed, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        task.Status.Should().Be(AgentTaskStatus.Ready);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.Ready)]
    [InlineData(AgentTaskStatus.Failed, AgentTaskStatus.InProgress)]
    public void Transition_FromFailed_ValidTransitions_Succeeds(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from, Errors = "Previous error" };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Failed)]
    [InlineData(AgentTaskStatus.Ready, AgentTaskStatus.Rejected)]
    public void Transition_FromReady_ToFailedOrRejected_RequiresErrors(AgentTaskStatus from, AgentTaskStatus to)
    {
        var service = new AgentTaskStatusTransitionService();
        var task = new AgentTask { Status = from };

        var result = service.Transition(task, to, "user");

        result.IsSuccess.Should().BeFalse();
        task.Status.Should().Be(from);
    }
}
