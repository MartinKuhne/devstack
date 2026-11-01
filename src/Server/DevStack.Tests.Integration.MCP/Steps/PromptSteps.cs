using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the <c>prompts/list</c> and <c>prompts/get</c> protocol surface (MCP-05).
/// </summary>
[Binding]
public sealed class PromptSteps
{
    private readonly ScenarioContext _scenarioContext;
    private IList<McpClientPrompt>? _prompts;
    private GetPromptResult? _promptResult;
    private Exception? _promptError;

    public PromptSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [When(@"I request the prompt list")]
    public async Task WhenIRequestThePromptList()
    {
        _prompts = await Client.ListPromptsAsync();
        _scenarioContext["Prompts"] = _prompts;
    }

    [Then(@"the prompts should include ""(.*)""")]
    public void ThenThePromptsShouldInclude(string promptName)
    {
        _prompts.Should().NotBeNullOrEmpty("the server must advertise at least one prompt");
        _prompts!.Should().Contain(p => p.Name == promptName, $"prompt '{promptName}' should be advertised");
    }

    [When(@"I resolve the prompt ""(.*)""")]
    public async Task WhenIResolveThePrompt(string promptName)
    {
        _promptResult = await Client.GetPromptAsync(promptName);
        _scenarioContext["PromptResult"] = _promptResult;
    }

    /// <summary>Requests a prompt that is not part of the prompt list and records the raised error.</summary>
    [When(@"I resolve the unknown prompt ""(.*)""")]
    public async Task WhenIResolveTheUnknownPrompt(string promptName)
    {
        try
        {
            await Client.GetPromptAsync(promptName);
        }
        catch (McpException ex)
        {
            _promptError = ex;
        }
    }

    [Then(@"the server rejects the prompt request with an error containing ""(.*)""")]
    public void ThenTheServerRejectsThePromptRequestWithAnErrorContaining(string fragment)
    {
        _promptError.Should().NotBeNull("requesting an unknown prompt must be rejected instead of returning a prompt");
        _promptError!.Should().BeOfType<McpProtocolException>();
        _promptError!.Message.Should().Contain(fragment);
    }

    [Then(@"the prompt returns a user message containing ""(.*)""")]
    public void ThenThePromptReturnsAUserMessageContaining(string fragment)
    {
        _promptResult.Should().NotBeNull();
        _promptResult!.Messages.Should().NotBeEmpty("the deliverable workflow prompt must return at least one message");

        var message = _promptResult.Messages[0];
        message.Role.Should().Be(Role.User);
        message.Content.Should().BeOfType<TextContentBlock>();

        var text = ((TextContentBlock)message.Content).Text;
        text.Should().NotBeNullOrWhiteSpace("the prompt message must carry the workflow guidance");
        text.Should().Contain(fragment);
    }
}
