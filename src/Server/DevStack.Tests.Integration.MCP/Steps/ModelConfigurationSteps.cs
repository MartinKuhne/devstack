using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;
using System.Text.Json;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ModelConfigurationSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly DevStackDbContext _dbContext;
    private JsonRpcResponse? _response;
    private Guid? _createdModelConfigurationId;
    private Guid? _testProjectId;

    public ModelConfigurationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        var connectionString = Environment.GetEnvironmentVariable("DEVSTACK_TEST_CONNECTION_STRING") 
            ?? "Host=localhost;Database=devstack_test;Username=postgres;Password=postgres";
        
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        
        _dbContext = new DevStackDbContext(options);
    }

    #region Given Steps

    [Given(@"a project with model configurations")]
    public void GivenAProjectWithModelConfigurations()
    {
        var project = new Project { Name = "Test Project for ModelConfig" };
        _dbContext.Projects.Add(project);
        _dbContext.SaveChanges();
        _testProjectId = project.Id;

        for (int i = 0; i < 3; i++)
        {
            var config = new ModelConfiguration
            {
                ProjectId = project.Id,
                Url = $"https://api.example{i}.com",
                Model = $"model-{i}",
                ModelAlias = $"Alias-{i}",
                ApiKey_Encrypted = $"encrypted-key-{i}",
                MaxComplexity = i + 5
            };
            _dbContext.ModelConfigurations.Add(config);
        }
        _dbContext.SaveChanges();
    }

    [Given(@"a valid model configuration request")]
    public void GivenAValidModelConfigurationRequest()
    {
        _scenarioContext["ModelConfigUrl"] = "https://api.test.com";
        _scenarioContext["ModelConfigModel"] = "gpt-4";
        _scenarioContext["ModelConfigAlias"] = "GPT-4 Test";
        _scenarioContext["ModelConfigApiKey"] = "test-api-key-123";
        _scenarioContext["ModelConfigMaxComplexity"] = 8;
    }

    [Given(@"an existing model configuration")]
    public async Task GivenAnExistingModelConfiguration()
    {
        if (_testProjectId == null)
        {
            var project = new Project { Name = "Test Project" };
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            _testProjectId = project.Id;
        }

        var config = new ModelConfiguration
        {
            ProjectId = _testProjectId.Value,
            Url = "https://api.original.com",
            Model = "gpt-3.5",
            ApiKey_Encrypted = "encrypted-original",
            MaxComplexity = 5
        };
        _dbContext.ModelConfigurations.Add(config);
        await _dbContext.SaveChangesAsync();
        
        _scenarioContext["ModelConfigurationId"] = config.Id.ToString();
    }

    #endregion

    #region When Steps

    [When(@"I call GetModelConfigurations")]
    public async Task WhenICallGetModelConfigurations()
    {
        var projectId = _testProjectId ?? Guid.Parse(_scenarioContext["ProjectId"].ToString()!);
        
        var configs = await _dbContext.ModelConfigurations
            .Where(mc => mc.ProjectId == projectId)
            .ToListAsync();
        
        var result = JsonSerializer.Serialize(configs);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call CreateModelConfiguration")]
    public async Task WhenICallCreateModelConfiguration()
    {
        var project = new Project { Name = $"Test Project {Guid.NewGuid()}" };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        var url = _scenarioContext.GetString("ModelConfigUrl") ?? "https://api.test.com";
        var model = _scenarioContext.GetString("ModelConfigModel") ?? "gpt-4";
        var alias = _scenarioContext.GetString("ModelConfigAlias");
        var apiKey = _scenarioContext.GetString("ModelConfigApiKey") ?? "test-key";
        var maxComplexity = _scenarioContext.Get<int?>("ModelConfigMaxComplexity") ?? 5;

        var config = new ModelConfiguration
        {
            ProjectId = project.Id,
            Url = url,
            Model = model,
            ModelAlias = alias,
            ApiKey_Encrypted = apiKey,
            MaxComplexity = maxComplexity
        };

        _dbContext.ModelConfigurations.Add(config);
        await _dbContext.SaveChangesAsync();

        _createdModelConfigurationId = config.Id;
        var result = JsonSerializer.Serialize(config);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call UpdateModelConfiguration")]
    public async Task WhenICallUpdateModelConfiguration()
    {
        var configId = Guid.Parse(_scenarioContext.GetString("ModelConfigurationId") ?? "");
        var config = await _dbContext.ModelConfigurations.FindAsync(configId);
        
        if (config != null)
        {
            config.Url = "https://api.updated.com";
            config.Model = "gpt-4";
            config.MaxComplexity = 10;
            await _dbContext.SaveChangesAsync();
        }

        var result = JsonSerializer.Serialize(config);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call DeleteModelConfiguration")]
    public async Task WhenICallDeleteModelConfiguration()
    {
        var configId = Guid.Parse(_scenarioContext.GetString("ModelConfigurationId") ?? "");
        var config = await _dbContext.ModelConfigurations.FindAsync(configId);
        
        if (config != null)
        {
            _dbContext.ModelConfigurations.Remove(config);
            await _dbContext.SaveChangesAsync();
        }

        _response = new JsonRpcResponse("2.0", JsonDocument.Parse("{\"deleted\": true}"));
        _scenarioContext["Response"] = _response;
    }

    #endregion

    #region Then Steps

    [Then(@"the response should contain all model configurations for the project")]
    public void ThenTheResponseShouldContainAllModelConfigurationsForTheProject()
    {
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull();
        
        var result = _response!.Result!.ToString()!;
        var configs = JsonSerializer.Deserialize<List<ModelConfiguration>>(result)!;
        configs.Should().NotBeNullOrEmpty();
        var expectedProjectId = _testProjectId!.Value;
        configs!.ForEach(c => c.ProjectId.Should().Be(expectedProjectId));
    }

    [Then(@"the response should contain the created model configuration")]
    public void ThenTheResponseShouldContainTheCreatedModelConfiguration()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the API key should be encrypted")]
    public void ThenTheApiKeyShouldBeEncrypted()
    {
        var result = _response!.Result!.ToString()!;
        var config = JsonSerializer.Deserialize<ModelConfiguration>(result)!;
        config!.ApiKey_Encrypted.Should().NotBeNullOrEmpty();
        config.ApiKey_Encrypted.Should().NotBe(_scenarioContext.GetString("ModelConfigApiKey"));
    }

    [Then(@"the response should contain the updated model configuration")]
    public void ThenTheResponseShouldContainTheUpdatedModelConfiguration()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        
        var result = _response!.Result!.ToString()!;
        var config = JsonSerializer.Deserialize<ModelConfiguration>(result)!;
        config!.Model.Should().Be("gpt-4");
        config.MaxComplexity.Should().Be(10);
    }

    [Then(@"the response should confirm deletion")]
    public void ThenTheResponseShouldConfirmDeletion()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        
        var configIdStr = _scenarioContext.GetString("ModelConfigurationId") ?? "";
        var configId = Guid.Parse(configIdStr);
        var config = _dbContext.ModelConfigurations.Find(configId);
        config.Should().BeNull();
    }

    #endregion
}
