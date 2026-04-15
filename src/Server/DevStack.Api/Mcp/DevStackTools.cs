using DevStack.Domain.Enums;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.WorkflowRuns;
using DevStack.Infrastructure.Services;
using DevStack.Infrastructure.Persistence;
using DevStack.Domain.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.Mcp;

[McpServerToolType]
public class DevStackTools(
    DevStackDbContext dbContext,
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

    [McpServerTool, Description("Get a project by ID")]
    public DevStack.Domain.Entities.Project? GetProjectById(Guid id)
    {
        return dbContext.Projects.Find(id);
    }

    [McpServerTool, Description("Get all projects")]
    public List<DevStack.Domain.Entities.Project> GetProjects(int first = 50, int? skip = null)
    {
        var query = dbContext.Projects.AsQueryable();
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        return query.OrderBy(p => p.CreatedAt).Take(first).ToList();
    }

    [McpServerTool, Description("Get all features with optional filtering")]
    public List<DevStack.Domain.Entities.Feature> GetFeatures(
        Guid? projectId = null,
        List<FeatureStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Features.AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(f => f.ProjectId == projectId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(f => status.Contains(f.Status));
        }
        if (createdAfter.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= createdAfter.Value);
        }
        if (createdBefore.HasValue)
        {
            query = query.Where(f => f.CreatedAt <= createdBefore.Value);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        return query.OrderBy(f => f.CreatedAt).Take(first).ToList();
    }

    [McpServerTool, Description("Get a feature by ID")]
    public DevStack.Domain.Entities.Feature? GetFeatureById(Guid id)
    {
        return dbContext.Features.Find(id);
    }

    [McpServerTool, Description("Get all defects with optional filtering")]
    public List<DevStack.Domain.Entities.Defect> GetDefects(
        Guid? projectId = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Defects.AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        return query.OrderBy(d => d.CreatedAt).Take(first).ToList();
    }

    [McpServerTool, Description("Get a defect by ID")]
    public DevStack.Domain.Entities.Defect? GetDefectById(Guid id)
    {
        return dbContext.Defects.Find(id);
    }

    [McpServerTool, Description("Get all tasks with optional filtering")]
    public List<DevStack.Domain.Entities.AgentTask> GetTasks(
        Guid? featureId = null,
        List<DevStack.Domain.Enums.TaskStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Tasks.AsQueryable();
        if (featureId.HasValue)
        {
            query = query.Where(t => t.FeatureId == featureId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(t => status.Contains(t.Status));
        }
        if (createdAfter.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= createdAfter.Value);
        }
        if (createdBefore.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= createdBefore.Value);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        return query.OrderBy(t => t.CreatedAt).Take(first).ToList();
    }

    [McpServerTool, Description("Get a task by ID")]
    public DevStack.Domain.Entities.AgentTask? GetTaskById(Guid id)
    {
        return dbContext.Tasks.Find(id);
    }

    [McpServerTool, Description("Get valid status transitions for a feature")]
    public List<FeatureStatus> GetValidStatusTransitions(Guid featureId)
    {
        var feature = dbContext.Features.Find(featureId);
        if (feature == null)
            return new List<FeatureStatus>();

        var service = new FeatureStatusTransitionService();
        var workItem = new DevStack.Domain.Entities.Feature
        {
            Id = feature.Id,
            Status = feature.Status,
            Result = feature.Result,
            Errors = feature.Errors,
            OpenQuestions = feature.OpenQuestions
        };

        var validTargets = new List<FeatureStatus>();
        foreach (var targetStatus in Enum.GetValues<FeatureStatus>())
        {
            var result = service.Transition(workItem, targetStatus, "query-validation");
            if (result.IsSuccess)
            {
                validTargets.Add(targetStatus);
            }
        }

        return validTargets;
    }

    [McpServerTool, Description("Get dashboard summary statistics")]
    public object GetDashboardSummary()
    {
        return new
        {
            ProjectsInFlight = dbContext.Projects.Count(),
            FeaturesInReview = dbContext.Features.Count(f => f.Status == FeatureStatus.InReview),
            FeaturesFailed = dbContext.Features.Count(f => f.Status == FeatureStatus.Failed),
            TasksInProgress = dbContext.Tasks.Count(t => t.Status == DevStack.Domain.Enums.TaskStatus.Code),
            TasksFailed = dbContext.Tasks.Count(t => t.Status == DevStack.Domain.Enums.TaskStatus.Failed)
        };
    }
}
