using DevStack.Domain.Entities;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Services;

public class AgentTaskStatusTransitionService
{
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
        if (task == null)
            return TransitionResult<Unit>.Failure(["AgentTask is null"]);

        if (string.IsNullOrWhiteSpace(actor))
            return TransitionResult<Unit>.Failure(["Actor is required"]);

        var oldStatus = task.Status;
        if (oldStatus == targetStatus)
            return TransitionResult<Unit>.Failure(["Task is already in the target status"]);

        var allowed = _allowedTransitions.GetValueOrDefault(oldStatus, []);

        if (!allowed.Contains(targetStatus))
            return TransitionResult<Unit>.Failure([
                $"Cannot transition from {oldStatus} to {targetStatus}. Allowed transitions: {string.Join(", ", allowed)}"
            ]);

        var constraints = ValidateConstraints(task, targetStatus);
        if (constraints.Count > 0)
            return TransitionResult<Unit>.Failure(constraints);

        task.Status = targetStatus;

        return TransitionResult<Unit>.Success(Unit.Value);
    }

    private List<string> ValidateConstraints(AgentTask task, AgentTaskStatus targetStatus)
    {
        var errors = new List<string>();

        switch (targetStatus)
        {
            case AgentTaskStatus.Done:
                if (string.IsNullOrWhiteSpace(task.Result))
                    errors.Add("Result is required to mark a task as Done");
                break;
            case AgentTaskStatus.Failed:
                if (string.IsNullOrWhiteSpace(task.Errors))
                    errors.Add("Errors is required to mark a task as Failed");
                break;
            case AgentTaskStatus.Rejected:
                if (string.IsNullOrWhiteSpace(task.Errors))
                    errors.Add("Errors is required to mark a task as Rejected");
                break;
        }

        return errors;
    }
}
