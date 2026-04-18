#nullable disable

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;

namespace DevStack.Domain.Services;

public sealed class ItemStatusTransitionService(bool limitFeatureStatusTransitions = false)
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Allowed transitions for each FeatureStatus.
    /// Planning -> Ready, InProgress, Rejected
    /// Ready -> InProgress, Failed, Rejected
    /// InProgress -> ReadyForTest, Failed, Rejected, Planning (for rework)
    /// ReadyForTest -> Testing, InProgress (back to development)
    /// Testing -> Done, Failed, InProgress (back to development)
    /// Done -> InProgress (for rework), Rejected (if issues found post-release)
    /// Failed -> Ready, InProgress, Rejected
    /// Rejected -> Planning (for re-evaluation), Ready
    /// InReview -> ReadyForTest, Testing, InProgress (back to development), Rejected
    /// </summary>
    private static readonly Dictionary<FeatureStatus, List<FeatureStatus>> _allowedTransitions = new()
    {
        { FeatureStatus.Planning, new() { FeatureStatus.Ready, FeatureStatus.InProgress, FeatureStatus.Rejected } },
        { FeatureStatus.Ready, new() { FeatureStatus.InProgress, FeatureStatus.Failed, FeatureStatus.Rejected } },
        { FeatureStatus.InProgress, new() { FeatureStatus.ReadyForTest, FeatureStatus.Failed, FeatureStatus.Rejected, FeatureStatus.Planning } },
        { FeatureStatus.ReadyForTest, new() { FeatureStatus.Testing, FeatureStatus.InProgress } },
        { FeatureStatus.Testing, new() { FeatureStatus.Done, FeatureStatus.Failed, FeatureStatus.InProgress } },
        { FeatureStatus.Done, new() { FeatureStatus.InProgress, FeatureStatus.Rejected } },
        { FeatureStatus.Failed, new() { FeatureStatus.Ready, FeatureStatus.InProgress, FeatureStatus.Rejected } },
        { FeatureStatus.Rejected, new() { FeatureStatus.Planning, FeatureStatus.Ready } },
        { FeatureStatus.InReview, new() { FeatureStatus.ReadyForTest, FeatureStatus.Testing, FeatureStatus.InProgress, FeatureStatus.Rejected } }
    };

    public TransitionResult<Unit> Transition(Item item, FeatureStatus targetStatus, string actor)
    {
        var errors = new List<string>();

        if (item == null)
            errors.Add("Item cannot be null.");

        if (string.IsNullOrWhiteSpace(actor))
            errors.Add("Actor (operator or workflow name) is required.");

        if (errors.Count > 0)
            return TransitionResult<Unit>.Failure(errors);

        // Check if transition is allowed
        if (limitFeatureStatusTransitions &&
            (!_allowedTransitions.TryGetValue(item.Status, out var allowedTargets) || !allowedTargets.Contains(targetStatus)))
        {
            errors.Add($"Transition from {item.Status} to {targetStatus} is not allowed.");
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
        var oldStatus = item.Status;
        item.Status = targetStatus;
        item.UpdatedAt = DateTime.UtcNow;

        // Emit domain event
        _domainEvents.Add(new ItemStatusChangedEvent(item.Id, oldStatus, targetStatus, actor));

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private List<string> ValidateConstraints(Item item, FeatureStatus targetStatus)
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
            case ItemSubtype.Enabler:
                errors.AddRange(ValidateEnablerConstraints(item, targetStatus));
                break;
        }

        return errors;
    }

    private List<string> ValidateFeatureConstraints(Item item, FeatureStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case FeatureStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A result must be provided before marking a feature as Done.");
                break;

            case FeatureStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when a feature fails.");
                break;

            case FeatureStatus.Rejected:
                if (string.IsNullOrWhiteSpace(item.OpenQuestions) && string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("A reason must be provided when rejecting a feature (use OpenQuestions or Errors).");
                break;
        }

        return errors;
    }

    private List<string> ValidateDefectConstraints(Item item, FeatureStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case FeatureStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A resolution must be provided before marking a defect as Done.");
                if (item.Severity == null)
                    errors.Add("Severity must be set for defects.");
                if (string.IsNullOrWhiteSpace(item.RootCause))
                    errors.Add("Root cause must be documented for defects.");
                break;

            case FeatureStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when defect resolution fails.");
                break;

            case FeatureStatus.Rejected:
                if (string.IsNullOrWhiteSpace(item.OpenQuestions) && string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("A reason must be provided when rejecting a defect.");
                break;
        }

        return errors;
    }

    private List<string> ValidateMaintenanceConstraints(Item item, FeatureStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case FeatureStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A description of changes must be provided before marking maintenance as Done.");
                break;

            case FeatureStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when maintenance fails.");
                break;
        }

        return errors;
    }

    private List<string> ValidateEnablerConstraints(Item item, FeatureStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case FeatureStatus.Done:
                if (string.IsNullOrWhiteSpace(item.Result))
                    errors.Add("A description of the enabler outcome must be provided before marking as Done.");
                break;

            case FeatureStatus.Failed:
                if (string.IsNullOrWhiteSpace(item.Errors))
                    errors.Add("Errors must be documented when an enabler fails.");
                break;
        }

        return errors;
    }
}
