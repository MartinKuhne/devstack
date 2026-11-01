using DevStack.Tests.Integration.Shared;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Boots the <c>DevStack.Api</c> image, which hosts the very same MCP tools under <c>/mcp</c> (MCP-09).
/// </summary>
/// <remarks>
/// The API image registers the tools without <c>McpArgumentNormalizationFilter</c> and runs EF migrations
/// at start-up, so no external seeding is required. The image tag differs from the assembly one so that
/// concurrent runs never retag each other's image.
/// </remarks>
[Binding]
public sealed class ApiHostedMcpHooks
{
    private static readonly FeatureScopedTestEnvironment Environment = new(() =>
        DevStackTestEnvFactory.CreateBuilder()
            .WithMode(DevStackTestEnvMode.Api)
            .WithEnvironmentName("Development")
            .WithApiImageName("devstack-api:lanef")
            .Build());

    /// <summary>Builds the API environment before the first scenario of the feature runs.</summary>
    [BeforeFeature("api-hosted-mcp")]
    public static void BeforeFeature()
    {
        Console.WriteLine("[MCP-09] booting DevStack.Api image hosting /mcp");
        var environment = Environment.Current;
        Console.WriteLine($"[MCP-09] API-hosted MCP environment ready on {environment.AppUrl}");
    }

    /// <summary>Tears the API environment down after the last scenario of the feature.</summary>
    [AfterFeature("api-hosted-mcp")]
    public static void AfterFeature()
    {
        Environment.Dispose();
        Console.WriteLine("[MCP-09] API-hosted MCP environment disposed");
    }

    /// <summary>The running API environment.</summary>
    public static DevStackTestEnv Current => Environment.Current;
}
