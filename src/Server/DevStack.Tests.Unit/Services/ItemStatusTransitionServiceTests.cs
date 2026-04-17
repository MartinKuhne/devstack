using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Events;
using DevStack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Unit.Services;

public class ItemStatusTransitionServiceTests
{
    private readonly ItemStatusTransitionService _service = new();
    private readonly ItemStatusTransitionService _limitedService = new(limitFeatureStatusTransitions: true);

    [Fact]
    public void Transition_Successfully_Changes_Status()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Planning,
            Title = "Test Item",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.Status.Should().Be(FeatureStatus.Ready);
    }

    [Fact]
    public void Transition_Emits_Domain_Event()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Planning,
            Title = "Test Item",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Ready, "user@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _service.DomainEvents.Should().HaveCount(1);
        var @event = _service.DomainEvents.First().Should().BeOfType<ItemStatusChangedEvent>().Subject;
        @event.ItemId.Should().Be(item.Id);
        @event.OldStatus.Should().Be(FeatureStatus.Planning);
        @event.NewStatus.Should().Be(FeatureStatus.Ready);
        @event.Actor.Should().Be("user@test.com");
    }

    [Theory]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Ready)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.ReadyForTest, FeatureStatus.Testing)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Done, FeatureStatus.InProgress)]
    public void Allowed_Transitions_Work(FeatureStatus source, FeatureStatus target)
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = source,
            Title = "Test Item",
            ProjectId = Guid.NewGuid()
        };

        if (target == FeatureStatus.Done || target == FeatureStatus.Failed)
            item.Result = "Test result";
        if (target == FeatureStatus.Rejected)
            item.Errors = "Test errors";

        // Act
        var result = _service.Transition(item, target, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.Status.Should().Be(target);
    }

    [Theory]
    [InlineData(FeatureStatus.Done, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Planning)]
    public void Disallowed_Transitions_Fail_When_Limit_Flag_Enabled(FeatureStatus source, FeatureStatus target)
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = source,
            Title = "Test Item",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _limitedService.Transition(item, target, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        item.Status.Should().Be(source);
    }

    [Theory]
    [InlineData(FeatureStatus.Done, FeatureStatus.Planning)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Testing, FeatureStatus.Planning)]
    public void Disallowed_Transitions_Succeed_When_Limit_Flag_Disabled(FeatureStatus source, FeatureStatus target)
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = source,
            Title = "Test Item",
            ProjectId = Guid.NewGuid(),
            Result = "result",
            Errors = "errors"
        };

        // Act
        var result = _service.Transition(item, target, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.Status.Should().Be(target);
    }

    [Fact]
    public void Transition_Fails_When_Item_Is_Null()
    {
        // Act
        var result = _service.Transition(null!, FeatureStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Item cannot be null.");
    }

    [Fact]
    public void Transition_Fails_When_Actor_Is_Whitespace()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Planning,
            Title = "Test Item",
            ProjectId = Guid.NewGuid()
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Ready, "   ");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Actor (operator or workflow name) is required.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result_For_Feature()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Testing,
            Title = "Test Item",
            ProjectId = Guid.NewGuid(),
            Result = null
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A result must be provided before marking a feature as Done.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result_And_RootCause_For_Defect()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Defect,
            Status = FeatureStatus.Testing,
            Title = "Test Defect",
            ProjectId = Guid.NewGuid(),
            Result = null,
            RootCause = null,
            Severity = Severity.Medium
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A resolution must be provided before marking a defect as Done.");
        result.Errors.Should().Contain("Root cause must be documented for defects.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result_For_Maintenance()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Maintenance,
            Status = FeatureStatus.Testing,
            Title = "Test Maintenance",
            ProjectId = Guid.NewGuid(),
            Result = null
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A description of changes must be provided before marking maintenance as Done.");
    }

    [Fact]
    public void Transition_To_Done_Requires_Result_For_Enabler()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Enabler,
            Status = FeatureStatus.Testing,
            Title = "Test Enabler",
            ProjectId = Guid.NewGuid(),
            Result = null
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Done, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A description of the enabler outcome must be provided before marking as Done.");
    }

    [Fact]
    public void Transition_To_Failed_Requires_Errors()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Testing,
            Title = "Test Item",
            ProjectId = Guid.NewGuid(),
            Errors = null
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Failed, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Errors must be documented when a feature fails.");
    }

    [Fact]
    public void Transition_To_Rejected_Requires_Reason()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Ready,
            Title = "Test Item",
            ProjectId = Guid.NewGuid(),
            OpenQuestions = null,
            Errors = null
        };

        // Act
        var result = _service.Transition(item, FeatureStatus.Rejected, "actor");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("A reason must be provided when rejecting a feature (use OpenQuestions or Errors).");
    }

    [Fact]
    public void Transition_Updates_UtcTimestamp()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Subtype = ItemSubtype.Feature,
            Status = FeatureStatus.Planning,
            Title = "Test Item",
            ProjectId = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var originalUpdatedAt = item.UpdatedAt;

        // Act
        var result = _service.Transition(item, FeatureStatus.Ready, "actor");

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }
}
