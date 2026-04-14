#nullable disable

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;

namespace DevStack.Domain.Services;

public readonly struct TransitionResult<T>
{
    private TransitionResult(T value, IReadOnlyList<string> errors)
    {
        Value = value;
        Errors = errors;
        IsSuccess = errors.Count == 0;
    }

    public T Value { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool IsSuccess { get; }

    public static TransitionResult<T> Success(T value) => new(value, Array.Empty<string>());
    public static TransitionResult<T> Failure(IReadOnlyList<string> errors) => new(default!, errors);
}

public sealed class FeatureStatusTransitionService
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
    private static readonly Dictionary<global::DevStack.Domain.Enums.FeatureStatus, List<global::DevStack.Domain.Enums.FeatureStatus>> _allowedTransitions = new()
        {
            { global::DevStack.Domain.Enums.FeatureStatus.Planning, new() { global::DevStack.Domain.Enums.FeatureStatus.Ready, global::DevStack.Domain.Enums.FeatureStatus.InProgress, global::DevStack.Domain.Enums.FeatureStatus.Rejected } },
            { global::DevStack.Domain.Enums.FeatureStatus.Ready, new() { global::DevStack.Domain.Enums.FeatureStatus.InProgress, global::DevStack.Domain.Enums.FeatureStatus.Failed, global::DevStack.Domain.Enums.FeatureStatus.Rejected } },
            { global::DevStack.Domain.Enums.FeatureStatus.InProgress, new() { global::DevStack.Domain.Enums.FeatureStatus.ReadyForTest, global::DevStack.Domain.Enums.FeatureStatus.Failed, global::DevStack.Domain.Enums.FeatureStatus.Rejected, global::DevStack.Domain.Enums.FeatureStatus.Planning } },
            { global::DevStack.Domain.Enums.FeatureStatus.ReadyForTest, new() { global::DevStack.Domain.Enums.FeatureStatus.Testing, global::DevStack.Domain.Enums.FeatureStatus.InProgress } },
            { global::DevStack.Domain.Enums.FeatureStatus.Testing, new() { global::DevStack.Domain.Enums.FeatureStatus.Done, global::DevStack.Domain.Enums.FeatureStatus.Failed, global::DevStack.Domain.Enums.FeatureStatus.InProgress } },
            { global::DevStack.Domain.Enums.FeatureStatus.Done, new() { global::DevStack.Domain.Enums.FeatureStatus.InProgress, global::DevStack.Domain.Enums.FeatureStatus.Rejected } },
            { global::DevStack.Domain.Enums.FeatureStatus.Failed, new() { global::DevStack.Domain.Enums.FeatureStatus.Ready, global::DevStack.Domain.Enums.FeatureStatus.InProgress, global::DevStack.Domain.Enums.FeatureStatus.Rejected } },
            { global::DevStack.Domain.Enums.FeatureStatus.Rejected, new() { global::DevStack.Domain.Enums.FeatureStatus.Planning, global::DevStack.Domain.Enums.FeatureStatus.Ready } },
            { global::DevStack.Domain.Enums.FeatureStatus.InReview, new() { global::DevStack.Domain.Enums.FeatureStatus.ReadyForTest, global::DevStack.Domain.Enums.FeatureStatus.Testing, global::DevStack.Domain.Enums.FeatureStatus.InProgress, global::DevStack.Domain.Enums.FeatureStatus.Rejected } }
        };

    public TransitionResult<Unit> Transition(WorkItem workItem, global::DevStack.Domain.Enums.FeatureStatus targetStatus, string actor)
    {
        var errors = new List<string>();

        if (workItem == null)
            errors.Add("WorkItem cannot be null.");

        if (string.IsNullOrWhiteSpace(actor))
            errors.Add("Actor (operator or workflow name) is required.");

        if (errors.Count > 0)
            return TransitionResult<Unit>.Failure(errors);

        // Check if transition is allowed
        if (!_allowedTransitions.TryGetValue(workItem.Status, out var allowedTargets) || !allowedTargets!.Contains(targetStatus))
        {
            errors.Add($"Transition from {workItem.Status} to {targetStatus} is not allowed.");
            return TransitionResult<Unit>.Failure(errors);
        }

        // Validate transition constraints
        var constraintErrors = ValidateConstraints(workItem, targetStatus);
        if (constraintErrors.Count > 0)
        {
            errors.AddRange(constraintErrors);
            return TransitionResult<Unit>.Failure(errors);
        }

        // Perform the transition
        var oldStatus = workItem.Status;
        workItem.Status = targetStatus;
        workItem.UpdatedAt = DateTime.UtcNow;

        // Emit domain event
        _domainEvents.Add(new FeatureStatusChangedEvent(workItem.Id, oldStatus, targetStatus, actor));

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private List<string> ValidateConstraints(WorkItem workItem, FeatureStatus targetStatus)
    {
        var errors = new List<string>();

        // Specific validation rules based on target status
        switch (targetStatus)
        {
            case FeatureStatus.Done:
                // Feature should have some work done - basic validation
                if (string.IsNullOrWhiteSpace(workItem.Result))
                    errors.Add("A result must be provided before marking a feature as Done.");
                break;

            case FeatureStatus.Failed:
                // Failed should include error information
                if (string.IsNullOrWhiteSpace(workItem.Errors))
                    errors.Add("Errors must be documented when a feature fails.");
                break;

            case FeatureStatus.Rejected:
                // Rejection should have a reason (open questions or errors)
                if (string.IsNullOrWhiteSpace(workItem.OpenQuestions) && string.IsNullOrWhiteSpace(workItem.Errors))
                    errors.Add("A reason must be provided when rejecting a feature (use OpenQuestions or Errors).");
                break;
        }

        return errors;
    }
}

// Unit type for C# - used when we only care about success/failure
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
