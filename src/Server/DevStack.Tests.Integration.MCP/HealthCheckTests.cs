using DevStack.Tests.Integration.Shared;
using FluentAssertions;
using Xunit;

namespace DevStack.Tests.Integration.MCP;

public class HealthCheckTests : IClassFixture<DevStackTestEnv>
{
    public HealthCheckTests(DevStackTestEnv env)
    {
        Env = env;
    }

    public DevStackTestEnv Env { get; }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Env.AppUrl)
        };

        var response = await httpClient.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }
}
