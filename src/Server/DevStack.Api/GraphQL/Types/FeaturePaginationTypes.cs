using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class FeatureConnection
{
    public List<Feature> Nodes { get; set; } = default!;
    public FeaturePageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class FeaturePageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class FeatureFilterInput
{
    public Guid? ProjectId { get; set; }
    public List<Domain.Enums.FeatureStatus>? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}
