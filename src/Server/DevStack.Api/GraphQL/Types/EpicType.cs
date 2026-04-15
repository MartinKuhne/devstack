using DevStack.Domain.Entities;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class EpicType : ObjectType<Epic>
{
    protected override void Configure(IObjectTypeDescriptor<Epic> descriptor)
    {
        descriptor.Field(e => e.Id).Type<IdType>().ID();
        descriptor.Field(e => e.Title).Type<StringType>();
        descriptor.Field(e => e.Description).Type<StringType>();
        descriptor.Field(e => e.CreatedAt).Type<DateTimeType>();
        descriptor.Field(e => e.UpdatedAt).Type<DateTimeType>();
        
        descriptor.Field("features").Resolve(ctx => ctx.Parent<Epic>().Features);
    }
}
