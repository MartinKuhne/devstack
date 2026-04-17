using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevStack.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Hooks;

[Binding]
public sealed class SpecFlowHooks
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IConfiguration _configuration;
    private IDevStackClient? _client;
    private IServiceProvider? _serviceProvider;

    public SpecFlowHooks(ScenarioContext scenarioContext, IConfiguration configuration)
    {
        _scenarioContext = scenarioContext;
        _configuration = configuration;
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

        _scenarioContext["GraphQLClient"] = _client;
        _scenarioContext["GraphQLUrl"] = graphQlUrl;
    }

    [AfterScenario]
    public void AfterScenario()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
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