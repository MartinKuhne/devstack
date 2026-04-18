using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DevStack.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace DevStack.Tests.Integration.GraphQL.Client.Hooks;

[Binding]
public sealed class SpecFlowHooks
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IConfiguration _configuration;
    private IDevStackClient? _client;
    private IServiceProvider? _serviceProvider;
    private HttpClient? _httpClient;

    private static readonly Lazy<IConfiguration> Configuration = new(() =>
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        return configuration;
    });

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _configuration = Configuration.Value;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        var graphQlUrl = _configuration["GraphQL:Url"] ?? "http://localhost:8087/graphql";
        
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

        CleanupTestDataAsync().Wait();
    }

    [AfterScenario]
    public void AfterScenario()
    {
        CleanupTestDataAsync().Wait();

        if (_httpClient is IDisposable httpDisposable)
        {
            httpDisposable.Dispose();
        }
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
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
}