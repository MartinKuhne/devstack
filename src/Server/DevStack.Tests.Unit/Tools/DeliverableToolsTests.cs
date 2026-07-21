using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DevStack.Application;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
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

public class DeliverableToolsTests
{
    private readonly ILogger<DeliverableTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateDeliverableCommand> _createDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableCommand> _updateDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableStatusCommand> _updateDeliverableStatusHandler;
    private readonly ICommandHandler<Deliverable?, GetDeliverableByIdQuery> _getDeliverableByIdHandler;
    private readonly DeliverableTools _tools;

    private static DbContextOptions<DevStackDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<DevStackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public DeliverableToolsTests()
    {
        _logger = Substitute.For<ILogger<DeliverableTools>>();
        _dbContext = new DevStackDbContext(CreateOptions());
        _createDeliverableHandler = Substitute.For<ICommandHandler<Guid, CreateDeliverableCommand>>();
        _updateDeliverableHandler = Substitute.For<ICommandHandler<UpdateDeliverableCommand>>();
        _updateDeliverableStatusHandler = Substitute.For<ICommandHandler<UpdateDeliverableStatusCommand>>();
        _getDeliverableByIdHandler = Substitute.For<ICommandHandler<Deliverable?, GetDeliverableByIdQuery>>();
        _tools = new DeliverableTools(
            _logger,
            _dbContext,
            _createDeliverableHandler,
            _updateDeliverableHandler,
            _updateDeliverableStatusHandler,
            _getDeliverableByIdHandler);
    }

    [Fact]
    public async Task GetDeliverable_WithValidId_ReturnsDeliverableData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var deliverable = new Deliverable
        {
            Id = id,
            ProjectId = Guid.NewGuid(),
            Title = "Test Deliverable",
            Description = "Test description",
            Status = DeliverableStatus.Draft,
            Type = DeliverableType.Feature
        };

        _getDeliverableByIdHandler.Handle(Arg.Any<GetDeliverableByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Deliverable?>(deliverable));

        // Act
        var result = await _tools.GetDeliverable(id);

        // Assert
        result.Should().Contain("Test Deliverable");
        result.Should().Contain("Test description");
        result.Should().Contain("```json");
        result.Should().Contain("## Deliverable");

        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        json.Should().NotBeNull();
        json!.TryGetValue("Id", out var idProp).Should().BeTrue();
        idProp!.ToString()!.ToLowerInvariant().Should().Be(id.ToString().ToLowerInvariant());
    }

    [Fact]
    public async Task GetDeliverable_WithNotFoundId_ReturnsErrorMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        _getDeliverableByIdHandler.Handle(Arg.Any<GetDeliverableByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Deliverable?>(null));

        // Act
        var result = await _tools.GetDeliverable(id);

        // Assert
        result.Should().Contain("Deliverable not found");
    }

    [Fact]
    public async Task CreateDeliverable_WithValidData_CreatesDeliverable()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var title = "New Deliverable";
        var description = "New description";

        _createDeliverableHandler.Handle(Arg.Any<CreateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateDeliverable(
            projectId,
            title,
            description,
            null, null, null, null, null, null, null);

        // Assert
        result.Should().Contain("Deliverable Created");
        result.Should().Contain(newId.ToString());
        result.Should().Contain("Feature");
        result.Should().Contain("Ready");

        await _createDeliverableHandler.Received(1).Handle(
            Arg.Is<CreateDeliverableCommand>(cmd =>
                cmd.ProjectId == projectId &&
                cmd.Title == title &&
                cmd.Description == description &&
                cmd.Type == DeliverableType.Feature &&
                cmd.InitialStatus == DeliverableStatus.Draft),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDeliverable_WithNullProjectId_ThrowsMcpProtocolException()
    {
        // Arrange
        var title = "New Deliverable";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateDeliverable(null, title, null, null, null, null, null, null, null, null));

        exception.Message.Should().Be("Project ID is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateDeliverable_WithEmptyProjectId_ThrowsMcpProtocolException()
    {
        // Arrange
        var title = "New Deliverable";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<McpProtocolException>(
            () => _tools.CreateDeliverable(Guid.Empty, title, null, null, null, null, null, null, null, null));

        exception.Message.Should().Be("Project ID is required");
        exception.ErrorCode.Should().Be(McpErrorCode.InvalidParams);
    }

    [Fact]
    public async Task CreateDeliverable_WithAllFields_CreatesDeliverableWithAllFields()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var newId = Guid.NewGuid();
        var acceptanceCriteria = "Must work";
        var executionPlan = "Step by step";
        var securityImpact = "Low risk";
        var performanceImpact = "Minimal";
        var testPlan = "Run tests";
        var deploymentPlan = "Deploy to prod";

        _createDeliverableHandler.Handle(Arg.Any<CreateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateDeliverable(
            projectId,
            "Title",
            "Description",
            "Design",
            acceptanceCriteria,
            executionPlan,
            securityImpact,
            performanceImpact,
            testPlan,
            deploymentPlan);

        // Assert
        result.Should().Contain("Deliverable Created");
        await _createDeliverableHandler.Received(1).Handle(
            Arg.Is<CreateDeliverableCommand>(cmd =>
                cmd.AcceptanceCriteria == acceptanceCriteria &&
                cmd.ExecutionPlan == executionPlan &&
                cmd.SecurityImpact == securityImpact &&
                cmd.PerformanceImpact == performanceImpact &&
                cmd.TestPlan == testPlan &&
                cmd.DeploymentPlan == deploymentPlan),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDeliverable_WithNullTitle_CreatesDeliverable()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var newId = Guid.NewGuid();

        _createDeliverableHandler.Handle(Arg.Any<CreateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(newId));

        // Act
        var result = await _tools.CreateDeliverable(
            projectId,
            null!,
            null, null, null, null, null, null, null, null);

        // Assert
        result.Should().Contain("Deliverable Created");
        result.Should().Contain(newId.ToString());
    }

    [Fact]
    public async Task GetDeliverable_WithNullResult_ReturnsErrorMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        _getDeliverableByIdHandler.Handle(Arg.Any<GetDeliverableByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Deliverable?>(null));

        // Act
        var result = await _tools.GetDeliverable(id);

        // Assert
        result.Should().Contain("Deliverable not found");
    }

    [Fact]
    public async Task UpdateDeliverable_WithValidData_UpdatesDeliverable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var newDescription = "Updated description";

        _updateDeliverableHandler.Handle(Arg.Any<UpdateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateDeliverable(
            id,
            newDescription,
            null, null, null, null, null, null, null, null, null);

        // Assert
        result.Should().Contain("Deliverable Updated");
        result.Should().Contain(id.ToString());
        result.Should().Contain("true");

        await _updateDeliverableHandler.Received(1).Handle(
            Arg.Is<UpdateDeliverableCommand>(cmd => cmd.Id == id && cmd.Description == newDescription),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateDeliverable_WithAllFields_UpdatesAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateDeliverableHandler.Handle(Arg.Any<UpdateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateDeliverable(
            id,
            "desc", "design", "acceptance", "plan",
            "security", "performance", "test", "deployment",
            "feedback", "blocking");

        // Assert
        result.Should().Contain("Deliverable Updated");
        await _updateDeliverableHandler.Received(1).Handle(
            Arg.Is<UpdateDeliverableCommand>(cmd =>
                cmd.Id == id &&
                cmd.Description == "desc" &&
                cmd.Design == "design" &&
                cmd.AcceptanceCriteria == "acceptance" &&
                cmd.ExecutionPlan == "plan" &&
                cmd.AgentFeedback == "feedback" &&
                cmd.SecurityImpact == "security" &&
                cmd.PerformanceImpact == "performance" &&
                cmd.TestPlan == "test" &&
                cmd.DeploymentPlan == "deployment" &&
                cmd.Blocking == "blocking"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionDeliverableStatus_WithValidData_TransitionsStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = DeliverableStatus.Design;
        var actor = "test-user";

        _updateDeliverableStatusHandler.Handle(Arg.Any<UpdateDeliverableStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.TransitionDeliverableStatus(id, targetStatus, actor);

        // Assert
        result.Should().Contain("Deliverable State Transitioned");
        result.Should().Contain(id.ToString());
        result.Should().Contain("Design");
        result.Should().Contain("test-user");

        await _updateDeliverableStatusHandler.Received(1).Handle(
            Arg.Is<UpdateDeliverableStatusCommand>(cmd =>
                cmd.Id == id &&
                cmd.TargetStatus == targetStatus &&
                cmd.ChangedBy == actor),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionDeliverableStatus_WithAllStatuses_ReturnsCorrectResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var actor = "test-user";

        _updateDeliverableStatusHandler.Handle(Arg.Any<UpdateDeliverableStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act & Assert - test each status value
        foreach (var status in Enum.GetValues<DeliverableStatus>())
        {
            var result = await _tools.TransitionDeliverableStatus(id, status, actor);
            result.Should().Contain(status.ToString());
        }
    }

    [Fact]
    public async Task UpdateDeliverable_WithAllNullFields_UpdatesOnlyId()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateDeliverableHandler.Handle(Arg.Any<UpdateDeliverableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tools.UpdateDeliverable(
            id,
            null, null, null, null, null, null, null, null, null, null);

        // Assert
        result.Should().Contain("Deliverable Updated");
        result.Should().Contain(id.ToString());
        await _updateDeliverableHandler.Received(1).Handle(
            Arg.Is<UpdateDeliverableCommand>(cmd => cmd.Id == id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransitionDeliverableStatus_WithHandlerException_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = DeliverableStatus.Done;
        var actor = "test-user";
        var errorMessage = "Invalid status transition";

        _updateDeliverableStatusHandler.Handle(Arg.Any<UpdateDeliverableStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UpdateDeliverableStatusCommand>(new Exception(errorMessage)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _tools.TransitionDeliverableStatus(id, targetStatus, actor));

        exception.Message.Should().Be(errorMessage);
    }
}
