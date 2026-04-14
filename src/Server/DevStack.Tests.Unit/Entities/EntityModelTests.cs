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
        Assert.NotNull(project.Features);
        Assert.NotNull(project.Defects);
        Assert.NotNull(project.ModelConfigurations);
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

public class FeatureTests
{
    [Fact]
    public void Feature_Creation_Inherits_From_WorkItem()
    {
        var feature = new Feature();
        
        Assert.NotEqual(Guid.Empty, feature.Id);
        Assert.NotNull(feature.Title);
       Assert.Equal(string.Empty, feature.Title);
        Assert.Equal(FeatureStatus.Planning, feature.Status);
        Assert.NotNull(feature.Tasks);
    }

    [Fact]
    public void Feature_Has_Nullable_Properties()
    {
        var feature = new Feature();
        
        Assert.Null(feature.Description);
        Assert.Null(feature.AcceptanceCriteria);
        Assert.Null(feature.Plan);
        Assert.Null(feature.SecurityImpact);
        Assert.Null(feature.PerformanceImpact);
        Assert.Null(feature.TestPlan);
        Assert.Null(feature.DeploymentPlan);
        Assert.Null(feature.OpenQuestions);
        Assert.Null(feature.Result);
        Assert.Null(feature.Errors);
    }
}

public class DefectTests
{
    [Fact]
    public void Defect_Creation_Inherits_From_WorkItem()
    {
        var defect = new Defect();
        
        Assert.NotEqual(Guid.Empty, defect.Id);
        Assert.NotNull(defect.Title);
        Assert.Equal(string.Empty, defect.Title);
        Assert.Equal(FeatureStatus.Planning, defect.Status);
    }

    [Fact]
    public void Defect_Has_ParentFeatureId_Nullable()
    {
        var defect = new Defect();
        Assert.Null(defect.ParentFeatureId);
    }

    [Fact]
    public void Defect_Has_Severity_Nullable()
    {
        var defect = new Defect();
        Assert.Null(defect.Severity);
    }

    [Fact]
    public void Defect_Has_ParentFeature_Nullable()
    {
        var defect = new Defect();
        Assert.Null(defect.ParentFeature);
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
        Assert.Null(workflowRun.FeatureId);
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
