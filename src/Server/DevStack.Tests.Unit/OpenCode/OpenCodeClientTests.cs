using System.Net;
using System.Text;
using System.Text.Json;

using DevStack.OpenCode.Client;
using DevStack.OpenCode.Models;
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

    // ----- Schema & global -----

    [Fact]
    public async Task GetSchemaJsonAsync_SendsGetRequestToConfiguredUri()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var json = await client.GetSchemaJsonAsync();

        json.Should().Be(SampleSchema);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri.Should().Be(new Uri("https://opencode.ai/config.json"));
    }

    [Fact]
    public async Task GetSchemaJsonAsync_SetsUserAgentHeader()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions { UserAgent = "Custom-Agent/9.9" }));

        await client.GetSchemaJsonAsync();

        handler.LastRequest!.Headers.UserAgent.ToString().Should().Contain("Custom-Agent/9.9");
    }

    [Fact]
    public async Task GetSchemaAsync_DeserializesSchemaDocument()
    {
        var handler = new RecordingHandler(SampleSchema, HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var doc = await client.GetSchemaAsync();

        doc.Schema.Should().Be("https://json-schema.org/draft/2020-12/schema");
        doc.Ref.Should().Be("#/$defs/Config");
    }

    [Fact]
    public async Task GetHealthAsync_HitsGlobalHealthEndpoint()
    {
        var handler = new RecordingHandler("""{"healthy":true,"version":"1.2.3"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var health = await client.GetHealthAsync();

        health.Healthy.Should().BeTrue();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/global/health");
    }

    // ----- Config -----

    [Fact]
    public async Task GetConfigAsync_SendsGetToConfigPath()
    {
        var handler = new RecordingHandler("""{"model":"anthropic/claude"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var config = await client.GetConfigAsync();

        config.Model.Should().Be("anthropic/claude");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/config");
    }

    [Fact]
    public async Task ReplaceConfigAsync_SendsPutToConfigPath()
    {
        var handler = new RecordingHandler("""{"model":"anthropic/claude"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        await client.ReplaceConfigAsync(new OpenCodeConfig { Model = "anthropic/claude" });

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequestBody.Should().Contain("anthropic/claude");
    }

    [Fact]
    public async Task PatchConfigAsync_SendsPatch()
    {
        var handler = new RecordingHandler("""{"model":"anthropic/claude"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        using var patch = JsonDocument.Parse("""{"model":"anthropic/claude"}""");
        await client.PatchConfigAsync(patch);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Patch);
    }

    [Fact]
    public async Task Config_GetProvidersAsync_HitsConfigProvidersEndpoint()
    {
        var handler = new RecordingHandler(
            """{"providers":[{"id":"anthropic","name":"Anthropic"}],"default":{"anthropic":"claude"}}""",
            HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var list = await client.Config.GetProvidersAsync();

        list.Providers.Should().HaveCount(1);
        list.Default!["anthropic"].Should().Be("claude");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/config/providers");
    }

    // ----- Session -----

    [Fact]
    public async Task Session_ListAsync_HitsSessionPath()
    {
        var handler = new RecordingHandler("""[{"id":"s1","title":"S1"}]""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var sessions = await client.Session.ListAsync();

        sessions.Should().HaveCount(1);
        sessions[0].Id.Should().Be("s1");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/session");
    }

    [Fact]
    public async Task Session_CreateAsync_SendsPostWithBody()
    {
        var handler = new RecordingHandler("""{"id":"new","title":"Created"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var created = await client.Session.CreateAsync(new SessionCreateRequest { Title = "Created" });

        created.Id.Should().Be("new");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequestBody.Should().Contain("\"title\":\"Created\"");
    }

    [Fact]
    public async Task Session_DeleteAsync_SendsDelete_ReturnsFalseOnNotFound()
    {
        var handler = new RecordingHandler("", HttpStatusCode.NotFound);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var deleted = await client.Session.DeleteAsync("missing");

        deleted.Should().BeFalse();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
    }

    [Fact]
    public async Task Session_PromptAsync_SendsPostWithParts()
    {
        var handler = new RecordingHandler(
            """{"info":{"role":"assistant","id":"m1"},"parts":[{"type":"text","text":"Hi"}]}""",
            HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var response = await client.Session.PromptAsync("s1", new SessionPromptRequest
        {
            Parts = new[] { PartInput.Text("Hello") },
        });

        response.Info.Kind.Should().Be("assistant");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequestBody.Should().Contain("Hello");
    }

    [Fact]
    public async Task Session_ReplyToPermissionAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ok = await client.Session.ReplyToPermissionAsync("s1", "p1", new PermissionReplyRequest { Response = PermissionResponse.Once });

        ok.Should().BeTrue();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/session/s1/permissions/p1");
    }

    [Fact]
    public async Task Session_AbortAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        await client.Session.AbortAsync("s1");

        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/session/s1/abort");
    }

    [Fact]
    public async Task Session_GetMessagesAsync_HitsMessagePath()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var messages = await client.Session.GetMessagesAsync("s1");

        messages.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/session/s1/message");
    }

    // ----- Project, Path, VCS, Instance -----

    [Fact]
    public async Task Project_ListAsync_HitsProjectPath()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var projects = await client.Project.ListAsync();

        projects.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/project");
    }

    [Fact]
    public async Task Project_GetCurrentAsync_HitsCurrentPath()
    {
        var handler = new RecordingHandler("""{"id":"p1","worktree":"/tmp"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var project = await client.Project.GetCurrentAsync();

        project.Id.Should().Be("p1");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/project/current");
    }

    [Fact]
    public async Task Path_GetAsync_HitsPathEndpoint()
    {
        var handler = new RecordingHandler("""{"state":"ready","config":"opencode.json","worktree":"/tmp","directory":"/tmp"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var path = await client.Path.GetAsync();

        path.Config.Should().Be("opencode.json");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/path");
    }

    [Fact]
    public async Task Vcs_GetAsync_HitsVcsEndpoint()
    {
        var handler = new RecordingHandler("""{"branch":"main"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var vcs = await client.Vcs.GetAsync();

        vcs.Branch.Should().Be("main");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/vcs");
    }

    [Fact]
    public async Task Instance_DisposeAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        await client.Instance.DisposeAsync();

        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/instance/dispose");
    }

    // ----- Find, File -----

    [Fact]
    public async Task Find_FindFilesAsync_SendsPattern()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var files = await client.Find.FindFilesAsync("*.ts");

        files.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/find/file");
    }

    [Fact]
    public async Task Find_FindTextAsync_SendsPattern()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var matches = await client.Find.FindTextAsync("opencode");

        matches.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/find");
    }

    [Fact]
    public async Task File_ReadAsync_HitsContentPath()
    {
        var handler = new RecordingHandler("""{"type":"text","content":"hello"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var content = await client.File.ReadAsync("src/index.ts");

        content.Content.Should().Be("hello");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/file/content");
    }

    [Fact]
    public async Task File_GetStatusAsync_HitsStatusPath()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var status = await client.File.GetStatusAsync();

        status.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/file/status");
    }

    // ----- Provider, MCP, App -----

    [Fact]
    public async Task Provider_ListAsync_HitsProviderPath()
    {
        var handler = new RecordingHandler("""{"all":[],"default":{},"connected":[]}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var list = await client.Provider.ListAsync();

        list.Connected.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/provider");
    }

    [Fact]
    public async Task Provider_OAuthAuthorize_SendsPost()
    {
        var handler = new RecordingHandler("""{"url":"https://oauth","method":"auto","instructions":"go"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var auth = await client.Provider.OAuth.AuthorizeAsync("anthropic", 0);

        auth.Url.Should().Be("https://oauth");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/provider/anthropic/oauth/authorize");
    }

    [Fact]
    public async Task Mcp_GetStatusAsync_HitsMcpPath()
    {
        var handler = new RecordingHandler("""{}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var status = await client.Mcp.GetStatusAsync();

        status.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/mcp");
    }

    [Fact]
    public async Task Mcp_ConnectAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ok = await client.Mcp.ConnectAsync("devstack");

        ok.Should().BeTrue();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/mcp/devstack/connect");
    }

    [Fact]
    public async Task App_ListAgentsAsync_HitsAgentPath()
    {
        var handler = new RecordingHandler("[]", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var agents = await client.App.ListAgentsAsync();

        agents.Should().BeEmpty();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/agent");
    }

    // ----- TUI, Auth -----

    [Fact]
    public async Task Tui_AppendPromptAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        await client.Tui.AppendPromptAsync("hello");

        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/tui/append-prompt");
        handler.LastRequestBody.Should().Contain("hello");
    }

    [Fact]
    public async Task Tui_ShowToastAsync_SendsPost()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        await client.Tui.ShowToastAsync(new TuiToastRequest
        {
            Title = "Done",
            Message = "Task complete",
            Variant = "success",
        });

        handler.LastRequestBody.Should().Contain("Task complete");
    }

    [Fact]
    public async Task Auth_SetAsync_SendsPut()
    {
        var handler = new RecordingHandler("true", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var ok = await client.Auth.SetAsync("anthropic", Auth.FromApiKey("sk-test"));

        ok.Should().BeTrue();
        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequestBody.Should().Contain("sk-test");
    }

    // ----- Section management (DevStack extensions) -----

    [Fact]
    public async Task GetServerAsync_HitsServerPath()
    {
        var handler = new RecordingHandler("""{"port":4096}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var server = await client.GetServerAsync();

        server!.Port.Should().Be(4096);
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/config/server");
    }

    [Fact]
    public async Task ListAgentsAsync_ReturnsMapKeys()
    {
        var handler = new RecordingHandler(
            """{"build":{"model":"anthropic/claude"},"plan":{"model":"anthropic/claude-haiku"}}""",
            HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var names = await client.ListAgentsAsync();

        names.Should().BeEquivalentTo(new[] { "build", "plan" });
    }

    [Fact]
    public async Task GetAgentAsync_HitsAgentByNamePath()
    {
        var handler = new RecordingHandler("""{"model":"anthropic/claude"}""", HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var agent = await client.GetAgentAsync("build");

        agent!.Model.Should().Be("anthropic/claude");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/config/agent/build");
    }

    // ----- Streaming (Event) -----

    [Fact]
    public async Task Event_SubscribeAsync_StreamsSseEvents()
    {
        var sse = "data: {\"type\":\"session.idle\",\"properties\":{\"sessionID\":\"s1\"}}\n\n" +
                  "data: {\"type\":\"message.updated\",\"properties\":{\"info\":{\"role\":\"assistant\",\"id\":\"m1\"}}}\n\n";
        var bytes = Encoding.UTF8.GetBytes(sse);
        var handler = new StreamingHandler(bytes);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://opencode.ai/") };
        var client = new OpenCodeClient(http, Options.Create(new OpenCodeOptions()));

        var events = new List<SdkEvent>();
        await foreach (var evt in client.Event.SubscribeAsync())
        {
            events.Add(evt);
        }

        events.Should().HaveCount(2);
        events[0].Type.Should().Be("session.idle");
        events[1].Type.Should().Be("message.updated");
    }

    // ----- Helper -----

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
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class StreamingHandler : HttpMessageHandler
    {
        private readonly byte[] _bytes;
        public StreamingHandler(byte[] bytes) => _bytes = bytes;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(_bytes)),
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");
            return Task.FromResult(response);
        }
    }
}
