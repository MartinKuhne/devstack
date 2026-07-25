using System;
using System.Collections.Generic;

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;

using FluentAssertions;

using Xunit;

public class DeliverableCompletionServiceTests
{
    private static AgentTask CreateTask(AgentTaskStatus status)
    {
        return new AgentTask(Guid.NewGuid(), Guid.NewGuid(), "Sample Task", status: status);
    }

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
            CreateTask(AgentTaskStatus.Done),
            CreateTask(AgentTaskStatus.Done),
            CreateTask(AgentTaskStatus.Done)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeTrue();
    }

    [Fact]
    public void CheckAllTasksDone_WithOneTaskNotDone_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.Done),
            CreateTask(AgentTaskStatus.InProgress),
            CreateTask(AgentTaskStatus.Done)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithAllTasksInProgress_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.InProgress),
            CreateTask(AgentTaskStatus.Ready)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithFailedTask_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.Done),
            CreateTask(AgentTaskStatus.Failed)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithSingleTaskDone_ReturnsTrue()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.Done)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeTrue();
    }

    [Fact]
    public void CheckAllTasksDone_WithSingleTaskNotDone_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.NeedsReview)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckAllTasksDone_WithRejectedTask_ReturnsFalse()
    {
        var tasks = new List<AgentTask>
        {
            CreateTask(AgentTaskStatus.Done),
            CreateTask(AgentTaskStatus.Rejected)
        };

        var result = DeliverableCompletionService.CheckAllTasksDone(tasks);

        result.Should().BeFalse();
    }
}
