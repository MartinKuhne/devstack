using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class ProjectType : ObjectType<Project>
{
    protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
    {
        descriptor.Field(p => p.Id).Type<IdType>().ID();
        descriptor.Field(p => p.Name).Type<StringType>();
        descriptor.Field(p => p.Description).Type<StringType>();
        descriptor.Field(p => p.Repository).Type<StringType>();
        descriptor.Field(p => p.LargeLanguageModels).Type<ListType<LargeLanguageModelType>>();
    }
}

public class DeliverableObject : ObjectType<Deliverable>
{
    protected override void Configure(IObjectTypeDescriptor<Deliverable> descriptor)
    {
        descriptor.Field(d => d.Id).Type<IdType>().ID();
        descriptor.Field(d => d.ProjectId).Type<IdType>();
        descriptor.Field(d => d.Type).Type<EnumType<DeliverableType>>();
        descriptor.Field(d => d.Title).Type<StringType>();
        descriptor.Field(d => d.Status).Type<EnumType<DeliverableStatus>>();
        descriptor.Field(d => d.Description).Type<StringType>();
        descriptor.Field(d => d.AcceptanceCriteria).Type<StringType>();
        descriptor.Field(d => d.ExecutionPlan).Type<StringType>();
        descriptor.Field(d => d.AgentFeedback).Type<StringType>();
        descriptor.Field(d => d.SecurityImpact).Type<StringType>();
        descriptor.Field(d => d.PerformanceImpact).Type<StringType>();
        descriptor.Field(d => d.TestPlan).Type<StringType>();
        descriptor.Field(d => d.DeploymentPlan).Type<StringType>();
        descriptor.Field(d => d.Blocking).Type<StringType>();
    }
}

public class AgentTaskType : ObjectType<AgentTask>
{
    protected override void Configure(IObjectTypeDescriptor<AgentTask> descriptor)
    {
        descriptor.Field(t => t.Id).Type<IdType>().ID();
        descriptor.Field(t => t.ProjectId).Type<IdType>();
        descriptor.Field(t => t.DeliverableId).Type<IdType>();
        descriptor.Field(t => t.Title).Type<StringType>();
        descriptor.Field(t => t.Status).Type<EnumType<AgentTaskStatus>>();
        descriptor.Field(t => t.Result).Type<StringType>();
        descriptor.Field(t => t.Errors).Type<StringType>();
        descriptor.Field(t => t.CommitHash).Type<StringType>();
        descriptor.Field(t => t.ComplexityRating).Type<IntType>();
        descriptor.Field(t => t.DependsOnDevTask).Type<StringType>();
        descriptor.Field(t => t.PromptTokens).Type<IntType>();
        descriptor.Field(t => t.CompletionTokens).Type<IntType>();
        descriptor.Field(t => t.ExecutionDurationInSeconds).Type<IntType>();
        descriptor.Field(t => t.Model).Type<StringType>();
    }
}

public class LargeLanguageModelType : ObjectType<LargeLanguageModel>
{
    protected override void Configure(IObjectTypeDescriptor<LargeLanguageModel> descriptor)
    {
        descriptor.Field(m => m.Id).Type<IdType>().ID();
        descriptor.Field(m => m.ProjectId).Type<IdType>();
        descriptor.Field(m => m.Url).Type<StringType>();
        descriptor.Field(m => m.Model).Type<StringType>();
        descriptor.Field(m => m.ModelAlias).Type<StringType>();
        descriptor.Field(m => m.ApiKey).Type<StringType>();
        descriptor.Field(m => m.MaxComplexity).Type<IntType>();
        descriptor.Field(m => m.MaxConcurrency).Type<IntType>();
    }
}
