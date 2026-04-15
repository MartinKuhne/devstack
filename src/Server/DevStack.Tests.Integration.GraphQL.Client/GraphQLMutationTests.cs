using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

public class GraphQLMutationTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public GraphQLMutationTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProject_Mutation_ShouldCreateProject()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var name = $"Test Project {Guid.NewGuid()}";
        var description = "Test description";
        var architecture = "Microservices";
        var memory = "8GB";
        var githubUrl = new Uri("https://github.com/test/repo");

        var project = new Project
        {
            Name = name,
            Description = description,
            Architecture = architecture,
            Memory = memory,
            GithubUrl = githubUrl
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be(name);
    }

    [Fact]
    public async Task UpdateProject_Mutation_ShouldUpdateProject()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project
        {
            Name = "Original Name",
            Description = "Original Description"
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        project.Name = "Updated Name";
        project.Description = "Updated Description";
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched!.Name.Should().Be("Updated Name");
        fetched.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteProject_Mutation_ShouldDeleteProject()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "To Delete" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var id = project.Id;
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateFeature_Mutation_ShouldCreateFeature()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Test Feature",
            Description = "Feature description",
            AcceptanceCriteria = "Acceptance criteria",
            Status = FeatureStatus.Planning
        };

        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Feature");
        fetched.Status.Should().Be(FeatureStatus.Planning);
    }

    [Fact]
    public async Task UpdateFeature_Mutation_ShouldUpdateFeature()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Original Title"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        feature.Title = "Updated Title";
        feature.Description = "Updated Description";
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched!.Title.Should().Be("Updated Title");
        fetched.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task TransitionFeatureStatus_Mutation_ShouldTransitionStatus()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Test Feature",
            Status = FeatureStatus.Planning
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        feature.Status = FeatureStatus.InProgress;
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched!.Status.Should().Be(FeatureStatus.InProgress);
    }

    [Fact]
    public async Task DeleteFeature_Mutation_ShouldDeleteFeature()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "To Delete"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var id = feature.Id;
        context.Features.Remove(feature);
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateDefect_Mutation_ShouldCreateDefect()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Parent Feature"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var defect = new Defect
        {
            ProjectId = project.Id,
            ParentFeatureId = feature.Id,
            Severity = Severity.High,
            Title = "Test Defect",
            Description = "Defect description",
            Status = FeatureStatus.Planning
        };

        context.Defects.Add(defect);
        await context.SaveChangesAsync();

        var fetched = await context.Defects.FindAsync(defect.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Defect");
        fetched.Severity.Should().Be(Severity.High);
    }

    [Fact]
    public async Task UpdateDefect_Mutation_ShouldUpdateDefect()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var defect = new Defect
        {
            ProjectId = project.Id,
            Title = "Original Title",
            Severity = Severity.Low
        };
        context.Defects.Add(defect);
        await context.SaveChangesAsync();

        defect.Title = "Updated Title";
        defect.Severity = Severity.Critical;
        defect.Description = "Updated Description";
        await context.SaveChangesAsync();

        var fetched = await context.Defects.FindAsync(defect.Id);
        fetched!.Title.Should().Be("Updated Title");
        fetched.Severity.Should().Be(Severity.Critical);
    }

    [Fact]
    public async Task TransitionDefectStatus_Mutation_ShouldTransitionStatus()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var defect = new Defect
        {
            ProjectId = project.Id,
            Severity = Severity.Medium,
            Title = "Test Defect",
            Status = FeatureStatus.Planning
        };
        context.Defects.Add(defect);
        await context.SaveChangesAsync();

        defect.Status = FeatureStatus.Done;
        await context.SaveChangesAsync();

        var fetched = await context.Defects.FindAsync(defect.Id);
        fetched!.Status.Should().Be(FeatureStatus.Done);
    }

    [Fact]
    public async Task DeleteDefect_Mutation_ShouldDeleteDefect()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var defect = new Defect
        {
            ProjectId = project.Id,
            Severity = Severity.Low,
            Title = "To Delete"
        };
        context.Defects.Add(defect);
        await context.SaveChangesAsync();

        var id = defect.Id;
        context.Defects.Remove(defect);
        await context.SaveChangesAsync();

        var fetched = await context.Defects.FindAsync(id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateTask_Mutation_ShouldCreateTask()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Parent Feature"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            FeatureId = feature.Id,
            Title = "Test Task",
            Deliverable = "Deliverable description",
            AcceptanceCriteria = "Acceptance criteria",
            ComplexityRating = 5,
            Status = Domain.Enums.TaskStatus.Planning
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var fetched = await context.Tasks.FindAsync(task.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Task");
        fetched.ComplexityRating.Should().Be(5);
    }

    [Fact]
    public async Task UpdateTask_Mutation_ShouldUpdateTask()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Parent Feature"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            FeatureId = feature.Id,
            Title = "Original Title",
            ComplexityRating = 3
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        task.Title = "Updated Title";
        task.ComplexityRating = 7;
        task.Deliverable = "Updated Deliverable";
        await context.SaveChangesAsync();

        var fetched = await context.Tasks.FindAsync(task.Id);
        fetched!.Title.Should().Be("Updated Title");
        fetched.ComplexityRating.Should().Be(7);
    }

    [Fact]
    public async Task TransitionTaskStatus_Mutation_ShouldTransitionStatus()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Parent Feature"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            FeatureId = feature.Id,
            Title = "Test Task",
            Status = Domain.Enums.TaskStatus.Planning
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        task.Status = Domain.Enums.TaskStatus.Done;
        await context.SaveChangesAsync();

        var fetched = await context.Tasks.FindAsync(task.Id);
        fetched!.Status.Should().Be(Domain.Enums.TaskStatus.Done);
    }

    [Fact]
    public async Task DeleteTask_Mutation_ShouldDeleteTask()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Feature
        {
            ProjectId = project.Id,
            Title = "Parent Feature"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            FeatureId = feature.Id,
            Title = "To Delete"
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var id = task.Id;
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();

        var fetched = await context.Tasks.FindAsync(id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateModelConfiguration_Mutation_ShouldCreateModelConfiguration()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var modelConfig = new ModelConfiguration
        {
            ProjectId = project.Id,
            Url = "https://api.example.com",
            Model = "gpt-4",
            ModelAlias = "GPT-4",
            ApiKey_Encrypted = "encrypted_key",
            MaxComplexity = 8
        };

        context.ModelConfigurations.Add(modelConfig);
        await context.SaveChangesAsync();

        var fetched = await context.ModelConfigurations.FindAsync(modelConfig.Id);
        fetched.Should().NotBeNull();
        fetched!.Url.Should().Be("https://api.example.com");
        fetched.Model.Should().Be("gpt-4");
    }

    [Fact]
    public async Task UpdateModelConfiguration_Mutation_ShouldUpdateModelConfiguration()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var modelConfig = new ModelConfiguration
        {
            ProjectId = project.Id,
            Url = "https://api.example.com",
            Model = "gpt-3.5",
            MaxComplexity = 5
        };
        context.ModelConfigurations.Add(modelConfig);
        await context.SaveChangesAsync();

        modelConfig.Url = "https://api.newexample.com";
        modelConfig.Model = "gpt-4";
        modelConfig.MaxComplexity = 10;
        await context.SaveChangesAsync();

        var fetched = await context.ModelConfigurations.FindAsync(modelConfig.Id);
        fetched!.Url.Should().Be("https://api.newexample.com");
        fetched.Model.Should().Be("gpt-4");
        fetched.MaxComplexity.Should().Be(10);
    }

    [Fact]
    public async Task DeleteModelConfiguration_Mutation_ShouldDeleteModelConfiguration()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var modelConfig = new ModelConfiguration
        {
            ProjectId = project.Id,
            Url = "https://api.example.com",
            Model = "gpt-4"
        };
        context.ModelConfigurations.Add(modelConfig);
        await context.SaveChangesAsync();

        var id = modelConfig.Id;
        context.ModelConfigurations.Remove(modelConfig);
        await context.SaveChangesAsync();

        var fetched = await context.ModelConfigurations.FindAsync(id);
        fetched.Should().BeNull();
    }
}
