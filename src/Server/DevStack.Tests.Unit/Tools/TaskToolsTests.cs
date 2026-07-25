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

using FluentAssertions;

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
        var agentTask = new AgentTask(
            projectId: Guid.NewGuid(),
            deliverableId: Guid.NewGuid(),
            title: "Test Task",
            description: "Test description",
            complexityRating: 5,
            status: AgentTaskStatus.Ready,
            id: id);

        _getAgentTaskByIdHandler.Handle(Arg.Any<GetAgentTaskByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AgentTask>(agentTask));

        // Act
        var result = await _tools.GetTask(id);

        // Assert
        result.Should().Contain("Test Task");
        result.Should().Contain("Test description");
        result.Should().Contain("```json");
        result.Should().Contain("## Agent Task");
        result.Should().Contain("Ready");

        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        json.Should().NotBeNull();
        json!.TryGetValue("Id", out var idProp).Should().BeTrue();
        idProp!.ToString()!.ToLowerInvariant().Should().Be(id.ToString().ToLowerInvariant());
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

        exception.Message.Should().BeEquivalentTo($"AgentTask with ID {id} not found");
    }

    [Fact]
    public async Task GetTask_WithNullResult_ThrowsMcpProtocolException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _getAgentTaskByIdHandler.Handle(Arg.Any<GetAgentTaskByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AgentTask>(null!));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.GetTask(id));

        exception.Message.Should().BeEquivalentTo($"AgentTask with ID {id} not found");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateAgentTask_WithValidData_CreatesTask()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var deliverableId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var title = "New Task";

        _dbContext.Deliverables.Add(new Deliverable(projectId, DeliverableType.Feature, "Test", id: deliverableId));
        await _dbContext.SaveChangesAsync();

        _createAgentTaskHandler.Handle(Arg.Any<CreateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateAgentTask(
            deliverableId,
            title,
            null);

        // Assert
        result.Should().Contain("Task Created");
        result.Should().Contain(newId.ToString());
        result.Should().Contain("Ready");

        await _createAgentTaskHandler.Received(1).Handle(
            Arg.Is<CreateAgentTaskCommand>(cmd =>
                cmd.ProjectId == projectId &&
                cmd.DeliverableId == deliverableId &&
                cmd.Title == title &&
                cmd.ComplexityRating == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAgentTask_WithNonExistentDeliverable_ThrowsMcpProtocolException()
    {
        // Arrange
        var deliverableId = Guid.NewGuid();
        var title = "New Task";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateAgentTask(deliverableId, title, null));

        exception.Message.Should().Contain("Deliverable");
        exception.Message.Should().Contain("not found");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
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

        _dbContext.Deliverables.Add(new Deliverable(projectId, DeliverableType.Feature, "Test", id: deliverableId));
        await _dbContext.SaveChangesAsync();

        _createAgentTaskHandler.Handle(Arg.Any<CreateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateAgentTask(
            deliverableId,
            title,
            description);

        // Assert
        result.Should().Contain("Task Created");
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
        result.Should().Contain("Task Updated");
        result.Should().Contain(id.ToString());
        result.Should().Contain("true");

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
        result.Should().Contain("Task Updated");
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
        result.Should().Contain("Task State Transitioned");
        result.Should().Contain(id.ToString());
        result.Should().Contain("InProgress");
        result.Should().Contain("test-user");

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
            result.Should().Contain(status.ToString());
        }
    }

    [Fact]
    public async Task UpdateAgentTask_WithAllNullFields_UpdatesOnlyId()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateAgentTaskHandler.Handle(Arg.Any<UpdateAgentTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateAgentTask(
            id,
            null, null, null, null, null, null);

        // Assert
        result.Should().Contain("Task Updated");
        result.Should().Contain(id.ToString());
        await _updateAgentTaskHandler.Received(1).Handle(
            Arg.Is<UpdateAgentTaskCommand>(cmd => cmd.Id == id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionAgentTaskStatus_WithHandlerException_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = AgentTaskStatus.Done;
        var actor = "test-user";
        var errorMessage = "Invalid status transition";

        _updateAgentTaskStatusHandler.Handle(Arg.Any<UpdateAgentTaskStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UpdateAgentTaskStatusCommand>(new Exception(errorMessage)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _tools.TransitionAgentTaskStatus(id, targetStatus, actor));

        exception.Message.Should().Be(errorMessage);
    }
}
