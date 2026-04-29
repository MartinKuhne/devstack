using System.Net;
using System.Text;
using System.Text.Json;

using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class NotificationSteps
{
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _httpResponse;

    public NotificationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private HttpClient HttpClient => _scenarioContext.TryGetValue<HttpClient>("HttpClient", out var hc) ? hc : throw new InvalidOperationException("HttpClient not initialized.");

    [Given(@"a valid JSON-RPC notification")]
    public void GivenAValidJsonRpcNotification()
    {
        _scenarioContext["NotificationPayload"] = new { jsonrpc = "2.0", method = "notifications/initialized" };
    }

    [Given(@"a notification for an unimplemented method")]
    public void GivenANotificationForAnUnimplementedMethod()
    {
        _scenarioContext["NotificationPayload"] = new { jsonrpc = "2.0", method = "notifications/unimplemented" };
    }

    [When(@"I send the notification")]
    public async Task WhenISendTheNotification()
    {
        if (_scenarioContext.TryGetValue<object>("NotificationPayload", out var payload))
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var port = _scenarioContext["McpPort"];
            _httpResponse = await HttpClient.PostAsync($"http://localhost:{port}/mcp", content);
        }

        _scenarioContext["HttpResponse"] = _httpResponse;
    }

    [Then(@"the server should return HTTP 204 No Content")]
    public void ThenTheServerShouldReturnHttp204NoContent()
    {
        _httpResponse!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Then(@"the server should not send a JSON-RPC response")]
    public void ThenTheServerShouldNotSendAJsonRpcResponse()
    {
        var body = _httpResponse!.Content.ReadAsStringAsync().Result;
        body.Should().BeNullOrEmpty();
    }

    [Then(@"the server should not send an error response")]
    public void ThenTheServerShouldNotSendAnErrorResponse()
    {
        _httpResponse!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
