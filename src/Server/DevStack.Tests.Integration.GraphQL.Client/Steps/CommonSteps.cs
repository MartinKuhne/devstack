using System;
using System.Threading.Tasks;
using DevStack.Client;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class CommonSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IDevStackClient _client;

    public CommonSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _client = Hooks.SpecFlowHooks.GetClient(scenarioContext);
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
        var graphQlUrl = Hooks.SpecFlowHooks.GetGraphQLUrl(_scenarioContext);
        graphQlUrl.Should().NotBeEmpty();
        _client.Should().NotBeNull();
    }

    [Given("a parent project exists")]
    public async Task GivenAParentProjectExists()
    {
        var input = new CreateProjectInput
        {
            Name = "Test Parent Project",
            Description = "Parent project for test scenarios"
        };

        var result = await _client.CreateProject.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdProjectId = result.Data!.CreateProject.Project?.Id;
        createdProjectId.Should().NotBeNull();

        _scenarioContext["ParentProjectId"] = createdProjectId;
        _scenarioContext["ParentProject"] = result.Data!.CreateProject.Project;
    }

    [Given("a parent feature exists")]
    public async Task GivenAParentFeatureExists()
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var input = new CreateFeatureInput
        {
            ProjectId = Guid.Parse(pid!),
            Title = "Test Parent Feature",
            Description = "Parent feature for test scenarios"
        };

        var result = await _client.CreateFeature.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdFeatureId = result.Data!.CreateFeature.Item?.Id;
        createdFeatureId.Should().NotBeNullOrEmpty();

        _scenarioContext["ParentFeatureId"] = createdFeatureId;
        _scenarioContext["ParentFeature"] = result.Data!.CreateFeature.Item;
    }

    [Given(@"a project ""(.*)"" exists")]
    public async Task GivenAProjectExists(string projectName)
    {
        var input = new CreateProjectInput
        {
            Name = projectName,
            Description = $"Project for testing: {projectName}"
        };

        var result = await _client.CreateProject.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdProjectId = result.Data!.CreateProject.Project?.Id;
        createdProjectId.Should().NotBeNull();

        _scenarioContext[$"ProjectId_{projectName}"] = createdProjectId;
    }

    [Given(@"a feature ""(.*)"" exists")]
    public async Task GivenAFeatureExists(string featureTitle)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var input = new CreateFeatureInput
        {
            ProjectId = Guid.Parse(pid!),
            Title = featureTitle,
            Description = $"Feature for testing: {featureTitle}"
        };

        var result = await _client.CreateFeature.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdFeatureId = result.Data!.CreateFeature.Item?.Id;
        createdFeatureId.Should().NotBeNullOrEmpty();

        _scenarioContext[$"FeatureId_{featureTitle}"] = createdFeatureId;
    }

    [Given(@"a task ""(.*)"" exists")]
    public async Task GivenATaskExists(string taskTitle)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        var featureId = _scenarioContext.TryGetValue<string>("ParentFeatureId", out var fid) ? fid : null;
        
        projectId.Should().NotBeNullOrEmpty();
        featureId.Should().NotBeNullOrEmpty();

        var input = new CreateTaskInput
        {
            ProjectId = Guid.Parse(projectId!),
            ItemId = Guid.Parse(featureId!),
            Title = taskTitle,
            ComplexityRating = 5
        };

        var result = await _client.CreateTask.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdTaskId = result.Data!.CreateTask.Task?.Id;
        createdTaskId.Should().NotBeNull();

        _scenarioContext[$"TaskId_{taskTitle}"] = createdTaskId;
    }

    [Given(@"a defect ""(.*)"" exists")]
    public async Task GivenADefectExists(string defectTitle)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var input = new CreateDefectInput
        {
            ProjectId = Guid.Parse(projectId!),
            ParentFeatureId = null,
            Title = defectTitle,
            Severity = Severity.High
        };

        var result = await _client.CreateDefect.ExecuteAsync(input);
        result.Data.Should().NotBeNull();
        result.Errors.Should().BeEmpty();

        var createdDefectId = result.Data!.CreateDefect.Item?.Id;
        createdDefectId.Should().NotBeNull();

        _scenarioContext[$"DefectId_{defectTitle}"] = createdDefectId;
    }

    [Given("a task with status \"(.*)\" exists")]
    public async Task GivenATaskWithStatusExists(string status)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        var featureId = _scenarioContext.TryGetValue<string>("ParentFeatureId", out var fid) ? fid : null;
        
        projectId.Should().NotBeNullOrEmpty();
        featureId.Should().NotBeNullOrEmpty();

        var input = new CreateTaskInput
        {
            ProjectId = Guid.Parse(projectId!),
            ItemId = Guid.Parse(featureId!),
            Title = $"Task with status {status}",
            ComplexityRating = 5
        };

        var createResult = await _client.CreateTask.ExecuteAsync(input);
        createResult.Data.Should().NotBeNull();
        
        var taskId = createResult.Data!.CreateTask.Task?.Id;
        taskId.Should().NotBeNull();

        if (!string.Equals(status, "Todo", StringComparison.OrdinalIgnoreCase))
        {
            var statusEnum = Enum.Parse<DevStack.Client.TaskStatus>(status, ignoreCase: true);
            var statusInput = new TransitionTaskInput
            {
                Id = Guid.Parse(taskId!),
                TargetStatus = statusEnum,
                Actor = "Test"
            };

            var transitionResult = await _client.TransitionTaskStatus.ExecuteAsync(statusInput);
            transitionResult.Data.Should().NotBeNull();
        }

        _scenarioContext["CurrentTaskId"] = taskId!;
        _scenarioContext["CurrentTaskStatus"] = status;
    }

    [Given("a feature with status \"(.*)\" exists")]
    public async Task GivenAFeatureWithStatusExists(string status)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var createInput = new CreateFeatureInput
        {
            ProjectId = Guid.Parse(projectId!),
            Title = $"Feature with status {status}"
        };

        var createResult = await _client.CreateFeature.ExecuteAsync(createInput);
        createResult.Data.Should().NotBeNull();

        var featureId = createResult.Data!.CreateFeature.Item?.Id;
        featureId.Should().NotBeNull();

        if (!string.Equals(status, "Planning", StringComparison.OrdinalIgnoreCase))
        {
            var statusEnum = Enum.Parse<FeatureStatus>(status, ignoreCase: true);
            var statusInput = new TransitionFeatureInput
            {
                Id = Guid.Parse(featureId!),
                TargetStatus = statusEnum,
                Actor = "Test"
            };

            var transitionResult = await _client.TransitionFeatureStatus.ExecuteAsync(statusInput);
            transitionResult.Data.Should().NotBeNull();
        }

        _scenarioContext["CurrentFeatureId"] = featureId!;
        _scenarioContext["CurrentFeatureStatus"] = status;
    }

    [Given("a defect with status \"(.*)\" exists")]
    public async Task GivenADefectWithStatusExists(string status)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var createInput = new CreateDefectInput
        {
            ProjectId = Guid.Parse(projectId!),
            Title = $"Defect with status {status}",
            Severity = Severity.High
        };

        var createResult = await _client.CreateDefect.ExecuteAsync(createInput);
        createResult.Data.Should().NotBeNull();

        var defectId = createResult.Data!.CreateDefect.Item?.Id;
        defectId.Should().NotBeNull();

        if (!string.Equals(status, "Planning", StringComparison.OrdinalIgnoreCase))
        {
            var statusEnum = Enum.Parse<FeatureStatus>(status, ignoreCase: true);
            var statusInput = new TransitionDefectInput
            {
                Id = Guid.Parse(defectId!),
                TargetStatus = statusEnum,
                Actor = "Test"
            };

            var transitionResult = await _client.TransitionDefectStatus.ExecuteAsync(statusInput);
            transitionResult.Data.Should().NotBeNull();
        }

        _scenarioContext["CurrentDefectId"] = defectId!;
        _scenarioContext["CurrentDefectStatus"] = status;
    }
}