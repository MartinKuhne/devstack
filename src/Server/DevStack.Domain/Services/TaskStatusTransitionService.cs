#nullable disable

using DevStack.Domain.Entities;
using DevStack.Domain.Events;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;

namespace DevStack.Domain.Services;

public sealed class TaskStatusTransitionService
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private static readonly Dictionary<TaskStatus, List<TaskStatus>> _allowedTransitions = new()
    {
        { TaskStatus.Planning, new() { TaskStatus.Ready, TaskStatus.Prepare, TaskStatus.Rejected } },
        { TaskStatus.Ready, new() { TaskStatus.Prepare, TaskStatus.Failed, TaskStatus.Rejected } },
        { TaskStatus.Prepare, new() { TaskStatus.Code, TaskStatus.Ready, TaskStatus.Rejected } },
        { TaskStatus.Code, new() { TaskStatus.Review, TaskStatus.Prepare, TaskStatus.Failed, TaskStatus.Rejected } },
        { TaskStatus.Review, new() { TaskStatus.Code, TaskStatus.ReadyForTest, TaskStatus.Rejected } },
        { TaskStatus.ReadyForTest, new() { TaskStatus.Testing, TaskStatus.Code } },
        { TaskStatus.Testing, new() { TaskStatus.Done, TaskStatus.Failed, TaskStatus.Code, TaskStatus.Review } },
        { TaskStatus.Done, new() { } },
        { TaskStatus.Failed, new() { TaskStatus.Ready, TaskStatus.Prepare, TaskStatus.Code, TaskStatus.Review, TaskStatus.Rejected } },
        { TaskStatus.Rejected, new() { TaskStatus.Planning, TaskStatus.Ready } },
        { TaskStatus.InReview, new() { TaskStatus.ReadyForTest, TaskStatus.Testing, TaskStatus.Code, TaskStatus.Review, TaskStatus.Rejected } }
    };

    public TransitionResult<Unit> Transition(AgentTask task, TaskStatus targetStatus, string actor)
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

        _domainEvents.Add(new TaskStatusChangedEvent(task.Id, oldStatus, targetStatus, actor));

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private List<string> ValidateConstraints(AgentTask task, TaskStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case TaskStatus.Done:
                if (string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("A result must be provided before marking a task as Done.");
                break;

            case TaskStatus.Failed:
                if (string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("Results/errors must be documented when a task fails.");
                break;

            case TaskStatus.Rejected:
                if (string.IsNullOrWhiteSpace(task.RequiredFollowUps) && string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("A reason must be provided when rejecting a task (use RequiredFollowUps or Result).");
                break;
        }

        return errors;
    }
}
