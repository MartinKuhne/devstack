using System.Net;
using System.Text;

using DevStack.OpenCode.Client;
using DevStack.OpenCode.Options;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

public class OpenCodeClientTests
{
    private const string SampleSchema = """
    {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$ref": "#/$defs/Config",
      "allowComments": true,
      "allowTrailingCommas": true
    }
    """;

    [Fact]
    public async Task GetSchemaJsonAsync_SendsGetRequestToConfiguredUri()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };
        var options = Options.Create(new OpenCodeOptions());
        var client = new OpenCodeClient(http, options);

        var json = await client.GetSchemaJsonAsync();

        json.Should().Be(SampleSchema);
        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri.Should().Be(new Uri("https://opencode.ai/config.json"));
    }

    [Fact]
    public async Task GetSchemaJsonAsync_SetsUserAgentHeader()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };
        var options = Options.Create(new OpenCodeOptions { UserAgent = "Custom-Agent/9.9" });
        var client = new OpenCodeClient(http, options);

        await client.GetSchemaJsonAsync();

        handler.LastRequest!.Headers.UserAgent.ToString().Should().Contain("Custom-Agent/9.9");
    }

    [Fact]
    public async Task GetSchemaJsonAsync_SetsJsonAcceptHeader()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };
        var options = Options.Create(new OpenCodeOptions());
        var client = new OpenCodeClient(http, options);

        await client.GetSchemaJsonAsync();

        handler.LastRequest!.Headers.Accept.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSchemaAsync_DeserializesSchemaDocument()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };
        var options = Options.Create(new OpenCodeOptions());
        var client = new OpenCodeClient(http, options);

        var doc = await client.GetSchemaAsync();

        doc.Schema.Should().Be("https://json-schema.org/draft/2020-12/schema");
        doc.Ref.Should().Be("#/$defs/Config");
        doc.AllowComments.Should().BeTrue();
        doc.AllowTrailingCommas.Should().BeTrue();
    }

    [Fact]
    public async Task GetSchemaAsync_NonSuccessStatus_ThrowsHttpRequestException()
    {
        var handler = new RecordingHandler("not found", HttpStatusCode.NotFound);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };
        var options = Options.Create(new OpenCodeOptions());
        var client = new OpenCodeClient(http, options);

        var act = () => client.GetSchemaAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public void SchemaUri_ResolvesFromBaseAddress()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/"),
        };
        var options = Options.Create(new OpenCodeOptions { BaseUrl = new Uri("https://example.test/") });
        var client = new OpenCodeClient(http, options);

        client.SchemaUri.Should().Be(new Uri("https://example.test/config.json"));
    }

    [Fact]
    public void Constructor_NullHttp_Throws()
    {
        var act = () => new OpenCodeClient(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("http");
    }

    [Fact]
    public void Constructor_NullOptions_UsesDefaults()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://opencode.ai/"),
        };

        var act = () => new OpenCodeClient(http, options: null);

        act.Should().NotThrow();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _statusCode;

        public RecordingHandler(string body, HttpStatusCode statusCode)
        {
            _body = body;
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
