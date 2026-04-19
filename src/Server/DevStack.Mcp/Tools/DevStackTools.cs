using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class DevStackTools
{
    private readonly ILogger<DevStackTools> _logger;
    private readonly DevStackDbContext _dbContext;

    public DevStackTools(ILogger<DevStackTools> logger, DevStackDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    #region Project Tools

    [McpServerTool, Description("Read all projects from DevStack. Returns project name, id, and repository.")]
    public async Task<string> GetProjects()
    {
        var projects = await _dbContext.Projects.ToListAsync();
        return JsonSerializer.Serialize(projects);
    }

    [McpServerTool, Description("Read a project by its ID.")]
    public async Task<string> GetProjectById([Description("The project ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Project ID is required" });
        }

        var project = await _dbContext.Projects.FindAsync([id.Value]);
        if (project == null)
        {
            return JsonSerializer.Serialize(new { error = "Project not found" });
        }

        return JsonSerializer.Serialize(project);
    }

    [McpServerTool, Description("Create a new project in DevStack.")]
    public async Task<string> CreateProject(
        [Description("The project name")] string name,
        [Description("The project description")][DefaultValue(null)] string? description,
        [Description("The repository URL")][DefaultValue(null)] string? repository)
    {
        try
        {
            var project = new Project
            {
                Name = name,
                Description = description,
                Repository = repository ?? string.Empty
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created project with ID: {Id}", project.Id);
            return JsonSerializer.Serialize(new { id = project.Id.ToString(), name = project.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            throw;
        }
    }

    [McpServerTool, Description("Update an existing project in DevStack.")]
    public async Task<string> UpdateProject(
        [Description("The project ID")] Guid id,
        [Description("The updated name")][DefaultValue(null)] string? name,
        [Description("The updated description")][DefaultValue(null)] string? description,
        [Description("The updated repository")][DefaultValue(null)] string? repository)
    {
        try
        {
            var project = await _dbContext.Projects.FindAsync([id]);
            if (project == null)
                return JsonSerializer.Serialize(new { error = "Project not found" });

            if (name is not null) project.Name = name;
            if (description is not null) project.Description = description;
            if (repository is not null) project.Repository = repository;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated project with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true, name = project.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Delete a project from DevStack.")]
    public async Task<string> DeleteProject([Description("The project ID")] Guid id)
    {
        try
        {
            var project = await _dbContext.Projects.FindAsync([id]);
            if (project == null)
                return JsonSerializer.Serialize(new { error = "Project not found" });

            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted project with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), deleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Deliverable Tools

    [McpServerTool, Description("Create a new deliverable (Feature) in DevStack. New deliverables are created in Planning state.")]
    public async Task<string> CreateDeliverable(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? description)
    {
        try
        {
            var deliverable = new Deliverable
            {
                ProjectId = projectId ?? Guid.Empty,
                Title = title,
                Description = description,
                Type = DeliverableType.Feature,
                Status = DeliverableStatus.Planning
            };

            _dbContext.Deliverables.Add(deliverable);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created deliverable with ID: {Id}", deliverable.Id);
            return JsonSerializer.Serialize(new { id = deliverable.Id.ToString(), type = "Feature", status = "Planning" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deliverable");
            throw;
        }
    }

    [McpServerTool, Description("Read a deliverable by its ID.")]
    public async Task<string> GetDeliverableById([Description("The deliverable ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Deliverable ID is required" });
        }

        var deliverable = await _dbContext.Deliverables.FindAsync([id.Value]);
        if (deliverable == null)
        {
            return JsonSerializer.Serialize(new { error = "Deliverable not found" });
        }

        return JsonSerializer.Serialize(deliverable);
    }

    [McpServerTool, Description("Read all deliverables, optionally filtered by project ID.")]
    public async Task<string> GetDeliverables([Description("The project ID filter")][DefaultValue(null)] Guid? projectId)
    {
        IQueryable<Deliverable> query = _dbContext.Deliverables;
        if (projectId is not null)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }

        var deliverables = await query.ToListAsync();
        return JsonSerializer.Serialize(deliverables);
    }

    [McpServerTool, Description("Update an existing deliverable in DevStack.")]
    public async Task<string> UpdateDeliverable(
        [Description("The deliverable ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The updated description")][DefaultValue(null)] string? description)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            if (title is not null) deliverable.Title = title;
            if (description is not null) deliverable.Description = description;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated deliverable with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deliverable: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Transition a deliverable status in DevStack.")]
    public async Task<string> TransitionDeliverableStatus(
        [Description("The deliverable ID")] Guid id,
        [Description("The target status")] DeliverableStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transitioned deliverable {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(new { id = id.ToString(), status = targetStatus.ToString(), actor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning deliverable status: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Get valid status transitions for a deliverable.")]
    public async Task<string> GetDeliverableStatusTransitions([Description("The deliverable ID")] Guid id)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var service = new DeliverableStatusTransitionService();
            var transitions = service.GetValidTransitions(deliverable);

            return JsonSerializer.Serialize(new { id = id.ToString(), currentStatus = deliverable.Status.ToString(), validTransitions = transitions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid status transitions for deliverable: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Delete a deliverable from DevStack.")]
    public async Task<string> DeleteDeliverable([Description("The deliverable ID")] Guid id)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            _dbContext.Deliverables.Remove(deliverable);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted deliverable with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), deleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deliverable: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Defect Tools

    [McpServerTool, Description("Create a new defect (Deliverable of type Defect) in DevStack. New defects are created in Planning state.")]
    public async Task<string> CreateDefect(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The parent feature ID")][DefaultValue(null)] Guid? parentFeatureId,
        [Description("The defect title")] string title,
        [Description("The defect description")][DefaultValue(null)] string? description)
    {
        try
        {
            var defect = new Deliverable
            {
                ProjectId = projectId ?? Guid.Empty,
                Title = title,
                Description = description,
                Type = DeliverableType.Defect,
                Status = DeliverableStatus.Planning
            };

            if (parentFeatureId is not null)
            {
                defect.AcceptanceCriteria = $"Parent Feature: {parentFeatureId}";
            }

            _dbContext.Deliverables.Add(defect);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created defect with ID: {Id}", defect.Id);
            return JsonSerializer.Serialize(new { id = defect.Id.ToString(), type = "Defect", status = "Planning" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defect");
            throw;
        }
    }

    [McpServerTool, Description("Read a defect by its ID.")]
    public async Task<string> GetDefectById([Description("The defect ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Defect ID is required" });
        }

        var deliverable = await _dbContext.Deliverables.FindAsync([id.Value]);
        if (deliverable == null || deliverable.Type != DeliverableType.Defect)
        {
            return JsonSerializer.Serialize(new { error = "Defect not found" });
        }

        return JsonSerializer.Serialize(deliverable);
    }

    [McpServerTool, Description("Read all defects, optionally filtered by project ID.")]
    public async Task<string> GetDefects([Description("The project ID filter")][DefaultValue(null)] Guid? projectId)
    {
        IQueryable<Deliverable> query = _dbContext.Deliverables.Where(d => d.Type == DeliverableType.Defect);
        if (projectId is not null)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }

        var defects = await query.ToListAsync();
        return JsonSerializer.Serialize(defects);
    }

    [McpServerTool, Description("Update an existing defect in DevStack.")]
    public async Task<string> UpdateDefect(
        [Description("The defect ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The updated description")][DefaultValue(null)] string? description)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null || deliverable.Type != DeliverableType.Defect)
                return JsonSerializer.Serialize(new { error = "Defect not found" });

            if (title is not null) deliverable.Title = title;
            if (description is not null) deliverable.Description = description;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated defect with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defect: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Transition a defect status in DevStack.")]
    public async Task<string> TransitionDefectStatus(
        [Description("The defect ID")] Guid id,
        [Description("The target status")] DeliverableStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null || deliverable.Type != DeliverableType.Defect)
                return JsonSerializer.Serialize(new { error = "Defect not found" });

            var service = new DeliverableStatusTransitionService();
            var result = service.Transition(deliverable, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transitioned defect {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(new { id = id.ToString(), status = targetStatus.ToString(), actor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning defect status: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Delete a defect from DevStack.")]
    public async Task<string> DeleteDefect([Description("The defect ID")] Guid id)
    {
        try
        {
            var deliverable = await _dbContext.Deliverables.FindAsync([id]);
            if (deliverable == null || deliverable.Type != DeliverableType.Defect)
                return JsonSerializer.Serialize(new { error = "Defect not found" });

            _dbContext.Deliverables.Remove(deliverable);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted defect with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), deleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defect: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Agent Task Tools

    [McpServerTool, Description("Create a new agent task in DevStack. New tasks are created in Ready state.")]
    public async Task<string> CreateAgentTask(
        [Description("The project ID")] Guid projectId,
        [Description("The deliverable/feature ID")] Guid itemId,
        [Description("The task title")] string title,
        [Description("The deliverable description")][DefaultValue(null)] string? deliverableDescription,
        [Description("The complexity rating (1-10)")] int complexityRating = 5)
    {
        try
        {
            var deliverableEntity = await _dbContext.Deliverables.FindAsync([itemId]);
            if (deliverableEntity == null)
                return JsonSerializer.Serialize(new { error = "Deliverable not found" });

            var agentTask = new AgentTask
            {
                ProjectId = projectId,
                DeliverableId = itemId,
                Title = title,
                ComplexityRating = complexityRating,
                Status = AgentTaskStatus.Ready
            };

            _dbContext.AgentTasks.Add(agentTask);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created agent task with ID: {Id}", agentTask.Id);
            return JsonSerializer.Serialize(new { id = agentTask.Id.ToString(), status = "Ready" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent task");
            throw;
        }
    }

    [McpServerTool, Description("Read an agent task by its ID.")]
    public async Task<string> GetAgentTaskById([Description("The agent task ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "AgentTask ID is required" });
        }

        var agentTask = await _dbContext.AgentTasks.FindAsync([id.Value]);
        if (agentTask == null)
        {
            return JsonSerializer.Serialize(new { error = "AgentTask not found" });
        }

        return JsonSerializer.Serialize(agentTask);
    }

    [McpServerTool, Description("Read all agent tasks, optionally filtered by deliverable/feature ID.")]
    public async Task<string> GetAgentTasks([Description("The deliverable/feature ID filter")][DefaultValue(null)] Guid? featureId)
    {
        IQueryable<AgentTask> query = _dbContext.AgentTasks;
        if (featureId is not null)
        {
            query = query.Where(t => t.DeliverableId == featureId.Value);
        }

        var tasks = await query.ToListAsync();
        return JsonSerializer.Serialize(tasks);
    }

    [McpServerTool, Description("Update an existing agent task in DevStack.")]
    public async Task<string> UpdateAgentTask(
        [Description("The agent task ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The result")][DefaultValue(null)] string? result)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            if (title is not null) agentTask.Title = title;
            if (result is not null) agentTask.Result = result;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated agent task with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent task: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Transition an agent task status in DevStack.")]
    public async Task<string> TransitionAgentTaskStatus(
        [Description("The agent task ID")] Guid id,
        [Description("The target status")] AgentTaskStatus targetStatus,
        [Description("The actor performing the transition")] string actor)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            var service = new AgentTaskStatusTransitionService();
            var result = service.Transition(agentTask, targetStatus, actor);

            if (!result.IsSuccess)
                return JsonSerializer.Serialize(new { error = result.Errors[0] });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
            return JsonSerializer.Serialize(new { id = id.ToString(), status = targetStatus.ToString(), actor });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning agent task status: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Delete an agent task from DevStack.")]
    public async Task<string> DeleteAgentTask([Description("The agent task ID")] Guid id)
    {
        try
        {
            var agentTask = await _dbContext.AgentTasks.FindAsync([id]);
            if (agentTask == null)
                return JsonSerializer.Serialize(new { error = "AgentTask not found" });

            _dbContext.AgentTasks.Remove(agentTask);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted agent task with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), deleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent task: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Epic Tools

    [McpServerTool, Description("Create a new epic in DevStack.")]
    public async Task<string> CreateEpic(
        [Description("The project ID")] Guid projectId,
        [Description("The epic title")] string title,
        [Description("The epic description")][DefaultValue(null)] string? description)
    {
        try
        {
            var epic = new Epic
            {
                ProjectId = projectId,
                Title = title,
                Description = description
            };

            _dbContext.Epics.Add(epic);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created epic with ID: {Id}", epic.Id);
            return JsonSerializer.Serialize(new { id = epic.Id.ToString(), title = epic.Title });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating epic");
            throw;
        }
    }

    [McpServerTool, Description("Read an epic by its ID.")]
    public async Task<string> GetEpicById([Description("The epic ID")][DefaultValue(null)] Guid? id)
    {
        if (id == null)
        {
            return JsonSerializer.Serialize(new { error = "Epic ID is required" });
        }

        var epic = await _dbContext.Epics.FindAsync([id.Value]);
        if (epic == null)
        {
            return JsonSerializer.Serialize(new { error = "Epic not found" });
        }

        return JsonSerializer.Serialize(epic);
    }

    [McpServerTool, Description("Read all epics, optionally filtered by title.")]
    public async Task<string> GetEpics([Description("The title filter")][DefaultValue(null)] string? title)
    {
        IQueryable<Epic> query = _dbContext.Epics;
        if (title is not null)
        {
            query = query.Where(e => e.Title.Contains(title));
        }

        var epics = await query.ToListAsync();
        return JsonSerializer.Serialize(epics);
    }

    [McpServerTool, Description("Update an existing epic in DevStack.")]
    public async Task<string> UpdateEpic(
        [Description("The epic ID")] Guid id,
        [Description("The updated title")][DefaultValue(null)] string? title,
        [Description("The updated description")][DefaultValue(null)] string? description)
    {
        try
        {
            var epic = await _dbContext.Epics.FindAsync([id]);
            if (epic == null)
                return JsonSerializer.Serialize(new { error = "Epic not found" });

            if (title is not null) epic.Title = title;
            if (description is not null) epic.Description = description;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated epic with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), updated = true, title = epic.Title });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating epic: {Id}", id);
            throw;
        }
    }

    [McpServerTool, Description("Delete an epic from DevStack.")]
    public async Task<string> DeleteEpic([Description("The epic ID")] Guid id)
    {
        try
        {
            var epic = await _dbContext.Epics.FindAsync([id]);
            if (epic == null)
                return JsonSerializer.Serialize(new { error = "Epic not found" });

            _dbContext.Epics.Remove(epic);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted epic with ID: {Id}", id);
            return JsonSerializer.Serialize(new { id = id.ToString(), deleted = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting epic: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Dashboard Tool

    [McpServerTool, Description("Get a dashboard summary with counts of projects, features, and tasks by status.")]
    public async Task<string> GetDashboardSummary()
    {
        try
        {
            var totalProjects = await _dbContext.Projects.CountAsync();
            var totalFeatures = await _dbContext.Deliverables.CountAsync(d => d.Type == DeliverableType.Feature);
            var totalTasks = await _dbContext.AgentTasks.CountAsync();
            var totalDefects = await _dbContext.Deliverables.CountAsync(d => d.Type == DeliverableType.Defect);

            var featuresInReview = await _dbContext.Deliverables.CountAsync(
                d => d.Type == DeliverableType.Feature && d.Status == DeliverableStatus.NeedsReview);
            var featuresFailed = await _dbContext.Deliverables.CountAsync(
                d => d.Type == DeliverableType.Feature && d.Status == DeliverableStatus.Failed);
            var featuresInprogress = await _dbContext.Deliverables.CountAsync(
                d => d.Type == DeliverableType.Feature && d.Status == DeliverableStatus.InProgress);
            var tasksInProgress = await _dbContext.AgentTasks.CountAsync(
                t => t.Status == AgentTaskStatus.InProgress);
            var tasksFailed = await _dbContext.AgentTasks.CountAsync(
                t => t.Status == AgentTaskStatus.Failed);

            var summary = new
            {
                projectCount = totalProjects,
                featureCount = totalFeatures,
                taskCount = totalTasks,
                defectCount = totalDefects,
                featuresInReview,
                featuresFailed,
                featuresInProgress = featuresInprogress,
                tasksInProgress,
                tasksFailed
            };

            return JsonSerializer.Serialize(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            throw;
        }
    }

    #endregion
}
