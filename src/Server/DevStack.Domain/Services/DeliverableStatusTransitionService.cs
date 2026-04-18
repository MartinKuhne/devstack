#nullable disable
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;

namespace DevStack.Domain.Services;

public sealed class DeliverableStatusTransitionService(bool limitStatusTransitions = false)
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Allowed transitions for each DeliverableStatus.
    /// Draft -> Ready, Planning
    /// Planning -> Ready, InProgress, Rejected
    /// Ready -> InProgress, Failed, Rejected
    /// InProgress -> Done, Failed, Rejected, NeedsReview
    /// Done -> InProgress (for rework), Rejected
    /// Failed -> Ready, InProgress, Rejected
    /// Rejected -> Planning, Ready
    /// NeedsReview -> Done, InProgress, Rejected
    /// </summary>
    private static readonly Dictionary<DeliverableStatus, List<DeliverableStatus>> _allowedTransitions = new()
    {
        { DeliverableStatus.Draft, new() { DeliverableStatus.Ready, DeliverableStatus.Planning } },
        { DeliverableStatus.Planning, new() { DeliverableStatus.Ready, DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Ready, new() { DeliverableStatus.InProgress, DeliverableStatus.Failed, DeliverableStatus.Rejected } },
        { DeliverableStatus.InProgress, new() { DeliverableStatus.Done, DeliverableStatus.Failed, DeliverableStatus.Rejected, DeliverableStatus.NeedsReview } },
        { DeliverableStatus.Done, new() { DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Failed, new() { DeliverableStatus.Ready, DeliverableStatus.InProgress, DeliverableStatus.Rejected } },
        { DeliverableStatus.Rejected, new() { DeliverableStatus.Planning, DeliverableStatus.Ready } },
        { DeliverableStatus.NeedsReview, new() { DeliverableStatus.Done, DeliverableStatus.InProgress, DeliverableStatus.Rejected } }
    };

    public TransitionResult<Unit> Transition(Item item, DeliverableStatus targetStatus, string actor)
    {
        var errors = new List<string>();

        if (item == null)
            errors.Add("Item cannot be null.");

        if (string.IsNullOrWhiteSpace(actor))
            errors.Add("Actor (operator or workflow name) is required.");

        if (errors.Count > 0)
            return TransitionResult<Unit>.Failure(errors);

        // Check if transition is allowed
        if (limitStatusTransitions &&
            (!_allowedTransitions.TryGetValue(GetDeliverableStatus(item), out var allowedTargets) || !allowedTargets.Contains(targetStatus)))
        {
            errors.Add($"Transition from {GetDeliverableStatus(item)} to {targetStatus} is not allowed.");
            return TransitionResult<Unit>.Failure(errors);
        }

        // Validate transition constraints
        var constraintErrors = ValidateConstraints(item, targetStatus);
        if (constraintErrors.Count > 0)
        {
            errors.AddRange(constraintErrors);
            return TransitionResult<Unit>.Failure(errors);
        }

        // Perform the transition
        var oldStatus = GetDeliverableStatus(item);
        SetDeliverableStatus(item, targetStatus);
        item.UpdatedAt = DateTime.UtcNow;

        // Emit domain event (convert back to FeatureStatus for compatibility)
        var oldFeatureStatus = MapDeliverableStatusToFeatureStatus(oldStatus);
        var newFeatureStatus = MapDeliverableStatusToFeatureStatus(targetStatus);
        _domainEvents.Add(new ItemStatusChangedEvent(item.Id, oldFeatureStatus, newFeatureStatus, actor));

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private static DeliverableStatus GetDeliverableStatus(Item item)
    {
        return item.Status switch
        {
            FeatureStatus.Planning => DeliverableStatus.Planning,
            FeatureStatus.Ready => DeliverableStatus.Ready,
            FeatureStatus.InProgress => DeliverableStatus.InProgress,
            FeatureStatus.Done => DeliverableStatus.Done,
            FeatureStatus.Failed => DeliverableStatus.Failed,
            FeatureStatus.Rejected => DeliverableStatus.Rejected,
            FeatureStatus.InReview => DeliverableStatus.NeedsReview,
            _ => DeliverableStatus.Draft
        };
    }

    private static void SetDeliverableStatus(Item item, DeliverableStatus status)
    {
        item.Status = status switch
        {
            DeliverableStatus.Planning => FeatureStatus.Planning,
            DeliverableStatus.Ready => FeatureStatus.Ready,
            DeliverableStatus.InProgress => FeatureStatus.InProgress,
            DeliverableStatus.Done => FeatureStatus.Done,
            DeliverableStatus.Failed => FeatureStatus.Failed,
            DeliverableStatus.Rejected => FeatureStatus.Rejected,
            DeliverableStatus.NeedsReview => FeatureStatus.InReview,
            _ => FeatureStatus.Planning
        };
    }

    private static FeatureStatus MapDeliverableStatusToFeatureStatus(DeliverableStatus status)
    {
        return status switch
        {
            DeliverableStatus.Planning => FeatureStatus.Planning,
            DeliverableStatus.Ready => FeatureStatus.Ready,
            DeliverableStatus.InProgress => FeatureStatus.InProgress,
            DeliverableStatus.Done => FeatureStatus.Done,
            DeliverableStatus.Failed => FeatureStatus.Failed,
            DeliverableStatus.Rejected => FeatureStatus.Rejected,
            DeliverableStatus.NeedsReview => FeatureStatus.InReview,
            DeliverableStatus.Draft => FeatureStatus.Planning,
            _ => FeatureStatus.Planning
        };
    }

    private List<string> ValidateConstraints(Item item, DeliverableStatus targetStatus)
    {
        var errors = new List<string>();

        // Subtype-specific validation
        switch (item.ItemType)
        {
            case ItemSubtype.Feature:
                errors.AddRange(ValidateFeatureConstraints(item, targetStatus));
                break;
            case ItemSubtype.Defect:
                errors.AddRange(ValidateDefectConstraints(item, targetStatus));
                break;
            case ItemSubtype.Maintenance:
                errors.AddRange(ValidateMaintenanceConstraints(item, targetStatus));
                break;
        }

        return errors;
    }

    private List<string> ValidateFeatureConstraints(Item item, DeliverableStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case DeliverableStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A result must be provided before marking a feature as Done.");
                break;

            case DeliverableStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when a feature fails.");
                break;

            case DeliverableStatus.Rejected:
                if (string.IsNullOrWhiteSpace(item.OpenQuestions) && string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("A reason must be provided when rejecting a feature (use OpenQuestions or Errors).");
                break;
        }

        return errors;
    }

    private List<string> ValidateDefectConstraints(Item item, DeliverableStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case DeliverableStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A resolution must be provided before marking a defect as Done.");
                if (item.Severity == null)
                    errors.Add("Severity must be set for defects.");
                if (string.IsNullOrWhiteSpace(item.RootCause))
                    errors.Add("Root cause must be documented for defects.");
                break;

            case DeliverableStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when defect resolution fails.");
                break;

            case DeliverableStatus.Rejected:
                if (string.IsNullOrWhiteSpace(item.OpenQuestions) && string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("A reason must be provided when rejecting a defect.");
                break;
        }

        return errors;
    }

    private List<string> ValidateMaintenanceConstraints(Item item, DeliverableStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case DeliverableStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A description of changes must be provided before marking maintenance as Done.");
                break;

            case DeliverableStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when maintenance fails.");
                break;
        }

        return errors;
    }
}
