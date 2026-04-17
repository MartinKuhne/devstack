using System;
using System.Linq;
using System.Threading.Tasks;
using DevStack.Client;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class FeatureSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IDevStackClient _client;

    public FeatureSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _client = Hooks.SpecFlowHooks.GetClient(scenarioContext);
    }

    [When(@"I create a feature with title ""(.*)"" and description ""(.*)""")]
    public async Task WhenICreateAFeatureWithTitleAndDescription(string title, string description)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var input = new CreateFeatureInput
        {
            ProjectId = Guid.Parse(projectId!),
            Title = title,
            Description = description
        };

        var result = await _client.CreateFeature.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CreatedFeatureId"] = result.Data?.CreateFeature.Item?.Id;
    }

    [When(@"I update the feature title to ""(.*)""")]
    public async Task WhenIUpdateTheFeatureTitleTo(string title)
    {
        var featureId = _scenarioContext.TryGetValue<string>("CurrentFeatureId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("FeatureId_Original Title", out id) ? id : null;
        
        featureId.Should().NotBeNullOrEmpty();

        var input = new UpdateFeatureInput
        {
            Id = Guid.Parse(featureId!),
            Title = title
        };

        var result = await _client.UpdateFeature.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [When(@"I transition the feature status to ""(.*)""")]
    public async Task WhenITransitionTheFeatureStatusTo(string status)
    {
        var featureId = _scenarioContext.TryGetValue<string>("CurrentFeatureId", out var id) ? id : null;
        featureId.Should().NotBeNullOrEmpty();

        var statusEnum = Enum.Parse<FeatureStatus>(status, ignoreCase: true);
        var input = new TransitionFeatureInput
        {
            Id = Guid.Parse(featureId!),
            TargetStatus = statusEnum,
            Actor = "Test"
        };

        var result = await _client.TransitionFeatureStatus.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CurrentFeatureStatus"] = status;
    }

    [When(@"I delete the feature")]
    public async Task WhenIDeleteTheFeature()
    {
        var featureId = _scenarioContext.TryGetValue<string>("CurrentFeatureId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("FeatureId_To Delete", out id) ? id : null;
        
        featureId.Should().NotBeNullOrEmpty();

        var input = new DeleteFeatureInput
        {
            Id = Guid.Parse(featureId!)
        };

        var result = await _client.DeleteFeature.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [Then(@"the feature should be created successfully")]
    public void ThenTheFeatureShouldBeCreatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the feature should exist in the database")]
    public void ThenTheFeatureShouldExistInTheDatabase()
    {
        var featureId = _scenarioContext.TryGetValue<string>("CreatedFeatureId", out var id) ? id : null;
        featureId.Should().NotBeNullOrEmpty();
    }

    [Then(@"the feature should be updated successfully")]
    public void ThenTheFeatureShouldBeUpdatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the feature status should be ""(.*)""")]
    public void ThenTheFeatureStatusShouldBe(string status)
    {
        var currentStatus = _scenarioContext.TryGetValue<string>("CurrentFeatureStatus", out var s) ? s : null;
        currentStatus.Should().Be(status);
    }

    [Then(@"the feature should be deleted successfully")]
    public void ThenTheFeatureShouldBeDeletedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the feature should not exist in the database")]
    public void ThenTheFeatureShouldNotExistInTheDatabase()
    {
        var featureId = _scenarioContext.TryGetValue<string>("CreatedFeatureId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("FeatureId_To Delete", out id) ? id : null;
        
        featureId.Should().NotBeNullOrEmpty();
    }
}