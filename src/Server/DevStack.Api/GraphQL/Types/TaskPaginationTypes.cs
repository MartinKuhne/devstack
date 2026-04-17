using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class TaskConnection
{
    public List<global::DevStack.Domain.Entities.AgentTask> Nodes { get; set; } = default!;
    public TaskPageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class TaskPageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class TaskFilterInput
{
    public Guid? ItemId { get; set; }
    public List<global::DevStack.Domain.Enums.TaskStatus>? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}
