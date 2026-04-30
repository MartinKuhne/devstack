namespace DevStack.Domain.Services;

public static class DeliverableCompletionService
{
    /// <summary>
    /// Checks whether all agent tasks for a deliverable are in the Done status.
    /// Returns true if there are no tasks (empty set is considered complete)
    /// or if all tasks have status Done.
    /// Returns false if any task is not in Done status.
    /// </summary>
    public static bool CheckAllTasksDone(IEnumerable<AgentTask> tasks)
    {
        return !tasks.Any() || tasks.All(t => t.Status == AgentTaskStatus.Done);
    }
}
