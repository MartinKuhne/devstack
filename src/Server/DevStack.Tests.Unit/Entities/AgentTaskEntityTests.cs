using System;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Entities;

public class AgentTaskEntityTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var task = new AgentTask(projectId, deliverableId, "Task Title", "Task Desc", complexityRating: 3);

        task.ProjectId.Should().Be(projectId);
        task.DeliverableId.Should().Be(deliverableId);
        task.Title.Should().Be("Task Title");
        task.Description.Should().Be("Task Desc");
        task.ComplexityRating.Should().Be(3);
        task.Status.Should().Be(AgentTaskStatus.Ready);
    }

    [Fact]
    public void Constructor_InvalidComplexity_ThrowsArgumentException()
    {
        var act = () => new AgentTask(Guid.NewGuid(), Guid.NewGuid(), "Task Title", complexityRating: 15);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateMetadata_ValidValues_UpdatesProperties()
    {
        var task = new AgentTask(Guid.NewGuid(), Guid.NewGuid(), "Initial Title");

        task.UpdateMetadata(title: "Updated Title", description: "Updated Desc", complexityRating: 5);

        task.Title.Should().Be("Updated Title");
        task.Description.Should().Be("Updated Desc");
        task.ComplexityRating.Should().Be(5);
    }

    [Fact]
    public void TransitionStatus_UpdatesStatus()
    {
        var task = new AgentTask(Guid.NewGuid(), Guid.NewGuid(), "Task Title");

        task.TransitionStatus(AgentTaskStatus.InProgress);

        task.Status.Should().Be(AgentTaskStatus.InProgress);
    }
}
