using DevStack.Domain.Enums;
using Xunit;

namespace DevStack.Tests.Unit.Enums;

public class EnumTests
{
    [Fact]
    public void DeliverableStatus_Enums_Are_Defined()
    {
        Assert.Equal(8, Enum.GetValues<DeliverableStatus>().Length);
        Assert.Contains(DeliverableStatus.Draft, Enum.GetValues<DeliverableStatus>());
        Assert.Contains(DeliverableStatus.Done, Enum.GetValues<DeliverableStatus>());
    }

    [Fact]
    public void DeliverableType_Enums_Are_Defined()
    {
        Assert.Equal(3, Enum.GetValues<DeliverableType>().Length);
        Assert.Contains(DeliverableType.Feature, Enum.GetValues<DeliverableType>());
        Assert.Contains(DeliverableType.Defect, Enum.GetValues<DeliverableType>());
        Assert.Contains(DeliverableType.Maintenance, Enum.GetValues<DeliverableType>());
    }

    [Fact]
    public void AgentTaskStatus_Enums_Are_Defined()
    {
        Assert.Equal(6, Enum.GetValues<AgentTaskStatus>().Length);
        Assert.Contains(AgentTaskStatus.Ready, Enum.GetValues<AgentTaskStatus>());
        Assert.Contains(AgentTaskStatus.Done, Enum.GetValues<AgentTaskStatus>());
    }
}
