using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
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
        
        // Task-specific fields
        descriptor.Field("deliverable").Resolve(ctx => ctx.Parent<Item>().Deliverable).Type<StringType>();
        descriptor.Field("risks").Resolve(ctx => ctx.Parent<Item>().Risks).Type<StringType>();
        descriptor.Field("complexityRating").Resolve(ctx => ctx.Parent<Item>().ComplexityRating).Type<IntType>();
        
        descriptor.Field("dependsOnId").Resolve(ctx => ctx.Parent<Item>().DependsOnId).Type(typeof(IdType));
    }
}

[Obsolete("Use ItemType with Subtype=Task filter instead")]
public class TaskType : ObjectType<DevStack.Domain.Entities.AgentTask>
{
    protected override void Configure(IObjectTypeDescriptor<DevStack.Domain.Entities.AgentTask> descriptor)
    {
        descriptor.Field(t => t.Id).Type<IdType>().ID();
        descriptor.Field(t => t.ItemId).Type<IdType>();
        descriptor.Field(t => t.Title).Type<StringType>();
        descriptor.Field(t => t.Status).Type<EnumType<DevStack.Domain.Enums.TaskStatus>>();
        descriptor.Field(t => t.Deliverable).Type<StringType>();
        descriptor.Field(t => t.AcceptanceCriteria).Type<StringType>();
        descriptor.Field(t => t.Risks).Type<StringType>();
        descriptor.Field(t => t.Result).Type<StringType>();
        descriptor.Field(t => t.RequiredFollowUps).Type<StringType>();
        descriptor.Field(t => t.ComplexityRating).Type<IntType>();
        descriptor.Field(t => t.CreatedAt).Type<DateTimeType>();
        descriptor.Field(t => t.UpdatedAt).Type<DateTimeType>();
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
        descriptor.Field(m => m.ProjectId).Type<IdType>();
        descriptor.Field(m => m.CreatedAt).Type<DateTimeType>();
        descriptor.Field(m => m.UpdatedAt).Type<DateTimeType>();
    }
}

public class WorkflowRunType : ObjectType<WorkflowRun>
{
    protected override void Configure(IObjectTypeDescriptor<WorkflowRun> descriptor)
    {
        descriptor.Field(w => w.Id).Type<IdType>().ID();
        descriptor.Field(w => w.ProjectId).Type<IdType>();
        descriptor.Field(w => w.ItemId).Type<IdType>();
        descriptor.Field(w => w.TaskId).Type<IdType>();
        descriptor.Field(w => w.WorkflowType).Type<EnumType<WorkflowType>>();
        descriptor.Field(w => w.Status).Type<EnumType<WorkflowRunStatus>>();
        descriptor.Field(w => w.StartedAt).Type<DateTimeType>();
        descriptor.Field(w => w.CompletedAt).Type<DateTimeType>();
        descriptor.Field(w => w.ErrorMessage).Type<StringType>();
        descriptor.Field(w => w.InputPayload).Type<StringType>();
        descriptor.Field(w => w.OutputPayload).Type<StringType>();
        descriptor.Field(w => w.CreatedAt).Type<DateTimeType>();
    }
}

public class AuditEventType : ObjectType<AuditEvent>
{
    protected override void Configure(IObjectTypeDescriptor<AuditEvent> descriptor)
    {
        descriptor.Field(a => a.Id).Type<IdType>().ID();
        descriptor.Field(a => a.EntityType).Type<StringType>();
        descriptor.Field(a => a.EntityId).Type<IdType>();
        descriptor.Field(a => a.EventType).Type<StringType>();
        descriptor.Field(a => a.OldValue).Type<StringType>();
        descriptor.Field(a => a.NewValue).Type<StringType>();
        descriptor.Field(a => a.Actor).Type<StringType>();
        descriptor.Field(a => a.OccurredAt).Type<DateTimeType>();
    }
}
