using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class ProjectType : ObjectType<Project>
{
    protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
    {
        descriptor.Field(p => p.Id).Type<IdType>().ID();
        descriptor.Field(p => p.Name).Type<StringType>();
        descriptor.Field(p => p.Description).Type<StringType>();
        descriptor.Field(p => p.Architecture).Type<StringType>();
        descriptor.Field(p => p.Memory).Type<StringType>();
        descriptor.Field(p => p.Repository).Type<StringType>();
        descriptor.Field(p => p.GithubUrl).Type<StringType>();
        descriptor.Field("githubToken_Encrypted").Resolve(ctx => ctx.Parent<Project>().GithubToken_Encrypted).Type<StringType>();
        descriptor.Field(p => p.CreatedAt).Type<DateTimeType>();
        descriptor.Field(p => p.UpdatedAt).Type<DateTimeType>();
        
        descriptor.Field("items").Type<ListType<ItemType>>().Resolve(ctx => ctx.Parent<Project>().Items);
        descriptor.Field("features").Type<ListType<ItemType>>().Deprecated("Use items field instead").Resolve(ctx => ctx.Parent<Project>().Items);
    }
}

public class ItemType : ObjectType<Item>
{
    protected override void Configure(IObjectTypeDescriptor<Item> descriptor)
    {
        descriptor.Field(i => i.Id).Type<IdType>().ID();
        descriptor.Field(i => i.ProjectId).Type<IdType>();
        descriptor.Field(i => i.ItemType).Type<EnumType<DevStack.Domain.Enums.ItemSubtype>>();
        descriptor.Field(i => i.Title).Type<StringType>();
        descriptor.Field(i => i.Status).Type<EnumType<DevStack.Domain.Enums.FeatureStatus>>();
        descriptor.Field(i => i.Description).Type<StringType>();
        descriptor.Field(i => i.AcceptanceCriteria).Type<StringType>();
        descriptor.Field(i => i.Plan).Type<StringType>();
        descriptor.Field(i => i.SecurityImpact).Type<StringType>();
        descriptor.Field(i => i.PerformanceImpact).Type<StringType>();
        descriptor.Field(i => i.TestPlan).Type<StringType>();
        descriptor.Field(i => i.DeploymentPlan).Type<StringType>();
        descriptor.Field(i => i.OpenQuestions).Type<StringType>();
        descriptor.Field(i => i.Result).Type<StringType>();
        descriptor.Field(i => i.Errors).Type<StringType>();
        descriptor.Field(i => i.CreatedAt).Type<DateTimeType>();
        descriptor.Field(i => i.UpdatedAt).Type<DateTimeType>();
        descriptor.Field("parentFeatureId").Resolve(ctx => ctx.Parent<Item>().ParentFeatureId).Type(typeof(IdType));
        descriptor.Field(i => i.Severity).Type<EnumType<Severity>>();
        descriptor.Field(i => i.RootCause).Type<StringType>();
        descriptor.Field("dependsOnId").Resolve(ctx => ctx.Parent<Item>().DependsOnId).Type(typeof(IdType));
    }
}

public class LargeLanguageModelType : ObjectType<LargeLanguageModel>
{
    protected override void Configure(IObjectTypeDescriptor<LargeLanguageModel> descriptor)
    {
        descriptor.Field(m => m.Id).Type<IdType>().ID();
        descriptor.Field(m => m.Url).Type<StringType>();
        descriptor.Field(m => m.Model).Type<StringType>();
        descriptor.Field(m => m.ModelAlias).Type<StringType>();
        descriptor.Field("apiKey_Encrypted").Resolve(ctx => ctx.Parent<LargeLanguageModel>().ApiKey_Encrypted).Type<StringType>();
        descriptor.Field(m => m.MaxComplexity).Type<IntType>();
        descriptor.Field(m => m.MaxConcurrency).Type<IntType>();
        descriptor.Field(m => m.ProjectId).Type<IdType>();
        descriptor.Field(m => m.CreatedAt).Type<DateTimeType>();
        descriptor.Field(m => m.UpdatedAt).Type<DateTimeType>();
    }
}
