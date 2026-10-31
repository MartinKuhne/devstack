using System.Text.Json;

using DevStack.Tests.Integration.MCP.Hooks;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Collects the ids of the rows a scenario creates so that the AfterScenario hook (MCP-10) can delete
/// exactly those rows.
/// </summary>
/// <remarks>
/// The MCP host does not expose the GraphQL <c>deleteTestData</c> mutation, so cleanup has to be driven
/// from the ids the step code received back in the tool responses. The project seeded in
/// <c>BeforeTestRun</c> is shared by every scenario and is therefore never registered, even though its
/// name matches the <c>[DeleteAfterTest]</c> predicate.
/// </remarks>
internal static class CreatedRowRegistry
{
    /// <summary>The <see cref="ScenarioContext"/> key holding the list of ids created by the running scenario.</summary>
    internal const string ScenarioKey = "CreatedRowIds";

    /// <summary>Registers the id of a row the scenario has just created. Values that are not a GUID are ignored.</summary>
    public static void Register(ScenarioContext context, string? id)
    {
        if (Guid.TryParse(id, out var parsed))
        {
            Register(context, parsed);
        }
    }

    /// <summary>Registers the id of a row the scenario has just created; the shared seeded project is never registered.</summary>
    public static void Register(ScenarioContext context, Guid id)
    {
        if (id == Guid.Empty || id == SpecFlowHooks.GetSeededProjectId())
        {
            return;
        }

        if (!context.TryGetValue<List<Guid>>(ScenarioKey, out var ids))
        {
            ids = [];
            context[ScenarioKey] = ids;
        }

        if (!ids.Contains(id))
        {
            ids.Add(id);
        }
    }

    /// <summary>
    /// Registers the <c>Id</c> property of a <c>create_*</c> tool payload. Payloads that are not a JSON
    /// object carry no id and are ignored.
    /// </summary>
    public static void RegisterFromPayload(ScenarioContext context, string payloadJson)
    {
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                Register(context, McpToolResult.StringProperty(document.RootElement, "Id"));
            }
        }
        catch (JsonException)
        {
            Console.WriteLine($"[CLEANUP] tool payload carries no parsable id: {payloadJson}");
        }
    }

    /// <summary>Returns the ids registered by the scenario; empty when the scenario created nothing.</summary>
    public static IReadOnlyList<Guid> Of(ScenarioContext context)
    {
        return context.TryGetValue<List<Guid>>(ScenarioKey, out var ids) ? ids : [];
    }
}
