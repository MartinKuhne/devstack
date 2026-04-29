using DevStack.Tests.Integration.MCP.Hooks;
using ModelContextProtocol.Client;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class NotificationSteps
{
    private readonly ScenarioContext _scenarioContext;

    public NotificationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"the MCP client is connected")]
    public void GivenTheMcpClientIsConnected()
    {
        Client.Should().NotBeNull();
    }

    [When(@"I send the notifications/initialized notification")]
    public async Task WhenISendTheInitializedNotification()
    {
        await Client.SendNotificationAsync("notifications/initialized");
    }

    [Then(@"the server should accept the notification")]
    public void ThenTheServerShouldAcceptTheNotification()
    {
        // Streamable HTTP transport returns 204 No Content for notifications
        // If we get here without an exception, the notification was accepted
        _scenarioContext["NotificationSent"] = true;
    }
}
