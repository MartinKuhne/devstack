using System;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Entities;

public class DeliverableEntityTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var projectId = Guid.NewGuid();
        var deliverable = new Deliverable(projectId, DeliverableType.Feature, "Test Title");

        deliverable.ProjectId.Should().Be(projectId);
        deliverable.Title.Should().Be("Test Title");
        deliverable.Type.Should().Be(DeliverableType.Feature);
        deliverable.Status.Should().Be(DeliverableStatus.Draft);
    }

    [Fact]
    public void Constructor_EmptyTitle_ThrowsArgumentException()
    {
        var act = () => new Deliverable(Guid.NewGuid(), DeliverableType.Feature, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateMetadata_ValidValues_UpdatesProperties()
    {
        var deliverable = new Deliverable(Guid.NewGuid(), DeliverableType.Feature, "Initial");

        deliverable.UpdateMetadata(title: "Updated Title", description: "New Desc", acceptanceCriteria: "New Criteria");

        deliverable.Title.Should().Be("Updated Title");
        deliverable.Description.Should().Be("New Desc");
        deliverable.AcceptanceCriteria.Should().Be("New Criteria");
    }

    [Fact]
    public void UpdateMetadata_EmptyTitle_ThrowsArgumentException()
    {
        var deliverable = new Deliverable(Guid.NewGuid(), DeliverableType.Feature, "Initial");

        var act = () => deliverable.UpdateMetadata(title: "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TransitionStatus_ValidTransition_UpdatesStatus()
    {
        var deliverable = new Deliverable(Guid.NewGuid(), DeliverableType.Feature, "Initial", status: DeliverableStatus.Draft);

        deliverable.TransitionStatus(DeliverableStatus.Design);

        deliverable.Status.Should().Be(DeliverableStatus.Design);
    }
}
