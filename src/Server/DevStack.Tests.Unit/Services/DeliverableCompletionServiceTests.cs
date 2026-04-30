using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;

using FluentAssertions;

using Xunit;

public class DeliverableCompletionServiceTests
{
    [Fact]
    public void CheckAllTasksDone_WithEmptyCollection_ReturnsTrue()
    {
        var tasks = new List<AgentTask>();

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeTrue();
    }

    [Fact]
    public void CheckAllTasksDone_WithAllTasksDone_ReturnsTrue()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.Done },
            new() { Status = AgentTaskStatus.Done },
            new() { Status = AgentTaskStatus.Done }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeTrue();
    }

    [Fact]
    public void CheckAllTasksDone_WithOneTaskNotDone_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.Done },
            new() { Status = AgentTaskStatus.InProgress },
            new() { Status = AgentTaskStatus.Done }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithAllTasksInProgress_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.InProgress },
            new() { Status = AgentTaskStatus.Ready }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithFailedTask_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.Done },
            new() { Status = AgentTaskStatus.Failed }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithSingleTaskDone_ReturnsTrue()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.Done }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeTrue();
    }

    [Fact]
    public void CheckAllTasksDone_WithSingleTaskNotDone_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.NeedsReview }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithRejectedTask_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            new() { Status = AgentTaskStatus.Done },
            new() { Status = AgentTaskStatus.Rejected }
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }
}
