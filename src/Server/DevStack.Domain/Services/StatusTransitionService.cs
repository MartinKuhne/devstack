namespace DevStack.Domain.Services;

public static class StatusTransitionService
{
    /// <summary>
    /// Determines whether a transition from the source status to the target status is allowed.
    /// Returns true if the transition is valid according to the defined status machine rules.
    /// </summary>
    public static bool CanTransition(DeliverableStatus from, DeliverableStatus to)
    {
        if (from == to)
            return true;

        return AllowedTransitions.Contains((from, to));
    }

    private static readonly HashSet<(DeliverableStatus From, DeliverableStatus To)> AllowedTransitions = new()
    {
        #region Draft transitions
        (DeliverableStatus.Draft, DeliverableStatus.Design),
        (DeliverableStatus.Draft, DeliverableStatus.Failed),
        (DeliverableStatus.Draft, DeliverableStatus.Rejected),
        #endregion

        #region Design transitions
        (DeliverableStatus.Design, DeliverableStatus.Plan),
        (DeliverableStatus.Design, DeliverableStatus.Draft),
        (DeliverableStatus.Design, DeliverableStatus.Failed),
        (DeliverableStatus.Design, DeliverableStatus.Rejected),
        #endregion

        #region Plan transitions
        (DeliverableStatus.Plan, DeliverableStatus.Implement),
        (DeliverableStatus.Plan, DeliverableStatus.Design),
        (DeliverableStatus.Plan, DeliverableStatus.Failed),
        (DeliverableStatus.Plan, DeliverableStatus.Rejected),
        #endregion

        #region Implement transitions
        (DeliverableStatus.Implement, DeliverableStatus.Merge),
        (DeliverableStatus.Implement, DeliverableStatus.Plan),
        (DeliverableStatus.Implement, DeliverableStatus.Failed),
        #endregion

        #region Merge transitions
        (DeliverableStatus.Merge, DeliverableStatus.Deploy),
        (DeliverableStatus.Merge, DeliverableStatus.Implement),
        (DeliverableStatus.Merge, DeliverableStatus.Failed),
        #endregion

        #region Deploy transitions
        (DeliverableStatus.Deploy, DeliverableStatus.Test),
        (DeliverableStatus.Deploy, DeliverableStatus.Merge),
        (DeliverableStatus.Deploy, DeliverableStatus.Failed),
        #endregion

        #region Test transitions
        (DeliverableStatus.Test, DeliverableStatus.Done),
        (DeliverableStatus.Test, DeliverableStatus.NeedsReview),
        (DeliverableStatus.Test, DeliverableStatus.Failed),
        #endregion

        #region NeedsReview transitions
        (DeliverableStatus.NeedsReview, DeliverableStatus.Test),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Design),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Plan),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Implement),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Merge),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Deploy),
        (DeliverableStatus.NeedsReview, DeliverableStatus.Failed),
        #endregion

        #region Failed re-open transitions
        (DeliverableStatus.Failed, DeliverableStatus.Draft),
        (DeliverableStatus.Failed, DeliverableStatus.Design),
        (DeliverableStatus.Failed, DeliverableStatus.Plan),
        (DeliverableStatus.Failed, DeliverableStatus.Implement),
        (DeliverableStatus.Failed, DeliverableStatus.Merge),
        (DeliverableStatus.Failed, DeliverableStatus.Deploy),
        (DeliverableStatus.Failed, DeliverableStatus.Test),
        #endregion

        #region Rejected re-open transitions
        (DeliverableStatus.Rejected, DeliverableStatus.Draft),
        (DeliverableStatus.Rejected, DeliverableStatus.Design),
        (DeliverableStatus.Rejected, DeliverableStatus.Plan),
        (DeliverableStatus.Rejected, DeliverableStatus.Implement),
        (DeliverableStatus.Rejected, DeliverableStatus.Merge),
        (DeliverableStatus.Rejected, DeliverableStatus.Deploy),
        (DeliverableStatus.Rejected, DeliverableStatus.Test),
        #endregion
    };
}
