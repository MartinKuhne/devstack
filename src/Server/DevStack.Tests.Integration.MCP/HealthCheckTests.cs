using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;
using DevStack.Tests.Integration.Shared;

using Xunit;

namespace DevStack.Tests.Integration.MCP;

public class HealthCheckTests
{
    /// <summary>Verifies the API health and readiness endpoints return a healthy status.</summary>
    [Theory]
    [InlineData("/health")]
    [InlineData("/ready")]
    public async Task GetHealthEndpoints_ReturnHealthy(string path)
    {
        var env = SpecFlowHooks.GetTestEnvironment();

        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(env.AppUrl)
        };

        var response = await httpClient.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(content);
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
    }
}
