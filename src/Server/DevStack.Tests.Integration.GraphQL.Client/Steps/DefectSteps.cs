using System;
using System.Linq;
using System.Threading.Tasks;
using DevStack.Client;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class DefectSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IDevStackClient _client;

    public DefectSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _client = Hooks.SpecFlowHooks.GetClient(scenarioContext);
    }

    [When(@"I create a defect with title ""(.*)"" and severity ""(.*)""")]
    public async Task WhenICreateADefectWithTitleAndSeverity(string title, string severity)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        projectId.Should().NotBeNullOrEmpty();

        var input = new CreateDefectInput
        {
            ProjectId = Guid.Parse(projectId!),
            Title = title,
            Severity = Enum.Parse<Severity>(severity, ignoreCase: true)
        };

        var result = await _client.CreateDefect.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CreatedDefectId"] = result.Data?.CreateDefect.Item?.Id;
    }

    [When(@"I update the defect title to ""(.*)""")]
    public async Task WhenIUpdateTheDefectTitleTo(string title)
    {
        var defectId = _scenarioContext.TryGetValue<string>("CurrentDefectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("DefectId_Original Title", out id) ? id : null;
        
        defectId.Should().NotBeNullOrEmpty();

        var input = new UpdateDefectInput
        {
            Id = Guid.Parse(defectId!),
            Title = title
        };

        var result = await _client.UpdateDefect.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [When(@"I transition the defect status to ""(.*)""")]
    public async Task WhenITransitionTheDefectStatusTo(string status)
    {
        var defectId = _scenarioContext.TryGetValue<string>("CurrentDefectId", out var id) ? id : null;
        defectId.Should().NotBeNullOrEmpty();

        var statusEnum = Enum.Parse<FeatureStatus>(status, ignoreCase: true);
        var input = new TransitionDefectInput
        {
            Id = Guid.Parse(defectId!),
            TargetStatus = statusEnum,
            Actor = "Test"
        };

        var result = await _client.TransitionDefectStatus.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CurrentDefectStatus"] = status;
    }

    [When(@"I delete the defect")]
    public async Task WhenIDeleteTheDefect()
    {
        var defectId = _scenarioContext.TryGetValue<string>("CurrentDefectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("DefectId_To Delete", out id) ? id : null;
        
        defectId.Should().NotBeNullOrEmpty();

        var input = new DeleteDefectInput
        {
            Id = Guid.Parse(defectId!)
        };

        var result = await _client.DeleteDefect.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [Then(@"the defect should be created successfully")]
    public void ThenTheDefectShouldBeCreatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the defect should exist in the database")]
    public void ThenTheDefectShouldExistInTheDatabase()
    {
        var defectId = _scenarioContext.TryGetValue<string>("CreatedDefectId", out var id) ? id : null;
        defectId.Should().NotBeNullOrEmpty();
    }

    [Then(@"the defect should be updated successfully")]
    public void ThenTheDefectShouldBeUpdatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the defect status should be ""(.*)""")]
    public void ThenTheDefectStatusShouldBe(string status)
    {
        var currentStatus = _scenarioContext.TryGetValue<string>("CurrentDefectStatus", out var s) ? s : null;
        currentStatus.Should().Be(status);
    }

    [Then(@"the defect should be deleted successfully")]
    public void ThenTheDefectShouldBeDeletedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the defect should not exist in the database")]
    public void ThenTheDefectShouldNotExistInTheDatabase()
    {
        var defectId = _scenarioContext.TryGetValue<string>("CreatedDefectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("DefectId_To Delete", out id) ? id : null;
        
        defectId.Should().NotBeNullOrEmpty();
    }
}