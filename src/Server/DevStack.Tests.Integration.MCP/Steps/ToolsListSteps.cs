using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the <c>tools/list</c> protocol surface, including <c>inputSchema</c> inspection (MCP-07).
/// </summary>
[Binding]
public sealed class ToolsListSteps
{
    /// <summary>The complete set of tools the MCP server is expected to register.</summary>
    private static readonly string[] ExpectedTools =
    [
        "get_projects",
        "get_project",
        "create_project",
        "get_deliverable",
        "get_next_deliverable",
        "create_deliverable",
        "update_deliverable",
        "update_deliverable_status",
        "get_task",
        "get_next_task",
        "create_task",
        "update_task",
        "update_task_status",
    ];

    private readonly ScenarioContext _scenarioContext;
    private IList<McpClientTool>? _tools;

    public ToolsListSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    /// <summary>Returns the tool with the given name, failing the scenario when it is not advertised.</summary>
    private McpClientTool FindTool(string toolName)
    {
        _tools.Should().NotBeNull("the tool list must have been requested first");
        var tool = _tools!.SingleOrDefault(t => t.Name == toolName);
        tool.Should().NotBeNull($"tool '{toolName}' should be advertised. Found: {string.Join(", ", _tools!.Select(t => t.Name))}");
        return tool!;
    }

    /// <summary>Returns the <c>required</c> parameter names declared by a tool schema, or an empty sequence.</summary>
    private static IEnumerable<string> RequiredParameters(JsonElement schema)
    {
        if (!schema.TryGetProperty("required", out var required) || required.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return required.EnumerateArray().Select(entry => entry.GetString() ?? string.Empty);
    }

    [When(@"I request the tool list")]
    public async Task WhenIRequestTheToolList()
    {
        _tools = await Client.ListToolsAsync();
        _scenarioContext["Tools"] = _tools;
    }

    [Then(@"the response should contain a list of tools")]
    public void ThenTheResponseShouldContainAListOfTools()
    {
        _tools.Should().NotBeNull();
        _tools!.Should().NotBeEmpty();
    }

    [Then(@"the tools should be exactly the expected tool set")]
    public void ThenTheToolsShouldBeExactlyTheExpectedToolSet()
    {
        _tools.Should().NotBeNull();
        _tools!.Select(t => t.Name).Should().BeEquivalentTo(ExpectedTools);
    }

    [Then(@"the tools should include ""(.*)""")]
    public void ThenTheToolsShouldInclude(string toolName)
    {
        _tools.Should().Contain(t => t.Name == toolName, $"Tool '{toolName}' should be available");
    }

    [Then(@"each tool should have a name")]
    public void ThenEachToolShouldHaveAName()
    {
        foreach (var tool in _tools!)
        {
            tool.Name.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"each tool should have a description")]
    public void ThenEachToolShouldHaveADescription()
    {
        foreach (var tool in _tools!)
        {
            tool.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"each tool should have inputSchema")]
    public void ThenEachToolShouldHaveInputSchema()
    {
        foreach (var tool in _tools!)
        {
            var schema = tool.ProtocolTool.InputSchema;
            schema.ValueKind.Should().Be(JsonValueKind.Object, $"tool '{tool.Name}' must expose an inputSchema object");

            schema.TryGetProperty("type", out var type).Should().BeTrue($"tool '{tool.Name}' inputSchema must declare a type");
            type.GetString().Should().Be("object", $"tool '{tool.Name}' inputSchema must describe an object");

            schema.TryGetProperty("properties", out var properties).Should().BeTrue($"tool '{tool.Name}' inputSchema must declare its properties");
            properties.ValueKind.Should().Be(JsonValueKind.Object, $"tool '{tool.Name}' inputSchema properties must be an object");

            foreach (var parameter in properties.EnumerateObject())
            {
                parameter.Value.ValueKind.Should().Be(JsonValueKind.Object, $"parameter '{parameter.Name}' of tool '{tool.Name}' must be described by an object schema");
                parameter.Value.TryGetProperty("type", out _).Should().BeTrue($"parameter '{parameter.Name}' of tool '{tool.Name}' must declare a type");

                if (parameter.Value.TryGetProperty("enum", out var enumeration))
                {
                    enumeration.ValueKind.Should().Be(JsonValueKind.Array, $"the enum of parameter '{parameter.Name}' of tool '{tool.Name}' must be an array");
                    enumeration.GetArrayLength().Should().BeGreaterThan(0, $"the enum of parameter '{parameter.Name}' of tool '{tool.Name}' must not be empty");
                }
            }

            foreach (var required in RequiredParameters(schema))
            {
                properties.TryGetProperty(required, out _).Should().BeTrue(
                    $"tool '{tool.Name}' lists '{required}' as required but does not declare it in properties");
            }
        }
    }

    [Then(@"the tool ""(.*)"" should require the parameters (.*)")]
    public void ThenTheToolShouldRequireTheParameters(string toolName, string parameterList)
    {
        var tool = FindTool(toolName);
        var schema = tool.ProtocolTool.InputSchema;
        schema.TryGetProperty("properties", out var properties).Should().BeTrue();

        var expected = parameterList.Split(',').Select(name => name.Trim()).Where(name => name.Length > 0).ToArray();

        foreach (var parameter in expected)
        {
            properties.TryGetProperty(parameter, out _).Should().BeTrue(
                $"tool '{toolName}' must declare the parameter '{parameter}'. Declared: {string.Join(", ", properties.EnumerateObject().Select(p => p.Name))}");
        }

        schema.TryGetProperty("required", out _).Should().BeTrue($"tool '{toolName}' must declare a required list");
        RequiredParameters(schema).Should().BeEquivalentTo(expected, $"tool '{toolName}' must require exactly {string.Join(", ", expected)}");
    }

    [Then(@"the tool ""(.*)"" should not require any parameters")]
    public void ThenTheToolShouldNotRequireAnyParameters(string toolName)
    {
        var tool = FindTool(toolName);
        RequiredParameters(tool.ProtocolTool.InputSchema).Should().BeEmpty($"tool '{toolName}' must not declare required parameters");
    }
}
