using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DevStack.Tests.Integration.Shared;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace DevStack.Tests.Integration.GraphQL.Client.Hooks;

[Binding]
public sealed class SpecFlowHooks : IDisposable
{
    private readonly ScenarioContext _scenarioContext;
    private HttpClient? _httpClient;
    private bool _disposed;

    private static DevStackTestEnv? _env;

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        _env = DevStackTestEnvFactory.CreateApi();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        _env?.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        if (_env is null)
        {
            throw new InvalidOperationException("DevStackTestEnv not initialized. Check BeforeTestRun.");
        }

        var graphQlUrl = $"{_env.AppUrl}/graphql";

        _httpClient = new HttpClient { BaseAddress = new Uri(graphQlUrl) };

        _scenarioContext["GraphQLUrl"] = graphQlUrl;
        _scenarioContext["HttpClient"] = _httpClient;
    }

    [AfterScenario]
    public void AfterScenario()
    {
        CleanupTestDataAsync().GetAwaiter().GetResult();

        if (_httpClient is IDisposable httpDisposable)
        {
            httpDisposable.Dispose();
        }

        _httpClient = null;
    }

    private async Task CleanupTestDataAsync()
    {
        try
        {
            var mutation = @"
                mutation CleanupTestData {
                    deleteTestData {
                        success
                        message
                    }
                }";

            var content = new StringContent(
                JsonSerializer.Serialize(new { query = mutation }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient!.PostAsync("", content);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
        }
    }

    public static string GetGraphQLUrl(ScenarioContext context)
    {
        return context.TryGetValue<string>("GraphQLUrl", out var url)
            ? url
            : throw new InvalidOperationException("GraphQL URL not configured.");
    }

    public static HttpClient GetHttpClient(ScenarioContext context)
    {
        return context.TryGetValue<HttpClient>("HttpClient", out var httpClient)
            ? httpClient
            : throw new InvalidOperationException("HttpClient not initialized. Ensure BeforeScenario hook has run.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _httpClient?.Dispose();
    }
}
