using DevStack.Domain.Entities;

namespace DevStack.Api.GraphQL.Types;

public class ProjectConnection
{
    public List<Project> Nodes { get; set; } = default!;
    public ProjectPageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class ProjectPageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}
