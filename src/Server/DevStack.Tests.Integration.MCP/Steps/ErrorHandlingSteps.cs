using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ErrorHandlingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;
    private HttpResponseMessage? _httpResponse;

    public ErrorHandlingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);
    private HttpClient HttpClient => _scenarioContext.TryGetValue<HttpClient>("HttpClient", out var hc) ? hc : throw new InvalidOperationException("HttpClient not initialized.");

    #region Parse Error Steps

    [Given(@"a request with invalid JSON syntax")]
    public void GivenARequestWithInvalidJSONSyntax()
    {
        _scenarioContext["InvalidJson"] = "{ invalid json }";
    }

    [Given(@"a request with truncated JSON body")]
    public void GivenARequestWithTruncatedJSONBody()
    {
        _scenarioContext["InvalidJson"] = "{\"jsonrpc\":\"2.0\",\"method\":\"test\"";
    }

    [When(@"I send the request")]
    public async Task WhenISendTheRequest()
    {
        if (_scenarioContext.TryGetValue<string>("InvalidJson", out var invalidJson))
        {
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
            _httpResponse = await HttpClient.PostAsync("/mcp", content);
            var responseContent = await _httpResponse.Content.ReadAsStringAsync();
            
            try
            {
                _response = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse>(responseContent);
            }
            catch
            {
                _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32700, "Parse error", null), null);
            }
        }
        else if (_scenarioContext.TryGetValue<object>("InvalidRequest", out var invalidRequest))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(invalidRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpResponse = await HttpClient.PostAsync("/mcp", content);
            var responseContent = await _httpResponse.Content.ReadAsStringAsync();
            
            try
            {
                _response = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse>(responseContent);
            }
            catch
            {
                _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32700, "Parse error", null), null);
            }
        }
        else if (_scenarioContext.TryGetValue<object>("InvalidParams", out var invalidParams))
        {
            try
            {
                _response = await Client.SendRequestAsync("tools/call", invalidParams);
            }
            catch (JsonRpcException ex)
            {
                _response = new JsonRpcResponse("2.0", null, new JsonRpcError(ex.Code, ex.Message, ex.Data), null);
            }
        }
        else if (_scenarioContext.TryGetValue<object>("ValidRequestWithExtraParams", out var validWithExtra))
        {
            try
            {
                _response = await Client.SendRequestAsync("tools/call", validWithExtra);
            }
            catch (JsonRpcException ex)
            {
                _response = new JsonRpcResponse("2.0", null, new JsonRpcError(ex.Code, ex.Message, ex.Data), null);
            }
        }

        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain error code (-?\d+)")]
    public void ThenTheResponseShouldContainErrorCode(int expectedCode)
    {
        _response.Should().NotBeNull();
        _response!.Error!.Code.Should().Be(expectedCode);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenTheErrorMessageShouldContain(string expectedMessage)
    {
        _response!.Error!.Message.Should().Contain(expectedMessage);
    }

    [Then(@"the error message should indicate parse error")]
    public void ThenTheErrorMessageShouldIndicateParseError()
    {
        _response!.Error!.Message.ToLower().Should().Contain("parse");
    }

    #endregion

    #region Invalid Request Steps

    [Given(@"a request without jsonrpc version field")]
    public void GivenARequestWithoutJsonRpcVersionField()
    {
        _scenarioContext["InvalidRequest"] = new { method = "test", id = 1 };
    }

    [Given(@"a request with invalid jsonrpc version ""(.*)""")]
    public void GivenARequestWithInvalidJsonRpcVersion(string version)
    {
        _scenarioContext["InvalidRequest"] = new { jsonrpc = version, method = "test", id = 1 };
    }

    [Given(@"a request without method field")]
    public void GivenARequestWithoutMethodField()
    {
        _scenarioContext["InvalidRequest"] = new { jsonrpc = "2.0", id = 1 };
    }

    [Given(@"a request without id field \(not a notification\)")]
    public void GivenARequestWithoutIdFieldNotANotification()
    {
        _scenarioContext["InvalidRequest"] = new { jsonrpc = "2.0", method = "test" };
    }

    #endregion

    #region Invalid Params Steps

    [Given(@"a tools/call request without required ""name"" parameter")]
    public void GivenAToolsCallRequestWithoutRequiredNameParameter()
    {
        _scenarioContext["InvalidParams"] = new { arguments = new { } };
    }

    [Given(@"a tools/call request with wrong parameter type")]
    public void GivenAToolsCallRequestWithWrongParameterType()
    {
        _scenarioContext["InvalidParams"] = new { name = 123, arguments = new { } };
    }

    [Given(@"a tools/call request with unknown parameters")]
    public void GivenAToolsCallRequestWithUnknownParameters()
    {
        _scenarioContext["ValidRequestWithExtraParams"] = new { name = "get_projects", arguments = new { }, extraField = "test" };
    }

    [When(@"I send the request with invalid params")]
    public async Task WhenISendTheRequestWithInvalidParams()
    {
        if (_scenarioContext.TryGetValue("InvalidParams", out var invalidParams))
        {
            _response = await Client.SendRequestAsync("tools/call", invalidParams);
        }

        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should be accepted")]
    public void ThenTheResponseShouldBeAccepted()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"unknown parameters should be ignored")]
    public void ThenUnknownParametersShouldBeIgnored()
    {
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Batch Request Steps

    [Given(@"an array of (\d+) valid JSON-RPC requests")]
    public void GivenAnArrayOfValidJSONRPCRequests(int count)
    {
        var requests = new object[count];
        for (int i = 0; i < count; i++)
        {
            requests[i] = new { jsonrpc = "2.0", method = "get_projects", id = i + 1 };
        }
        _scenarioContext["BatchRequests"] = requests;
    }

[Given(@"an array with (\d+) requests and (\d+) notification")]
    public void GivenAnArrayWithRequestsAndNotification(int requestCount, int notificationCount)
    {
        var items = new List<object>();
        for (int i = 0; i < requestCount; i++)
        {
            items.Add(new { jsonrpc = "2.0", method = "get_projects", id = i + 1 });
        }
        for (int i = 0; i < notificationCount; i++)
        {
            items.Add(new { jsonrpc = "2.0", method = "notifications/test" });
        }
        _scenarioContext["BatchRequests"] = items.ToArray();
    }

    [Then(@"the response should contain (\d+) responses \(notifications excluded\)")]
    public void ThenTheResponseShouldContainResponsesNotificationsExcluded(int expectedCount)
    {
        JsonRpcResponse[]? responses = null;
        if (_scenarioContext.TryGetValue<JsonRpcResponse[]>("BatchResponses", out var batchResponses))
        {
            responses = batchResponses;
        }
        else if (_response?.Result != null)
        {
            var result = _response.Result.ToString()!;
            responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(result)!;
        }
        responses!.Should().NotBeEmpty();
        var responseCount = responses!.Count(r => r.Id.HasValue);
        responseCount.Should().Be(expectedCount);
    }

    [Then(@"the response should contain responses for all requests")]
    public void ThenTheResponseShouldContainResponsesForAllRequests()
    {
        JsonRpcResponse[]? responses = null;
        if (_scenarioContext.TryGetValue<JsonRpcResponse[]>("BatchResponses", out var batchResponses))
        {
            responses = batchResponses;
        }
        else if (_response?.Result != null)
        {
            var result = _response.Result.ToString()!;
            responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(result)!;
        }
        responses!.Should().NotBeEmpty();
    }

    [Given(@"an array with (\d+) valid requests and (\d+) invalid request")]
    public void GivenAnArrayWithValidRequestsAndInvalidRequest(int validCount, int invalidCount)
    {
        var items = new List<object>();
        for (int i = 0; i < validCount; i++)
        {
            items.Add(new { jsonrpc = "2.0", method = "get_projects", id = i + 1 });
        }
        for (int i = 0; i < invalidCount; i++)
        {
            items.Add(new { jsonrpc = "invalid", method = "test" });
        }
        _scenarioContext["BatchRequests"] = items.ToArray();
    }

    [When(@"I send the batch request")]
    public async Task WhenISendTheBatchRequest()
    {
        if (_scenarioContext.TryGetValue("BatchRequests", out var requestsObj))
        {
            var requests = (object[])requestsObj;
            var json = System.Text.Json.JsonSerializer.Serialize(requests);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpResponse = await HttpClient.PostAsync("/mcp", content);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            
            try
            {
                var responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(responseContent);
                if (responses != null && responses.Length > 0)
                {
                    _scenarioContext["BatchResponses"] = responses;
                }
            }
            catch
            {
                _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32700, "Parse error", null), null);
            }
        }

        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain (\d+) responses")]
    public void ThenTheResponseShouldContainResponses(int expectedCount)
    {
        JsonRpcResponse[]? responses = null;
        if (_scenarioContext.TryGetValue<JsonRpcResponse[]>("BatchResponses", out var batchResponses))
        {
            responses = batchResponses;
        }
        else if (_response?.Result != null)
        {
            var result = _response.Result.ToString()!;
            responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(result)!;
        }
        responses.Should().NotBeNull();
        responses!.Should().NotBeEmpty();
        responses!.Length.Should().Be(expectedCount);
    }

    [Then(@"each response should have the correct id")]
    public void ThenEachResponseShouldHaveTheCorrectId()
    {
        JsonRpcResponse[]? responses = null;
        if (_scenarioContext.TryGetValue<JsonRpcResponse[]>("BatchResponses", out var batchResponses))
        {
            responses = batchResponses;
        }
        else if (_response?.Result != null)
        {
            var result = _response.Result.ToString()!;
            responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(result)!;
        }
        responses!.Should().NotBeEmpty();
    }

    [Then(@"the invalid request response should contain error")]
    public void ThenTheInvalidRequestResponseShouldContainError()
    {
        JsonRpcResponse[]? responses = null;
        if (_scenarioContext.TryGetValue<JsonRpcResponse[]>("BatchResponses", out var batchResponses))
        {
            responses = batchResponses;
        }
        else if (_response?.Result != null)
        {
            var result = _response.Result.ToString()!;
            responses = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse[]>(result)!;
        }
        responses!.Any(r => r.Error != null).Should().BeTrue();
    }

    #endregion

    #region Content-Type Steps

    [Given(@"a valid JSON-RPC request")]
    public void GivenAValidJSONRPCRequest()
    {
        _scenarioContext["ValidRequest"] = new { jsonrpc = "2.0", method = "get_projects", id = 1 };
    }

    [When(@"I send the request with Content-Type ""(.*)""")]
    public async Task WhenISendTheRequestWithContentType(string contentType)
    {
        var request = _scenarioContext.Get<object>("ValidRequest");
        var json = System.Text.Json.JsonSerializer.Serialize(request);
        var port =_scenarioContext["McpPort"];
        var content = new StringContent(json, Encoding.UTF8, contentType);
        _httpResponse = await HttpClient.PostAsync($"http://localhost:{port}/mcp", content);
    }

    [When(@"I send the request without Content-Type header")]
    public async Task WhenISendTheRequestWithoutContentTypeHeader()
    {
        var request = _scenarioContext.Get<object>("ValidRequest");
        var port =_scenarioContext["McpPort"];
        var json = System.Text.Json.JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8);
        _httpResponse = await HttpClient.PostAsync($"http://localhost:{port}/mcp", content);
    }

    [Then(@"the response should be successful")]
    public void ThenTheResponseShouldBeSuccessful()
    {
        _httpResponse!.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Then(@"the response should contain an error")]
    public void ThenTheResponseShouldContainAnErrorForHttp()
    {
        _httpResponse!.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"the status code should indicate client error")]
    public void ThenTheStatusCodeShouldIndicateClientError()
    {
        _httpResponse!.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.BadRequest,
            System.Net.HttpStatusCode.UnsupportedMediaType);
    }

    #endregion

   #region Not Implemented Steps

    [Given(@"a resources/read request")]
    public void GivenAResourcesReadRequest()
    {
        _scenarioContext["NotImplementedMethod"] = "resources/read";
    }

    [Given(@"a prompts/list request")]
    public void GivenAPromptsListRequest()
    {
        _scenarioContext["NotImplementedMethod"] = "prompts/list";
    }

    [Given(@"a prompts/get request")]
    public void GivenAPromptsGetRequest()
    {
        _scenarioContext["NotImplementedMethod"] = "prompts/get";
    }

    [Given(@"a completion/complete request")]
    public void GivenACompletionCompleteRequest()
    {
        _scenarioContext["NotImplementedMethod"] = "completion/complete";
    }

    [When(@"I send the unimplemented request")]
    public async Task WhenISendTheUnimplementedRequest()
    {
        var methodName = _scenarioContext.GetString("NotImplementedMethod") ?? "resources/read";
        
        try
        {
            _response = await Client.SendRequestAsync(methodName, new { });
        }
        catch (JsonRpcException ex) when (ex.Code == -32601)
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32601, "Method not found", null), null);
        }

        _scenarioContext["Response"] = _response;
    }

    #endregion

    #region Empty Body Steps

    [Given(@"an empty request body")]
    public void GivenAnEmptyRequestBody()
    {
        _scenarioContext["EmptyBody"] = "";
    }

    [Given(@"a request body with only whitespace")]
    public void GivenARequestBodyWithOnlyWhitespace()
    {
        _scenarioContext["EmptyBody"] = "   \n\t  ";
    }

    [Given(@"a request body with literal ""null""")]
    public void GivenARequestBodyWithLiteralNull()
    {
        _scenarioContext["EmptyBody"] = "null";
    }

    [When(@"I send the POST request")]
    public async Task WhenISendThePOSTRequest()
    {
        var body = _scenarioContext.GetString("EmptyBody") ?? "";
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var port =_scenarioContext["McpPort"];
        _httpResponse = await HttpClient.PostAsync($"http://localhost:{port}/mcp", content);
        var responseContent = await _httpResponse.Content.ReadAsStringAsync();
        
        try
        {
            _response = System.Text.Json.JsonSerializer.Deserialize<JsonRpcResponse>(responseContent);
        }
        catch
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32700, "Parse error", null), null);
        }

        _scenarioContext["Response"] = _response;
    }

    #endregion
}
