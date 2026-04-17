using DevStack.Domain.Enums;

namespace DevStack.Domain.Events;

public abstract class DomainEvent
{
    protected DomainEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }

    public DateTime OccurredAt { get; }
}

public sealed class ItemStatusChangedEvent : DomainEvent
{
    public ItemStatusChangedEvent(Guid itemId, FeatureStatus oldStatus, FeatureStatus newStatus, string actor)
    {
        ItemId = itemId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Actor = actor ?? string.Empty;
    }

    public Guid ItemId { get; }
    public FeatureStatus OldStatus { get; }
    public FeatureStatus NewStatus { get; }
    public string Actor { get; }
}

[Obsolete("Use ItemStatusChangedEvent instead")]
public sealed class FeatureStatusChangedEvent : DomainEvent
{
    public FeatureStatusChangedEvent(Guid featureId, FeatureStatus oldStatus, FeatureStatus newStatus, string actor)
    {
        FeatureId = featureId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Actor = actor ?? string.Empty;
    }

    public Guid FeatureId { get; }
    public FeatureStatus OldStatus { get; }
    public FeatureStatus NewStatus { get; }
    public string Actor { get; }
}

public sealed class TaskStatusChangedEvent : DomainEvent
    {
        public TaskStatusChangedEvent(Guid taskId, global::DevStack.Domain.Enums.TaskStatus oldStatus, global::DevStack.Domain.Enums.TaskStatus newStatus, string actor)
        {
            TaskId = taskId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Actor = actor ?? string.Empty;
        }

        public Guid TaskId { get; }
        public global::DevStack.Domain.Enums.TaskStatus OldStatus { get; }
        public global::DevStack.Domain.Enums.TaskStatus NewStatus { get; }
        public string Actor { get; }
    }
