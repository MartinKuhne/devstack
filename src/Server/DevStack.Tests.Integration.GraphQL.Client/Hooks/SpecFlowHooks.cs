using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DevStack.Client;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;
using Testcontainers.PostgreSql;

namespace DevStack.Tests.Integration.GraphQL.Client.Hooks;

[Binding]
public sealed class SpecFlowHooks : IDisposable
{
    private readonly ScenarioContext _scenarioContext;
    private IDevStackClient? _client;
    private IServiceProvider? _serviceProvider;
    private HttpClient? _httpClient;
    private bool _disposed;

    public static TestcontainersModule Module => TestcontainersModule.Instance;

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Module singleton initializes containers in constructor
        _ = Module;
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        TestcontainersModule.DisposeInstance();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        var graphQlUrl = Module.GraphQlUrl
            ?? throw new InvalidOperationException("GraphQL URL not configured. TestcontainersModule initialization failed.");

        var services = new ServiceCollection();
        services
            .AddDevStackClient()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphQlUrl));

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<IDevStackClient>();
        _httpClient = new HttpClient { BaseAddress = new Uri(graphQlUrl) };

        _scenarioContext["GraphQLClient"] = _client;
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
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _httpClient = null;
        _serviceProvider = null;
        _client = null;
    }

    private async Task CleanupTestDataAsync()
    {
        try
        {
            var mutation = @"
                mutation CleanupTestData {
                    cleanupTestData {
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

    public static IDevStackClient GetClient(ScenarioContext context)
    {
        return context.TryGetValue<IDevStackClient>("GraphQLClient", out var client)
            ? client
            : throw new InvalidOperationException("GraphQL client not initialized. Ensure BeforeScenario hook has run.");
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
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
