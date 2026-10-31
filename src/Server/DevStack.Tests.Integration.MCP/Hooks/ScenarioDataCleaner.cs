using Npgsql;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Removes the rows a scenario created directly over Npgsql (MCP-10).
/// </summary>
/// <remarks>
/// The MCP host has no GraphQL endpoint, so the <c>deleteTestData</c> mutation used by the GraphQL suite
/// is unavailable. Cleanup therefore deletes exactly the ids the step code registered, in an order that
/// respects the foreign keys: tasks before deliverables before projects.
/// </remarks>
internal static class ScenarioDataCleaner
{
    /// <summary>The tables to clean, children before parents.</summary>
    private static readonly string[] TablesInDeleteOrder =
    [
        "\"AgentTasks\"",
        "\"Deliverables\"",
        "\"Projects\"",
        "\"LargeLanguageModels\"",
    ];

    /// <summary>
    /// Deletes exactly the supplied ids and returns the number of removed rows.
    /// A failing table is reported but never re-thrown: cleanup must not turn a passing scenario into a failure.
    /// </summary>
    public static async Task<int> DeleteAsync(string connectionString, IReadOnlyList<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return 0;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var deleted = 0;
        foreach (var table in TablesInDeleteOrder)
        {
            try
            {
                await using var command = new NpgsqlCommand($"DELETE FROM {table} WHERE \"Id\" = ANY(@ids)", connection);
                command.Parameters.AddWithValue("ids", ids.ToArray());
                deleted += await command.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex)
            {
                Console.WriteLine($"[CLEANUP] {table}: {ex.MessageText}");
            }
        }

        return deleted;
    }
}
