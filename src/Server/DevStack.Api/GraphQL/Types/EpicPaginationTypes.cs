using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

[Obsolete("Use ItemConnection with Subtype=Epic filter instead")]
public class EpicConnection
{
    public List<Epic> Nodes { get; set; } = default!;
    public EpicPageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class EpicPageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class EpicFilterInput
{
    public string? Title { get; set; }
}
