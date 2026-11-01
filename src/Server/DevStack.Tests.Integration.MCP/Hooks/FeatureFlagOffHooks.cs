using DevStack.Tests.Integration.Shared;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Boots a second, isolated API environment with <c>FeatureManagement:AgentTaskTools</c> switched off (MCP-08).
/// </summary>
/// <remarks>
/// The flag is read once at start-up by <c>DevStack.Api/Program.cs</c>, so it cannot be toggled on the
/// assembly-level environment; a dedicated container with its own network and Postgres is used instead.
/// A distinct explicit image name is used locally; CI's shared prebuilt API image override takes precedence.
/// </remarks>
[Binding]
public sealed class FeatureFlagOffHooks
{
    private static readonly FeatureScopedTestEnvironment Environment = new(() =>
        DevStackTestEnvFactory.CreateBuilder()
            .WithApiImageName("devstack-api:mcp-flagoff")
            .WithEnvironmentName("Development")
            .WithEnvironment("FeatureManagement__AgentTaskTools", "false")
            .Build());

    /// <summary>Builds the flag-off API environment before the feature runs.</summary>
    [BeforeFeature("feature-flag-off")]
    public static void BeforeFeature()
    {
        Console.WriteLine("[MCP-08] booting API environment with FeatureManagement__AgentTaskTools=false");
        var environment = Environment.Current;
        Console.WriteLine($"[MCP-08] flag-off API environment ready on {environment.AppUrl}");
    }

    /// <summary>Tears the flag-off environment down after the last scenario of the feature.</summary>
    [AfterFeature("feature-flag-off")]
    public static void AfterFeature()
    {
        Environment.Dispose();
        Console.WriteLine("[MCP-08] flag-off API environment disposed");
    }

    /// <summary>The running flag-off environment.</summary>
    public static DevStackTestEnv Current => Environment.Current;
}
