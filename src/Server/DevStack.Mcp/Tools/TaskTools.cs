using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;

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

    [McpServerTool(Name = "get_task"), Description("Read an agent task by its ID. Returns all fields including title, status, description, result, and errors. Usage hint: Provide a valid task ID obtained from create_task or other operations.")]
    public async Task<string> GetTask([Description("The agent task ID")] Guid id, CancellationToken ct = default)
    {
        var agentTask = await _getAgentTaskByIdHandler.Handle(new GetAgentTaskByIdQuery(id), ct);
        if (agentTask == null)
            throw new KeyNotFoundException($"AgentTask with ID {id} not found");

        var data = new { id = agentTask.Id.ToString(), projectId = agentTask.ProjectId.ToString(), title = agentTask.Title, status = agentTask.Status.ToString(), description = agentTask.Description, result = agentTask.Result, errors = agentTask.Errors, commitHash = agentTask.CommitHash, agent = agentTask.Agent };
        return $"## Agent Task\n\n```json\n{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}\n```\n\n";
    }

    [McpServerTool(Name = "create_task"), Description("Create a new agent task in DevStack. New tasks are created in Ready state. Usage hint: Both ProjectId and DeliverableId must reference existing entities.")]
    public async Task<string> CreateAgentTask(
        [Description("The project ID")][DefaultValue(null)] Guid? projectId,
        [Description("The deliverable/feature ID")][DefaultValue(null)] Guid? deliverableId,
        [Description("The task title")] string title,
        [Description("The task description")][DefaultValue(null)] string? description,
        CancellationToken ct = default)
    {
        try
        {
            if (projectId == null || projectId == Guid.Empty)
            {
                throw new McpProtocolException("Project ID is required", McpErrorCode.InvalidParams);
            }

            var projectExists = await _dbContext.Projects.AnyAsync(p => p.Id == projectId, ct);
            if (!projectExists)
            {
                throw new McpProtocolException($"Project with ID {projectId} not found", McpErrorCode.InvalidParams);
            }

            if (deliverableId == null || deliverableId == Guid.Empty)
            {
                throw new McpProtocolException("Deliverable ID is required", McpErrorCode.InvalidParams);
            }

            var deliverableExists = await _dbContext.Deliverables.AnyAsync(d => d.Id == deliverableId, ct);
            if (!deliverableExists)
            {
                throw new McpProtocolException($"Deliverable with ID {deliverableId} not found", McpErrorCode.InvalidParams);
            }

            var id = await _createAgentTaskHandler.Handle(
                new CreateAgentTaskCommand(
                    projectId.Value,
                    deliverableId.Value,
                    title,
                    description ?? string.Empty,
                    5,
                    null),
                ct);

            _logger.LogInformation("Created agent task with ID: {Id}", id);
            var result = new { id = id.ToString(), status = "Ready" };
            return $"## Task Created\n\n```json\n{JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}\n```\n\nUsage hint: Use the returned ID for subsequent get_task, update_task, or update_task_status calls.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent task");
            throw;
        }
    }

    [McpServerTool(Name = "update_task"), Description("Modify an existing agent task in DevStack. Only non-null fields are updated. Usage hint: Provide the task ID and only the fields you want to change.")]
    public async Task<string> UpdateAgentTask(
        [Description("The agent task ID")] Guid id,
        [Description("The updated status")][DefaultValue(null)] AgentTaskStatus? status,
        [Description("The updated description")][DefaultValue(null)] string? description,
        [Description("The result")][DefaultValue(null)] string? result,
        [Description("The errors")][DefaultValue(null)] string? errors,
        [Description("The commit hash")][DefaultValue(null)] string? commitHash,
        [Description("The agent")][DefaultValue(null)] string? agent,
        CancellationToken ct = default)
    {
        try
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
            var response = new { id = id.ToString(), updated = true };
            return $"## Task Updated\n\n```json\n{JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}\n```\n\nUsage hint: Use get_task to verify the changes.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent task: {Id}", id);
            throw;
        }
    }

    [McpServerTool(Name = "update_task_status"), Description("Change the state of an agent task in DevStack. Valid transitions are enforced by the state machine. Usage hint: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.")]
    public async Task<string> TransitionAgentTaskStatus(
        [Description("The agent task ID")] Guid id,
        [Description("The target status")] AgentTaskStatus targetStatus,
        [Description("The actor performing the transition")] string actor,
        CancellationToken ct = default)
    {
        try
        {
            await _updateAgentTaskStatusHandler.Handle(
                new UpdateAgentTaskStatusCommand(id, targetStatus, actor),
                ct);

            _logger.LogInformation("Transitioned agent task {Id} to {Status} by {Actor}", id, targetStatus, actor);
            var response = new { id = id.ToString(), status = targetStatus.ToString(), actor };
            return $"## Task State Transitioned\n\n```json\n{JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}\n```\n\n";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning agent task status: {Id}", id);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
