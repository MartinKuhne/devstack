namespace DevStack.Domain.Enums;

public enum FeatureStatus
{
    Planning,
    Ready,
    InProgress,
    ReadyForTest,
    Testing,
    Done,
    Failed,
    Rejected,
    InReview
}

public enum TaskStatus
{
    Planning,
    Ready,
    Prepare,
    Code,
    Review,
    ReadyForTest,
    Testing,
    Done,
    Failed,
    Rejected,
    InReview
}

public enum WorkflowType
{
    Planner,
    DevLead,
    Coder,
    Tester,
    Architect
}

public enum WorkflowRunStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled
}