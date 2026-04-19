using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;
using System.Text.Json;
using System.Threading;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class DevStackToolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;
    private string? _createdProjectId;
    private string? _createdDeliverableId;
    private string? _createdTaskId;

    public DevStackToolsSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    #region Project Steps

    [Given(@"a valid project creation request with name ""(.*)""")]
    public void GivenAValidProjectCreationRequest(string projectName)
    {
        _scenarioContext["ProjectName"] = projectName;
    }

    [Given(@"an existing project ID")]
    public async Task GivenAnExistingProjectID()
    {
        if (_createdProjectId == null)
        {
            _createdProjectId = await CreateTestProjectAsync();
        }
        _scenarioContext["ProjectId"] = _createdProjectId;
    }

    [Given(@"existing projects in the system")]
    public async Task GivenExistingProjectsInTheSystem()
    {
        if (_createdProjectId == null)
        {
            _createdProjectId = await CreateTestProjectAsync();
        }
    }

    [When(@"I call devstack_createProject")]
    public async Task WhenICallDevstackCreateProject()
    {
        var projectName = _scenarioContext.GetString("ProjectName") ?? "Test Project";
        var request = new { name = projectName, description = "Test project description" };
        _response = await Client.SendRequestAsync("devstack_createProject", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjectById with the ID")]
    public async Task WhenICallDevstackGetProjectById()
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId };
        _response = await Client.SendRequestAsync("devstack_getProjectById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjects")]
    public async Task WhenICallDevstackGetProjects()
    {
        _response = await Client.SendRequestAsync("devstack_getProjects", default);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateProject with updated name ""(.*)""")]
    public async Task WhenICallDevstackUpdateProject(string updatedName)
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId, name = updatedName };
        _response = await Client.SendRequestAsync("devstack_updateProject", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created project")]
    public void ThenTheResponseShouldContainTheCreatedProject()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the project should have a valid ID")]
    public void ThenTheProjectShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdProjectId = idElement.GetString();
            _createdProjectId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the response should contain the project details")]
    public void ThenTheResponseShouldContainTheProjectDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the project name should match")]
    public void ThenTheProjectNameShouldMatch()
    {
        var expectedName = _scenarioContext.GetString("ProjectName");
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedName ?? "");
    }

    [Then(@"the response should contain a list of projects")]
    public void ThenTheResponseShouldContainAListOfProjects()
    {
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the list should not be empty")]
    public void ThenTheListShouldNotBeEmpty()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBe("[]");
    }

    [Then(@"the response should contain the updated project")]
    public void ThenTheResponseShouldContainTheUpdatedProject()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the project name should be ""(.*)""")]
    public void ThenTheProjectNameShouldBe(string expectedName)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedName);
    }

    #endregion

    #region Deliverable Steps

    [Given(@"a valid deliverable creation request with title ""(.*)""")]
    public void GivenAValidDeliverableCreationRequest(string deliverableTitle)
    {
        _scenarioContext["DeliverableTitle"] = deliverableTitle;
    }

    [Given(@"an existing deliverable ID")]
    public async Task GivenAnExistingDeliverableID()
    {
        if (_createdDeliverableId == null)
        {
            _createdDeliverableId = await CreateTestDeliverableAsync();
        }
        _scenarioContext["DeliverableId"] = _createdDeliverableId;
    }

    [Given(@"a deliverable in ""(.*)"" status")]
    public void GivenADeliverableInStatus(string status)
    {
        _scenarioContext["DeliverableStatus"] = status;
    }

    [When(@"I call devstack_createDeliverable")]
    public async Task WhenICallDevstackCreateDeliverable()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var title = _scenarioContext.GetString("DeliverableTitle") ?? "Test Deliverable";
        var request = new { projectId, title, description = "Test deliverable description" };
        _response = await Client.SendRequestAsync("devstack_createDeliverable", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateDeliverable with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateDeliverable(string updatedTitle)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var request = new { id = Guid.Parse(deliverableId), title = updatedTitle };
        _response = await Client.SendRequestAsync("devstack_updateDeliverable", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_transitionDeliverableStatus to ""(.*)""")]
    public async Task WhenICallDevstackTransitionDeliverableStatus(string targetStatus)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var request = new { id = Guid.Parse(deliverableId), targetStatus, actor = "test" };
        _response = await Client.SendRequestAsync("devstack_transitionDeliverableStatus", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created deliverable")]
    public void ThenTheResponseShouldContainTheCreatedDeliverable()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the deliverable should have a valid ID")]
    public void ThenTheDeliverableShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdDeliverableId = idElement.GetString();
            _createdDeliverableId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the deliverable status should be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBe(string expectedStatus)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedStatus);
    }

    [Then(@"the response should contain the updated deliverable")]
    public void ThenTheResponseShouldContainTheUpdatedDeliverable()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the deliverable title should be ""(.*)""")]
    public void ThenTheDeliverableTitleShouldBe(string expectedTitle)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTitle);
    }

    [Then(@"the response should contain the deliverable with new status")]
    public void ThenTheResponseShouldContainTheDeliverableWithNewStatus()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Agent Task Steps

    [Given(@"a valid agent task creation request with title ""(.*)""")]
    public void GivenAValidAgentTaskCreationRequest(string taskTitle)
    {
        _scenarioContext["TaskTitle"] = taskTitle;
    }

    [Given(@"an existing task ID")]
    public async Task GivenAnExistingTaskID()
    {
        if (_createdTaskId == null)
        {
            _createdTaskId = await CreateTestTaskAsync();
        }
        _scenarioContext["TaskId"] = _createdTaskId;
    }

    [Given(@"a task in ""(.*)"" status")]
    public void GivenATaskInStatus(string status)
    {
        _scenarioContext["TaskStatus"] = status;
    }

    [When(@"I call devstack_createAgentTask")]
    public async Task WhenICallDevstackCreateAgentTask()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var deliverableId = await GetOrCreateTestDeliverableIdAsync();
        var title = _scenarioContext.GetString("TaskTitle") ?? "Test Task";
        var request = new { projectId, itemId = deliverableId, title, deliverableDescription = "Test deliverable" };
        _response = await Client.SendRequestAsync("devstack_createAgentTask", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateAgentTask with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateAgentTask(string updatedTitle)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId), title = updatedTitle };
        _response = await Client.SendRequestAsync("devstack_updateAgentTask", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_transitionAgentTaskStatus to ""(.*)""")]
    public async Task WhenICallDevstackTransitionAgentTaskStatus(string targetStatus)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId), targetStatus, actor = "test" };
        _response = await Client.SendRequestAsync("devstack_transitionAgentTaskStatus", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created task")]
    public void ThenTheResponseShouldContainTheCreatedTask()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the task should have a valid ID")]
    public void ThenTheTaskShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdTaskId = idElement.GetString();
            _createdTaskId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the task status should be ""(.*)""")]
    public void ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedStatus);
    }

    [Then(@"the response should contain the updated task")]
    public void ThenTheResponseShouldContainTheUpdatedTask()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the task title should be ""(.*)""")]
    public void ThenTheTaskTitleShouldBe(string expectedTitle)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTitle);
    }

    [Then(@"the response should contain the task with new status")]
    public void ThenTheResponseShouldContainTheTaskWithNewStatus()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the status should be ""(.*)""")]
    public void ThenTheStatusShouldBe(string expectedStatus)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedStatus);
    }

    #endregion

    #region Helper Methods

    private async Task<string> CreateTestProjectAsync()
    {
        var request = new { name = $"Test Project {Guid.NewGuid()}", description = "Auto-generated test project" };
        var response = await Client.SendRequestAsync("devstack_createProject", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    private async Task<string> GetOrCreateTestProjectIdAsync()
    {
        var projectId = _scenarioContext.GetString("ProjectId");
        if (!string.IsNullOrEmpty(projectId))
        {
            return projectId;
        }

        var newProjectId = await CreateTestProjectAsync();
        _scenarioContext["ProjectId"] = newProjectId;
        return newProjectId;
    }

    private async Task<string> CreateTestDeliverableAsync()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var request = new { projectId, title = $"Test Deliverable {Guid.NewGuid()}", description = "Auto-generated test deliverable" };
        var response = await Client.SendRequestAsync("devstack_createDeliverable", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    private async Task<string> GetOrCreateTestDeliverableIdAsync()
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId");
        if (!string.IsNullOrEmpty(deliverableId))
        {
            return deliverableId;
        }

        var newDeliverableId = await CreateTestDeliverableAsync();
        _scenarioContext["DeliverableId"] = newDeliverableId;
        return newDeliverableId;
    }

    private async Task<string> CreateTestTaskAsync()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var deliverableId = await GetOrCreateTestDeliverableIdAsync();
        var request = new { projectId, itemId = deliverableId, title = $"Test Task {Guid.NewGuid()}", deliverableDescription = "Auto-generated test task" };
        var response = await Client.SendRequestAsync("devstack_createAgentTask", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    #endregion
}
