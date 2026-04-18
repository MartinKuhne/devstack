#nullable disable

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;

namespace DevStack.Domain.Services;

[Obsolete("Use ItemStatusTransitionService with Item.Subtype=Task instead")]
public sealed class TaskStatusTransitionService
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private static readonly Dictionary<AgentTaskStatus, List<AgentTaskStatus>> _allowedTransitions = new()
    {
        { AgentTaskStatus.Ready, new() { AgentTaskStatus.InProgress, AgentTaskStatus.Failed, AgentTaskStatus.Rejected } },
        { AgentTaskStatus.InProgress, new() { AgentTaskStatus.NeedsReview, AgentTaskStatus.Failed, AgentTaskStatus.Rejected } },
        { AgentTaskStatus.NeedsReview, new() { AgentTaskStatus.InProgress, AgentTaskStatus.Ready, AgentTaskStatus.Rejected, AgentTaskStatus.Done } },
        { AgentTaskStatus.Done, new() { } },
        { AgentTaskStatus.Failed, new() { AgentTaskStatus.Ready, AgentTaskStatus.InProgress, AgentTaskStatus.Rejected } },
        { AgentTaskStatus.Rejected, new() { AgentTaskStatus.Ready } }
    };

    public TransitionResult<Unit> Transition(AgentTask task, AgentTaskStatus targetStatus, string actor)
    {
        var errors = new List<string>();

        if (task == null)
            errors.Add("Task cannot be null.");

        if (string.IsNullOrWhiteSpace(actor))
            errors.Add("Actor (operator or workflow name) is required.");

        if (errors.Count > 0)
            return TransitionResult<Unit>.Failure(errors);

        if (!_allowedTransitions.TryGetValue(task.Status, out var allowedTargets))
        {
            errors.Add($"Transition from {task.Status} to {targetStatus} is not allowed.");
            return TransitionResult<Unit>.Failure(errors);
        }

        if (!allowedTargets!.Contains(targetStatus))
        {
            errors.Add($"Transition from {task.Status} to {targetStatus} is not allowed.");
            return TransitionResult<Unit>.Failure(errors);
        }

        var constraintErrors = ValidateConstraints(task, targetStatus);
        if (constraintErrors.Count > 0)
        {
            errors.AddRange(constraintErrors);
            return TransitionResult<Unit>.Failure(errors);
        }

        var oldStatus = task.Status;
        task.Status = targetStatus;
        task.UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new AgentTaskStatusChangedEvent(task.Id, oldStatus, targetStatus, actor));

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private List<string> ValidateConstraints(AgentTask task, AgentTaskStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case AgentTaskStatus.Done:
                if (string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("A result must be provided before marking a task as Done.");
                break;

            case AgentTaskStatus.Failed:
                if (string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("Results/errors must be documented when a task fails.");
                break;

            case AgentTaskStatus.Rejected:
                if (string.IsNullOrWhiteSpace(task.RequiredFollowUps) && string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("A reason must be provided when rejecting a task (use RequiredFollowUps or Result).");
                break;
        }

        return errors;
    }
}
