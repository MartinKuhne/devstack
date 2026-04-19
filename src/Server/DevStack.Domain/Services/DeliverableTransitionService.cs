#nullable disable
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Services;

public sealed class DeliverableStatusTransitionService(bool limitStatusTransitions = false)
{
    private static readonly Dictionary<DeliverableStatus, List<DeliverableStatus>> _allowedTransitions = new()
    {
        { DeliverableStatus.Draft, new() { DeliverableStatus.Planning } },
        { DeliverableStatus.Planning, new() { DeliverableStatus.Ready, DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Ready, new() { DeliverableStatus.InProgress, DeliverableStatus.Failed, DeliverableStatus.Rejected } },
        { DeliverableStatus.InProgress, new() { DeliverableStatus.Done, DeliverableStatus.Failed, DeliverableStatus.Rejected, DeliverableStatus.NeedsReview } },
        { DeliverableStatus.Done, new() { DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Failed, new() { DeliverableStatus.Ready, DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Rejected, new() { DeliverableStatus.Planning, DeliverableStatus.Ready } },
        { DeliverableStatus.NeedsReview, new() { DeliverableStatus.Done, DeliverableStatus.InProgress, DeliverableStatus.Rejected } }
    };

    public TransitionResult<Unit> Transition(Deliverable deliverable, DeliverableStatus targetStatus, string actor)
    {
        var errors = new List<string>();

        if (deliverable == null)
            errors.Add("Deliverable cannot be null.");

        if (string.IsNullOrWhiteSpace(actor))
            errors.Add("Actor (operator or workflow name) is required.");

        if (errors.Count > 0)
            return TransitionResult<Unit>.Failure(errors);

        var currentStatus = deliverable.Status;
        var allowed = _allowedTransitions.GetValueOrDefault(currentStatus, []);

        if (limitStatusTransitions && !allowed.Contains(targetStatus))
        {
            errors.Add($"Cannot transition from {currentStatus} to {targetStatus}. Allowed transitions: {string.Join(", ", allowed)}");
            return TransitionResult<Unit>.Failure(errors);
        }

        if (!allowed.Contains(targetStatus))
        {
            errors.Add($"Cannot transition from {currentStatus} to {targetStatus}. Allowed transitions: {string.Join(", ", allowed)}");
            return TransitionResult<Unit>.Failure(errors);
        }

        deliverable.Status = targetStatus;

        return TransitionResult<Unit>.Success(Unit.Value);
    }
}
