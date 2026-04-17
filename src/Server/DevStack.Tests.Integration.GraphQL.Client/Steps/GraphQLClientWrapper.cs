using System;
using System.Threading.Tasks;
using DevStack.Client;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

public class GraphQLClientWrapper
{
    private readonly IDevStackClient _client;
    private readonly string _graphQlUrl;

    public GraphQLClientWrapper(IDevStackClient client, string graphQlUrl)
    {
        _client = client;
        _graphQlUrl = graphQlUrl;
    }

    public static IServiceProvider CreateServiceProvider(string graphQlUrl)
    {
        var services = new ServiceCollection();
        services
            .AddDevStackClient()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphQlUrl));

        return services.BuildServiceProvider();
    }
}