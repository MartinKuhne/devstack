using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class EntityConnection<TNode>
{
    public List<TNode> Nodes { get; set; } = default!;
    public PageInfo PageInfo { get; set; } = default!;
    public int TotalCount { get; set; }
}

public class PageInfo
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalCount { get; set; }
}

public class ProjectConnectionType : ObjectType<EntityConnection<Project>>
{
    protected override void Configure(IObjectTypeDescriptor<EntityConnection<Project>> descriptor)
    {
        descriptor.Field(e => e.Nodes).Type<ListType<ProjectType>>();
        descriptor.Field(e => e.PageInfo).Type<PageInfoType>();
        descriptor.Field(e => e.TotalCount).Type<IntType>();
    }
}

public class DeliverableConnectionType : ObjectType<EntityConnection<Deliverable>>
{
    protected override void Configure(IObjectTypeDescriptor<EntityConnection<Deliverable>> descriptor)
    {
        descriptor.Field(e => e.Nodes).Type<ListType<DeliverableObject>>();
        descriptor.Field(e => e.PageInfo).Type<PageInfoType>();
        descriptor.Field(e => e.TotalCount).Type<IntType>();
    }
}

public class AgentTaskConnectionType : ObjectType<EntityConnection<AgentTask>>
{
    protected override void Configure(IObjectTypeDescriptor<EntityConnection<AgentTask>> descriptor)
    {
        descriptor.Field(e => e.Nodes).Type<ListType<AgentTaskType>>();
        descriptor.Field(e => e.PageInfo).Type<PageInfoType>();
        descriptor.Field(e => e.TotalCount).Type<IntType>();
    }
}

public class LargeLanguageModelConnectionType : ObjectType<EntityConnection<LargeLanguageModel>>
{
    protected override void Configure(IObjectTypeDescriptor<EntityConnection<LargeLanguageModel>> descriptor)
    {
        descriptor.Field(e => e.Nodes).Type<ListType<LargeLanguageModelType>>();
        descriptor.Field(e => e.PageInfo).Type<PageInfoType>();
        descriptor.Field(e => e.TotalCount).Type<IntType>();
    }
}

public class PageInfoType : ObjectType<PageInfo>
{
    protected override void Configure(IObjectTypeDescriptor<PageInfo> descriptor)
    {
        descriptor.Field(e => e.HasNextPage).Type<NonNullType<BooleanType>>();
        descriptor.Field(e => e.HasPreviousPage).Type<NonNullType<BooleanType>>();
        descriptor.Field(e => e.TotalCount).Type<NonNullType<IntType>>();
    }
}
