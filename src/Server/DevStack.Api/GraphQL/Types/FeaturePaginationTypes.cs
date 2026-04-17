using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class ItemConnection
{
    public List<Item> Nodes { get; set; } = default!;
    public ItemPageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class ItemPageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class ItemFilterInput
{
    public Guid? ProjectId { get; set; }
    public List<Domain.Enums.FeatureStatus>? Status { get; set; }
    public List<Domain.Enums.ItemSubtype>? Subtype { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

public class FeatureConnection
{
    public List<Item> Nodes { get; set; } = default!;
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
