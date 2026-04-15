using DevStack.Domain.Entities;

namespace DevStack.Api.GraphQL.Types;

public class DefectConnection
{
    public List<Defect> Nodes { get; set; } = default!;
    public DefectPageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class DefectPageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class DefectFilterInput
{
    public Guid? ProjectId { get; set; }
}
