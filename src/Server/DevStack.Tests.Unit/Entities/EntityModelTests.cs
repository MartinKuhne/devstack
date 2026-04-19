using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using Xunit;

namespace DevStack.Tests.Unit.Entities;

public class ProjectTests
{
    [Fact]
    public void Project_Creation_Sets_Default_Values()
    {
        var project = new Project();
        
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.NotNull(project.Name);
        Assert.Equal(string.Empty, project.Name);
        Assert.NotNull(project.Memory);
        Assert.Equal(string.Empty, project.Memory);
        Assert.NotNull(project.Items);
        Assert.Null(project.Description);
        Assert.Null(project.Architecture);
        Assert.Null(project.GithubUrl);
        Assert.Null(project.GithubToken_Encrypted);
    }

    [Fact]
    public void Project_Name_Is_Required()
    {
        var project = new Project();
        Assert.NotNull(project.Name);
    }

    [Fact]
    public void Project_Memory_Is_Required()
    {
        var project = new Project();
        Assert.NotNull(project.Memory);
    }

    [Fact]
    public void Project_GithubToken_Encrypted_Is_Nullable()
    {
        var project = new Project();
        Assert.Null(project.GithubToken_Encrypted);
    }
}

public class LargeLanguageModelTests
{
    [Fact]
    public void LargeLanguageModel_Creation_Sets_Default_Values()
    {
        var config = new LargeLanguageModel();
        
        Assert.NotEqual(Guid.Empty, config.Id);
        Assert.NotNull(config.Url);
        Assert.Equal(string.Empty, config.Url);
        Assert.NotNull(config.Model);
        Assert.Equal(string.Empty, config.Model);
        Assert.NotNull(config.ApiKey_Encrypted);
        Assert.Equal(string.Empty, config.ApiKey_Encrypted);
        Assert.Equal(0, config.MaxComplexity);
    }

    [Fact]
    public void LargeLanguageModel_Has_Required_Fields()
    {
       var config = new LargeLanguageModel();
        
        Assert.NotNull(config.Url);
        Assert.NotNull(config.Model);
        Assert.NotNull(config.ApiKey_Encrypted);
    }

    [Fact]
    public void LargeLanguageModel_ModelAlias_Is_Nullable()
    {
        var config = new LargeLanguageModel();
        Assert.Null(config.ModelAlias);
    }
}

public class ItemTests
{
    [Fact]
    public void Item_Creation_Inherits_From_WorkItem()
    {
        var item = new Item();
        
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.NotNull(item.Title);
       Assert.Equal(string.Empty, item.Title);
        Assert.Equal(FeatureStatus.Planning, item.Status);
    }

    [Fact]
    public void Item_Has_Nullable_Properties()
    {
        var item = new Item();
        
        Assert.Null(item.Description);
        Assert.Null(item.AcceptanceCriteria);
        Assert.Null(item.Plan);
        Assert.Null(item.SecurityImpact);
        Assert.Null(item.PerformanceImpact);
        Assert.Null(item.TestPlan);
        Assert.Null(item.DeploymentPlan);
        Assert.Null(item.OpenQuestions);
        Assert.Null(item.Result);
        Assert.Null(item.Errors);
    }
}

public class ItemDefectSubtypeTests
{
    [Fact]
    public void Item_With_DefectSubtype_Has_ParentFeatureId_Nullable()
    {
        var item = new Item { Subtype = ItemSubtype.Defect };
        Assert.Null(item.ParentFeatureId);
    }

    [Fact]
    public void Item_With_DefectSubtype_Has_Severity_Nullable()
    {
        var item = new Item { Subtype = ItemSubtype.Defect };
        Assert.Null(item.Severity);
    }
}

public class AgentTaskTests
{
    [Fact]
    public void AgentTask_Creation_Sets_Default_Values()
    {
        var task = new AgentTask();
        
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.NotNull(task.Title);
        Assert.Equal(string.Empty, task.Title);
        Assert.Equal(global::DevStack.Domain.Enums.AgentTaskStatus.Ready, task.Status);
        Assert.Equal(1, task.ComplexityRating);
    }

    [Fact]
    public void AgentTask_ComplexityRating_Can_Be_Set()
    {
        var task = new AgentTask();
        task.SetComplexityRating(5);
        Assert.Equal(5, task.ComplexityRating);
    }

    [Fact]
    public void AgentTask_ComplexityRating_Throws_On_Invalid_Value_Low()
    {
        var task = new AgentTask();
        Assert.Throws<ArgumentException>(() => task.SetComplexityRating(0));
    }

    [Fact]
    public void AgentTask_ComplexityRating_Throws_On_Invalid_Value_High()
    {
        var task = new AgentTask();
        Assert.Throws<ArgumentException>(() => task.SetComplexityRating(11));
    }

    [Fact]
    public void AgentTask_Has_Nullable_Properties()
    {
        var task = new AgentTask();
        
        Assert.Null(task.Result);
        Assert.Null(task.Errors);
        Assert.Null(task.CommitHash);
        Assert.Null(task.DependsOnDevTask);
        Assert.Null(task.PromptTokens);
        Assert.Null(task.CompletionTokens);
        Assert.Null(task.ExecutionDurationInSeconds);
        Assert.Null(task.Model);
    }
}

public class DeliverableTests
{
    [Fact]
    public void Deliverable_Creation_Sets_Default_Values()
    {
        var deliverable = new Deliverable();
        
        Assert.NotEqual(Guid.Empty, deliverable.Id);
        Assert.Equal(string.Empty, deliverable.Title);
        Assert.Equal(0, (int)deliverable.Status);
        Assert.Equal(0, (int)deliverable.Type);
        Assert.NotEqual(DateTime.MinValue, deliverable.CreatedAt);
        Assert.NotEqual(DateTime.MinValue, deliverable.UpdatedAt);
    }

    [Fact]
    public void Deliverable_Has_Required_Fields()
    {
        var deliverable = new Deliverable();
        Assert.Equal(string.Empty, deliverable.Title);
        Assert.Equal(DeliverableStatus.Draft, deliverable.Status);
        Assert.Equal(DeliverableType.Feature, deliverable.Type);
    }

    [Fact]
    public void Deliverable_Optional_Fields_Are_Nullable()
    {
        var deliverable = new Deliverable();
        Assert.Null(deliverable.Description);
        Assert.Null(deliverable.AcceptanceCriteria);
        Assert.Null(deliverable.Plan);
        Assert.Null(deliverable.Result);
        Assert.Null(deliverable.Errors);
    }
}
