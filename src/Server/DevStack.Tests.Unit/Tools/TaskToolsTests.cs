using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DevStack.Application;
using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Mcp.Tools;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using NSubstitute;
using Xunit;

namespace DevStack.Tests.Unit.Tools;

public class TaskToolsTests
{
    private readonly ILogger<TaskTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateAgentTaskCommand> _createAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskCommand> _updateAgentTaskHandler;
    private readonly ICommandHandler<UpdateAgentTaskStatusCommand> _updateAgentTaskStatusHandler;
    private readonly ICommandHandler<AgentTask, GetAgentTaskByIdQuery> _getAgentTaskByIdHandler;
    private readonly TaskTools _tools;

    public TaskToolsTests()
    {
        _logger = Substitute.For<ILogger<TaskTools>>();
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _dbContext = new DevStackDbContext(options);
        _createAgentTaskHandler = Substitute.For<ICommandHandler<Guid, CreateAgentTaskCommand>>();
        _updateAgentTaskHandler = Substitute.For<ICommandHandler<UpdateAgentTaskCommand>>();
        _updateAgentTaskStatusHandler = Substitute.For<ICommandHandler<UpdateAgentTaskStatusCommand>>();
        _getAgentTaskByIdHandler = Substitute.For<ICommandHandler<AgentTask, GetAgentTaskByIdQuery>>();
        _tools = new TaskTools(
            _logger,
            _dbContext,
            _createAgentTaskHandler,
            _updateAgentTaskHandler,
            _updateAgentTaskStatusHandler,
            _getAgentTaskByIdHandler);
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTaskData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agentTask = new AgentTask
        {
            Id = id,
            ProjectId = Guid.NewGuid(),
            DeliverableId = Guid.NewGuid(),
            Title = "Test Task",
            Status = AgentTaskStatus.Ready,
            Description = "Test description",
            ComplexityRating = 5
        };

        _getAgentTaskByIdHandler.Handle(Arg.Any<GetAgentTaskByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AgentTask>(agentTask));

        // Act
        var result = await _tools.GetTask(id);

        // Assert
        Assert.Contains("Test Task", result);
        Assert.Contains("Test description", result);
        Assert.Contains("```json", result);
        Assert.Contains("## Agent Task", result);
        Assert.Contains("Ready", result);

        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(json);
        var idValue = json!["id"]?.ToString() ?? string.Empty;
        Assert.Equal(id.ToString().ToLowerInvariant(), idValue.ToLowerInvariant());
    }

    [Fact]
    public async Task GetTask_WithNotFoundId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _getAgentTaskByIdHandler.Handle(Arg.Any<GetAgentTaskByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<AgentTask>(new KeyNotFoundException($"AgentTask with ID {id} not found")));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _tools.GetTask(id));

        Assert.Equal($"AgentTask with ID {id} not found", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithValidData_CreatesTask()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var title = "New Task";

        _dbContext.Projects.Add(new Project { Id = projectId, Name = "Test" });
        _dbContext.Deliverables.Add(new Deliverable { Id = deliverableId, ProjectId = projectId, Title = "Test" });
        await _dbContext.SaveChangesAsync();

        _createAgentTaskHandler.Handle(Arg.Any<CreateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateAgentTask(
            projectId,
            deliverableId,
            title,
            null);

        // Assert
        Assert.Contains("Task Created", result);
        Assert.Contains(newId.ToString(), result);
        Assert.Contains("Ready", result);

        await _createAgentTaskHandler.Received(1).Handle(
            Arg.Is<CreateAgentTaskCommand>(cmd =>
                cmd.ProjectId == projectId &&
                cmd.DeliverableId == deliverableId &&
                cmd.Title == title &&
                cmd.ComplexityRating == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAgentTask_WithNullProjectId_ThrowsMcpProtocolException()
    {
        // Arrange
        var deliverableId = Guid.NewGuid();
        var title = "New Task";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(null, deliverableId, title, null));

        Assert.Equal("Project ID is required", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithEmptyProjectId_ThrowsMcpProtocolException()
    {
        // Arrange
        var deliverableId = Guid.NewGuid();
        var title = "New Task";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(Guid.Empty, deliverableId, title, null));

        Assert.Equal("Project ID is required", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithNullDeliverableId_ThrowsMcpProtocolException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var title = "New Task";

        _dbContext.Projects.Add(new Project { Id = projectId, Name = "Test" });
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(projectId, null, title, null));

        Assert.Equal("Deliverable ID is required", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithEmptyDeliverableId_ThrowsMcpProtocolException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var title = "New Task";

        _dbContext.Projects.Add(new Project { Id = projectId, Name = "Test" });
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(projectId, Guid.Empty, title, null));

        Assert.Equal("Deliverable ID is required", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithNonExistentProject_ThrowsMcpProtocolException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var title = "New Task";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(projectId, deliverableId, title, null));

        Assert.Contains("Project", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithNonExistentDeliverable_ThrowsMcpProtocolException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var title = "New Task";

        _dbContext.Projects.Add(new Project { Id = projectId, Name = "Test" });
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(projectId, deliverableId, title, null));

        Assert.Contains("Deliverable", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CreateAgentTask_WithDescription_UsesProvidedDescription()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var title = "New Task";
        var description = "Task description";

        _dbContext.Projects.Add(new Project { Id = projectId, Name = "Test" });
        _dbContext.Deliverables.Add(new Deliverable { Id = deliverableId, ProjectId = projectId, Title = "Test" });
        await _dbContext.SaveChangesAsync();

        _createAgentTaskHandler.Handle(Arg.Any<CreateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateAgentTask(
            projectId,
            deliverableId,
            title,
            description);

        // Assert
        Assert.Contains("Task Created", result);
        await _createAgentTaskHandler.Received(1).Handle(
            Arg.Is<CreateAgentTaskCommand>(cmd => cmd.Description == description),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAgentTask_WithValidData_UpdatesTask()
    {
        // Arrange
        var id = Guid.NewGuid();
        var newDescription = "Updated description";

        _updateAgentTaskHandler.Handle(Arg.Any<UpdateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateAgentTask(
            id,
            null,
            newDescription,
            null, null, null, null);

        // Assert
        Assert.Contains("Task Updated", result);
        Assert.Contains(id.ToString(), result);
        Assert.Contains("true", result);

        await _updateAgentTaskHandler.Received(1).Handle(
            Arg.Is<UpdateAgentTaskCommand>(cmd => cmd.Id == id && cmd.Description == newDescription),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAgentTask_WithAllFields_UpdatesAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateAgentTaskHandler.Handle(Arg.Any<UpdateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateAgentTask(
            id,
            AgentTaskStatus.Done,
            "desc",
            "result",
            "errors",
            "commit123",
            "agent-name");

        // Assert
        Assert.Contains("Task Updated", result);
        await _updateAgentTaskHandler.Received(1).Handle(
            Arg.Is<UpdateAgentTaskCommand>(cmd =>
                cmd.Id == id &&
                cmd.Description == "desc" &&
                cmd.Result == "result" &&
                cmd.Errors == "errors" &&
                cmd.CommitHash == "commit123" &&
                cmd.Agent == "agent-name"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionAgentTaskStatus_WithValidData_TransitionsStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = AgentTaskStatus.InProgress;
        var actor = "test-user";

        _updateAgentTaskStatusHandler.Handle(Arg.Any<UpdateAgentTaskStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.TransitionAgentTaskStatus(id, targetStatus, actor);

        // Assert
        Assert.Contains("Task State Transitioned", result);
        Assert.Contains(id.ToString(), result);
        Assert.Contains("InProgress", result);
        Assert.Contains("test-user", result);

        await _updateAgentTaskStatusHandler.Received(1).Handle(
            Arg.Is<UpdateAgentTaskStatusCommand>(cmd =>
                cmd.Id == id &&
                cmd.Status == targetStatus &&
                cmd.Actor == actor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionAgentTaskStatus_WithAllStatuses_ReturnsCorrectResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var actor = "test-user";

        _updateAgentTaskStatusHandler.Handle(Arg.Any<UpdateAgentTaskStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act & Assert - test each status value
        foreach (var status in Enum.GetValues<AgentTaskStatus>())
        {
            var result = await _tools.TransitionAgentTaskStatus(id, status, actor);
            Assert.Contains(status.ToString(), result);
        }
    }

    [Fact]
    public async Task TransitionAgentTaskStatus_WithHandlerException_ReturnsErrorMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = AgentTaskStatus.Done;
        var actor = "test-user";
        var errorMessage = "Invalid status transition";

        _updateAgentTaskStatusHandler.Handle(Arg.Any<UpdateAgentTaskStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UpdateAgentTaskStatusCommand>(new Exception(errorMessage)));

        // Act
        var result = await _tools.TransitionAgentTaskStatus(id, targetStatus, actor);

        // Assert
        Assert.Contains(errorMessage, result);
    }
}
