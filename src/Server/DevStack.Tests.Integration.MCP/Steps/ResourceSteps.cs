using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the <c>resources/list</c> and <c>resources/read</c> protocol surface (MCP-06).
/// </summary>
[Binding]
public sealed class ResourceSteps
{
    /// <summary>URI the MCP SDK derives for the parameterless <c>ServerInfo</c> resource.</summary>
    private const string ServerInfoUri = "resource://mcp/server_info";

    private readonly ScenarioContext _scenarioContext;
    private IList<McpClientResource>? _resources;
    private ReadResourceResult? _readResult;
    private Exception? _readError;

    public ResourceSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [When(@"I request the resource list")]
    public async Task WhenIRequestTheResourceList()
    {
        _resources = await Client.ListResourcesAsync();
        _scenarioContext["Resources"] = _resources;
    }

    [Then(@"the resources should include the server info resource")]
    public void ThenTheResourcesShouldIncludeTheServerInfoResource()
    {
        _resources.Should().NotBeNullOrEmpty("the server must advertise at least one resource");

        var serverInfo = _resources!.SingleOrDefault(r => r.Uri == ServerInfoUri);
        serverInfo.Should().NotBeNull(
            $"the server info resource '{ServerInfoUri}' should be advertised. Found: {string.Join(", ", _resources!.Select(r => r.Uri ?? "<null>"))}");
        serverInfo!.Name.Should().Be("server_info");
    }

    [When(@"I read the server info resource")]
    public async Task WhenIReadTheServerInfoResource()
    {
        _readResult = await Client.ReadResourceAsync(ServerInfoUri);
        _scenarioContext["ReadResourceResult"] = _readResult;
    }

    [Then(@"the server info resource content contains ""(.*)""")]
    public void ThenTheServerInfoResourceContentContains(string fragment)
    {
        _readResult.Should().NotBeNull();
        _readResult!.Contents.Should().NotBeEmpty("reading the server info resource must return content");

        var text = _readResult.Contents.OfType<TextResourceContents>().SingleOrDefault();
        text.Should().NotBeNull($"the server info resource should return text content, got: {string.Join(", ", _readResult.Contents.Select(c => c.GetType().Name))}");
        text!.Uri.Should().Be(ServerInfoUri);
        text.Text.Should().NotBeNullOrWhiteSpace();
        text.Text.Should().Contain(fragment);
    }

    /// <summary>Reads a URI that is not part of the resource list and records the raised error.</summary>
    [When(@"I read the unknown resource ""(.*)""")]
    public async Task WhenIReadTheUnknownResource(string uri)
    {
        try
        {
            await Client.ReadResourceAsync(uri);
        }
        catch (McpException ex)
        {
            _readError = ex;
        }
    }

    [Then(@"the server rejects the resource request with an error containing ""(.*)""")]
    public void ThenTheServerRejectsTheResourceRequestWithAnErrorContaining(string fragment)
    {
        _readError.Should().NotBeNull("reading an unknown resource URI must be rejected instead of returning content");
        _readError!.Should().BeOfType<McpProtocolException>();
        _readError!.Message.Should().Contain(fragment);
    }
}
