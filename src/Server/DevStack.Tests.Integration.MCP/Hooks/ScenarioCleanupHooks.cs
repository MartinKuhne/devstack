using DevStack.Tests.Integration.MCP.Steps;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Deletes the rows a scenario created once the scenario has finished (MCP-10).
/// </summary>
/// <remarks>
/// Runs after the client-disposal hook of <see cref="SpecFlowHooks"/>; it talks to Postgres directly and
/// therefore does not depend on the MCP client. Only the ids registered by the step code through
/// <see cref="CreatedRowRegistry"/> are removed, so the project seeded for the whole assembly survives.
/// </remarks>
[Binding]
public sealed class ScenarioCleanupHooks
{
    private readonly ScenarioContext _scenarioContext;

    public ScenarioCleanupHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    /// <summary>Removes the rows the scenario registered, never the shared seeded project.</summary>
    [AfterScenario(Order = 10)]
    public async Task AfterScenarioAsync()
    {
        var ids = CreatedRowRegistry.Of(_scenarioContext);
        if (ids.Count == 0)
        {
            return;
        }

        try
        {
            var deleted = await ScenarioDataCleaner.DeleteAsync(
                SpecFlowHooks.GetTestEnvironment().PostgresConnectionString,
                ids);

            Console.WriteLine($"[CLEANUP] removed {deleted} of {ids.Count} scenario rows");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLEANUP] scenario cleanup failed for {ids.Count} rows: {ex.Message}");
        }
    }
}
