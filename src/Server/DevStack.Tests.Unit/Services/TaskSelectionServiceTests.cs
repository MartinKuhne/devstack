using System;
using System.Collections.Generic;
using System.Linq;

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
        // Act
        var act = () => TaskSelectionService.SelectNextTask(null!, new List<AgentTask>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectNextTask_NullTasks_ThrowsArgumentNullException()
    {
        // Act
        var act = () => TaskSelectionService.SelectNextTask(new List<Deliverable>(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectNextTask_EmptyDeliverables_ReturnsNull()
    {
        // Act
        var result = TaskSelectionService.SelectNextTask(new List<Deliverable>(), new List<AgentTask>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SelectNextTask_PrioritizesPartialProgressDeliverable()
    {
        // Arrange
        var d1Id = Guid.NewGuid();
        var d2Id = Guid.NewGuid();

        var d1 = new Deliverable { Id = d1Id, Title = "Deliverable 1" };
        var d2 = new Deliverable { Id = d2Id, Title = "Deliverable 2" };

        var tasks = new List<AgentTask>
        {
            // d1 has 1 pending task, 0 done tasks (no partial progress)
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d1Id, Status = AgentTaskStatus.Ready },
            
            // d2 has 1 done task and 1 pending task (has partial progress!)
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d2Id, Status = AgentTaskStatus.Done },
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d2Id, Status = AgentTaskStatus.Ready }
        };

        // Act
        var result = TaskSelectionService.SelectNextTask(new[] { d1, d2 }, tasks);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(d2Id);
        result.Value.Task.Should().NotBeNull();
    }

    [Fact]
    public void SelectNextTask_PrioritizesReadyTaskOverInProgress()
    {
        // Arrange
        var dId = Guid.NewGuid();
        var d = new Deliverable { Id = dId, Title = "Deliverable 1" };

        var inProgressTask = new AgentTask { Id = Guid.NewGuid(), DeliverableId = dId, Status = AgentTaskStatus.InProgress, Title = "InProgress Task" };
        var readyTask = new AgentTask { Id = Guid.NewGuid(), DeliverableId = dId, Status = AgentTaskStatus.Ready, Title = "Ready Task" };

        var tasks = new List<AgentTask> { inProgressTask, readyTask };

        // Act
        var result = TaskSelectionService.SelectNextTask(new[] { d }, tasks);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Task.Should().NotBeNull();
        result.Value.Task!.Title.Should().Be("Ready Task");
    }

    [Fact]
    public void SelectNextTask_AllTasksTerminal_ReturnsDeliverableWithNullTask()
    {
        // Arrange
        var dId = Guid.NewGuid();
        var d = new Deliverable { Id = dId, Title = "Deliverable 1" };

        var tasks = new List<AgentTask>
        {
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = dId, Status = AgentTaskStatus.Done },
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = dId, Status = AgentTaskStatus.Failed },
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = dId, Status = AgentTaskStatus.Rejected }
        };

        // Act
        var result = TaskSelectionService.SelectNextTask(new[] { d }, tasks);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(dId);
        result.Value.Task.Should().BeNull();
    }

    [Theory]
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Ready, 1)] // d2 has partial progress -> index 1 selected
    [InlineData(AgentTaskStatus.Done, AgentTaskStatus.Done, 1)]  // d2 has done tasks -> index 1 selected over d1 (0 done tasks)
    public void SelectNextTask_PropertyBasedCombinations_BehavesDeterministically(
        AgentTaskStatus task1Status, AgentTaskStatus task2Status, int expectedDeliverableIndex)
    {
        // Arrange: Deterministic IDs
        var d1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var d2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var d1 = new Deliverable { Id = d1Id, Title = "D1" };
        var d2 = new Deliverable { Id = d2Id, Title = "D2" };

        var tasks = new List<AgentTask>
        {
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d1Id, Status = AgentTaskStatus.Ready },
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d2Id, Status = task1Status },
            new AgentTask { Id = Guid.NewGuid(), DeliverableId = d2Id, Status = task2Status }
        };

        var deliverables = new[] { d1, d2 };
        var expectedId = deliverables[expectedDeliverableIndex].Id;

        // Act
        var result = TaskSelectionService.SelectNextTask(deliverables, tasks);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Deliverable.Id.Should().Be(expectedId);
    }
}
