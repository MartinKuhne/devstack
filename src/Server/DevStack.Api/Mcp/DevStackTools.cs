using DevStack.Domain.Enums;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.WorkflowRuns;
using DevStack.Infrastructure.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DevStack.Api.Mcp;

[McpServerToolType]
public class DevStackTools(
    ICreateProjectHandler createProjectHandler,
    IUpdateProjectHandler updateProjectHandler,
    IDeleteProjectHandler deleteProjectHandler,
    ICreateFeatureHandler createFeatureHandler,
    IUpdateFeatureHandler updateFeatureHandler,
    ITransitionFeatureStatusHandler transitionFeatureStatusHandler,
    IDeleteFeatureHandler deleteFeatureHandler,
    ICreateDefectHandler createDefectHandler,
    IUpdateDefectHandler updateDefectHandler,
    ITransitionDefectStatusHandler transitionDefectStatusHandler,
    IDeleteDefectHandler deleteDefectHandler,
    ICreateTaskHandler createTaskHandler,
    IUpdateTaskHandler updateTaskHandler,
    ITransitionTaskStatusHandler transitionTaskHandler,
    IDeleteTaskHandler deleteTaskHandler,
    ICreateModelConfigurationHandler createModelConfigurationHandler,
    IUpdateModelConfigurationHandler updateModelConfigurationHandler,
    IDeleteModelConfigurationHandler deleteModelConfigurationHandler,
    ICreateWorkflowRunHandler createWorkflowRunHandler,
    IUpdateWorkflowRunHandler updateWorkflowRunHandler,
    ICancelWorkflowRunHandler cancelWorkflowRunHandler,
    ISecretService secretService)
{
    [McpServerTool, Description("Create a new project")]
    public async Task<string> CreateProject(
        string name,
        string? description = null,
        string? architecture = null,
        string? memory = null,
        string? githubUrl = null,
        CancellationToken cancellationToken = default)
    {
        var id = await createProjectHandler.Handle(
            new CreateProjectCommand(name, description, architecture, memory, githubUrl),
            cancellationToken);
        
        return $"Project created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing project")]
    public async Task<string> UpdateProject(
        Guid id,
        string? name = null,
        string? description = null,
        string? architecture = null,
        string? memory = null,
        string? githubUrl = null,
        string? githubToken_Encrypted = null,
        CancellationToken cancellationToken = default)
    {
        await updateProjectHandler.Handle(
            new UpdateProjectCommand(id, name, description, architecture, memory, githubUrl, githubToken_Encrypted),
            cancellationToken);
        
        return $"Project {id} updated successfully";
    }

    [McpServerTool, Description("Delete a project")]
    public async Task<string> DeleteProject(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteProjectHandler.Handle(new DeleteProjectCommand(id), cancellationToken);
        return $"Project {id} deleted successfully";
    }

    [McpServerTool, Description("Create a new feature")]
    public async Task<string> CreateFeature(
        Guid projectId,
        string title,
        string? description = null,
        string? acceptanceCriteria = null,
        string? plan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? openQuestions = null,
        FeatureStatus? initialStatus = null,
        CancellationToken cancellationToken = default)
    {
        var id = await createFeatureHandler.Handle(
            new CreateFeatureCommand(projectId, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions, initialStatus),
            cancellationToken);
        
        return $"Feature created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing feature")]
    public async Task<string> UpdateFeature(
        Guid id,
        string? title = null,
        string? description = null,
        string? acceptanceCriteria = null,
        string? plan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? openQuestions = null,
        CancellationToken cancellationToken = default)
    {
        await updateFeatureHandler.Handle(
            new UpdateFeatureCommand(id, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions),
            cancellationToken);
        
        return $"Feature {id} updated successfully";
    }

    [McpServerTool, Description("Transition a feature to a new status")]
    public async Task<string> TransitionFeatureStatus(
        Guid id,
        FeatureStatus targetStatus,
        string actor,
        CancellationToken cancellationToken = default)
    {
        await transitionFeatureStatusHandler.Handle(
            new TransitionFeatureStatusCommand(id, targetStatus, actor),
            cancellationToken);
        
        return $"Feature {id} transitioned to {targetStatus}";
    }

    [McpServerTool, Description("Delete a feature")]
    public async Task<string> DeleteFeature(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteFeatureHandler.Handle(new DeleteFeatureCommand(id), cancellationToken);
        return $"Feature {id} deleted successfully";
    }

    [McpServerTool, Description("Create a new defect")]
    public async Task<string> CreateDefect(
        Guid projectId,
        Guid? parentFeatureId,
        Severity? severity,
        string title,
        string? description = null,
        string? acceptanceCriteria = null,
        string? plan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? openQuestions = null,
        FeatureStatus? initialStatus = null,
        CancellationToken cancellationToken = default)
    {
        var id = await createDefectHandler.Handle(
            new CreateDefectCommand(projectId, parentFeatureId, severity, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions, initialStatus),
            cancellationToken);
        
        return $"Defect created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing defect")]
    public async Task<string> UpdateDefect(
        Guid id,
        string? title = null,
        string? description = null,
        string? acceptanceCriteria = null,
        string? plan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? openQuestions = null,
        CancellationToken cancellationToken = default)
    {
        await updateDefectHandler.Handle(
            new UpdateDefectCommand(id, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions),
            cancellationToken);
        
        return $"Defect {id} updated successfully";
    }

    [McpServerTool, Description("Transition a defect to a new status")]
    public async Task<string> TransitionDefectStatus(
        Guid id,
        FeatureStatus targetStatus,
        string actor,
        CancellationToken cancellationToken = default)
    {
        await transitionDefectStatusHandler.Handle(
            new TransitionDefectStatusCommand(id, targetStatus, actor),
            cancellationToken);
        
        return $"Defect {id} transitioned to {targetStatus}";
    }

    [McpServerTool, Description("Delete a defect")]
    public async Task<string> DeleteDefect(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteDefectHandler.Handle(new DeleteDefectCommand(id), cancellationToken);
        return $"Defect {id} deleted successfully";
    }

    [McpServerTool, Description("Create a new task")]
    public async Task<string> CreateTask(
        Guid featureId,
        string title,
        string? deliverable = null,
        string? acceptanceCriteria = null,
        string? risks = null,
        string? result = null,
        string? requiredFollowUps = null,
        int complexityRating = 5,
        CancellationToken cancellationToken = default)
    {
        var id = await createTaskHandler.Handle(
            new CreateTaskCommand(featureId, title, deliverable, acceptanceCriteria, risks, result, requiredFollowUps, complexityRating),
            cancellationToken);
        
        return $"Task created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing task")]
    public async Task<string> UpdateTask(
        Guid id,
        string? title = null,
        string? deliverable = null,
        string? acceptanceCriteria = null,
        string? risks = null,
        string? result = null,
        string? requiredFollowUps = null,
        int? complexityRating = null,
        CancellationToken cancellationToken = default)
    {
        await updateTaskHandler.Handle(
            new UpdateTaskCommand(id, title, deliverable, acceptanceCriteria, risks, result, requiredFollowUps, complexityRating ?? 5),
            cancellationToken);
        
        return $"Task {id} updated successfully";
    }

    [McpServerTool, Description("Transition a task to a new status")]
    public async Task<string> TransitionTaskStatus(
        Guid id,
        DevStack.Domain.Enums.TaskStatus targetStatus,
        string actor,
        CancellationToken cancellationToken = default)
    {
        await transitionTaskHandler.Handle(
            new TransitionTaskStatusCommand(id, targetStatus, actor),
            cancellationToken);
        
        return $"Task {id} transitioned to {targetStatus}";
    }

    [McpServerTool, Description("Delete a task")]
    public async Task<string> DeleteTask(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteTaskHandler.Handle(new DeleteTaskCommand(id), cancellationToken);
        return $"Task {id} deleted successfully";
    }

    [McpServerTool, Description("Create a new model configuration")]
    public async Task<string> CreateModelConfiguration(
        Guid projectId,
        string url,
        string model,
        string apiKey,
        int maxComplexity,
        string? modelAlias = null,
        CancellationToken cancellationToken = default)
    {
        var encryptedApiKey = secretService.Encrypt(apiKey);
        
        var id = await createModelConfigurationHandler.Handle(
            new CreateModelConfigurationCommand(projectId, url, model, modelAlias, encryptedApiKey, maxComplexity),
            cancellationToken);
        
        return $"Model configuration created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing model configuration")]
    public async Task<string> UpdateModelConfiguration(
        Guid id,
        string? url = null,
        string? model = null,
        string? modelAlias = null,
        string? apiKey = null,
        int? maxComplexity = null,
        CancellationToken cancellationToken = default)
    {
        string? encryptedApiKey = null;
        if (apiKey is not null)
        {
            encryptedApiKey = secretService.Encrypt(apiKey);
        }

        await updateModelConfigurationHandler.Handle(
            new UpdateModelConfigurationCommand(id, url, model, modelAlias, encryptedApiKey, maxComplexity),
            cancellationToken);
        
        return $"Model configuration {id} updated successfully";
    }

    [McpServerTool, Description("Delete a model configuration")]
    public async Task<string> DeleteModelConfiguration(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteModelConfigurationHandler.Handle(new DeleteModelConfigurationCommand(id), cancellationToken);
        return $"Model configuration {id} deleted successfully";
    }

    [McpServerTool, Description("Create a new workflow run")]
    public async Task<string> CreateWorkflowRun(
        Guid projectId,
        Guid? featureId,
        Guid? taskId,
        WorkflowType workflowType,
        string inputPayload,
        CancellationToken cancellationToken = default)
    {
        var id = await createWorkflowRunHandler.Handle(
            new CreateWorkflowRunCommand(projectId, featureId, taskId, workflowType, inputPayload),
            cancellationToken);
        
        return $"Workflow run created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing workflow run")]
    public async Task<string> UpdateWorkflowRun(
        Guid id,
        WorkflowRunStatus status,
        string? outputPayload = null,
        CancellationToken cancellationToken = default)
    {
        await updateWorkflowRunHandler.Handle(
            new UpdateWorkflowRunCommand(id, status, outputPayload),
            cancellationToken);
        
        return $"Workflow run {id} updated to status {status}";
    }

    [McpServerTool, Description("Cancel a workflow run")]
    public async Task<string> CancelWorkflowRun(Guid id, CancellationToken cancellationToken = default)
    {
        await cancelWorkflowRunHandler.Handle(new CancelWorkflowRunCommand(id), cancellationToken);
        return $"Workflow run {id} cancelled";
    }
}
