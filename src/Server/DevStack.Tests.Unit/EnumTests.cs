using DevStack.Domain.Enums;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;
using Xunit;

namespace DevStack.Tests.Unit.Enums;

public class EnumTests
{
    [Fact]
    public void FeatureStatus_Enums_Are_Defined()
    {
        Assert.Equal(9, Enum.GetValues<FeatureStatus>().Length);
        Assert.Contains(FeatureStatus.Planning, Enum.GetValues<FeatureStatus>());
        Assert.Contains(FeatureStatus.Done, Enum.GetValues<FeatureStatus>());
    }

    [Fact]
    public void TaskStatus_Enums_Are_Defined()
    {
        Assert.Equal(11, Enum.GetValues<TaskStatus>().Length);
        Assert.Contains(TaskStatus.Planning, Enum.GetValues<TaskStatus>());
        Assert.Contains(TaskStatus.Done, Enum.GetValues<TaskStatus>());
    }

    [Fact]
    public void WorkflowType_Enums_Are_Defined()
    {
        Assert.Equal(5, Enum.GetValues<WorkflowType>().Length);
        Assert.Contains(WorkflowType.Planner, Enum.GetValues<WorkflowType>());
        Assert.Contains(WorkflowType.Architect, Enum.GetValues<WorkflowType>());
    }

    [Fact]
    public void WorkflowRunStatus_Enums_Are_Defined()
    {
        Assert.Equal(5, Enum.GetValues<WorkflowRunStatus>().Length);
        Assert.Contains(WorkflowRunStatus.Queued, Enum.GetValues<WorkflowRunStatus>());
        Assert.Contains(WorkflowRunStatus.Failed, Enum.GetValues<WorkflowRunStatus>());
    }
}