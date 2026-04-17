using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Services;
using DevStack.Infrastructure.WorkflowRuns;
using DevStack.Infrastructure.Epics;

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

public record CreateFeatureInput(
    Guid ProjectId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    FeatureStatus? InitialStatus);

public record UpdateFeatureInput(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions);

public record TransitionFeatureInput(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

public record DeleteFeatureInput(Guid Id);

public record FeaturePayload(Item? Item, List<string> Errors);

[Obsolete("Use CreateTaskInput with ItemId parameter instead")]
public record CreateTaskInput_Old(
    Guid ProjectId,
    Guid FeatureId,
    string Title,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int ComplexityRating);

public record CreateTaskInput(
    Guid ProjectId,
    Guid ItemId,
    string Title,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int ComplexityRating);

public record UpdateTaskInput(
    Guid Id,
    string? Title,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int? ComplexityRating);

public record TransitionTaskInput(
    Guid Id,
    global::DevStack.Domain.Enums.TaskStatus TargetStatus,
    string Actor);

public record DeleteTaskInput(Guid Id);

public record TaskPayload(global::DevStack.Domain.Entities.AgentTask? Task, List<string> Errors);

public record CreateModelConfigurationInput(
    string Url,
    string Model,
    string? ModelAlias,
    string ApiKey,
    int MaxComplexity);

public record UpdateModelConfigurationInput(
    Guid Id,
    string? Url,
    string? Model,
    string? ModelAlias,
    string? ApiKey,
    int? MaxComplexity);

public record DeleteModelConfigurationInput(Guid Id);

public record ModelConfigurationPayload(ModelConfiguration? ModelConfiguration, List<string> Errors);

public record CreateWorkflowRunInput(
    Guid ProjectId,
    Guid? ItemId,
    Guid? TaskId,
    WorkflowType WorkflowType,
    string InputPayload);

public record UpdateWorkflowRunInput(
    Guid Id,
    WorkflowRunStatus Status,
    string? OutputPayload);

public record CancelWorkflowRunInput(Guid Id);

public record WorkflowRunPayload(WorkflowRun? WorkflowRun, List<string> Errors);

public record CreateEpicInput(Guid ProjectId, string Title, string? Description);

public record UpdateEpicInput(Guid Id, string? Title, string? Description);

public record DeleteEpicInput(Guid Id);

public record EpicPayload(Epic? Epic, List<string> Errors);

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

        if (!string.IsNullOrEmpty(input.GithubUrl) && !Uri.TryCreate(input.GithubUrl, UriKind.Absolute, out _))
            errors.Add("GitHub URL is not a valid URI");

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

        if (!string.IsNullOrEmpty(input.GithubUrl) && !Uri.TryCreate(input.GithubUrl, UriKind.Absolute, out _))
            errors.Add("GitHub URL is not a valid URI");

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
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new ProjectPayload(null, ["CONCURRENCY_CONFLICT: The project has been modified by another process"]);
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
        var errors = new List<string>();

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

    public async Task<FeaturePayload> CreateFeatureAsync(
        CreateFeatureInput input,
        [Service] ICreateFeatureHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (errors.Count > 0)
            return new FeaturePayload(null, errors);

        try
        {
            var id = await handler.Handle(new CreateFeatureCommand(
                input.ProjectId,
                input.Title,
                input.Description,
                input.AcceptanceCriteria,
                input.Plan,
                input.SecurityImpact,
                input.PerformanceImpact,
                input.TestPlan,
                input.DeploymentPlan,
                input.OpenQuestions,
                input.InitialStatus), cancellationToken);
            
            var feature = new Item { Id = id, Subtype = Domain.Enums.ItemSubtype.Feature };
            return new FeaturePayload(feature, new List<string>());
        }
        catch (Exception ex)
        {
            return new FeaturePayload(null, [ex.Message]);
        }
    }

    public async Task<FeaturePayload> UpdateFeatureAsync(
        UpdateFeatureInput input,
        [Service] IUpdateFeatureHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new UpdateFeatureCommand(
                input.Id,
                input.Title,
                input.Description,
                input.AcceptanceCriteria,
                input.Plan,
                input.SecurityImpact,
                input.PerformanceImpact,
                input.TestPlan,
                input.DeploymentPlan,
                input.OpenQuestions), cancellationToken);

            var feature = new Item{ Id = input.Id };
            return new FeaturePayload(feature, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new FeaturePayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new FeaturePayload(null, ["CONCURRENCY_CONFLICT: The feature has been modified by another process"]);
        }
        catch (Exception ex)
        {
            return new FeaturePayload(null, [ex.Message]);
        }
    }

    public async Task<FeaturePayload> TransitionFeatureStatusAsync(
        TransitionFeatureInput input,
        [Service] ITransitionFeatureStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new TransitionFeatureStatusCommand(
                input.Id,
                input.TargetStatus,
                input.Actor), cancellationToken);

            var feature = new Item{ Id = input.Id };
            return new FeaturePayload(feature, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new FeaturePayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new FeaturePayload(null, ["CONCURRENCY_CONFLICT: The feature has been modified by another process"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transition failed"))
        {
            return new FeaturePayload(null, ["FEATURE_VALIDATION_ERROR: " + ex.Message.Replace("Transition failed: ", "")]);
        }
        catch (Exception ex)
        {
            return new FeaturePayload(null, [ex.Message]);
        }
    }

    public async Task<FeaturePayload> DeleteFeatureAsync(
        DeleteFeatureInput input,
        [Service] IDeleteFeatureHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteFeatureCommand(input.Id), cancellationToken);
            
            var feature = new Item{ Id = input.Id };
            return new FeaturePayload(feature, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new FeaturePayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (Exception ex)
        {
            return new FeaturePayload(null, [ex.Message]);
        }
    }

    public async Task<TaskPayload> CreateTaskAsync(
        CreateTaskInput input,
        [Service] ICreateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (input.ComplexityRating < 1 || input.ComplexityRating > 10)
            errors.Add("ComplexityRating must be between 1 and 10");

        if (errors.Count > 0)
            return new TaskPayload(null, errors);

        try
        {
            var id = await handler.Handle(new CreateTaskCommand(
                input.ProjectId,
                input.ItemId,
                input.Title,
                input.Deliverable,
                input.AcceptanceCriteria,
                input.Risks,
                input.Result,
                input.RequiredFollowUps,
                input.ComplexityRating), cancellationToken);
            
            var task = new global::DevStack.Domain.Entities.AgentTask { Id = id };
            return new TaskPayload(task, new List<string>());
        }
        catch (Exception ex)
        {
            return new TaskPayload(null, [ex.Message]);
        }
    }

    public async Task<TaskPayload> UpdateTaskAsync(
        UpdateTaskInput input,
        [Service] IUpdateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new UpdateTaskCommand(
                input.Id,
                input.Title,
                input.Deliverable,
                input.AcceptanceCriteria,
                input.Risks,
                input.Result,
                input.RequiredFollowUps,
                input.ComplexityRating), cancellationToken);
            
            var task = new global::DevStack.Domain.Entities.AgentTask { Id = input.Id };
            return new TaskPayload(task, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new TaskPayload(null, ["NOT_FOUND: Task not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new TaskPayload(null, ["CONCURRENCY_CONFLICT: The task has been modified by another process"]);
        }
        catch (Exception ex)
        {
            return new TaskPayload(null, [ex.Message]);
        }
    }

    public async Task<TaskPayload> TransitionTaskStatusAsync(
        TransitionTaskInput input,
        [Service] ITransitionTaskStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new TransitionTaskStatusCommand(
                input.Id,
                input.TargetStatus,
                input.Actor), cancellationToken);

            var task = new global::DevStack.Domain.Entities.AgentTask { Id = input.Id };
            return new TaskPayload(task, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new TaskPayload(null, ["NOT_FOUND: Task not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new TaskPayload(null, ["CONCURRENCY_CONFLICT: The task has been modified by another process"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transition failed"))
        {
            return new TaskPayload(null, ["TASK_VALIDATION_ERROR: " + ex.Message.Replace("Transition failed: ", "")]);
        }
        catch (Exception ex)
        {
            return new TaskPayload(null, [ex.Message]);
        }
    }

    public async Task<TaskPayload> DeleteTaskAsync(
        DeleteTaskInput input,
        [Service] IDeleteTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteTaskCommand(input.Id), cancellationToken);
            
            var task = new global::DevStack.Domain.Entities.AgentTask { Id = input.Id };
            return new TaskPayload(task, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new TaskPayload(null, ["NOT_FOUND: Task not found"]);
        }
        catch (Exception ex)
        {
            return new TaskPayload(null, [ex.Message]);
        }
    }

    public async Task<ModelConfigurationPayload> CreateModelConfigurationAsync(
        CreateModelConfigurationInput input,
        [Service] ICreateModelConfigurationHandler handler,
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
            return new ModelConfigurationPayload(null, errors);

        try
        {
            var encryptedApiKey = secretService.Encrypt(input.ApiKey);

            var id = await handler.Handle(new CreateModelConfigurationCommand(
                input.Url,
                input.Model,
                input.ModelAlias,
                encryptedApiKey,
                input.MaxComplexity), cancellationToken);

            var modelConfiguration = new ModelConfiguration { Id = id };
            return new ModelConfigurationPayload(modelConfiguration, new List<string>());
        }
        catch (Exception ex)
        {
            return new ModelConfigurationPayload(null, [ex.Message]);
        }
    }

    public async Task<ModelConfigurationPayload> UpdateModelConfigurationAsync(
        UpdateModelConfigurationInput input,
        [Service] IUpdateModelConfigurationHandler handler,
        ISecretService secretService,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            // If ApiKey is provided, encrypt it
            string? encryptedApiKey = null;
            if (input.ApiKey is not null)
            {
                encryptedApiKey = secretService.Encrypt(input.ApiKey);
            }

            await handler.Handle(new UpdateModelConfigurationCommand(
                input.Id,
                input.Url,
                input.Model,
                input.ModelAlias,
                encryptedApiKey,
                input.MaxComplexity), cancellationToken);

            var modelConfiguration = new ModelConfiguration { Id = input.Id };
            return new ModelConfigurationPayload(modelConfiguration, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ModelConfigurationPayload(null, ["NOT_FOUND: ModelConfiguration not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new ModelConfigurationPayload(null, ["CONCURRENCY_CONFLICT: The ModelConfiguration has been modified by another process"]);
        }
        catch (Exception ex)
        {
            return new ModelConfigurationPayload(null, [ex.Message]);
        }
    }

    public async Task<WorkflowRunPayload> CreateWorkflowRunAsync(
        CreateWorkflowRunInput input,
        [Service] ICreateWorkflowRunHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            var id = await handler.Handle(new CreateWorkflowRunCommand(
                input.ProjectId,
                input.ItemId,
                input.TaskId,
                input.WorkflowType,
                input.InputPayload), cancellationToken);

            var workflowRun = new WorkflowRun { Id = id };
            return new WorkflowRunPayload(workflowRun, new List<string>());
        }
        catch (Exception ex)
        {
            return new WorkflowRunPayload(null, [ex.Message]);
        }
    }

    public async Task<WorkflowRunPayload> UpdateWorkflowRunAsync(
        UpdateWorkflowRunInput input,
        [Service] IUpdateWorkflowRunHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new UpdateWorkflowRunCommand(
                input.Id,
                input.Status,
                input.OutputPayload), cancellationToken);

            var workflowRun = new WorkflowRun { Id = input.Id };
            return new WorkflowRunPayload(workflowRun, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new WorkflowRunPayload(null, ["NOT_FOUND: WorkflowRun not found"]);
        }
        catch (Exception ex)
        {
            return new WorkflowRunPayload(null, [ex.Message]);
        }
    }

    public async Task<WorkflowRunPayload> CancelWorkflowRunAsync(
        CancelWorkflowRunInput input,
        [Service] ICancelWorkflowRunHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new CancelWorkflowRunCommand(input.Id), cancellationToken);

            var workflowRun = new WorkflowRun { Id = input.Id };
            return new WorkflowRunPayload(workflowRun, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new WorkflowRunPayload(null, ["NOT_FOUND: WorkflowRun not found"]);
        }
        catch (Exception ex)
        {
            return new WorkflowRunPayload(null, [ex.Message]);
        }
    }

    public async Task<ModelConfigurationPayload> DeleteModelConfigurationAsync(
        DeleteModelConfigurationInput input,
        [Service] IDeleteModelConfigurationHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteModelConfigurationCommand(input.Id), cancellationToken);

            var modelConfiguration = new ModelConfiguration { Id = input.Id };
            return new ModelConfigurationPayload(modelConfiguration, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new ModelConfigurationPayload(null, ["NOT_FOUND: ModelConfiguration not found"]);
        }
        catch (Exception ex)
        {
            return new ModelConfigurationPayload(null, [ex.Message]);
        }
    }

    public async Task<EpicPayload> CreateEpicAsync(
        CreateEpicInput input,
        [Service] ICreateEpicHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (!string.IsNullOrEmpty(input.Title) && input.Title.Length > 200)
            errors.Add("Title must be 200 characters or less");

        if (errors.Count > 0)
            return new EpicPayload(null, errors);

        try
        {
            var id = await handler.Handle(new CreateEpicCommand(
                input.ProjectId,
                input.Title,
                input.Description), cancellationToken);
            
            var epic = new Epic { Id = id };
            return new EpicPayload(epic, new List<string>());
        }
        catch (Exception ex)
        {
            return new EpicPayload(null, [ex.Message]);
        }
    }

    public async Task<EpicPayload> UpdateEpicAsync(
        UpdateEpicInput input,
        [Service] IUpdateEpicHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new UpdateEpicCommand(
                input.Id,
                input.Title,
                input.Description), cancellationToken);

            var epic = new Epic { Id = input.Id };
            return new EpicPayload(epic, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new EpicPayload(null, ["NOT_FOUND: Epic not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new EpicPayload(null, ["CONCURRENCY_CONFLICT: The epic has been modified by another process"]);
        }
        catch (Exception ex)
        {
            return new EpicPayload(null, [ex.Message]);
        }
    }

    public async Task<EpicPayload> DeleteEpicAsync(
        DeleteEpicInput input,
        [Service] IDeleteEpicHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteEpicCommand(input.Id), cancellationToken);
            
            var epic = new Epic { Id = input.Id };
            return new EpicPayload(epic, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new EpicPayload(null, ["NOT_FOUND: Epic not found"]);
        }
        catch (Exception ex)
        {
            return new EpicPayload(null, [ex.Message]);
        }
    }
}