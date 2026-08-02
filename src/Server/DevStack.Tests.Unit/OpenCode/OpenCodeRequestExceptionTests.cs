using System.Net;

using DevStack.OpenCode.Client;
using DevStack.OpenCode.Models;
using DevStack.OpenCode.Options;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Xunit;

namespace DevStack.Tests.Unit.OpenCode;

/// <summary>
/// Verifies that the OpenCode SDK surfaces the server's actual error message
/// and <c>ref</c> id from a non-success response, rather than throwing the
/// generic "failed with status NNN" it used to throw before the
/// <see cref="OpenCodeRequestException"/> + envelope-parser plumbing.
/// </summary>
public class OpenCodeRequestExceptionTests
{
    private const string UserObservedBody =
        """{"name":"UnknownError","data":{"message":"Unexpected server error. Check server logs for details.","ref":"err_7d6a8375"}}""";

    // ----- Pure constructor -----

    [Fact]
    public void OpenCodeRequestException_Constructor_PopulatesAllProperties()
    {
        var uri = new Uri("https://opencode.ai/session/s1/message");
        var ex = new OpenCodeRequestException(
            requestUri: uri,
            statusCode: 500,
            rawBody: "raw",
            errorMessage: "msg",
            errorRef: "err_1",
            message: "summary");

        ex.RequestUri.Should().Be(uri);
        ex.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        ex.RawBody.Should().Be("raw");
        ex.ErrorMessage.Should().Be("msg");
        ex.ErrorRef.Should().Be("err_1");
        ex.Message.Should().Be("summary");

        // Must still be an HttpRequestException for existing catch blocks.
        ex.Should().BeAssignableTo<HttpRequestException>();
    }

    [Fact]
    public void OpenCodeRequestException_Constructor_AcceptsNullMessageAndRef()
    {
        var ex = new OpenCodeRequestException(
            requestUri: new Uri("https://opencode.ai/"),
            statusCode: 502,
            rawBody: "bad gateway",
            errorMessage: null,
            errorRef: null,
            message: "summary");

        ex.ErrorMessage.Should().BeNull();
        ex.ErrorRef.Should().BeNull();
        ex.RawBody.Should().Be("bad gateway");
    }

    // ----- 5xx with full data envelope (the case the user actually hit) -----

    [Fact]
    public async Task PromptAsync_500WithDataMessageAndRef_ThrowsWithParsedEnvelope()
    {
        var handler = new ErrorBodyHandler(UserObservedBody, HttpStatusCode.InternalServerError);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var act = () => client.Session.PromptAsync("ses_03f80d31fffed7KmqHMc4YTaxd", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        });

        var ex = await act.Should().ThrowAsync<OpenCodeRequestException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        ex.Which.ErrorMessage.Should().Be("Unexpected server error. Check server logs for details.");
        ex.Which.ErrorRef.Should().Be("err_7d6a8375");
        ex.Which.RawBody.Should().Be(UserObservedBody);
        ex.Which.RequestUri.AbsolutePath.Should().Be("/session/ses_03f80d31fffed7KmqHMc4YTaxd/message");

        ex.Which.Message.Should().Contain("status 500");
        ex.Which.Message.Should().Contain("Unexpected server error");
        ex.Which.Message.Should().Contain("server ref: err_7d6a8375");
    }

    // ----- 4xx with data.message but no ref -----

    [Fact]
    public async Task PromptAsync_400WithDataMessage_ThrowsWithMessageButNoRefInSummary()
    {
        var body = """{"name":"BadRequest","data":{"message":"Missing required field","kind":"Body"}}""";
        var handler = new ErrorBodyHandler(body, HttpStatusCode.BadRequest);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ex = await client.Invoking(c => c.Session.PromptAsync("s1", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        })).Should().ThrowAsync<OpenCodeRequestException>();

        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ex.Which.ErrorMessage.Should().Be("Missing required field");
        ex.Which.ErrorRef.Should().BeNull();
        ex.Which.Message.Should().Contain("Missing required field");
        ex.Which.Message.Should().NotContain("server ref:");
    }

    // ----- Root-level message fallback -----

    [Fact]
    public async Task PromptAsync_RootLevelMessage_ThrowsWithMessage()
    {
        var body = """{"message":"Something is off"}""";
        var handler = new ErrorBodyHandler(body, HttpStatusCode.UnprocessableEntity);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ex = await client.Invoking(c => c.Session.PromptAsync("s1", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        })).Should().ThrowAsync<OpenCodeRequestException>();

        ex.Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        ex.Which.ErrorMessage.Should().Be("Something is off");
        ex.Which.ErrorRef.Should().BeNull();
        ex.Which.Message.Should().Contain("Something is off");
    }

    // ----- Empty body -----

    [Fact]
    public async Task PromptAsync_EmptyBody_ThrowsWithStatusOnlyMessage()
    {
        var handler = new ErrorBodyHandler(string.Empty, HttpStatusCode.InternalServerError);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ex = await client.Invoking(c => c.Session.PromptAsync("s1", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        })).Should().ThrowAsync<OpenCodeRequestException>();

        ex.Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        ex.Which.ErrorMessage.Should().BeNull();
        ex.Which.ErrorRef.Should().BeNull();
        ex.Which.RawBody.Should().BeEmpty();
        ex.Which.Message.Should().Contain("status 500");
        ex.Which.Message.Should().NotContain("server ref:");
    }

    // ----- Non-JSON body (e.g. a reverse-proxy HTML error page) -----

    [Fact]
    public async Task PromptAsync_NonJsonBody_ThrowsAndPreservesRawBody()
    {
        const string html = "<html><body><h1>502 Bad Gateway</h1></body></html>";
        var handler = new ErrorBodyHandler(html, HttpStatusCode.BadGateway);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ex = await client.Invoking(c => c.Session.PromptAsync("s1", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        })).Should().ThrowAsync<OpenCodeRequestException>();

        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        ex.Which.ErrorMessage.Should().BeNull();
        ex.Which.ErrorRef.Should().BeNull();
        ex.Which.RawBody.Should().Be(html);
        ex.Which.Message.Should().Contain("status 502");
    }

    // ----- Helper -----

    /// <summary>
    /// Returns a fixed body and status code for every request, so a test can
    /// assert on the exception the helper throws for that response shape.
    /// </summary>
    private sealed class ErrorBodyHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _statusCode;

        public ErrorBodyHandler(string body, HttpStatusCode statusCode)
        {
            _body = body;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, System.Text.Encoding.UTF8, "text/plain"),
                RequestMessage = request,
            });
        }
    }
}
