using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class DeliverableTransitionServiceTests
{
    [Fact]
    public void Transition_NullDeliverable_ReturnsFailure()
    {
        var service = new DeliverableStatusTransitionService();
        Deliverable? deliverable = null;

        var result = service.Transition(deliverable!, DeliverableStatus.Planning, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Deliverable cannot be null.");
    }

    [Fact]
    public void Transition_EmptyActor_ReturnsFailure()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var result = service.Transition(deliverable, DeliverableStatus.Planning, "");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Actor (operator or workflow name) is required.");
    }

    [Fact]
    public void Transition_WhitespaceActor_ReturnsFailure()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var result = service.Transition(deliverable, DeliverableStatus.Planning, "   ");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Actor (operator or workflow name) is required.");
    }

    [Fact]
    public void Transition_NullDeliverableAndEmptyActor_ReturnsBothErrors()
    {
        var service = new DeliverableStatusTransitionService();
        Deliverable? deliverable = null;

        var result = service.Transition(deliverable!, DeliverableStatus.Planning, "");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Should().Be("Deliverable cannot be null.");
        result.Errors[1].Should().Be("Actor (operator or workflow name) is required.");
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Rejected)]
    public void Transition_ValidTransition_SucceedsAndUpdatesStatus(DeliverableStatus from, DeliverableStatus to)
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = from };

        var result = service.Transition(deliverable, to, "user");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        deliverable.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Ready, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.NeedsReview)]
     [InlineData(DeliverableStatus.Failed, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.InProgress)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Ready)]
    public void Transition_InvalidTransition_ReturnsFailure(DeliverableStatus from, DeliverableStatus to)
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = from };

        var result = service.Transition(deliverable, to, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain($"Cannot transition from {from} to {to}");
        deliverable.Status.Should().Be(from);
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Planning)]
    [InlineData(DeliverableStatus.Planning, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.InProgress, DeliverableStatus.Done)]
    public void Transition_LimitStatusTransitionsEnabled_ValidTransition_Succeeds(DeliverableStatus from, DeliverableStatus to)
    {
        var service = new DeliverableStatusTransitionService(limitStatusTransitions: true);
        var deliverable = new Deliverable { Status = from };

        var result = service.Transition(deliverable, to, "user");

        result.IsSuccess.Should().BeTrue();
        deliverable.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Ready)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Planning)]
    public void Transition_LimitStatusTransitionsEnabled_InvalidTransition_ReturnsFailure(DeliverableStatus from, DeliverableStatus to)
    {
        var service = new DeliverableStatusTransitionService(limitStatusTransitions: true);
        var deliverable = new Deliverable { Status = from };

        var result = service.Transition(deliverable, to, "user");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain($"Cannot transition from {from} to {to}");
        result.Errors[0].Should().Contain("Allowed transitions:");
        deliverable.Status.Should().Be(from);
    }

    [Fact]
    public void GetValidTransitions_Draft_ReturnsPlanning()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Planning" });
    }

    [Fact]
    public void GetValidTransitions_Planning_ReturnsReadyInProgressRejected()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Planning };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Ready", "InProgress", "Rejected" });
    }

    [Fact]
    public void GetValidTransitions_Ready_ReturnsInProgressFailedRejected()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Ready };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "InProgress", "Failed", "Rejected" });
    }

    [Fact]
    public void GetValidTransitions_InProgress_ReturnsDoneFailedRejectedNeedsReview()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.InProgress };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Done", "Failed", "Rejected", "NeedsReview" });
    }

    [Fact]
    public void GetValidTransitions_Done_ReturnsInProgressRejected()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Done };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "InProgress", "Rejected" });
    }

    [Fact]
    public void GetValidTransitions_Failed_ReturnsReadyInProgressRejected()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Failed };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Ready", "InProgress", "Rejected" });
    }

    [Fact]
    public void GetValidTransitions_Rejected_ReturnsPlanningReady()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Rejected };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Planning", "Ready" });
    }

    [Fact]
    public void GetValidTransitions_NeedsReview_ReturnsDoneInProgressRejected()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.NeedsReview };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().BeEquivalentTo(new[] { "Done", "InProgress", "Rejected" });
    }

    [Fact]
    public void GetValidTransitions_ReturnsImmutableList()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Planning };

        var validTransitions = service.GetValidTransitions(deliverable);

        validTransitions.Should().NotBeNull();
        validTransitions.Should().NotBeEmpty();
    }

    [Fact]
    public void Transition_DifferentActors_Allowed()
    {
        var service = new DeliverableStatusTransitionService();
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var result1 = service.Transition(deliverable, DeliverableStatus.Planning, "agent-1");
        var result2 = service.Transition(deliverable, DeliverableStatus.Ready, "agent-2");

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        deliverable.Status.Should().Be(DeliverableStatus.Ready);
    }

    [Fact]
    public void Transition_WithLimitStatusTransitions_True_AllowsValidTransitions()
    {
        var service = new DeliverableStatusTransitionService(limitStatusTransitions: true);
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var result = service.Transition(deliverable, DeliverableStatus.Planning, "user");

        result.IsSuccess.Should().BeTrue();
        deliverable.Status.Should().Be(DeliverableStatus.Planning);
    }

    [Fact]
    public void Transition_WithLimitStatusTransitions_False_AllowsValidTransitions()
    {
        var service = new DeliverableStatusTransitionService(limitStatusTransitions: false);
        var deliverable = new Deliverable { Status = DeliverableStatus.Draft };

        var result = service.Transition(deliverable, DeliverableStatus.Planning, "user");

        result.IsSuccess.Should().BeTrue();
        deliverable.Status.Should().Be(DeliverableStatus.Planning);
    }
}
