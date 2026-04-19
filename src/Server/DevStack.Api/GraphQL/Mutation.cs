using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
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
    FeatureStatus? InitialStatus,
    Guid? DependsOnId);

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
    string? OpenQuestions,
    Guid? DependsOnId);

public record TransitionFeatureInput(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

public record DeleteFeatureInput(Guid Id);

public record FeaturePayload(Item? Item, List<string> Errors);

public record CreateDefectInput(
    Guid ProjectId,
    Guid? ParentFeatureId,
    string Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    Severity? Severity,
    FeatureStatus? InitialStatus,
    Guid? DependsOnId);

public record UpdateDefectInput(
    Guid Id,
    string? Title,
    string? Description,
    string? AcceptanceCriteria,
    string? Plan,
    string? SecurityImpact,
    string? PerformanceImpact,
    string? TestPlan,
    string? DeploymentPlan,
    string? OpenQuestions,
    Severity? Severity,
    string? RootCause,
    Guid? DependsOnId);

public record TransitionDefectInput(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

public record DeleteDefectInput(Guid Id);

public record DefectPayload(Item? Item, List<string> Errors);

[Obsolete("Use CreateTaskInput with ItemType=Task instead")]
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

[Obsolete("Use Item mutations with ItemType=Task filter instead")]
public record CreateTaskInput(
    Guid ProjectId,
    string Title,
    string? Description,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int ComplexityRating,
    FeatureStatus? InitialStatus);

[Obsolete("Use Item mutations with ItemType=Task filter instead")]
public record UpdateTaskInput(
    Guid Id,
    string? Title,
    string? Description,
    string? Deliverable,
    string? AcceptanceCriteria,
    string? Risks,
    string? Result,
    string? RequiredFollowUps,
    int? ComplexityRating);

[Obsolete("Use Item mutations with ItemType=Task filter instead")]
public record TransitionTaskInput(
    Guid Id,
    FeatureStatus TargetStatus,
    string Actor);

[Obsolete("Use Item mutations with ItemType=Task filter instead")]
public record DeleteTaskInput(Guid Id);

[Obsolete("Use FeaturePayload with Item instead")]
public record TaskPayload(Item? Item, List<string> Errors);

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

public record CreateWorkflowRunInput(
    Guid ProjectId,
    Guid? ItemId,
    Guid? TaskId,
    string InputPayload);

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
                input.InitialStatus,
                input.DependsOnId), cancellationToken);
            
            var feature = new Item { Id = id, ItemType = Domain.Enums.ItemSubtype.Feature };
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
                input.OpenQuestions,
                input.DependsOnId), cancellationToken);

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

    public async Task<DefectPayload> CreateDefectAsync(
        CreateDefectInput input,
        [Service] ICreateDefectHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add("Title is required");

        if (errors.Count > 0)
            return new DefectPayload(null, errors);

        try
        {
           var id = await handler.Handle(new CreateDefectCommand(
                input.ProjectId,
                input.ParentFeatureId,
                input.Title,
                input.Description,
                input.AcceptanceCriteria,
                input.Plan,
                input.SecurityImpact,
                input.PerformanceImpact,
                input.TestPlan,
                input.DeploymentPlan,
                input.OpenQuestions,
                input.Severity,
                input.InitialStatus,
                input.DependsOnId), cancellationToken);
            
            var defect = new Item { Id = id, ItemType = Domain.Enums.ItemSubtype.Defect };
            return new DefectPayload(defect, new List<string>());
        }
        catch (Exception ex)
        {
            return new DefectPayload(null, [ex.Message]);
        }
    }

    public async Task<DefectPayload> UpdateDefectAsync(
        UpdateDefectInput input,
        [Service] IUpdateDefectHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new UpdateDefectCommand(
                input.Id,
                input.Title,
                input.Description,
                input.AcceptanceCriteria,
                input.Plan,
                input.SecurityImpact,
                input.PerformanceImpact,
                input.TestPlan,
                input.DeploymentPlan,
                input.OpenQuestions,
                input.Severity,
                input.RootCause,
                input.DependsOnId), cancellationToken);

            var defect = new Item { Id = input.Id };
            return new DefectPayload(defect, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new DefectPayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new DefectPayload(null, ["CONCURRENCY_CONFLICT: The defect has been modified by another process"]);
        }
        catch (Exception ex)
        {
            return new DefectPayload(null, [ex.Message]);
        }
    }

    public async Task<DefectPayload> TransitionDefectStatusAsync(
        TransitionDefectInput input,
        [Service] ITransitionDefectStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new TransitionDefectStatusCommand(
                input.Id,
                input.TargetStatus,
                input.Actor), cancellationToken);

            var defect = new Item { Id = input.Id };
            return new DefectPayload(defect, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new DefectPayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new DefectPayload(null, ["CONCURRENCY_CONFLICT: The defect has been modified by another process"]);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transition failed"))
        {
            return new DefectPayload(null, ["DEFECT_VALIDATION_ERROR: " + ex.Message.Replace("Transition failed: ", "")]);
        }
        catch (Exception ex)
        {
            return new DefectPayload(null, [ex.Message]);
        }
    }

    public async Task<DefectPayload> DeleteDefectAsync(
        DeleteDefectInput input,
        [Service] IDeleteDefectHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteDefectCommand(input.Id), cancellationToken);
            
            var defect = new Item { Id = input.Id };
            return new DefectPayload(defect, new List<string>());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return new DefectPayload(null, ["NOT_FOUND: Item not found"]);
        }
        catch (Exception ex)
        {
            return new DefectPayload(null, [ex.Message]);
        }
    }

    [Obsolete("Use Item mutations with ItemType=Task filter instead")]
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
                input.Title,
                input.Description,
                input.Deliverable,
                input.AcceptanceCriteria,
                input.Risks,
                input.Result,
                input.RequiredFollowUps,
                input.ComplexityRating,
                input.InitialStatus ?? FeatureStatus.Planning,
                null), cancellationToken);
            
            var task = new Item { Id = id, ItemType = Domain.Enums.ItemSubtype.Task };
            return new TaskPayload(task, new List<string>());
        }
        catch (Exception ex)
        {
            return new TaskPayload(null, [ex.Message]);
        }
    }

    [Obsolete("Use Item mutations with ItemType=Task filter instead")]
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
                input.Description,
                input.Deliverable,
                input.AcceptanceCriteria,
                input.Risks,
                input.Result,
                input.RequiredFollowUps,
                input.ComplexityRating), cancellationToken);
            
            var task = new Item { Id = input.Id, ItemType = Domain.Enums.ItemSubtype.Task };
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

    [Obsolete("Use Item mutations with ItemType=Task filter instead")]
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

            var task = new Item { Id = input.Id, ItemType = Domain.Enums.ItemSubtype.Task };
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

    [Obsolete("Use Item mutations with ItemType=Task filter instead")]
    public async Task<TaskPayload> DeleteTaskAsync(
        DeleteTaskInput input,
        [Service] IDeleteTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        try
        {
            await handler.Handle(new DeleteTaskCommand(input.Id), cancellationToken);
            
            var task = new Item { Id = input.Id, ItemType = Domain.Enums.ItemSubtype.Task };
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
            // If ApiKey is provided, encrypt it
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency"))
        {
            return new LargeLanguageModelPayload(null, ["CONCURRENCY_CONFLICT: The LargeLanguageModel has been modified by another process"]);
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
        var errors = new List<string>();

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