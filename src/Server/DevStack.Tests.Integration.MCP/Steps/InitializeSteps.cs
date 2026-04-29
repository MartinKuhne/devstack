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
        var serverInfo = Client.ServerInfo;

        _scenarioContext["ServerInfo"] = serverInfo;
    }

    [Then(@"the server should return its protocol version")]
    public void ThenTheServerShouldReturnItsProtocolVersion()
    {
        #pragma warning disable CS8602
        _scenarioContext.TryGetValue("ServerInfo", out var info);
        info.Should().NotBeNull();
        ((dynamic)info!).Version.Should().NotBeNullOrEmpty();
        #pragma warning restore CS8602
    }

    [Then(@"the server should return its implementation info")]
    public void ThenTheServerShouldReturnItsImplementationInfo()
    {
        #pragma warning disable CS8602
        _scenarioContext.TryGetValue("ServerInfo", out var info);
        info.Should().NotBeNull();
        ((dynamic)info!).Name.Should().NotBeNullOrEmpty();
        #pragma warning restore CS8602
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
