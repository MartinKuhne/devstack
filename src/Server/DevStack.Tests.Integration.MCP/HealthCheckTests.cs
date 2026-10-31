using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;
using DevStack.Tests.Integration.Shared;

using Xunit;

namespace DevStack.Tests.Integration.MCP;

public class HealthCheckTests
{
    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        var env = SpecFlowHooks.GetTestEnvironment();

        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(env.AppUrl)
        };

        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("healthy");
    }
}
