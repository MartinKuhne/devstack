using System.Net.Http;
using System.Text;
using System.Text.Json;
using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

[Binding]
public sealed class LargeLanguageModelSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        var errors = response.GetProperty("data").GetProperty(mutationName).GetProperty("errors");
        return errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0;
    }

    public LargeLanguageModelSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    [When(@"I create a large language model with url ""(.*)"" and model ""(.*)"" and api key ""(.*)""")]
    public void WhenICreateALargeLanguageModel(string url, string model, string apiKey)
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors } }",
            variables = new { input = new { url, model, modelAlias = (string?)null, apiKey, maxComplexity = 10, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var llmData = result.GetProperty("data").GetProperty("createLargeLanguageModel").GetProperty("largeLanguageModel");
        var llmId = llmData.GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
        _scenarioContext["Response"] = result;
    }

    [Given(@"a large language model exists")]
    public void GivenALargeLanguageModelExists()
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors } }",
            variables = new { input = new { url = "https://api.example.com", model = "gpt-4", modelAlias = (string?)null, apiKey = "test-key-123", maxComplexity = 10, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var llmId = result.GetProperty("data").GetProperty("createLargeLanguageModel").GetProperty("largeLanguageModel").GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
    }

    [When(@"I update the large language model model alias to ""(.*)""")]
    public void WhenIUpdateTheLargeLanguageModelAlias(string modelAlias)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors } }",
            variables = new { input = new { id = llmId, url = (string?)null, model = (string?)null, modelAlias, apiKey = (string?)null, maxComplexity = (int?)null, maxConcurrency = (int?)null } },
            operationName = "UpdateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the large language model")]
    public void WhenIDeleteTheLargeLanguageModel()
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteLargeLanguageModel($input: DeleteLargeLanguageModelInput!) { deleteLargeLanguageModel(input: $input) { largeLanguageModel { id } errors } }",
            variables = new { input = new { id = llmId } },
            operationName = "DeleteLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the large language model should be created successfully")]
    public void ThenTheLargeLanguageModelShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createLargeLanguageModel").Should().BeFalse("errors should be empty");
    }

    [Then(@"the large language model should be updated successfully")]
    public void ThenTheLargeLanguageModelShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateLargeLanguageModel").Should().BeFalse("errors should be empty");
    }

    [Then(@"the large language model should be deleted successfully")]
    public void ThenTheLargeLanguageModelShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "deleteLargeLanguageModel").Should().BeFalse("errors should be empty");
    }

    [Then(@"the large language model should exist in the database")]
    public void ThenTheLargeLanguageModelShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var llm = response.GetProperty("data").GetProperty("createLargeLanguageModel").GetProperty("largeLanguageModel");
        llm.ValueKind.Should().NotBe(JsonValueKind.Null);
        var llmId = llm.GetProperty("id").ToString();
        llmId.Should().NotBeNullOrEmpty();
        _scenarioContext["LargeLanguageModelId"] = llmId;
    }

    [Then(@"the large language model should not exist in the database")]
    public void ThenTheLargeLanguageModelShouldNotExistInDatabase()
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var query = new
        {
            query = @"query GetLargeLanguageModelById($id: UUID!) { largeLanguageModelById(id: $id) { id } }",
            variables = new { id = llmId },
            operationName = "GetLargeLanguageModelById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var llm = result.GetProperty("data").GetProperty("largeLanguageModelById");
        llm.ValueKind.Should().Be(JsonValueKind.Null);
    }
}
