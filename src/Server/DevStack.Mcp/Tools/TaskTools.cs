using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Domain.Services;
using DevStack.Mcp.Dto;

using ModelContextProtocol;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class TaskTools
{
    private readonly ILogger<TaskTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateAgentTaskCommand> _createAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskCommand> _updateAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskStatusCommand> _updateAgentTaskStatusHandler;
    private readonly ICommandHandler<AgentTask, GetAgentTaskByIdQuery> _getAgentTaskByIdHandler;

    public TaskTools(
        ILogger<TaskTools> logger,
        DevStackDbContext dbContext,
        ICommandHandler<Guid, CreateAgentTaskCommand> createAgentTaskHandler,
        ICommandHandler<UpdateAgentTaskCommand> updateAgentTaskHandler,
        ICommandHandler<UpdateAgentTaskStatusCommand> updateAgentTaskStatusHandler,
        ICommandHandler<AgentTask, GetAgentTaskByIdQuery> getAgentTaskByIdHandler)
    {
        _logger = logger;
        _dbContext = dbContext;
        _createAgentTaskHandler = createAgentTaskHandler;
        _updateAgentTaskHandler = updateAgentTaskHandler;
        _updateAgentTaskStatusHandler = updateAgentTaskStatusHandler;
        _getAgentTaskByIdHandler = getAgentTaskByIdHandler;
    }

    [McpServerTool(Name = "get_task"), Description(Descriptions.TaskTools.GetTask)]
    public async Task<string> GetTask([Description(Descriptions.TaskTools.Id)] Guid id, CancellationToken ct = default)
    {
        var agentTask = await _getAgentTaskByIdHandler.Handle(new GetAgentTaskByIdQuery(id), ct);
        if (agentTask == null)
            throw new McpProtocolException($"AgentTask with ID {id} not found", McpErrorCode.InvalidParams);

        var data = new GetAgentTaskResponse(
            agentTask.Id.ToString(),
            agentTask.ProjectId.ToString(),
            agentTask.Title,
            agentTask.Status.ToString(),
            agentTask.Description,
            agentTask.Result,
            agentTask.Errors,
            agentTask.CommitHash,
            agentTask.Agent);

        return ToolResponse.Success("Agent Task", data);
    }

    [McpServerTool(Name = "get_next_task"), Description(Descriptions.TaskTools.GetNextTask)]
    public async Task<string> GetNextTask(
        [Description(Descriptions.TaskTools.RepositoryUrl)][DefaultValue(null)] string? repositoryUrl,
        [Description(Descriptions.TaskTools.ProjectId)][DefaultValue(null)] Guid? projectId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl) && (projectId == null || projectId == Guid.Empty))
            throw new McpProtocolException("Either repositoryUrl or projectId must be provided", McpErrorCode.InvalidParams);

        Project? project;
        if (projectId is not null && projectId != Guid.Empty)
        {
            project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId.Value, ct);
        }
        else
        {
            project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Repository == repositoryUrl, ct);
        }

        if (project == null)
            return ToolResponse.Error("Project not found");

        var deliverables = await _dbContext.Deliverables
            .Where(d => d.ProjectId == project.Id && d.Status == DeliverableStatus.Implement)
            .ToListAsync(ct);

        if (deliverables.Count == 0)
            return ToolResponse.Error("No deliverables found in Implement status for this project");

        var deliverableIds = deliverables.Select(d => d.Id).ToList();

        var tasks = await _dbContext.AgentTasks
            .Where(t => deliverableIds.Contains(t.DeliverableId))
            .ToListAsync(ct);

        var selectionResult = TaskSelectionService.SelectNextTask(deliverables, tasks);
        if (selectionResult == null)
            return ToolResponse.Error("No deliverables found in Implement status for this project");

        var (bestDeliverable, nextTask) = selectionResult.Value;

        if (nextTask == null)
            return ToolResponse.Success("No Pending Tasks",
                new { Message = "All tasks are completed for the selected deliverable", bestDeliverable.Id, bestDeliverable.Title });

        var data = new GetAgentTaskResponse(
            nextTask.Id.ToString(),
            nextTask.ProjectId.ToString(),
            nextTask.Title,
            nextTask.Status.ToString(),
            nextTask.Description,
            nextTask.Result,
            nextTask.Errors,
            nextTask.CommitHash,
            nextTask.Agent);

        return ToolResponse.Success("Next Task", data);
    }

    [McpServerTool(Name = "create_task"), Description(Descriptions.TaskTools.CreateTask)]
    public async Task<string> CreateAgentTask(
        [Description(Descriptions.TaskTools.DeliverableId)] Guid deliverableId,
        [Description(Descriptions.TaskTools.Title)] string title,
        [Description(Descriptions.TaskTools.Description)][DefaultValue(null)] string? description,
        CancellationToken ct = default)
    {
        var deliverable = await _dbContext.Deliverables.FirstOrDefaultAsync(d => d.Id == deliverableId, ct);
        if (deliverable == null)
        {
            throw new McpProtocolException($"Deliverable with ID {deliverableId} not found", McpErrorCode.InvalidParams);
        }

        var id = await _createAgentTaskHandler.Handle(
            new CreateAgentTaskCommand(
                deliverable.ProjectId,
                deliverableId,
                title,
                description ?? string.Empty,
                5,
                null),
            ct);

        _logger.LogInformation("Created agent task with ID: {Id}", id);
        return ToolResponse.Success("Task Created",
            new CreateAgentTaskResponse(id.ToString(), "Ready"),
            Descriptions.TaskTools.CreateUsageHint);
    }

    [McpServerTool(Name = "update_task"), Description(Descriptions.TaskTools.UpdateTask)]
    public async Task<string> UpdateAgentTask(
        [Description(Descriptions.TaskTools.Id)] Guid id,
        [Description(Descriptions.TaskTools.Status)][DefaultValue(null)] AgentTaskStatus? status,
        [Description(Descriptions.TaskTools.Description)][DefaultValue(null)] string? description,
        [Description(Descriptions.TaskTools.Result)][DefaultValue(null)] string? result,
        [Description(Descriptions.TaskTools.Errors)][DefaultValue(null)] string? errors,
        [Description(Descriptions.TaskTools.CommitHash)][DefaultValue(null)] string? commitHash,
        [Description(Descriptions.TaskTools.Agent)][DefaultValue(null)] string? agent,
        CancellationToken ct = default)
    {
        await _updateAgentTaskHandler.Handle(
            new UpdateAgentTaskCommand(
                id,
                null,
                description,
                result,
                errors,
                commitHash,
                null,
                null,
                null,
                null,
                null,
                agent),
            ct);

        _logger.LogInformation("Updated agent task with ID: {Id}", id);
        return ToolResponse.Success("Task Updated",
            new UpdateAgentTaskResponse(id.ToString(), true),
            Descriptions.TaskTools.UpdateUsageHint);
    }

    [McpServerTool(Name = "update_task_status"), Description(Descriptions.TaskTools.UpdateTaskStatus)]
    public async Task<string> TransitionAgentTaskStatus(
        [Description(Descriptions.TaskTools.Id)] Guid id,
        [Description(Descriptions.TaskTools.TargetStatus)] AgentTaskStatus targetStatus,
        [Description(Descriptions.TaskTools.Actor)] string actor,
        CancellationToken ct = default)
    {
        await _updateAgentTaskStatusHandler.Handle(
            new UpdateAgentTaskStatusCommand(id, targetStatus, actor),
            ct);

        _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
        return ToolResponse.Success("Task State Transitioned",
            new TransitionAgentTaskStatusResponse(id.ToString(), targetStatus.ToString(), actor));
    }
}
