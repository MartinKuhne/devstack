using System.Text.Json;
using System.Text.RegularExpressions;

using DevStack.Tests.Integration.MCP.Hooks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class DevStackToolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private CallToolResult? _result;
    private string? _createdDeliverableId;
    private string? _createdTaskId;

    public DevStackToolsSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private string GetResultText(CallToolResult callResult)
    {
        callResult.Should().NotBeNull("MCP call result should not be null");

        foreach (var content in callResult.Content)
        {
            if (content is TextContentBlock textBlock && !string.IsNullOrEmpty(textBlock.Text))
            {
                return textBlock.Text;
            }
        }

        throw new InvalidOperationException("No text content found in call result");
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

    private string GetResultJson(CallToolResult callResult)
    {
        var text = GetResultText(callResult);
        return ExtractJsonFromMarkdown(text);
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
    public async Task GivenADeliverableInStatus(string status)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId");
        if (string.IsNullOrEmpty(deliverableId))
        {
            deliverableId = await CreateTestDeliverableAsync();
            _scenarioContext["DeliverableId"] = deliverableId;
        }
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(deliverableId).ToString(),
            ["targetStatus"] = status,
            ["actor"] = "test"
        };
        var transitionResult = await Client.CallToolAsync("update_deliverable_state", args);
        if (transitionResult.IsError is true)
        {
            var text = GetResultText(transitionResult);
            Console.WriteLine($"[DEBUG] Deliverable status transition error: {text}");
        }
        _scenarioContext["DeliverableStatus"] = status;
    }

    [When(@"I call create_deliverable")]
    public async Task WhenICallDevstackCreateDeliverable()
    {
        var projectId = Guid.Parse(await GetOrCreateTestProjectIdAsync());
        var title = _scenarioContext.GetString("DeliverableTitle") ?? "Test Deliverable";
        Console.WriteLine($"[DEBUG] Creating deliverable with projectId={projectId}, title={title}");
        var args = new Dictionary<string, object?>
        {
            ["projectId"] = projectId.ToString(),
            ["title"] = title,
            ["description"] = "Test deliverable description"
        };
        _result = await Client.CallToolAsync("create_deliverable", args);
        _scenarioContext["Result"] = _result;
    }

    [When(@"I call update_deliverable with updated description ""(.*)""")]
    public async Task WhenICallDevstackUpdateDeliverable(string updatedDescription)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(deliverableId).ToString(),
            ["description"] = updatedDescription
        };
        _result = await Client.CallToolAsync("update_deliverable", args);
        _scenarioContext["Result"] = _result;
    }

    [When(@"I call update_deliverable_state to ""(.*)""")]
    public async Task WhenICallDevstackTransitionDeliverableStatus(string targetStatus)
    {
        var deliverableId = _scenarioContext.GetString("DeliverableId") ?? "";
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(deliverableId).ToString(),
            ["targetStatus"] = targetStatus,
            ["actor"] = "test"
        };
        _result = await Client.CallToolAsync("update_deliverable_state", args);
        _scenarioContext["Result"] = _result;
    }

    [Then(@"the response should contain the created deliverable")]
    public void ThenTheResponseShouldContainTheCreatedDeliverable()
    {
        _result.Should().NotBeNull();
        if (_result!.IsError is true)
        {
            var text = GetResultText(_result);
            Console.WriteLine($"[DEBUG] Result error text: {text}");
        }
        else
        {
            var text = GetResultText(_result);
            Console.WriteLine($"[DEBUG] Result text: {text}");
        }
    }

    [Then(@"the deliverable should have a valid ID")]
    public void ThenTheDeliverableShouldHaveAValidID()
    {
        var resultJson = GetResultJson(_result!);
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
        var resultJson = GetResultJson(_result!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
        {
            statusElement.GetString().Should().Contain(expectedStatus);
        }
        else
        {
            var text = GetResultText(_result!);
            text.Should().Contain(expectedStatus);
        }
    }

    [Then(@"the response should contain the updated deliverable")]
    public void ThenTheResponseShouldContainTheUpdatedDeliverable()
    {
        _result.Should().NotBeNull();
        _result!.IsError.Should().NotBeTrue();
        var resultJson = GetResultJson(_result!);
        resultJson.Should().Contain("updated");
    }

    [Then(@"the response should contain the deliverable with new status")]
    public void ThenTheResponseShouldContainTheDeliverableWithNewStatus()
    {
        _result.Should().NotBeNull();
        _result!.IsError.Should().NotBeTrue();
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
    public async Task GivenATaskInStatus(string status)
    {
        var taskId = _scenarioContext.GetString("TaskId");
        if (string.IsNullOrEmpty(taskId))
        {
            taskId = await CreateTestTaskAsync();
            _scenarioContext["TaskId"] = taskId;
        }
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(taskId).ToString(),
            ["targetStatus"] = status,
            ["actor"] = "test"
        };
        var transitionResult = await Client.CallToolAsync("update_task_state", args);
        if (transitionResult.IsError is true)
        {
            var text = GetResultText(transitionResult);
            Console.WriteLine($"[DEBUG] Task status transition error: {text}");
        }
        _scenarioContext["TaskStatus"] = status;
    }

    [When(@"I call create_task")]
    public async Task WhenICallDevstackCreateAgentTask()
    {
        var projectId = await GetOrCreateTestProjectIdAsync();
        var deliverableId = await GetOrCreateTestDeliverableIdAsync();
        var title = _scenarioContext.GetString("TaskTitle") ?? "Test Task";
        var args = new Dictionary<string, object?>
        {
            ["projectId"] = projectId,
            ["itemId"] = deliverableId,
            ["title"] = title,
            ["deliverableDescription"] = "Test deliverable"
        };
        _result = await Client.CallToolAsync("create_task", args);
        _scenarioContext["Result"] = _result;
    }

    [When(@"I call update_task with updated description ""(.*)""")]
    public async Task WhenICallDevstackUpdateAgentTask(string updatedDescription)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(taskId).ToString(),
            ["description"] = updatedDescription
        };
        _result = await Client.CallToolAsync("update_task", args);
        _scenarioContext["Result"] = _result;
    }

    [When(@"I call update_task_state to ""(.*)""")]
    public async Task WhenICallDevstackTransitionAgentTaskStatus(string targetStatus)
    {
        var taskId = _scenarioContext.GetString("TaskId") ?? "";
        var args = new Dictionary<string, object?>
        {
            ["id"] = Guid.Parse(taskId).ToString(),
            ["targetStatus"] = targetStatus,
            ["actor"] = "test"
        };
        _result = await Client.CallToolAsync("update_task_state", args);
        _scenarioContext["Result"] = _result;
    }

    [Then(@"the response should contain the created task")]
    public void ThenTheResponseShouldContainTheCreatedTask()
    {
        _result.Should().NotBeNull();
        _result!.IsError.Should().NotBeTrue();
    }

    [Then(@"the task should have a valid ID")]
    public void ThenTheTaskShouldHaveAValidID()
    {
        var resultJson = GetResultJson(_result!);
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
        var resultJson = GetResultJson(_result!);
        var jsonDoc = JsonDocument.Parse(resultJson);
        if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
        {
            statusElement.GetString().Should().Contain(expectedStatus);
        }
        else
        {
            var text = GetResultText(_result!);
            text.Should().Contain(expectedStatus);
        }
    }

    [Then(@"the response should contain the updated task")]
    public void ThenTheResponseShouldContainTheUpdatedTask()
    {
        _result.Should().NotBeNull();
        _result!.IsError.Should().NotBeTrue();
        var resultJson = GetResultJson(_result!);
        resultJson.Should().Contain("updated");
    }

    [Then(@"the response should contain the task with new status")]
    public void ThenTheResponseShouldContainTheTaskWithNewStatus()
    {
        _result.Should().NotBeNull();
        _result!.IsError.Should().NotBeTrue();
    }

    [Then(@"the status should be ""(.*)""")]
    public void ThenTheStatusShouldBe(string expectedStatus)
    {
        _result.Should().NotBeNull();

        var resultJson = GetResultJson(_result!);

        if (string.IsNullOrWhiteSpace(resultJson))
        {
            var text = GetResultText(_result!);
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
                var text = GetResultText(_result!);
                text.Should().Contain(expectedStatus);
            }
        }
        catch (JsonException)
        {
            var text = GetResultText(_result!);
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

        var projects = await Client.CallToolAsync("get_projects", new Dictionary<string, object?>());
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
        var args = new Dictionary<string, object?>
        {
            ["projectId"] = projectId.ToString(),
            ["title"] = $"Test Deliverable {Guid.NewGuid()}",
            ["description"] = "Auto-generated test deliverable"
        };
        var callResult = await Client.CallToolAsync("create_deliverable", args);

        if (callResult.IsError is true)
        {
            var text = GetResultText(callResult);
            throw new InvalidOperationException($"Tool returned error: {text}");
        }

        var result = GetResultJson(callResult);

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
        var args = new Dictionary<string, object?>
        {
            ["projectId"] = projectId.ToString(),
            ["itemId"] = deliverableId.ToString(),
            ["title"] = $"Test Task {Guid.NewGuid()}",
            ["deliverableDescription"] = "Auto-generated test task"
        };
        var callResult = await Client.CallToolAsync("create_task", args);

        if (callResult.IsError is true)
        {
            var text = GetResultText(callResult);
            throw new InvalidOperationException($"Tool returned error: {text}");
        }

        var result = GetResultJson(callResult);

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

            throw new InvalidOperationException($"Result does not contain 'id' property: {result}");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Failed to parse result as JSON: {result}");
        }
    }

    #endregion
}
