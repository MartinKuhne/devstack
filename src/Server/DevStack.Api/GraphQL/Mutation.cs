using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.ModelConfigurations;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.GraphQL.Types;

public record CreateProjectInput(
    string Name,
    string? Description,
    string? Repository);

public record UpdateProjectInput(
    Guid Id,
    string? Name,
    string? Description,
    string? Repository);

public record DeleteProjectInput(Guid Id);

public record ProjectPayload(Project? Project, List<FieldError> Errors);

public record CreateDeliverableInput(
    Guid ProjectId,
    string Title,
    string Type,
    string? Description,
    string? AcceptanceCriteria,
    string? AgentFeedback,
    string? ExecutionPlan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? Blocking,
    DeliverableStatus? InitialStatus);

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

public record TransitionDeliverableInput(
    Guid Id,
    DeliverableStatus TargetStatus,
    string Actor);

public record DeleteDeliverableInput(Guid Id);

public record DeliverablePayload(Deliverable? Deliverable, List<FieldError> Errors);

public record CreateAgentTaskInput(
    Guid DeliverableId,
    string Title,
    string Description,
    int ComplexityRating,
    string? Result,
    string? Errors,
    string? CommitHash,
    Guid? DependsOnAgentTaskId,
    int? PromptTokens,
    int? CompletionTokens,
    int? ExecutionDurationInSeconds,
    string? Agent);

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

public record TransitionAgentTaskInput(
    Guid Id,
    AgentTaskStatus TargetStatus,
    string Actor);

public record DeleteAgentTaskInput(Guid Id);

public record AgentTaskPayload(AgentTask? AgentTask, List<FieldError> Errors);

public record CreateLargeLanguageModelInput(
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int MaxComplexity,
    int? MaxConcurrency);

public record UpdateLargeLanguageModelInput(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity,
    int? MaxConcurrency);

public record DeleteLargeLanguageModelInput(Guid Id);

public record LargeLanguageModelPayload(LargeLanguageModel? LargeLanguageModel, List<FieldError> Errors);

public record CleanupTestDataPayload(bool Success, string? Message);

public class Mutation
{
    private readonly DevStackDbContext _dbContext;

    public Mutation(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectPayload> CreateProjectAsync(
        CreateProjectInput input,
        [Service] ICreateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        var validator = new CreateProjectInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ProjectPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var id = await handler.Handle(new DevStack.Infrastructure.Projects.CreateProjectCommand(
                input.Name,
                input.Description,
                input.Repository), cancellationToken);

            var project = await _dbContext.Projects.FindAsync(id, cancellationToken);
            return new ProjectPayload(project, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<ProjectPayload> UpdateProjectAsync(
        UpdateProjectInput input,
        [Service] IUpdateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateProjectInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ProjectPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            await handler.Handle(new DevStack.Infrastructure.Projects.UpdateProjectCommand(
                input.Id,
                input.Name,
                input.Description,
                input.Repository), cancellationToken);

            var project = await _dbContext.Projects.FindAsync(input.Id, cancellationToken);
            return new ProjectPayload(project, new List<FieldError>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ProjectPayload(null, [new FieldError("Server", "Project not found")]);
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<ProjectPayload> DeleteProjectAsync(
        DeleteProjectInput input,
        [Service] IDeleteProjectHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DevStack.Infrastructure.Projects.DeleteProjectCommand(input.Id), cancellationToken);

            var project = new Project { Id = input.Id };
            return new ProjectPayload(project, new List<FieldError>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ProjectPayload(null, [new FieldError("Server", "Project not found")]);
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<DeliverablePayload> CreateDeliverableAsync(
        CreateDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validator = new CreateDeliverableInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new DeliverablePayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var deliverableType = (DevStack.Domain.Enums.DeliverableType)Enum.Parse(typeof(DevStack.Domain.Enums.DeliverableType), input.Type, ignoreCase: true);

            var deliverable = new Deliverable
            {
                ProjectId = input.ProjectId,
                Title = input.Title,
                Type = deliverableType,
                Description = input.Description,
                AcceptanceCriteria = input.AcceptanceCriteria,
                ExecutionPlan = input.ExecutionPlan,
                AgentFeedback = input.AgentFeedback,
                SecurityImpact = input.SecurityImpact,
                PerformanceImpact = input.PerformanceImpact,
                TestPlan = input.TestPlan,
                DeploymentPlan = input.DeploymentPlan,
                Blocking = input.Blocking,
                Status = input.InitialStatus ?? DeliverableStatus.Planning
            };

            dbContext.Deliverables.Add(deliverable);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<DeliverablePayload> UpdateDeliverableAsync(
        UpdateDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateDeliverableInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new DeliverablePayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
            if (deliverable == null)
                return new DeliverablePayload(null, [new FieldError("Server", "Deliverable not found")]);

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

            return new DeliverablePayload(deliverable, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<DeliverablePayload> TransitionDeliverableStatusAsync(
        TransitionDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
            if (deliverable == null)
                return new DeliverablePayload(null, [new FieldError("Server", "Deliverable not found")]);

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, input.TargetStatus, input.Actor);

            if (!result.IsSuccess)
                return new DeliverablePayload(null, result.Errors.Select(e => new FieldError("StatusTransition", e)).ToList());

            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<DeliverablePayload> DeleteDeliverableAsync(
        DeleteDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
            if (deliverable == null)
                return new DeliverablePayload(null, [new FieldError("Server", "Deliverable not found")]);

            dbContext.Deliverables.Remove(deliverable);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<AgentTaskPayload> CreateAgentTaskAsync(
        CreateAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validator = new CreateAgentTaskInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new AgentTaskPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.DeliverableId], cancellationToken);
            if (deliverable == null)
                return new AgentTaskPayload(null, [new FieldError("DeliverableId", "Deliverable not found")]);

            var agentTask = new AgentTask
            {
                ProjectId = deliverable.ProjectId,
                DeliverableId = input.DeliverableId,
                Title = input.Title,
                Description = input.Description,
                ComplexityRating = input.ComplexityRating,
                Result = input.Result,
                Errors = input.Errors,
                CommitHash = input.CommitHash,
                DependsOnAgentTaskId = input.DependsOnAgentTaskId,
                PromptTokens = input.PromptTokens,
                CompletionTokens = input.CompletionTokens,
                ExecutionDurationInSeconds = input.ExecutionDurationInSeconds,
                Agent = input.Agent,
                Status = AgentTaskStatus.Ready
            };

            dbContext.AgentTasks.Add(agentTask);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<AgentTaskPayload> UpdateAgentTaskAsync(
        UpdateAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateAgentTaskInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new AgentTaskPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
            if (agentTask == null)
                return new AgentTaskPayload(null, [new FieldError("Server", "AgentTask not found")]);

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

            return new AgentTaskPayload(agentTask, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<AgentTaskPayload> TransitionAgentTaskStatusAsync(
        TransitionAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
            if (agentTask == null)
                return new AgentTaskPayload(null, [new FieldError("Server", "AgentTask not found")]);

            var service = new AgentTaskStatusTransitionService();
            var result = service.Transition(agentTask, input.TargetStatus, input.Actor);

            if (!result.IsSuccess)
                return new AgentTaskPayload(null, result.Errors.Select(e => new FieldError("StatusTransition", e)).ToList());

            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<AgentTaskPayload> DeleteAgentTaskAsync(
        DeleteAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
            if (agentTask == null)
                return new AgentTaskPayload(null, [new FieldError("Server", "AgentTask not found")]);

            dbContext.AgentTasks.Remove(agentTask);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<LargeLanguageModelPayload> CreateLargeLanguageModelAsync(
        CreateLargeLanguageModelInput input,
        [Service] ICreateLargeLanguageModelHandler handler,
        CancellationToken cancellationToken)
    {
        var validator = new CreateLargeLanguageModelInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new LargeLanguageModelPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            var id = await handler.Handle(new CreateLargeLanguageModelCommand(
                input.Url,
                input.Model,
                input.ModelAlias,
                input.ApiKey,
                input.MaxComplexity,
                input.MaxConcurrency ?? 0), cancellationToken);

            var model = new LargeLanguageModel { Id = id };
            return new LargeLanguageModelPayload(model, new List<FieldError>());
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<LargeLanguageModelPayload> UpdateLargeLanguageModelAsync(
        UpdateLargeLanguageModelInput input,
        [Service] IUpdateLargeLanguageModelHandler handler,
        CancellationToken cancellationToken)
    {
        var validator = new UpdateLargeLanguageModelInputValidator();
        var validationResult = await validator.ValidateAsync(input, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new LargeLanguageModelPayload(null, FieldErrorMapper.Map(validationResult));
        }

        try
        {
            await handler.Handle(new UpdateLargeLanguageModelCommand(
                input.Id,
                input.Url,
                input.Model,
                input.ModelAlias,
                input.ApiKey,
                input.MaxComplexity,
                input.MaxConcurrency), cancellationToken);

            var model = new LargeLanguageModel { Id = input.Id };
            return new LargeLanguageModelPayload(model, new List<FieldError>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new LargeLanguageModelPayload(null, [new FieldError("Server", "LargeLanguageModel not found")]);
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [new FieldError("Server", ex.Message)]);
        }
    }

    public async Task<LargeLanguageModelPayload> DeleteLargeLanguageModelAsync(
        DeleteLargeLanguageModelInput input,
        [Service] IDeleteLargeLanguageModelHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(new DeleteLargeLanguageModelCommand(input.Id), cancellationToken);

            var model = new LargeLanguageModel { Id = input.Id };
            return new LargeLanguageModelPayload(model, new List<FieldError>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new LargeLanguageModelPayload(null, [new FieldError("Server", "LargeLanguageModel not found")]);
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [new FieldError("Server", ex.Message)]);
        }
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
}
