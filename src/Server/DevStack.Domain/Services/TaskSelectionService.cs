using System;
using System.Collections.Generic;
using System.Linq;

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;

namespace DevStack.Domain.Services;

/// <summary>
/// Domain service providing pure algorithms for selecting deliverables and pending agent tasks.
/// </summary>
public static class TaskSelectionService
{
    /// <summary>
    /// Selects the best deliverable and the next pending agent task to be executed.
    /// </summary>
    /// <param name="deliverables">Collection of active deliverables candidate for task execution.</param>
    /// <param name="tasks">Collection of agent tasks associated with the deliverables.</param>
    /// <returns>
    /// A tuple containing the selected <see cref="Deliverable"/> and optional next <see cref="AgentTask"/> if pending,
    /// or <c>null</c> if <paramref name="deliverables"/> is empty.
    /// </returns>
    /// <remarks>
    /// Contract:
    /// - Purpose: Select higher-priority deliverable (partially completed > started > ID) and next eligible task (Ready > InProgress > other non-terminal).
    /// - Inputs: Non-null collections of <see cref="Deliverable"/> and <see cref="AgentTask"/>.
    /// - Outputs: Tuple of (Deliverable, AgentTask?) or null if deliverables is empty.
    /// - Purity: Pure function. No I/O, state mutation, or side-effects.
    /// </remarks>
    public static (Deliverable Deliverable, AgentTask? Task)? SelectNextTask(
        IReadOnlyCollection<Deliverable> deliverables,
        IReadOnlyCollection<AgentTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(deliverables);
        ArgumentNullException.ThrowIfNull(tasks);

        if (deliverables.Count == 0)
            return null;

        var taskGroups = tasks
            .GroupBy(t => t.DeliverableId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var bestDeliverable = deliverables
            .Select(d =>
            {
                var deliverableTasks = taskGroups.GetValueOrDefault(d.Id, []);
                var doneCount = deliverableTasks.Count(t => t.Status == AgentTaskStatus.Done);
                var notDoneCount = deliverableTasks.Count(t => t.Status != AgentTaskStatus.Done 
                    && t.Status != AgentTaskStatus.Failed 
                    && t.Status != AgentTaskStatus.Rejected);
                return (
                    Deliverable: d, 
                    Tasks: deliverableTasks, 
                    HasPartialProgress: doneCount > 0 && notDoneCount > 0, 
                    HasDone: doneCount > 0
                );
            })
            .OrderByDescending(x => x.HasPartialProgress)
            .ThenByDescending(x => x.HasDone)
            .ThenBy(x => x.Deliverable.Id)
            .First();

        var nextTask = bestDeliverable.Tasks
            .Where(t => t.Status != AgentTaskStatus.Done 
                && t.Status != AgentTaskStatus.Failed 
                && t.Status != AgentTaskStatus.Rejected)
            .OrderBy(t => t.Status == AgentTaskStatus.Ready ? 0 : t.Status == AgentTaskStatus.InProgress ? 1 : 2)
            .ThenBy(t => t.Id)
            .FirstOrDefault();

        return (bestDeliverable.Deliverable, nextTask);
    }
}
