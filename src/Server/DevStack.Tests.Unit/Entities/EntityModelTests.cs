using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using TaskStatus = DevStack.Domain.Enums.TaskStatus;
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

public class ModelConfigurationTests
{
    [Fact]
    public void ModelConfiguration_Creation_Sets_Default_Values()
    {
        var config = new ModelConfiguration();
        
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
    public void ModelConfiguration_Has_Required_Fields()
    {
       var config = new ModelConfiguration();
        
        Assert.NotNull(config.Url);
        Assert.NotNull(config.Model);
        Assert.NotNull(config.ApiKey_Encrypted);
    }

    [Fact]
    public void ModelConfiguration_ModelAlias_Is_Nullable()
    {
        var config = new ModelConfiguration();
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
        Assert.NotNull(item.Tasks);
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

    [Fact]
    public void Item_With_DefectSubtype_Has_ParentFeature_Returns_Self()
    {
        var item = new Item { Subtype = ItemSubtype.Defect };
        Assert.NotNull(item.ParentFeature);
        Assert.Same(item, item.ParentFeature);
    }

    [Fact]
    public void Item_Has_RootCause_Nullable()
    {
        var item = new Item();
        Assert.Null(item.RootCause);
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
        Assert.Equal(TaskStatus.Planning, task.Status);
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
        
        Assert.Null(task.Deliverable);
        Assert.Null(task.AcceptanceCriteria);
        Assert.Null(task.Risks);
        Assert.Null(task.Result);
        Assert.Null(task.RequiredFollowUps);
    }
}

public class WorkflowRunTests
{
    [Fact]
    public void WorkflowRun_Creation_Sets_Default_Values()
    {
        var workflowRun = new WorkflowRun();
        
        Assert.NotEqual(Guid.Empty, workflowRun.Id);
        Assert.Equal(WorkflowType.Planner, workflowRun.WorkflowType);
        Assert.Equal(WorkflowRunStatus.Queued, workflowRun.Status);
        Assert.NotEqual(DateTime.MinValue, workflowRun.StartedAt);
        Assert.NotEqual(DateTime.MinValue, workflowRun.CreatedAt);
    }

    [Fact]
    public void WorkflowRun_Has_Nullable_Foreign_Keys()
    {
        var workflowRun = new WorkflowRun();
        Assert.Null(workflowRun.ItemId);
        Assert.Null(workflowRun.TaskId);
    }

    [Fact]
    public void WorkflowRun_Has_Nullable_Properties()
    {
        var workflowRun = new WorkflowRun();
        
        Assert.Null(workflowRun.ErrorMessage);
        Assert.Null(workflowRun.InputPayload);
        Assert.Null(workflowRun.OutputPayload);
        Assert.Null(workflowRun.CompletedAt);
    }
}

public class AuditEventTests
{
    [Fact]
    public void AuditEvent_Creation_Sets_Default_Values()
    {
        var auditEvent = new AuditEvent();
        
        Assert.NotEqual(Guid.Empty, auditEvent.Id);
        Assert.NotNull(auditEvent.EntityType);
        Assert.Equal(string.Empty, auditEvent.EntityType);
        Assert.NotNull(auditEvent.EventType);
        Assert.Equal(string.Empty, auditEvent.EventType);
        Assert.NotNull(auditEvent.Actor);
        Assert.Equal(string.Empty, auditEvent.Actor);
        Assert.NotEqual(DateTime.MinValue, auditEvent.OccurredAt);
    }

    [Fact]
    public void AuditEvent_Has_Nullable_Properties()
    {
        var auditEvent = new AuditEvent();
        
        Assert.Null(auditEvent.OldValue);
        Assert.Null(auditEvent.NewValue);
    }
}
