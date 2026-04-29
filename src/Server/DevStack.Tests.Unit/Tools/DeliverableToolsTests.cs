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

using Microsoft.Extensions.Logging;

using NSubstitute;

using Xunit;

namespace DevStack.Tests.Unit.Tools;

public class DeliverableToolsTests
{
    private readonly ILogger<DeliverableTools> _logger;
    private readonly ICommandHandler<Guid, CreateDeliverableCommand> _createDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableCommand> _updateDeliverableHandler;
    private readonly ICommandHandler<UpdateDeliverableStatusCommand> _updateDeliverableStatusHandler;
    private readonly ICommandHandler<Deliverable?, GetDeliverableByIdQuery> _getDeliverableByIdHandler;
    private readonly DeliverableTools _tools;

    public DeliverableToolsTests()
    {
        _logger = Substitute.For<ILogger<DeliverableTools>>();
        _createDeliverableHandler = Substitute.For<ICommandHandler<Guid, CreateDeliverableCommand>>();
        _updateDeliverableHandler = Substitute.For<ICommandHandler<UpdateDeliverableCommand>>();
        _updateDeliverableStatusHandler = Substitute.For<ICommandHandler<UpdateDeliverableStatusCommand>>();
        _getDeliverableByIdHandler = Substitute.For<ICommandHandler<Deliverable?, GetDeliverableByIdQuery>>();
        _tools = new DeliverableTools(
            _logger,
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
        Assert.Contains("Test Deliverable", result);
        Assert.Contains("Test description", result);
        Assert.Contains("```json", result);
        Assert.Contains("## Deliverable", result);

        var jsonStart = result.IndexOf("{");
        var jsonEnd = result.LastIndexOf("}");
        var jsonStr = result.Substring(jsonStart, jsonEnd - jsonStart + 1);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(json);
        var idValue = json!["id"]?.ToString() ?? string.Empty;
        Assert.Equal(id.ToString().ToLowerInvariant(), idValue.ToLowerInvariant());
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
        Assert.Contains("Deliverable not found", result);
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
        Assert.Contains("Deliverable Created", result);
        Assert.Contains(newId.ToString(), result);
        Assert.Contains("Feature", result);
        Assert.Contains("Ready", result);

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
    public async Task CreateDeliverable_WithNullProjectId_ThrowsArgumentException()
    {
        // Arrange
        var title = "New Deliverable";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _tools.CreateDeliverable(null, title, null, null, null, null, null, null, null, null));

        Assert.Equal("Project ID is required", exception.Message);
    }

    [Fact]
    public async Task CreateDeliverable_WithEmptyProjectId_ThrowsArgumentException()
    {
        // Arrange
        var title = "New Deliverable";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _tools.CreateDeliverable(Guid.Empty, title, null, null, null, null, null, null, null, null));

        Assert.Equal("Project ID is required", exception.Message);
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
        Assert.Contains("Deliverable Created", result);
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
        Assert.Contains("Deliverable Updated", result);
        Assert.Contains(id.ToString(), result);
        Assert.Contains("true", result);

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
        Assert.Contains("Deliverable Updated", result);
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
        Assert.Contains("Deliverable State Transitioned", result);
        Assert.Contains(id.ToString(), result);
        Assert.Contains("Design", result);
        Assert.Contains("test-user", result);

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
            Assert.Contains(status.ToString(), result);
        }
    }

    [Fact]
    public async Task TransitionDeliverableStatus_WithHandlerException_ReturnsErrorMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var targetStatus = DeliverableStatus.Done;
        var actor = "test-user";
        var errorMessage = "Invalid status transition";

        _updateDeliverableStatusHandler.Handle(Arg.Any<UpdateDeliverableStatusCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<UpdateDeliverableStatusCommand>(new Exception(errorMessage)));

        // Act
        var result = await _tools.TransitionDeliverableStatus(id, targetStatus, actor);

        // Assert
        Assert.Contains(errorMessage, result);
    }
}
