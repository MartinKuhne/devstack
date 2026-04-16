using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;
using System.Text.Json;
using System.Threading;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class DevStackToolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IMcpJsonRpcClient _client;
    private JsonRpcResponse? _response;
    private string? _createdResourceId;
    private string? _createdFeatureId;
    private string? _createdDefectId;
    private string? _createdTaskId;
    private string? _createdEpicId;

    public DevStackToolsSteps(ScenarioContext scenarioContext, IMcpJsonRpcClient client)
    {
        _scenarioContext = scenarioContext;
        _client = client;
    }

    #region Project Steps

    [Given(@"a valid project creation request with name ""(.*)""")]
    public void GivenAValidProjectCreationRequest(string projectName)
    {
        _scenarioContext["ProjectName"] = projectName;
    }

    [Given(@"an existing project ID")]
    public async Task GivenAnExistingProjectID()
    {
        if (_createdResourceId == null)
        {
            _createdResourceId = await CreateTestProjectAsync();
        }
        _scenarioContext["ProjectId"] = _createdResourceId;
    }

    [When(@"I call devstack_createProject")]
    public async Task WhenICallDevstackCreateProject()
    {
        var projectName = _scenarioContext.GetString("ProjectName") ?? "Test Project";
        var request = new { name = projectName, description = "Test project description" };
        _response = await _client.SendRequestAsync("devstack_createProject", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjectById with the ID")]
    public async Task WhenICallDevstackGetProjectById()
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId };
        _response = await _client.SendRequestAsync("devstack_getProjectById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjects")]
    public async Task WhenICallDevstackGetProjects()
    {
        _response = await _client.SendRequestAsync("devstack_getProjects", default(CancellationToken));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateProject with updated name ""(.*)""")]
    public async Task WhenICallDevstackUpdateProject(string updatedName)
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId, name = updatedName };
        _response = await _client.SendRequestAsync("devstack_updateProject", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteProject with the ID")]
    public async Task WhenICallDevstackDeleteProject()
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId };
        _response = await _client.SendRequestAsync("devstack_deleteProject", request);
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
            _createdResourceId = idElement.GetString();
            _createdResourceId.Should().NotBeNullOrEmpty();
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

    [Then(@"the response should confirm deletion")]
    public void ThenTheResponseShouldConfirmDeletion()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Feature Steps

    [Given(@"a valid feature creation request with title ""(.*)""")]
    public void GivenAValidFeatureCreationRequest(string featureTitle)
    {
        _scenarioContext["FeatureTitle"] = featureTitle;
    }

    [Given(@"an existing feature ID")]
    public async Task GivenAnExistingFeatureID()
    {
        if (_createdFeatureId == null)
        {
            _createdFeatureId = await CreateTestFeatureAsync();
        }
        _scenarioContext["FeatureId"] = _createdFeatureId;
    }

    [Given(@"existing features in the system")]
    public void GivenExistingFeaturesInTheSystem()
    {
    }

    [When(@"I call devstack_createFeature")]
    public async Task WhenICallDevstackCreateFeature()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var title = _scenarioContext.GetString("FeatureTitle") ?? "Test Feature";
        var request = new { projectId, title, description = "Test feature description" };
        _response = await _client.SendRequestAsync("devstack_createFeature", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getFeatureById with the ID")]
    public async Task WhenICallDevstackGetFeatureById()
    {
        var featureId = _scenarioContext.GetString("FeatureId") ?? "";
        var request = new { id = Guid.Parse(featureId) };
        _response = await _client.SendRequestAsync("devstack_getFeatureById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getFeatures with projectId filter")]
    public async Task WhenICallDevstackGetFeaturesWithProjectIdFilter()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var request = new { projectId };
        _response = await _client.SendRequestAsync("devstack_getFeatures", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateFeature with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateFeature(string updatedTitle)
    {
        var featureId = _scenarioContext.GetString("FeatureId") ?? "";
        var request = new { id = Guid.Parse(featureId), title = updatedTitle };
        _response = await _client.SendRequestAsync("devstack_updateFeature", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_transitionFeatureStatus to ""(.*)""")]
    public async Task WhenICallDevstackTransitionFeatureStatus(string targetStatus)
    {
        var featureId = _scenarioContext.GetString("FeatureId") ?? "";
        var request = new { id = Guid.Parse(featureId), targetStatus, actor = "test" };
        _response = await _client.SendRequestAsync("devstack_transitionFeatureStatus", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getValidStatusTransitions")]
    public async Task WhenICallDevstackGetValidStatusTransitions()
    {
        var featureId = _scenarioContext.GetString("FeatureId") ?? "";
        var request = new { featureId = Guid.Parse(featureId) };
        _response = await _client.SendRequestAsync("devstack_getValidStatusTransitions", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteFeature with the ID")]
    public async Task WhenICallDevstackDeleteFeature()
    {
        var featureId = _scenarioContext.GetString("FeatureId") ?? "";
        var request = new { id = Guid.Parse(featureId) };
        _response = await _client.SendRequestAsync("devstack_deleteFeature", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created feature")]
    public void ThenTheResponseShouldContainTheCreatedFeature()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the feature should have a valid ID")]
    public void ThenTheFeatureShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdFeatureId = idElement.GetString();
            _createdFeatureId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the response should contain the feature details")]
    public void ThenTheResponseShouldContainTheFeatureDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the response should contain filtered features")]
    public void ThenTheResponseShouldContainFilteredFeatures()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"all features should belong to the specified project")]
    public void ThenAllFeaturesShouldBelongToTheSpecifiedProject()
    {
        var result = _response!.Result!.ToString();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.GetArrayLength() > 0)
        {
            var expectedProjectId = _scenarioContext.GetString("ProjectId") ?? "";
            foreach (var feature in jsonDoc.RootElement.EnumerateArray())
            {
                if (feature.TryGetProperty("projectId", out var projectIdElement))
                {
                    projectIdElement.GetString().Should().Be(expectedProjectId);
                }
            }
        }
    }

    [Then(@"the response should contain the updated feature")]
    public void ThenTheResponseShouldContainTheUpdatedFeature()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the feature title should be ""(.*)""")]
    public void ThenTheFeatureTitleShouldBe(string expectedTitle)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTitle);
    }

    [Given(@"a feature in ""(.*)"" status")]
    public void GivenAFeatureInStatus(string status)
    {
        _scenarioContext["FeatureStatus"] = status;
    }

    [Then(@"the response should contain the feature with new status")]
    public void ThenTheResponseShouldContainTheFeatureWithNewStatus()
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

    [Then(@"the response should contain valid transitions")]
    public void ThenTheResponseShouldContainValidTransitions()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"""(.*)"" should be a valid transition")]
    public void ThenShouldBeAValidTransition(string expectedTransition)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTransition);
    }

    #endregion

    #region Defect Steps

    [Given(@"a valid defect creation request with title ""(.*)""")]
    public void GivenAValidDefectCreationRequest(string defectTitle)
    {
        _scenarioContext["DefectTitle"] = defectTitle;
    }

    [Given(@"an existing defect ID")]
    public async Task GivenAnExistingDefectID()
    {
        if (_createdDefectId == null)
        {
            _createdDefectId = await CreateTestDefectAsync();
        }
        _scenarioContext["DefectId"] = _createdDefectId;
    }

    [Given(@"existing defects in the system")]
    public void GivenExistingDefectsInTheSystem()
    {
    }

    [When(@"I call devstack_createDefect")]
    public async Task WhenICallDevstackCreateDefect()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var title = _scenarioContext.GetString("DefectTitle") ?? "Test Defect";
        var request = new { projectId, title, description = "Test defect description" };
        _response = await _client.SendRequestAsync("devstack_createDefect", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_createDefect with parentFeatureId")]
    public async Task WhenICallDevstackCreateDefectWithParentFeatureId()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var featureId = await GetOrCreateTestFeatureIdAsync();
        var request = new { projectId, parentFeatureId = featureId, title = "Test Defect with Parent" };
        _response = await _client.SendRequestAsync("devstack_createDefect", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getDefectById with the ID")]
    public async Task WhenICallDevstackGetDefectById()
    {
        var defectId = _scenarioContext.GetString("DefectId") ?? "";
        var request = new { id = Guid.Parse(defectId) };
        _response = await _client.SendRequestAsync("devstack_getDefectById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getDefects")]
    public async Task WhenICallDevstackGetDefects()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var request = new { projectId };
        _response = await _client.SendRequestAsync("devstack_getDefects", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateDefect with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateDefect(string updatedTitle)
    {
        var defectId = _scenarioContext.GetString("DefectId") ?? "";
        var request = new { id = Guid.Parse(defectId), title = updatedTitle };
        _response = await _client.SendRequestAsync("devstack_updateDefect", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_transitionDefectStatus to ""(.*)""")]
    public async Task WhenICallDevstackTransitionDefectStatus(string targetStatus)
    {
        var defectId = _scenarioContext.GetString("DefectId") ?? "";
        var request = new { id = Guid.Parse(defectId), targetStatus, actor = "test" };
        _response = await _client.SendRequestAsync("devstack_transitionDefectStatus", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteDefect with the ID")]
    public async Task WhenICallDevstackDeleteDefect()
    {
        var defectId = _scenarioContext.GetString("DefectId") ?? "";
        var request = new { id = Guid.Parse(defectId) };
        _response = await _client.SendRequestAsync("devstack_deleteDefect", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created defect")]
    public void ThenTheResponseShouldContainTheCreatedDefect()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the defect should have a valid ID")]
    public void ThenTheDefectShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdDefectId = idElement.GetString();
            _createdDefectId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the response should contain the defect with parent feature reference")]
    public void ThenTheResponseShouldContainTheDefectWithParentFeatureReference()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the response should contain the defect details")]
    public void ThenTheResponseShouldContainTheDefectDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the response should contain a list of defects")]
    public void ThenTheResponseShouldContainAListOfDefects()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the response should contain the updated defect")]
    public void ThenTheResponseShouldContainTheUpdatedDefect()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the defect title should be ""(.*)""")]
    public void ThenTheDefectTitleShouldBe(string expectedTitle)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTitle);
    }

    [Given(@"a defect in ""(.*)"" status")]
    public void GivenADefectInStatus(string status)
    {
        _scenarioContext["DefectStatus"] = status;
    }

    [Then(@"the response should contain the defect with new status")]
    public void ThenTheResponseShouldContainTheDefectWithNewStatus()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Task Steps

    [Given(@"a valid task creation request with title ""(.*)""")]
    public void GivenAValidTaskCreationRequest(string taskTitle)
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

    [Given(@"existing tasks in the system")]
    public void GivenExistingTasksInTheSystem()
    {
    }

    [When(@"I call devstack_createTask")]
    public async Task WhenICallDevstackCreateTask()
    {
        var featureId = await GetOrCreateTestFeatureIdAsync();
        var title = _scenarioContext.GetString("TaskTitle") ?? "Test Task";
        var request = new { featureId, title, deliverable = "Test deliverable" };
        _response = await _client.SendRequestAsync("devstack_createTask", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getTaskById with the ID")]
    public async Task WhenICallDevstackGetTaskById()
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId) };
        _response = await _client.SendRequestAsync("devstack_getTaskById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getTasks with featureId filter")]
    public async Task WhenICallDevstackGetTasksWithFeatureIdFilter()
    {
        var featureId = await GetOrCreateTestFeatureIdAsync();
        var request = new { featureId };
        _response = await _client.SendRequestAsync("devstack_getTasks", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateTask with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateTask(string updatedTitle)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId), title = updatedTitle };
        _response = await _client.SendRequestAsync("devstack_updateTask", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_transitionTaskStatus to ""(.*)""")]
    public async Task WhenICallDevstackTransitionTaskStatus(string targetStatus)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId), targetStatus, actor = "test" };
        _response = await _client.SendRequestAsync("devstack_transitionTaskStatus", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteTask with the ID")]
    public async Task WhenICallDevstackDeleteTask()
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var request = new { id = Guid.Parse(taskId) };
        _response = await _client.SendRequestAsync("devstack_deleteTask", request);
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

    [Then(@"the response should contain the task details")]
    public void ThenTheResponseShouldContainTheTaskDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the response should contain filtered tasks")]
    public void ThenTheResponseShouldContainFilteredTasks()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"all tasks should belong to the specified feature")]
    public void ThenAllTasksShouldBelongToTheSpecifiedFeature()
    {
        var result = _response!.Result!.ToString();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.GetArrayLength() > 0)
        {
            var expectedFeatureId = _scenarioContext.GetString("FeatureId") ?? "";
            foreach (var task in jsonDoc.RootElement.EnumerateArray())
            {
                if (task.TryGetProperty("featureId", out var featureIdElement))
                {
                    featureIdElement.GetString().Should().Be(expectedFeatureId);
                }
            }
        }
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

    [Given(@"a task in ""(.*)"" status")]
    public void GivenATaskInStatus(string status)
    {
        _scenarioContext["TaskStatus"] = status;
    }

    [Then(@"the response should contain the task with new status")]
    public void ThenTheResponseShouldContainTheTaskWithNewStatus()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Epic Steps

    [Given(@"a valid epic creation request with title ""(.*)""")]
    public void GivenAValidEpicCreationRequest(string epicTitle)
    {
        _scenarioContext["EpicTitle"] = epicTitle;
    }

    [Given(@"an existing epic ID")]
    public async Task GivenAnExistingEpicID()
    {
        if (_createdEpicId == null)
        {
            _createdEpicId = await CreateTestEpicAsync();
        }
        _scenarioContext["EpicId"] = _createdEpicId;
    }

    [Given(@"existing epics in the system")]
    public void GivenExistingEpicsInTheSystem()
    {
    }

    [When(@"I call devstack_createEpic")]
    public async Task WhenICallDevstackCreateEpic()
    {
        var title = _scenarioContext.GetString("EpicTitle") ?? "Test Epic";
        var request = new { title, description = "Test epic description" };
        _response = await _client.SendRequestAsync("devstack_createEpic", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getEpicById with the ID")]
    public async Task WhenICallDevstackGetEpicById()
    {
        var epicId = _scenarioContext.GetString("EpicId") ?? "";
        var request = new { id = Guid.Parse(epicId) };
        _response = await _client.SendRequestAsync("devstack_getEpicById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getEpics with title filter ""(.*)""")]
    public async Task WhenICallDevstackGetEpicsWithTitleFilter(string titleFilter)
    {
        var request = new { title = titleFilter };
        _response = await _client.SendRequestAsync("devstack_getEpics", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateEpic with updated title ""(.*)""")]
    public async Task WhenICallDevstackUpdateEpic(string updatedTitle)
    {
        var epicId = _scenarioContext.GetString("EpicId") ?? "";
        var request = new { id = Guid.Parse(epicId), title = updatedTitle };
        _response = await _client.SendRequestAsync("devstack_updateEpic", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteEpic with the ID")]
    public async Task WhenICallDevstackDeleteEpic()
    {
        var epicId = _scenarioContext.GetString("EpicId") ?? "";
        var request = new { id = Guid.Parse(epicId) };
        _response = await _client.SendRequestAsync("devstack_deleteEpic", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created epic")]
    public void ThenTheResponseShouldContainTheCreatedEpic()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the epic should have a valid ID")]
    public void ThenTheEpicShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdEpicId = idElement.GetString();
            _createdEpicId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the response should contain the epic details")]
    public void ThenTheResponseShouldContainTheEpicDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the response should contain filtered epics")]
    public void ThenTheResponseShouldContainFilteredEpics()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"all epics should contain ""(.*)"" in the title")]
    public void ThenAllEpicsShouldContainInTheTitle(string expectedText)
    {
        var result = _response!.Result!.ToString();
        var jsonDoc = JsonDocument.Parse(result!);
        foreach (var epic in jsonDoc.RootElement.EnumerateArray())
        {
            if (epic.TryGetProperty("title", out var titleElement))
            {
                titleElement.GetString().Should().Contain(expectedText);
            }
        }
    }

    [Then(@"the response should contain the updated epic")]
    public void ThenTheResponseShouldContainTheUpdatedEpic()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the epic title should be ""(.*)""")]
    public void ThenTheEpicTitleShouldBe(string expectedTitle)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedTitle);
    }

    #endregion

    #region Helper Methods

    private async Task<string> CreateTestProjectAsync()
    {
        var request = new { name = $"Test Project {Guid.NewGuid()}", description = "Auto-generated test project" };
        var response = await _client.SendRequestAsync("devstack_createProject", request);
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

    private async Task<string> CreateTestFeatureAsync()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var request = new { projectId, title = $"Test Feature {Guid.NewGuid()}", description = "Auto-generated test feature" };
        var response = await _client.SendRequestAsync("devstack_createFeature", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    private async Task<string> GetOrCreateTestFeatureIdAsync()
    {
        var featureId = _scenarioContext.GetString("FeatureId");
        if (!string.IsNullOrEmpty(featureId))
        {
            return featureId;
        }

        var newFeatureId = await CreateTestFeatureAsync();
        _scenarioContext["FeatureId"] = newFeatureId;
        return newFeatureId;
    }

    private async Task<string> CreateTestDefectAsync()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var request = new { projectId, title = $"Test Defect {Guid.NewGuid()}", description = "Auto-generated test defect" };
        var response = await _client.SendRequestAsync("devstack_createDefect", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    private async Task<string> CreateTestTaskAsync()
    {
        var featureId = await GetOrCreateTestFeatureIdAsync();
        var request = new { featureId, title = $"Test Task {Guid.NewGuid()}", deliverable = "Auto-generated test task" };
        var response = await _client.SendRequestAsync("devstack_createTask", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    private async Task<string> CreateTestEpicAsync()
    {
        var request = new { title = $"Test Epic {Guid.NewGuid()}", description = "Auto-generated test epic" };
        var response = await _client.SendRequestAsync("devstack_createEpic", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    #endregion
}
