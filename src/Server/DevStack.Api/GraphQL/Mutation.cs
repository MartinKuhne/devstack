using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.GraphQL.Types;

public record CreateProjectInput(
    string Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl);

public record UpdateProjectInput(
    Guid Id,
    string? Name,
    string? Description,
    string? Architecture,
    string? Memory,
    string? GithubUrl,
    string? GithubToken_Encrypted);

public record DeleteProjectInput(Guid Id);

public record ProjectPayload(Project? Project, List<string> Errors);

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
    string? Blocking,
    string? Result,
    string? Errors);

public record TransitionDeliverableInput(
    Guid Id,
    DeliverableStatus TargetStatus,
    string Actor);

public record DeleteDeliverableInput(Guid Id);

public record DeliverablePayload(Deliverable? Deliverable, List<string> Errors);

public record CreateAgentTaskInput(
    Guid ProjectId,
    Guid DeliverableId,
    string Title,
    int ComplexityRating,
    string? Result,
    string? Errors,
    string? CommitHash,
    string? DependsOnAgentTask,
    int? PromptTokens,
    int? CompletionTokens,
    double? ExecutionDurationInSeconds,
    string? Model);

public record UpdateAgentTaskInput(
    Guid Id,
    string? Title,
    string? Result,
    string? Errors,
    string? CommitHash,
    string? DependsOnAgentTask,
    int? ComplexityRating,
    int? PromptTokens,
    int? CompletionTokens,
    double? ExecutionDurationInSeconds,
    string? Model);

public record TransitionAgentTaskInput(
    Guid Id,
    AgentTaskStatus TargetStatus,
    string Actor);

public record DeleteAgentTaskInput(Guid Id);

public record AgentTaskPayload(AgentTask? AgentTask, List<string> Errors);

public record CreateLargeLanguageModelInput(
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int MaxComplexity,
    Guid ProjectId);

public record UpdateLargeLanguageModelInput(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity,
    Guid? ProjectId);

public record DeleteLargeLanguageModelInput(Guid Id);

public record LargeLanguageModelPayload(LargeLanguageModel? LargeLanguageModel, List<string> Errors);

public record CleanupTestDataPayload(bool Success, string? Message);

public class Mutation
{
    public async Task<ProjectPayload> CreateProjectAsync(
        CreateProjectInput input,
        [Service] ICreateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Name))
            errors.Add("Name is required");

        if (!string.IsNullOrEmpty(input.Name) && input.Name.Length > 200)
            errors.Add("Name must be 200 characters or less");

        if (errors.Count > 0)
            return new ProjectPayload(null, errors);

        try
        {
            var id = await handler.Handle(new DevStack.Infrastructure.Projects.CreateProjectCommand(
                input.Name,
                input.Description,
                input.Architecture,
                input.Memory,
                input.GithubUrl), cancellationToken);

            var project = new Project { Id = id };
            return new ProjectPayload(project, new List<string>());
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [ex.Message]);
        }
    }

    public async Task<ProjectPayload> UpdateProjectAsync(
        UpdateProjectInput input,
        [Service] IUpdateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(input.Name) && input.Name.Length > 200)
            errors.Add("Name must be 200 characters or less");

        if (errors.Count > 0)
            return new ProjectPayload(null, errors);

        try
        {
            await handler.Handle(new DevStack.Infrastructure.Projects.UpdateProjectCommand(
                input.Id,
                input.Name,
                input.Description,
                input.Architecture,
                input.Memory,
                input.GithubUrl,
                input.GithubToken_Encrypted), cancellationToken);

            var project = new Project { Id = input.Id };
            return new ProjectPayload(project, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ProjectPayload(null, ["NOT_FOUND: Project not found"]);
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [ex.Message]);
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
            return new ProjectPayload(project, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ProjectPayload(null, ["NOT_FOUND: Project not found"]);
        }
        catch (Exception ex)
        {
            return new ProjectPayload(null, [ex.Message]);
        }
    }

    public async Task<DeliverablePayload> CreateDeliverableAsync(
        CreateDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (errors.Count > 0)
            return new DeliverablePayload(null, errors);

        try
        {
            var deliverable = new Deliverable
            {
                ProjectId = input.ProjectId,
                Title = input.Title,
                Type = Enum.Parse<DeliverableType>(input.Type, ignoreCase: true),
                Description = input.Description,
                AcceptanceCriteria = input.AcceptanceCriteria,
                Plan = input.ExecutionPlan,
                SecurityImpact = input.SecurityImpact,
                PerformanceImpact = input.PerformanceImpact,
                TestPlan = input.TestPlan,
                DeploymentPlan = input.DeploymentPlan,
                Status = input.InitialStatus ?? DeliverableStatus.Planning,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Deliverables.Add(deliverable);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<string>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [ex.Message]);
        }
    }

    public async Task<DeliverablePayload> UpdateDeliverableAsync(
        UpdateDeliverableInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.Id], cancellationToken);
            if (deliverable == null)
                return new DeliverablePayload(null, ["NOT_FOUND: Deliverable not found"]);

            if (input.Title is not null) deliverable.Title = input.Title;
            if (input.Description is not null) deliverable.Description = input.Description;
            if (input.AcceptanceCriteria is not null) deliverable.AcceptanceCriteria = input.AcceptanceCriteria;
            if (input.ExecutionPlan is not null) deliverable.Plan = input.ExecutionPlan;
            if (input.SecurityImpact is not null) deliverable.SecurityImpact = input.SecurityImpact;
            if (input.PerformanceImpact is not null) deliverable.PerformanceImpact = input.PerformanceImpact;
            if (input.TestPlan is not null) deliverable.TestPlan = input.TestPlan;
            if (input.DeploymentPlan is not null) deliverable.DeploymentPlan = input.DeploymentPlan;
            if (input.Result is not null) deliverable.Result = input.Result;
            if (input.Errors is not null) deliverable.Errors = input.Errors;
            deliverable.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<string>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [ex.Message]);
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
                return new DeliverablePayload(null, ["NOT_FOUND: Deliverable not found"]);

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, input.TargetStatus, input.Actor);

            if (!result.IsSuccess)
                return new DeliverablePayload(null, [result.Errors[0]]);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<string>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [ex.Message]);
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
                return new DeliverablePayload(null, ["NOT_FOUND: Deliverable not found"]);

            dbContext.Deliverables.Remove(deliverable);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeliverablePayload(deliverable, new List<string>());
        }
        catch (Exception ex)
        {
            return new DeliverablePayload(null, [ex.Message]);
        }
    }

    public async Task<AgentTaskPayload> CreateAgentTaskAsync(
        CreateAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (input.ComplexityRating < 1 || input.ComplexityRating > 10)
            errors.Add("ComplexityRating must be between 1 and 10");

        if (errors.Count > 0)
            return new AgentTaskPayload(null, errors);

        try
        {
            var deliverable = await dbContext.Deliverables.FindAsync([input.DeliverableId], cancellationToken);
            if (deliverable == null)
                return new AgentTaskPayload(null, ["NOT_FOUND: Deliverable not found"]);

            var agentTask = new AgentTask
            {
                ProjectId = input.ProjectId,
                DeliverableId = input.DeliverableId,
                Title = input.Title,
                ComplexityRating = input.ComplexityRating,
                Result = input.Result,
                Errors = input.Errors,
                CommitHash = input.CommitHash,
                DependsOnDevTask = input.DependsOnAgentTask,
                PromptTokens = input.PromptTokens,
                CompletionTokens = input.CompletionTokens,
                ExecutionDurationInSeconds = input.ExecutionDurationInSeconds,
                Model = input.Model,
                Status = AgentTaskStatus.Ready,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.AgentTasks.Add(agentTask);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<string>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [ex.Message]);
        }
    }

    public async Task<AgentTaskPayload> UpdateAgentTaskAsync(
        UpdateAgentTaskInput input,
        [Service] DevStackDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var agentTask = await dbContext.AgentTasks.FindAsync([input.Id], cancellationToken);
            if (agentTask == null)
                return new AgentTaskPayload(null, ["NOT_FOUND: AgentTask not found"]);

            if (input.Title is not null) agentTask.Title = input.Title;
            if (input.Result is not null) agentTask.Result = input.Result;
            if (input.Errors is not null) agentTask.Errors = input.Errors;
            if (input.CommitHash is not null) agentTask.CommitHash = input.CommitHash;
            if (input.DependsOnAgentTask is not null) agentTask.DependsOnDevTask = input.DependsOnAgentTask;
            if (input.ComplexityRating.HasValue) agentTask.ComplexityRating = input.ComplexityRating.Value;
            if (input.PromptTokens.HasValue) agentTask.PromptTokens = input.PromptTokens;
            if (input.CompletionTokens.HasValue) agentTask.CompletionTokens = input.CompletionTokens;
            if (input.ExecutionDurationInSeconds.HasValue) agentTask.ExecutionDurationInSeconds = input.ExecutionDurationInSeconds;
            if (input.Model is not null) agentTask.Model = input.Model;
            agentTask.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<string>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [ex.Message]);
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
                return new AgentTaskPayload(null, ["NOT_FOUND: AgentTask not found"]);

            var service = new AgentTaskStatusTransitionService();
            var result = service.Transition(agentTask, input.TargetStatus, input.Actor);

            if (!result.IsSuccess)
                return new AgentTaskPayload(null, [result.Errors[0]]);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<string>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [ex.Message]);
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
                return new AgentTaskPayload(null, ["NOT_FOUND: AgentTask not found"]);

            dbContext.AgentTasks.Remove(agentTask);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new AgentTaskPayload(agentTask, new List<string>());
        }
        catch (Exception ex)
        {
            return new AgentTaskPayload(null, [ex.Message]);
        }
    }

    public async Task<LargeLanguageModelPayload> CreateLargeLanguageModelAsync(
        CreateLargeLanguageModelInput input,
        [Service] ICreateLargeLanguageModelHandler handler,
        ISecretService secretService,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Url))
            errors.Add("Url is required");

        if (string.IsNullOrWhiteSpace(input.Model))
            errors.Add("Model is required");

        if (string.IsNullOrWhiteSpace(input.ApiKey))
            errors.Add("ApiKey is required");

        if (errors.Count > 0)
            return new LargeLanguageModelPayload(null, errors);

        try
        {
            var encryptedApiKey = secretService.Encrypt(input.ApiKey);

            var id = await handler.Handle(new CreateLargeLanguageModelCommand(
                input.Url,
                input.Model,
                input.ModelAlias,
                encryptedApiKey,
                input.MaxComplexity,
                input.ProjectId), cancellationToken);

            var model = new LargeLanguageModel { Id = id };
            return new LargeLanguageModelPayload(model, new List<string>());
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [ex.Message]);
        }
    }

    public async Task<LargeLanguageModelPayload> UpdateLargeLanguageModelAsync(
        UpdateLargeLanguageModelInput input,
        [Service] IUpdateLargeLanguageModelHandler handler,
        ISecretService secretService,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            string? encryptedApiKey = null;
            if (input.ApiKey is not null)
            {
                encryptedApiKey = secretService.Encrypt(input.ApiKey);
            }

            await handler.Handle(new UpdateLargeLanguageModelCommand(
                input.Id,
                input.Url,
                input.Model,
                input.ModelAlias,
                encryptedApiKey,
                input.MaxComplexity,
                input.ProjectId), cancellationToken);

            var model = new LargeLanguageModel { Id = input.Id };
            return new LargeLanguageModelPayload(model, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new LargeLanguageModelPayload(null, ["NOT_FOUND: LargeLanguageModel not found"]);
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [ex.Message]);
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
            return new LargeLanguageModelPayload(model, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new LargeLanguageModelPayload(null, ["NOT_FOUND: LargeLanguageModel not found"]);
        }
        catch (Exception ex)
        {
            return new LargeLanguageModelPayload(null, [ex.Message]);
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
