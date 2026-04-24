using DevStack.Application;
using DevStack.Application.Projects.Commands;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.GraphQL.Types;

public record CreateProjectInput(
    string Name,
    string Repository,
    string? Description
);

public record UpdateProjectInput(
    Guid Id,
    string? Name,
    string? Description,
    string? Repository);

public record CreateDeliverableInput(
    Guid ProjectId,
    string Title,
    string Type,
    string Description,
    DeliverableStatus InitialStatus,
    string? AcceptanceCriteria,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan
    );

public record UpdateDeliverableInput(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? AgentFeedback,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? Blocking);

public record UpdateDeliverableStatusInput(
    Guid Id,
    DeliverableStatus TargetStatus,
    string Actor);

public record CreateAgentTaskInput(
    Guid DeliverableId,
    Guid ProjectId,
    string Title,
    string Description,
    Guid? DependsOnAgentTaskId,
        int ComplexityRating = 5
);

public record UpdateAgentTaskInput(
    Guid Id,
    string? Title,
    string? Description,
    string? Result,
    string? Errors,
    string? CommitHash,
    Guid? DependsOnAgentTaskId,
    int? ComplexityRating,
    int? PromptTokens,
    int? CompletionTokens,
    int? ExecutionDurationInSeconds,
    string? Agent);

public record UpdateAgentTaskStatusInput(
    Guid Id,
    AgentTaskStatus TargetStatus,
    string Actor);

public record CreateLargeLanguageModelInput(
    string Url,
    string Model,
    string? ModelAlias,
    string? ApiKey,
    int MaxComplexity = 10,
    int MaxConcurrency = 1);

public record UpdateLargeLanguageModelInput(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity,
    int? MaxConcurrency);

public record CleanupTestDataPayload(bool Success, string? Message);

public class Mutation
{
    public async Task<Project?> CreateProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<Guid, CreateProjectCommand> handler,
        CreateProjectInput input,
        CancellationToken cancellationToken)
    {
        var id = await handler.Handle(new CreateProjectCommand(
            input.Name,
            input.Description,
            input.Repository), cancellationToken);

        var project = await dbContext.Projects.FindAsync(id, cancellationToken);
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<UpdateProjectCommand> handler,
        UpdateProjectInput input,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateProjectCommand(
            input.Id,
            input.Name,
            input.Description,
            input.Repository), cancellationToken);

        var project = await dbContext.Projects.FindAsync(input.Id, cancellationToken);
        return project;
    }

    public async Task<bool> DeleteProjectAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICommandHandler<DeleteProjectCommand> handler,
        Guid id,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteProjectCommand(id), cancellationToken);
        return true;
    }

    public async Task<Deliverable?> CreateDeliverableAsync(
        [Service] DevStackDbContext dbContext,
        CreateDeliverableInput input,
        CancellationToken cancellationToken)
    {
        var deliverableType = (DeliverableType)Enum.Parse(typeof(DeliverableType), input.Type, ignoreCase: true);

        var deliverable = new Deliverable
        {
            ProjectId = input.ProjectId,
            Title = input.Title,
            Type = deliverableType,
            Description = input.Description,
            AcceptanceCriteria = input.AcceptanceCriteria,
            ExecutionPlan = input.ExecutionPlan,
            SecurityImpact = input.SecurityImpact,
            PerformanceImpact = input.PerformanceImpact,
            TestPlan = input.TestPlan,
            DeploymentPlan = input.DeploymentPlan,
            Status = input.InitialStatus
        };

        dbContext.Deliverables.Add(deliverable);
        await dbContext.SaveChangesAsync(cancellationToken);

        return deliverable;
    }

    public async Task<Deliverable> UpdateDeliverableAsync(
        [Service] DevStackDbContext dbContext,
        UpdateDeliverableInput input,
        CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
        if (deliverable == null)
        {
            throw new InvalidOperationException();
        }

        if (input.Title is not null) deliverable.Title = input.Title;
        if (input.Description is not null) deliverable.Description = input.Description;
        if (input.AcceptanceCriteria is not null) deliverable.AcceptanceCriteria = input.AcceptanceCriteria;
        if (input.ExecutionPlan is not null) deliverable.ExecutionPlan = input.ExecutionPlan;
        if (input.AgentFeedback is not null) deliverable.AgentFeedback = input.AgentFeedback;
        if (input.SecurityImpact is not null) deliverable.SecurityImpact = input.SecurityImpact;
        if (input.PerformanceImpact is not null) deliverable.PerformanceImpact = input.PerformanceImpact;
        if (input.TestPlan is not null) deliverable.TestPlan = input.TestPlan;
        if (input.DeploymentPlan is not null) deliverable.DeploymentPlan = input.DeploymentPlan;
        if (input.Blocking is not null) deliverable.Blocking = input.Blocking;

        await dbContext.SaveChangesAsync(cancellationToken);

        return deliverable;
    }

    public async Task<DeliverableStatus> UpdateDeliverableStatusAsync(
        [Service] DevStackDbContext dbContext,
        Guid id,
        DeliverableStatus targetStatus,
        string? actor,
        CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync(id, cancellationToken);
        if (deliverable == null)
        {
            throw new InvalidOperationException();
        }

        if (deliverable.Status != targetStatus)
        {
            deliverable.Status = targetStatus;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return targetStatus;
    }

    public async Task<bool> DeleteDeliverableAsync(
        [Service] DevStackDbContext dbContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync(id, cancellationToken);
        if (deliverable == null)
        {
            throw new InvalidOperationException();
        }

        dbContext.Deliverables.Remove(deliverable);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AgentTask> CreateAgentTaskAsync(
        [Service] DevStackDbContext dbContext,
        CreateAgentTaskInput input,
        CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync([input.DeliverableId], cancellationToken);
        if (deliverable == null)
        {
            throw new InvalidOperationException("Deliverable does not exist");
        }

        var agentTask = new AgentTask
        {
            ProjectId = deliverable.ProjectId,
            DeliverableId = input.DeliverableId,
            Title = input.Title,
            Description = input.Description,
            ComplexityRating = input.ComplexityRating,
            DependsOnAgentTaskId = input.DependsOnAgentTaskId,
            Status = AgentTaskStatus.Ready
        };

        dbContext.AgentTasks.Add(agentTask);
        await dbContext.SaveChangesAsync(cancellationToken);
        return agentTask;
    }

    public async Task<AgentTask> UpdateAgentTaskAsync(
        [Service] DevStackDbContext dbContext,
        UpdateAgentTaskInput input,
        CancellationToken cancellationToken)
    {

        var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("AgentTask does not exist");
        }

        if (input.Title is not null) agentTask.Title = input.Title;
        if (input.Description is not null) agentTask.Description = input.Description;
        if (input.Result is not null) agentTask.Result = input.Result;
        if (input.Errors is not null) agentTask.Errors = input.Errors;
        if (input.CommitHash is not null) agentTask.CommitHash = input.CommitHash;
        if (input.DependsOnAgentTaskId.HasValue) agentTask.DependsOnAgentTaskId = input.DependsOnAgentTaskId.Value;
        if (input.ComplexityRating.HasValue) agentTask.ComplexityRating = input.ComplexityRating.Value;
        if (input.PromptTokens.HasValue) agentTask.PromptTokens = input.PromptTokens;
        if (input.CompletionTokens.HasValue) agentTask.CompletionTokens = input.CompletionTokens;
        if (input.ExecutionDurationInSeconds.HasValue) agentTask.ExecutionDurationInSeconds = input.ExecutionDurationInSeconds;
        if (input.Agent is not null) agentTask.Agent = input.Agent;

        await dbContext.SaveChangesAsync(cancellationToken);
        return agentTask;
    }

    public async Task<AgentTaskStatus> UpdateAgentTaskStatusAsync(
        [Service] DevStackDbContext dbContext,
        Guid id,
        AgentTaskStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var agentTask = await dbContext.AgentTasks.FindAsync(id, cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("AgentTask does not exist");
        }

        if (agentTask.Status != targetStatus)
        {
            agentTask.Status = targetStatus;            
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (targetStatus == AgentTaskStatus.Done)
        {
            await CheckAndMarkDeliverableDoneAsync(dbContext, agentTask.DeliverableId, cancellationToken);
        }

        return targetStatus;
    }

    public async Task<bool> DeleteAgentTaskAsync(
        [Service] DevStackDbContext dbContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        var agentTask = await dbContext.AgentTasks.FindAsync(id, cancellationToken);
        if (agentTask == null)
        {
            throw new InvalidOperationException("AgentTask does not exist");
        }

        dbContext.AgentTasks.Remove(agentTask);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<LargeLanguageModel?> CreateLargeLanguageModelAsync(
        [Service] DevStackDbContext dbContext,
        [Service] ICreateLargeLanguageModelHandler handler,
        CreateLargeLanguageModelInput input,
        CancellationToken cancellationToken)
    {
        var id = await handler.Handle(new CreateLargeLanguageModelCommand(
            input.Url,
            input.Model,
            input.ModelAlias,
            input.ApiKey ?? String.Empty,
            input.MaxComplexity,
            input.MaxConcurrency), cancellationToken);

        var model = await dbContext.LargeLanguageModels.FindAsync(id, cancellationToken);
        return model;
    }

    public async Task<LargeLanguageModel?> UpdateLargeLanguageModelAsync(
        [Service] DevStackDbContext dbContext,
        [Service] IUpdateLargeLanguageModelHandler handler,
        UpdateLargeLanguageModelInput input,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UpdateLargeLanguageModelCommand(
            input.Id,
            input.Url,
            input.Model,
            input.ModelAlias,
            input.ApiKey,
            input.MaxComplexity,
            input.MaxConcurrency), cancellationToken);

        var model = await dbContext.LargeLanguageModels.FindAsync(input.Id, cancellationToken);
        return model;
    }

    public async Task<bool> DeleteLargeLanguageModelAsync(
        [Service] DevStackDbContext dbContext,
        Guid id,
        CancellationToken cancellationToken)
    {
        var model = await dbContext.LargeLanguageModels.FindAsync(id, cancellationToken);
        if (model == null)
        {
            throw new InvalidOperationException();
        }

        dbContext.LargeLanguageModels.Remove(model);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<CleanupTestDataPayload> CleanupTestDataAsync(
        [Service] DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await context.CleanupTestDataAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new CleanupTestDataPayload(true, "Test data cleaned up successfully");
        }
        catch (Exception ex)
        {
            return new CleanupTestDataPayload(false, ex.Message);
        }
    }

    public async Task<bool> CheckAndMarkDeliverableDoneAsync(
        [Service] DevStackDbContext dbContext,
        Guid deliverableId,
        CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync(deliverableId, cancellationToken);
        if (deliverable == null)
        {
            return false;
        }

        var allTasks = await dbContext.AgentTasks
            .Where(t => t.DeliverableId == deliverableId)
            .ToListAsync(cancellationToken);

        if (!allTasks.Any())
        {
            await SetDeliverableToDoneAsync(dbContext, deliverableId, cancellationToken);
            return true;
        }

        var allDone = allTasks.All(t => t.Status == AgentTaskStatus.Done);
        if (allDone)
        {
            await SetDeliverableToDoneAsync(dbContext, deliverableId, cancellationToken);
            return true;
        }

        return false;
    }

    private async Task SetDeliverableToDoneAsync(DevStackDbContext dbContext, Guid deliverableId, CancellationToken cancellationToken)
    {
        var deliverable = await dbContext.Deliverables.FindAsync(deliverableId, cancellationToken);
        if (deliverable != null && deliverable.Status != DeliverableStatus.Done)
        {
            deliverable.Status = DeliverableStatus.Done;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
