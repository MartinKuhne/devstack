namespace DevStack.Domain.Enums;

public enum DeliverableStatus
{
    Draft,
    Design,
    Plan,
    Implement,
    Merge,
    Deploy,
    Test,
    Done,
    Failed,
    Rejected,
    NeedsReview
}

public enum AgentTaskStatus
{
    Ready,
    InProgress,
    Done,
    Failed,
    Rejected,
    NeedsReview
}
