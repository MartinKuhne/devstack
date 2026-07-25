using System;
using System.Collections.Generic;

using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;

using FluentAssertions;

using Xunit;

namespace DevStack.Tests.Unit.Services;

public class TaskSelectionServiceTests
{
    [Fact]
    public void SelectNextTask_NullDeliverables_ThrowsArgumentNullException()
    {
        var act = () => TaskSelectionService.SelectNextTask(null!, new List<AgentTask>());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectNextTask_NullTasks_ThrowsArgumentNullException()
    {
        var act = () => TaskSelectionService.SelectNextTask(new List<Deliverable>(), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectNextTask_EmptyDeliverables_ReturnsNull()
    {
        var result = TaskSelectionService.SelectNextTask(new List<Deliverable>(), new List<AgentTask>());
        result.Should().BeNull();
    }

    [Fact]
    public void SelectNextTask_PrioritizesPartialProgressDeliverable()
    {
        var d1Id = Guid.NewGuid();
        var d2Id = Guid.NewGuid();
        var projId = Guid.NewGuid();

        var d1 = new Deliverable(projId, DeliverableType.Feature, "Deliverable 1", id: d1Id);
        var d2 = new Deliverable(projId, DeliverableType.Feature, "Deliverable 2", id: d2Id);

        var tasks = new List<AgentTask>
        {
            new AgentTask(projId, d1Id, "Task 1", status: AgentTaskStatus.Ready),
            new AgentTask(projId, d2Id, "Task 2", status: AgentTaskStatus.Done),
            new AgentTask(projId, d2Id, "Task 3", status: AgentTaskStatus.Ready)
        };

        var result = TaskSelectionService.SelectNextTask(new[] { d1, d2 }, tasks);

        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(d2Id);
        result.Value.Task.Should().NotBeNull();
    }

    [Fact]
    public void SelectNextTask_PrioritizesReadyTaskOverInProgress()
    {
        var dId = Guid.NewGuid();
        var projId = Guid.NewGuid();
        var d = new Deliverable(projId, DeliverableType.Feature, "Deliverable 1", id: dId);

        var inProgressTask = new AgentTask(projId, dId, "InProgress Task", status: AgentTaskStatus.InProgress);
        var readyTask = new AgentTask(projId, dId, "Ready Task", status: AgentTaskStatus.Ready);

        var tasks = new List<AgentTask> { inProgressTask, readyTask };

        var result = TaskSelectionService.SelectNextTask(new[] { d }, tasks);

        result.Should().NotBeNull();
        result!.Value.Task.Should().NotBeNull();
        result.Value.Task!.Title.Should().Be("Ready Task");
    }

    [Fact]
    public void SelectNextTask_AllTasksTerminal_ReturnsDeliverableWithNullTask()
    {
        var dId = Guid.NewGuid();
        var projId = Guid.NewGuid();
        var d = new Deliverable(projId, DeliverableType.Feature, "Deliverable 1", id: dId);

        var tasks = new List<AgentTask>
        {
            new AgentTask(projId, dId, "Task 1", status: AgentTaskStatus.Done),
            new AgentTask(projId, dId, "Task 2", status: AgentTaskStatus.Failed),
            new AgentTask(projId, dId, "Task 3", status: AgentTaskStatus.Rejected)
        };

        var result = TaskSelectionService.SelectNextTask(new[] { d }, tasks);

        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(dId);
        result.Value.Task.Should().BeNull();
    }

    [Theory]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Ready, 1)]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Done, 1)]
    public void SelectNextTask_PropertyBasedCombinations_BehavesDeterministically(
        AgentTaskStatus task1Status, AgentTaskStatus task2Status, int expectedDeliverableIndex)
    {
        var d1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var d2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var projId = Guid.NewGuid();

        var d1 = new Deliverable(projId, DeliverableType.Feature, "D1", id: d1Id);
        var d2 = new Deliverable(projId, DeliverableType.Feature, "D2", id: d2Id);

        var tasks = new List<AgentTask>
        {
            new AgentTask(projId, d1Id, "Task 1", status: AgentTaskStatus.Ready),
            new AgentTask(projId, d2Id, "Task 2", status: task1Status),
            new AgentTask(projId, d2Id, "Task 3", status: task2Status)
        };

        var deliverables = new[] { d1, d2 };
        var expectedId = deliverables[expectedDeliverableIndex].Id;

        var result = TaskSelectionService.SelectNextTask(deliverables, tasks);

        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(expectedId);
    }
}
