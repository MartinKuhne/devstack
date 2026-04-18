using DevStack.Domain.Enums;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.Services;
using DevStack.Infrastructure.Persistence;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Epics;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.Mcp;

[McpServerToolType]
public class DevStackTools(
    DevStackDbContext dbContext,
    ICreateProjectHandler createProjectHandler,
    IUpdateProjectHandler updateProjectHandler,
    ICreateFeatureHandler createFeatureHandler,
    IUpdateFeatureHandler updateFeatureHandler,
    ITransitionFeatureStatusHandler transitionFeatureStatusHandler,
    ICreateTaskHandler createTaskHandler,
    IUpdateTaskHandler updateTaskHandler,
    ITransitionTaskStatusHandler transitionTaskHandler,
    ICreateEpicHandler createEpicHandler,
    IUpdateEpicHandler updateEpicHandler)
{
    [McpServerTool, Description("Create a new project")]
    public async Task<string> CreateProject(
        string name,
        string? description = null,
        string? architecture = null,
        string? memory = null,
        string? githubUrl = null)
    {
        var id = await createProjectHandler.Handle(
            new CreateProjectCommand(name, description, architecture, memory, githubUrl),
            CancellationToken.None);
        
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
        string? githubToken_Encrypted = null)
    {
        await updateProjectHandler.Handle(
            new UpdateProjectCommand(id, name, description, architecture, memory, githubUrl, githubToken_Encrypted),
            CancellationToken.None);
        
        return $"Project {id} updated successfully";
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
        Guid? dependsOnId = null)
    {
        var id = await createFeatureHandler.Handle(
            new CreateFeatureCommand(projectId, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions, FeatureStatus.Ready, dependsOnId),
            CancellationToken.None);
        
        return $"Item created with ID: {id}";
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
        Guid? dependsOnId = null)
    {
        await updateFeatureHandler.Handle(
            new UpdateFeatureCommand(id, title, description, acceptanceCriteria, plan, securityImpact, performanceImpact, testPlan, deploymentPlan, openQuestions, dependsOnId),
            CancellationToken.None);
        
        return $"Item {id} updated successfully";
    }

    [McpServerTool, Description("Transition a feature to a new status")]
    public async Task<string> TransitionFeatureStatus(
        Guid id,
        FeatureStatus targetStatus,
        string actor)
    {
        await transitionFeatureStatusHandler.Handle(
            new TransitionFeatureStatusCommand(id, targetStatus, actor),
            CancellationToken.None);
        
        return $"Item {id} transitioned to {targetStatus}";
    }

   [McpServerTool, Description("Create a new task")]
    public async Task<string> CreateTask(
        Guid projectId,
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
            new CreateTaskCommand(projectId, featureId, title, deliverable, acceptanceCriteria, risks, result, requiredFollowUps, complexityRating, FeatureStatus.Ready),
            CancellationToken.None);
        
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
            CancellationToken.None);
        
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
            CancellationToken.None);
        
        return $"Task {id} transitioned to {targetStatus}";
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
    public List<DevStack.Domain.Entities.Item> GetFeatures(
        Guid? projectId = null,
        List<FeatureStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Items.AsQueryable();
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
    public DevStack.Domain.Entities.Item? GetFeatureById(Guid id)
    {
        return dbContext.Items.Find(id);
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
            query = query.Where(t => t.ItemId == featureId.Value);
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
        var feature = dbContext.Items.Find(featureId);
        if (feature == null)
            return new List<FeatureStatus>();

        var service = new ItemStatusTransitionService();
        var workItem = new DevStack.Domain.Entities.Item
        {
            Id = feature.Id,
            Subtype = feature.Subtype,
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

    [McpServerTool, Description("Get an epic by ID")]
    public DevStack.Domain.Entities.Item? GetEpicById(Guid id)
    {
        return dbContext.Items.FirstOrDefault(i => i.Id == id && i.Subtype == ItemSubtype.Epic);
    }

    [McpServerTool, Description("Get all epics with optional filtering")]
    public List<DevStack.Domain.Entities.Item> GetEpics(
        string? title = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Items.AsQueryable().Where(e => e.Subtype == ItemSubtype.Epic);
        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e => e.Title.Contains(title));
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        return query.OrderBy(e => e.CreatedAt).Take(first).ToList();
    }

    [McpServerTool, Description("Create a new epic")]
    public async Task<string> CreateEpic(
        Guid projectId,
        string title,
        string? description = null,
        Guid? dependsOnId = null,
        CancellationToken cancellationToken = default)
    {
        var id = await createEpicHandler.Handle(
            new CreateEpicCommand(projectId, title, description, FeatureStatus.Ready),
            CancellationToken.None);
        
        return $"Epic created with ID: {id}";
    }

    [McpServerTool, Description("Update an existing epic")]
    public async Task<string> UpdateEpic(
        Guid id,
        string? title = null,
        string? description = null,
        Guid? dependsOnId = null,
        CancellationToken cancellationToken = default)
    {
        await updateEpicHandler.Handle(
            new UpdateEpicCommand(id, title, description),
            CancellationToken.None);
        
        return $"Epic {id} updated successfully";
    }
}
