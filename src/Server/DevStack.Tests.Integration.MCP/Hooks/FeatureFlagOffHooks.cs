using DevStack.Persistence;
using DevStack.Tests.Integration.Shared;

using Microsoft.EntityFrameworkCore;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Boots a second, isolated MCP environment with <c>FeatureManagement:AgentTaskTools</c> switched off (MCP-08).
/// </summary>
/// <remarks>
/// The flag is read once at start-up by <c>DevStack.Mcp/Program.cs</c>, so it cannot be toggled on the
/// assembly-level environment; a dedicated container with its own network and Postgres is used instead.
/// The image tag differs from the assembly one so that concurrent runs never retag each other's image.
/// </remarks>
[Binding]
public sealed class FeatureFlagOffHooks
{
    private static readonly FeatureScopedTestEnvironment Environment = new(() =>
        DevStackTestEnvFactory.CreateBuilder()
            .WithMode(DevStackTestEnvMode.Mcp)
            .WithEnvironmentName("Development")
            .WithMcpImageName("devstack-mcp:flagoff")
            .WithEnvironment("FeatureManagement__AgentTaskTools", "false")
            .Build());

    /// <summary>Builds the flag-off environment and brings its own Postgres up to date before the feature runs.</summary>
    [BeforeFeature("feature-flag-off")]
    public static async Task BeforeFeature()
    {
        Console.WriteLine("[MCP-08] booting MCP environment with FeatureManagement__AgentTaskTools=false");
        var environment = Environment.Current;

        // The MCP image never migrates on start-up, so the isolated Postgres has no tables until this runs.
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(environment.PostgresConnectionString)
            .Options;

        await using var context = new DevStackDbContext(options);
        await context.Database.MigrateAsync();

        Console.WriteLine($"[MCP-08] flag-off MCP environment ready on {environment.AppUrl}");
    }

    /// <summary>Tears the flag-off environment down after the last scenario of the feature.</summary>
    [AfterFeature("feature-flag-off")]
    public static void AfterFeature()
    {
        Environment.Dispose();
        Console.WriteLine("[MCP-08] flag-off MCP environment disposed");
    }

    /// <summary>The running flag-off environment.</summary>
    public static DevStackTestEnv Current => Environment.Current;
}
