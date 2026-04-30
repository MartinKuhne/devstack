using DevStack.Domain.Enums;
using DevStack.Domain.Services;

using FluentAssertions;

using Xunit;

public class StatusTransitionServiceTests
{
    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Merge, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Deploy, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.NeedsReview)]
    public void CanTransition_SameStatus_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Merge, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Deploy, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Done)]
    public void CanTransition_ForwardProgression_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Merge, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Deploy, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Failed)]
    public void CanTransition_ToFailedFromActiveState_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Rejected)]
    public void CanTransition_ToRejectedFromEarlyStates_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Plan)]
    public void CanTransition_Regression_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.NeedsReview)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Test)]
    public void CanTransition_BetweenTestAndNeedsReview_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Deploy)]
    public void CanTransition_FromNeedsReviewToActiveState_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Test)]
    public void CanTransition_FromFailedToActiveState_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Test)]
    public void CanTransition_FromRejectedToActiveState_ReturnsTrue(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Test)]
    public void CanTransition_FromDoneToActiveState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Draft, DeliverableStatus.Done)]
    public void CanTransition_DraftToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Design, DeliverableStatus.Done)]
    public void CanTransition_DesignToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Merge)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Plan, DeliverableStatus.Done)]
    public void CanTransition_PlanToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Deploy)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Test)]
    [InlineData(DeliverableStatus.Implement, DeliverableStatus.Done)]
    public void CanTransition_ImplementToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Merge, DeliverableStatus.Done)]
    public void CanTransition_MergeToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Deploy, DeliverableStatus.Done)]
    public void CanTransition_DeployToNonAdjacentState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Design)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Plan)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Implement)]
    [InlineData(DeliverableStatus.Test, DeliverableStatus.Merge)]
    public void CanTransition_TestToEarlyState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Draft)]
    [InlineData(DeliverableStatus.NeedsReview, DeliverableStatus.Done)]
    public void CanTransition_NeedsReviewToInvalidState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Failed, DeliverableStatus.NeedsReview)]
    public void CanTransition_FromFailedToTerminalState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Done)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Rejected, DeliverableStatus.NeedsReview)]
    public void CanTransition_FromRejectedToTerminalState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Failed)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.Rejected)]
    [InlineData(DeliverableStatus.Done, DeliverableStatus.NeedsReview)]
    public void CanTransition_FromDoneToOtherTerminalState_ReturnsFalse(DeliverableStatus from, DeliverableStatus to)
    {
        var result = StatusTransitionService.CanTransition(from, to);
        result.Should().BeFalse();
    }
}
