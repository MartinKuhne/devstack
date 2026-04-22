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

    public LargeLanguageModelSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    private static JsonElement GetData(JsonElement response)
    {
        if (!response.TryGetProperty("data", out var data) || data.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("GraphQL response has no data: " + response.ToString());
        }
        return data;
    }

    private static JsonElement GetMutationResult(JsonElement data, string mutationName)
    {
        if (!data.TryGetProperty(mutationName, out var result) || result.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException($"GraphQL mutation '{mutationName}' returned null: " + data.ToString());
        }
        return result;
    }

    private static JsonElement GetNonNullData(JsonElement parent, string propertyName, string mutationName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            var errors = parent.TryGetProperty("errors", out var errorsElem) && errorsElem.ValueKind != JsonValueKind.Null
                ? string.Join("; ", errorsElem.EnumerateArray().Select(e => $"{e.GetProperty("field")}: {e.GetProperty("message")}".ToString()))
                : "no errors";
            throw new InvalidOperationException($"GraphQL mutation '{mutationName}' returned null for '{propertyName}': {errors}. Full response: {parent.ToString()}");
        }
        return value;
    }

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        var data = GetData(response);
        var result = GetMutationResult(data, mutationName);
        var errors = result.GetProperty("errors");
        return errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0;
    }

    [Given(@"a large language model exists")]
    public void GivenALargeLanguageModelExists()
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url = "https://api.example.com", model = "gpt-4", modelAlias = (string?)null, apiKey = "test-key-123", maxComplexity = 10, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var llmId = GetNonNullData(GetMutationResult(GetData(result), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel").GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
    }

    [Given(@"multiple large language models exist")]
    public void GivenMultipleLargeLanguageModelsExist()
    {
        var mutation1 = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url = "https://api.example.com/1", model = "gpt-4", modelAlias = (string?)null, apiKey = "test-key-1", maxComplexity = 10, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response1 = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation1), Encoding.UTF8, "application/json")).Result;
        var content1 = response1.Content.ReadAsStringAsync().Result;
        var result1 = JsonSerializer.Deserialize<JsonElement>(content1)!;
        var llmId1 = GetNonNullData(GetMutationResult(GetData(result1), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel").GetProperty("id").ToString();

        var mutation2 = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url = "https://api.example.com/2", model = "claude-3", modelAlias = (string?)null, apiKey = "test-key-2", maxComplexity = 8, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response2 = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation2), Encoding.UTF8, "application/json")).Result;
        var content2 = response2.Content.ReadAsStringAsync().Result;
        var result2 = JsonSerializer.Deserialize<JsonElement>(content2)!;
        var llmId2 = GetNonNullData(GetMutationResult(GetData(result2), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel").GetProperty("id").ToString();

        _scenarioContext["LargeLanguageModelId"] = llmId1;
        _scenarioContext["LargeLanguageModelId2"] = llmId2;
    }

    [When(@"I create a large language model with url ""(.*)"" model ""(.*)"" api key ""(.*)"" and max complexity (.*)")]
    public void WhenICreateALargeLanguageModelWithRequiredFields(string url, string model, string apiKey, int maxComplexity)
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url, model, modelAlias = (string?)null, apiKey, maxComplexity, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var llmData = GetNonNullData(GetMutationResult(GetData(result), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel");
        var llmId = llmData.GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a large language model with url ""(.*)"" model ""(.*)"" api key ""(.*)"" max complexity (.*) and alias ""(.*)""")]
    public void WhenICreateALargeLanguageModelWithAlias(string url, string model, string apiKey, int maxComplexity, string alias)
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url, model, modelAlias = alias, apiKey, maxComplexity, maxConcurrency = (int?)null } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var llmData = GetNonNullData(GetMutationResult(GetData(result), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel");
        var llmId = llmData.GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a large language model with url ""(.*)"" model ""(.*)"" api key ""(.*)"" max complexity (.*) and max concurrency (.*)")]
    public void WhenICreateALargeLanguageModelWithConcurrency(string url, string model, string apiKey, int maxComplexity, int maxConcurrency)
    {
        var mutation = new
        {
            query = @"mutation CreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { url, model, modelAlias = (string?)null, apiKey, maxComplexity, maxConcurrency } },
            operationName = "CreateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var llmData = GetNonNullData(GetMutationResult(GetData(result), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel");
        var llmId = llmData.GetProperty("id").ToString();
        _scenarioContext["LargeLanguageModelId"] = llmId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the large language model model alias to ""(.*)""")]
    public void WhenIUpdateTheLargeLanguageModelAlias(string modelAlias)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId, url = (string?)null, model = (string?)null, modelAlias, apiKey = (string?)null, maxComplexity = (int?)null, maxConcurrency = (int?)null } },
            operationName = "UpdateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the large language model url to ""(.*)""")]
    public void WhenIUpdateTheLargeLanguageModelUrl(string url)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId, url, model = (string?)null, modelAlias = (string?)null, apiKey = (string?)null, maxComplexity = (int?)null, maxConcurrency = (int?)null } },
            operationName = "UpdateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the large language model model name to ""(.*)""")]
    public void WhenIUpdateTheLargeLanguageModelName(string model)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId, url = (string?)null, model, modelAlias = (string?)null, apiKey = (string?)null, maxComplexity = (int?)null, maxConcurrency = (int?)null } },
            operationName = "UpdateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the large language model max complexity to (.*)")]
    public void WhenIUpdateTheLargeLanguageModelMaxComplexity(int maxComplexity)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId, url = (string?)null, model = (string?)null, modelAlias = (string?)null, apiKey = (string?)null, maxComplexity, maxConcurrency = (int?)null } },
            operationName = "UpdateLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the large language model max concurrency to (.*)")]
    public void WhenIUpdateTheLargeLanguageModelMaxConcurrency(int maxConcurrency)
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId, url = (string?)null, model = (string?)null, modelAlias = (string?)null, apiKey = (string?)null, maxComplexity = (int?)null, maxConcurrency } },
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
            query = @"mutation DeleteLargeLanguageModel($input: DeleteLargeLanguageModelInput!) { deleteLargeLanguageModel(input: $input) { largeLanguageModel { id } errors { field message } } }",
            variables = new { input = new { id = llmId } },
            operationName = "DeleteLargeLanguageModel"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query the large language model by id")]
    public void WhenIQueryTheLargeLanguageModelById()
    {
        var llmId = _scenarioContext["LargeLanguageModelId"]?.ToString()!;
        var query = new
        {
            query = @"query GetLargeLanguageModelById($id: UUID!) { largeLanguageModelById(id: $id) { id url model modelAlias maxComplexity maxConcurrency } }",
            variables = new { id = llmId },
            operationName = "GetLargeLanguageModelById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query all large language models")]
    public void WhenIQueryAllLargeLanguageModels()
    {
        var query = new
        {
            query = @"query GetAllLargeLanguageModels { largeLanguageModels { id url model modelAlias } }",
            operationName = "GetAllLargeLanguageModels"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
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
        var llm = GetNonNullData(GetMutationResult(GetData(response), "createLargeLanguageModel"), "largeLanguageModel", "createLargeLanguageModel");
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

    [Then(@"the large language model should be returned with correct data")]
    public void ThenTheLargeLanguageModelShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var llm = response.GetProperty("data").GetProperty("largeLanguageModelById");
        llm.ValueKind.Should().NotBe(JsonValueKind.Null);
        var llmId = llm.GetProperty("id").ToString();
        llmId.Should().NotBeNullOrEmpty();
        llmId.Should().Be(_scenarioContext["LargeLanguageModelId"]?.ToString());
    }

    [Then(@"the large language models list should contain the created models")]
    public void ThenTheLargeLanguageModelsListShouldContainTheCreatedModels()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var llms = response.GetProperty("data").GetProperty("largeLanguageModels");
        llms.ValueKind.Should().Be(JsonValueKind.Array);
        var llmCount = llms.GetArrayLength();
        llmCount.Should().BeGreaterOrEqualTo(2, "At least 2 models should exist after creating multiple");
    }
}
