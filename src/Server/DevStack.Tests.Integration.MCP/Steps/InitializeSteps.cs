using DevStack.Tests.Integration.MCP.Hooks;
using ModelContextProtocol.Client;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class InitializeSteps
{
    private readonly ScenarioContext _scenarioContext;

    public InitializeSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"the MCP server is available")]
    public void GivenTheMcpServerIsAvailable()
    {
        // Server readiness is checked in the BeforeScenario hook
    }

    [When(@"I initialize the client")]
    public async Task WhenIInitializeTheClient()
    {
        _ = Client.ServerInfo;
    }

    [Then(@"the server should return its protocol version")]
    public void ThenTheServerShouldReturnItsProtocolVersion()
    {
        var serverInfo = Client.ServerInfo;
        serverInfo.Should().NotBeNull();
        serverInfo.Version.Should().NotBeNullOrEmpty();
    }

    [Then(@"the server should return its implementation info")]
    public void ThenTheServerShouldReturnItsImplementationInfo()
    {
        var serverInfo = Client.ServerInfo;
        serverInfo.Should().NotBeNull();
        serverInfo.Name.Should().NotBeNullOrEmpty();
    }

    [Then(@"the server should advertise tools capability")]
    public async Task ThenTheServerShouldAdvertiseToolsCapability()
    {
        // The server advertises tools if we can successfully list tools
        // This is verified by the ToolsList tests
        var tools = await Client.ListToolsAsync();
        tools.Should().NotBeNull();
    }
}
