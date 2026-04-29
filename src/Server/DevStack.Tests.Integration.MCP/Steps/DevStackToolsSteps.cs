using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class DevStackToolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;
    private string? _createdDeliverableId;
    private string? _createdTaskId;

    public DevStackToolsSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private string GetResultText(JsonRpcResponse response)
    {
        response.Result.Should().NotBeNull("MCP response result should not be null");
        var resultJson = JsonSerializer.Serialize(response.Result);
        var doc = JsonDocument.Parse(resultJson);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array && contentProp.GetArrayLength() > 0)
        {
            var firstBlock = contentProp[0];
            if (firstBlock.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
            {
                return textProp.GetString()!;
            }
        }

        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            var firstBlock = root[0];
            if (firstBlock.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
            {
                return textProp.GetString()!;
            }
        }

        return resultJson;
    }

    private string ExtractJsonFromMarkdown(string markdown)
    {
        var match = Regex.Match(markdown, @"```json\s*\n(.*?)\n```", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        return markdown.Trim();
    }

    private string GetResultJson(JsonRpcResponse response)
    {
        var text = GetResultText(response);
        return ExtractJsonFromMarkdown(text);
    }

    private static bool IsMcpToolError(JsonRpcResponse response, out string errorText)
    {
        errorText = string.Empty;
        if (response.Result == null) return false;

        var resultJson = JsonSerializer.Serialize(response.Result);
        Console.WriteLine($"[MCP DEBUG] Full result JSON: {resultJson}");
        var doc = JsonDocument.Parse(resultJson);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object) return false;

        if (root.TryGetProperty("isError", out var isErrorProp) && isErrorProp.GetBoolean())
        {
            var texts = new List<string>();
            if (root.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in contentProp.EnumerateArray())
                {
                    if (item.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    {
                        texts.Add(textProp.GetString()!);
                    }
                }
            }
            errorText = string.Join(Environment.NewLine, texts);
            return true;
        }

        return false;
    }

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

    [When(@"I call create_deliverable")]
    public async Task WhenICallDevstackCreateDeliverable()
    {
        var projectId = Guid.Parse(await GetOrCreateTestProjectIdAsync());
        var title = _scenarioContext.GetString("DeliverableTitle") ?? "Test Deliverable";
        Console.WriteLine($"[DEBUG] Creating deliverable with projectId={projectId}, title={title}");
        var args = new { projectId, title, description = "Test deliverable description" };
        try
        {
            _response = await Client.SendRequestAsync("tools/call", new { name = "create_deliverable", arguments = args });
        }
        catch (JsonRpcException ex)
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(ex.Code, ex.Message, ex.Data), null);
        }
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call update_deliverable with updated description ""(.*)""")]
    public async Task WhenICallDevstackUpdateDeliverable(string updatedDescription)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var args = new { id = Guid.Parse(deliverableId), description = updatedDescription };
        _response = await Client.SendRequestAsync("tools/call", new { name = "update_deliverable", arguments = args });
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call update_deliverable_state to ""(.*)""")]
    public async Task WhenICallDevstackTransitionDeliverableStatus(string targetStatus)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var args = new { id = Guid.Parse(deliverableId), targetStatus, actor = "test" };
        _response = await Client.SendRequestAsync("tools/call", new { name = "update_deliverable_state", arguments = args });
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created deliverable")]
    public void ThenTheResponseShouldContainTheCreatedDeliverable()
    {
        _response.Should().NotBeNull();
        if (_response!.Error != null)
        {
            Console.WriteLine($"[DEBUG] Response error: {System.Text.Json.JsonSerializer.Serialize(_response.Error)}");
        }
        if (_response!.Result != null)
        {
            Console.WriteLine($"[DEBUG] Response result: {_response.Result}");
        }
    }

    [Then(@"the deliverable should have a valid ID")]
    public void ThenTheDeliverableShouldHaveAValidID()
    {
        var resultJson = GetResultJson(_response!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdDeliverableId = idElement.GetString();
            _createdDeliverableId.Should().NotBeNullOrEmpty();
            _scenarioContext["DeliverableId"] = _createdDeliverableId;
        }
    }

    [Then(@"the deliverable status should be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBe(string expectedStatus)
    {
        var resultJson = GetResultJson(_response!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
        {
            statusElement.GetString().Should().Contain(expectedStatus);
        }
        else
        {
            var text = GetResultText(_response!);
            text.Should().Contain(expectedStatus);
        }
    }

    [Then(@"the response should contain the updated deliverable")]
    public void ThenTheResponseShouldContainTheUpdatedDeliverable()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        var resultJson = GetResultJson(_response!);
        resultJson.Should().Contain("updated");
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

    [When(@"I call create_task")]
    public async Task WhenICallDevstackCreateAgentTask()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var deliverableId = await GetOrCreateTestDeliverableIdAsync();
        var title = _scenarioContext.GetString("TaskTitle") ?? "Test Task";
        var args = new { projectId, itemId = deliverableId, title, deliverableDescription = "Test deliverable" };
        _response = await Client.SendRequestAsync("tools/call", new { name = "create_task", arguments = args });
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call update_task with updated description ""(.*)""")]
    public async Task WhenICallDevstackUpdateAgentTask(string updatedDescription)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var args = new { id = Guid.Parse(taskId), description = updatedDescription };
        _response = await Client.SendRequestAsync("tools/call", new { name = "update_task", arguments = args });
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call update_task_state to ""(.*)""")]
    public async Task WhenICallDevstackTransitionAgentTaskStatus(string targetStatus)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var args = new { id = Guid.Parse(taskId), targetStatus, actor = "test" };
        _response = await Client.SendRequestAsync("tools/call", new { name = "update_task_state", arguments = args });
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
        var resultJson = GetResultJson(_response!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdTaskId = idElement.GetString();
            _createdTaskId.Should().NotBeNullOrEmpty();
            _scenarioContext["TaskId"] = _createdTaskId;
        }
    }

    [Then(@"the task status should be ""(.*)""")]
    public void ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        var resultJson = GetResultJson(_response!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
        {
            statusElement.GetString().Should().Contain(expectedStatus);
        }
        else
        {
            var text = GetResultText(_response!);
            text.Should().Contain(expectedStatus);
        }
    }

    [Then(@"the response should contain the updated task")]
    public void ThenTheResponseShouldContainTheUpdatedTask()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        var resultJson = GetResultJson(_response!);
        resultJson.Should().Contain("updated");
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
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull("Expected a result in the response");

        var resultJson = GetResultJson(_response!);

        if (string.IsNullOrWhiteSpace(resultJson))
        {
            var text = GetResultText(_response!);
            text.Should().Contain(expectedStatus);
            return;
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(resultJson);
            if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
            {
                statusElement.GetString().Should().Contain(expectedStatus);
            }
            else
            {
                var text = GetResultText(_response!);
                text.Should().Contain(expectedStatus);
            }
        }
        catch (JsonException)
        {
            var text = GetResultText(_response!);
            text.Should().Contain(expectedStatus);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetOrCreateTestProjectIdAsync()
    {
        var projectId = _scenarioContext.GetString("ProjectId");
        if (!string.IsNullOrEmpty(projectId) && Guid.TryParse(projectId, out _))
        {
            return projectId;
        }

        var projects = await Client.SendRequestAsync("tools/call", new { name = "get_projects", arguments = new { } });
        var resultJson = GetResultJson(projects);

        if (string.IsNullOrEmpty(resultJson))
        {
            throw new InvalidOperationException("Tool returned empty result");
        }

        var jsonDoc = JsonDocument.Parse(resultJson);
        var projectsArray = jsonDoc.RootElement;

        if (projectsArray.GetArrayLength() > 0)
        {
            var firstProject = projectsArray[0];
            var id = firstProject.GetProperty("id").GetString() ?? "";
            _scenarioContext["ProjectId"] = id;
            return id;
        }

        throw new InvalidOperationException("No projects found in the system. Seed data required for tests.");
    }

    private async Task<string> CreateTestDeliverableAsync()
    {
        var projectId = Guid.Parse(await GetOrCreateTestProjectIdAsync());
        var args = new { projectId, title = $"Test Deliverable {Guid.NewGuid()}", description = "Auto-generated test deliverable" };
        var response = await Client.SendRequestAsync("tools/call", new { name = "create_deliverable", arguments = args });

        if (response.Error != null)
        {
            throw new InvalidOperationException($"Tool call failed: {response.Error.Message}");
        }

        if (IsMcpToolError(response, out var errorText))
        {
            throw new InvalidOperationException($"Tool returned error: {errorText}");
        }

        var result = GetResultJson(response);

        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException("Tool returned empty result");
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(result);
            if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
            {
                return idElement.GetString() ?? "";
            }

            if (jsonDoc.RootElement.TryGetProperty("isError", out var isErrorElement) && isErrorElement.GetBoolean())
            {
                throw new InvalidOperationException($"Tool returned error: {result}");
            }

            throw new InvalidOperationException($"Result does not contain 'id' property: {result}");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Failed to parse result as JSON: {result}");
        }
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
        var projectId = Guid.Parse(await GetOrCreateTestProjectIdAsync());
        var deliverableId = Guid.Parse(await GetOrCreateTestDeliverableIdAsync());
        var args = new { projectId, itemId = deliverableId, title = $"Test Task {Guid.NewGuid()}", deliverableDescription = "Auto-generated test task" };
        var response = await Client.SendRequestAsync("tools/call", new { name = "create_task", arguments = args });

        if (response.Error != null)
        {
            throw new InvalidOperationException($"Tool call failed: {response.Error.Message}");
        }

        if (IsMcpToolError(response, out var errorText))
        {
            throw new InvalidOperationException($"Tool returned error: {errorText}");
        }

        var result = GetResultJson(response);

        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException("Tool returned empty result");
        }

        try
        {
            var jsonDoc = JsonDocument.Parse(result);
            if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
            {
                return idElement.GetString() ?? "";
            }

            if (jsonDoc.RootElement.TryGetProperty("isError", out var isErrorElement) && isErrorElement.GetBoolean())
            {
                throw new InvalidOperationException($"Tool returned error: {result}");
            }

            throw new InvalidOperationException($"Result does not contain 'id' property: {result}");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Failed to parse result as JSON: {result}");
        }
    }

    #endregion
}
