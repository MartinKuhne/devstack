using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Features;

[Binding]
public class DefectMutationsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IntegrationTestFixture _fixture;
    private Guid? _projectId;
    private Guid? _createdDefectId;

    public DefectMutationsSteps(ScenarioContext scenarioContext, IntegrationTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable() { }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists() { }

    [BeforeScenario]
    public async Task BeforeDefectScenario()
    {
        _projectId = await _fixture.CreateTestProjectAsync("Test Project");
    }

    [When(@"I create a defect with title ""([^""]*)"" and severity ""([^""]*)""")]
    public async Task WhenICreateADefectWithTitleAndSeverity(string title, string severity)
    {
        var sev = severity switch
        {
            "Low" => Severity.Low,
            "Medium" => Severity.Medium,
            "High" => Severity.High,
            "Critical" => Severity.Critical,
            _ => Severity.Low
        };
        
        _createdDefectId = await _fixture.CreateTestDefectAsync(_projectId!.Value, null, title, sev);
    }

    [Then("the defect should be created successfully")]
    public void ThenTheDefectShouldBeCreatedSuccessfully()
    {
        _createdDefectId.Should().NotBe(Guid.Empty);
    }

    [Then("the defect should exist in the database")]
    public async Task ThenTheDefectShouldExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect.Should().NotBeNull();
        defect!.Subtype.Should().Be(ItemSubtype.Defect);
    }

    [Given(@"a defect ""([^""]*)"" exists")]
    public async Task GivenADefectExists(string title)
    {
        _createdDefectId = await _fixture.CreateTestDefectAsync(_projectId!.Value, null, title, Severity.Low);
        
        var mutation = new Mutation();
        var input = new UpdateDefectInput(_createdDefectId.Value, title, "Original description", null, null, null, null, null, null, null, Severity.Low, null);
        var handler = new UpdateDefectHandler(_fixture.CreateDbContext());
        
        await mutation.UpdateDefectAsync(input, handler, CancellationToken.None);
    }

    [When(@"I update the defect title to ""([^""]*)""")]
    public async Task WhenIUpdateTheDefectTitleTo(string newTitle)
    {
        var mutation = new Mutation();
        var input = new UpdateDefectInput(_createdDefectId!.Value, newTitle, "Updated Description", null, null, null, null, null, null, null, null, null);
        var handler = new UpdateDefectHandler(_fixture.CreateDbContext());
        
        var result = await mutation.UpdateDefectAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("updateResult", result);
    }

    [Then("the defect should be updated successfully")]
    public async Task ThenTheDefectShouldBeUpdatedSuccessfully()
    {
        var result = _scenarioContext.Get<DefectPayload>("updateResult");
        result.Errors.Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect!.Title.Should().Be("Updated Title");
    }

    [Given(@"a defect with status ""([^""]*)"" exists")]
    public async Task GivenADefectWithStatusExists(string status)
    {
        var initialStatus = status switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            _ => FeatureStatus.Planning
        };
        
        var mutation = new Mutation();
        var input = new CreateDefectInput(_projectId!.Value, null, "Test Defect", null, null, null, null, null, null, null, Severity.Low, initialStatus);
        var handler = new CreateDefectHandler(_fixture.CreateDbContext());
        
        var result = await mutation.CreateDefectAsync(input, handler, CancellationToken.None);
        result.Errors.Should().BeEmpty();
        _createdDefectId = result.Item!.Id;
    }

    [When(@"I transition the defect status to ""([^""]*)""")]
    public async Task WhenITransitionTheDefectStatusTo(string targetStatus)
    {
        var target = targetStatus switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            "ReadyForTest" => FeatureStatus.ReadyForTest,
            "Testing" => FeatureStatus.Testing,
            "Done" => FeatureStatus.Done,
            "Failed" => FeatureStatus.Failed,
            "Rejected" => FeatureStatus.Rejected,
            "InReview" => FeatureStatus.InReview,
            _ => FeatureStatus.Planning
        };
        
        await _fixture.UpdateDefectStatusAsync(_createdDefectId!.Value, target, "test-user");
    }

    [Then(@"the defect status should be ""([^""]*)""")]
    public async Task ThenTheDefectStatusShouldBe(string expectedStatus)
    {
        var expected = expectedStatus switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            "ReadyForTest" => FeatureStatus.ReadyForTest,
            "Testing" => FeatureStatus.Testing,
            "Done" => FeatureStatus.Done,
            "Failed" => FeatureStatus.Failed,
            "Rejected" => FeatureStatus.Rejected,
            "InReview" => FeatureStatus.InReview,
            _ => FeatureStatus.Planning
        };
        
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect!.Status.Should().Be(expected);
    }

    [Given(@"a defect ""([^""]*)"" exists for deletion")]
    public async Task GivenADefectExistsForDeletion(string title)
    {
        _createdDefectId = await _fixture.CreateTestDefectAsync(_projectId!.Value, null, title, Severity.Low);
    }

    [When("I delete the defect")]
    public async Task WhenIDeleteTheDefect()
    {
        var mutation = new Mutation();
        var input = new DeleteDefectInput(_createdDefectId!.Value);
        var handler = new DeleteDefectHandler(_fixture.CreateDbContext());
        
        var result = await mutation.DeleteDefectAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("deleteResult", result);
    }

    [Then("the defect should be deleted successfully")]
    public void ThenTheDefectShouldBeDeletedSuccessfully()
    {
        var result = _scenarioContext.Get<DefectPayload>("deleteResult");
        result.Errors.Should().BeEmpty();
    }

    [Then("the defect should not exist in the database")]
    public async Task ThenTheDefectShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect.Should().BeNull();
    }
}