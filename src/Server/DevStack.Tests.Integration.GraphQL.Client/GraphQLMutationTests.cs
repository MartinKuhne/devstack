using System.Text.Json.Nodes;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<TestContainerFixture>
{
}

[Collection("Integration")]
public class GraphQLMutationTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public GraphQLMutationTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<JsonNode?> SendMutation(string query, object? variables = null)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("/graphql", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }

    [Fact]
    public async Task CreateProject_Mutation_ShouldCreateProject()
    {
        var name = $"Test Project {Guid.NewGuid()}";

        var data = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id name }
                errors
              }
            }
            """,
            new { input = new { name, description = "Test description", architecture = "Microservices", memory = "8GB", githubUrl = "https://github.com/test/repo" } });

        data!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
        var projectId = data["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Projects.FindAsync(projectId);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be(name);
    }

    [Fact]
    public async Task UpdateProject_Mutation_ShouldUpdateProject()
    {
        var createData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name = "Original Name", description = "Original Description" } });

        createData!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
        var projectId = createData["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var updateData = await SendMutation("""
            mutation UpdateProject($input: UpdateProjectInput!) {
              updateProject(input: $input) {
                project { id name }
                errors
              }
            }
            """,
            new { input = new { id = projectId, name = "Updated Name", description = "Updated Description" } });

        updateData!["updateProject"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Projects.FindAsync(projectId);
        fetched!.Name.Should().Be("Updated Name");
        fetched.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteProject_Mutation_ShouldDeleteProject()
    {
        var createData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name = "To Delete" } });

        var projectId = createData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var deleteData = await SendMutation("""
            mutation DeleteProject($input: DeleteProjectInput!) {
              deleteProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { id = projectId } });

        deleteData!["deleteProject"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Projects.FindAsync(projectId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateFeature_Mutation_ShouldCreateFeature()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                feature { id title }
                errors
              }
            }
            """,
            new { input = new { projectId, title = "Test Feature", description = "Feature description", acceptanceCriteria = "Acceptance criteria", initialStatus = "Planning" } });

        featureData!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
        var featureId = featureData["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Features.FindAsync(featureId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Feature");
        fetched.Status.Should().Be(DevStack.Domain.Enums.FeatureStatus.Planning);
    }

    [Fact]
    public async Task UpdateFeature_Mutation_ShouldUpdateFeature()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Original Title" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var updateData = await SendMutation("""
            mutation UpdateFeature($input: UpdateFeatureInput!) {
              updateFeature(input: $input) {
                feature { id title }
                errors
              }
            }
            """,
            new { input = new { id = featureId, title = "Updated Title", description = "Updated Description" } });

        updateData!["updateFeature"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Features.FindAsync(featureId);
        fetched!.Title.Should().Be("Updated Title");
        fetched.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task TransitionFeatureStatus_Mutation_ShouldTransitionStatus()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Test Feature", initialStatus = "Planning" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var transitionData = await SendMutation("""
            mutation TransitionFeatureStatus($input: TransitionFeatureInput!) {
              transitionFeatureStatus(input: $input) {
                feature { id }
                errors
              }
            }
            """,
            new { input = new { id = featureId, targetStatus = "InProgress", actor = "test-user" } });

        transitionData!["transitionFeatureStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Features.FindAsync(featureId);
        fetched!.Status.Should().Be(DevStack.Domain.Enums.FeatureStatus.InProgress);
    }

    [Fact]
    public async Task DeleteFeature_Mutation_ShouldDeleteFeature()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "To Delete" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var deleteData = await SendMutation("""
            mutation DeleteFeature($input: DeleteFeatureInput!) {
              deleteFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { id = featureId } });

        deleteData!["deleteFeature"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Features.FindAsync(featureId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateDefect_Mutation_ShouldCreateDefect()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Parent Feature" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var defectData = await SendMutation("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                defect { id title }
                errors
              }
            }
            """,
            new { input = new { projectId, parentFeatureId = featureId, severity = "High", title = "Test Defect", description = "Defect description", initialStatus = "Planning" } });

        defectData!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
        var defectId = defectData["createDefect"]!["defect"]!["id"]!.GetValue<Guid>();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Defects.FindAsync(defectId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Defect");
        fetched.Severity.Should().Be(DevStack.Domain.Enums.Severity.High);
    }

    [Fact]
    public async Task UpdateDefect_Mutation_ShouldUpdateDefect()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var defectData = await SendMutation("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) { defect { id } errors }
            }
            """, new { input = new { projectId, title = "Original Title", severity = "Low" } });
        var defectId = defectData!["createDefect"]!["defect"]!["id"]!.GetValue<Guid>();

        var updateData = await SendMutation("""
            mutation UpdateDefect($input: UpdateDefectInput!) {
              updateDefect(input: $input) {
                defect { id title }
                errors
              }
            }
            """,
            new { input = new { id = defectId, title = "Updated Title", description = "Updated Description" } });

        updateData!["updateDefect"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Defects.FindAsync(defectId);
        fetched!.Title.Should().Be("Updated Title");
        fetched.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task TransitionDefectStatus_Mutation_ShouldTransitionStatus()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var defectData = await SendMutation("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) { defect { id } errors }
            }
            """, new { input = new { projectId, title = "Test Defect", severity = "Medium", initialStatus = "Planning" } });
        var defectId = defectData!["createDefect"]!["defect"]!["id"]!.GetValue<Guid>();

        var transitionData = await SendMutation("""
            mutation TransitionDefectStatus($input: TransitionDefectInput!) {
              transitionDefectStatus(input: $input) {
                defect { id }
                errors
              }
            }
            """,
            new { input = new { id = defectId, targetStatus = "InProgress", actor = "test-user" } });

        transitionData!["transitionDefectStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Defects.FindAsync(defectId);
        fetched!.Status.Should().Be(DevStack.Domain.Enums.FeatureStatus.InProgress);
    }

    [Fact]
    public async Task DeleteDefect_Mutation_ShouldDeleteDefect()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var defectData = await SendMutation("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) { defect { id } errors }
            }
            """, new { input = new { projectId, title = "To Delete", severity = "Low" } });
        var defectId = defectData!["createDefect"]!["defect"]!["id"]!.GetValue<Guid>();

        var deleteData = await SendMutation("""
            mutation DeleteDefect($input: DeleteDefectInput!) {
              deleteDefect(input: $input) { defect { id } errors }
            }
            """, new { input = new { id = defectId } });

        deleteData!["deleteDefect"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Defects.FindAsync(defectId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateTask_Mutation_ShouldCreateTask()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Parent Feature" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var taskData = await SendMutation("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id title }
                errors
              }
            }
            """,
            new { input = new { projectId, featureId, title = "Test Task", deliverable = "Deliverable description", acceptanceCriteria = "Acceptance criteria", complexityRating = 5 } });

        taskData!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
        var taskId = taskData["createTask"]!["task"]!["id"]!.GetValue<Guid>();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(taskId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Task");
        fetched.ComplexityRating.Should().Be(5);
    }

    [Fact]
    public async Task UpdateTask_Mutation_ShouldUpdateTask()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Parent Feature" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var taskData = await SendMutation("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) { task { id } errors }
            }
            """, new { input = new { projectId, featureId, title = "Original Title", complexityRating = 3 } });
        var taskId = taskData!["createTask"]!["task"]!["id"]!.GetValue<Guid>();

        var updateData = await SendMutation("""
            mutation UpdateTask($input: UpdateTaskInput!) {
              updateTask(input: $input) {
                task { id title }
                errors
              }
            }
            """,
            new { input = new { id = taskId, title = "Updated Title", complexityRating = 7, deliverable = "Updated Deliverable" } });

        updateData!["updateTask"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(taskId);
        fetched!.Title.Should().Be("Updated Title");
        fetched.ComplexityRating.Should().Be(7);
    }

    [Fact]
    public async Task TransitionTaskStatus_Mutation_ShouldTransitionStatus()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Parent Feature" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var taskData = await SendMutation("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) { task { id } errors }
            }
            """, new { input = new { projectId, featureId, title = "Test Task", complexityRating = 5 } });
        var taskId = taskData!["createTask"]!["task"]!["id"]!.GetValue<Guid>();

        var transitionData = await SendMutation("""
            mutation TransitionTaskStatus($input: TransitionTaskInput!) {
              transitionTaskStatus(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { id = taskId, targetStatus = "Done", actor = "test-user" } });

        transitionData!["transitionTaskStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(taskId);
        fetched!.Status.Should().Be(DevStack.Domain.Enums.TaskStatus.Done);
    }

    [Fact]
    public async Task DeleteTask_Mutation_ShouldDeleteTask()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var featureData = await SendMutation("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) { feature { id } errors }
            }
            """, new { input = new { projectId, title = "Parent Feature" } });
        var featureId = featureData!["createFeature"]!["feature"]!["id"]!.GetValue<Guid>();

        var taskData = await SendMutation("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) { task { id } errors }
            }
            """, new { input = new { projectId, featureId, title = "To Delete", complexityRating = 3 } });
        var taskId = taskData!["createTask"]!["task"]!["id"]!.GetValue<Guid>();

        var deleteData = await SendMutation("""
            mutation DeleteTask($input: DeleteTaskInput!) {
              deleteTask(input: $input) { task { id } errors }
            }
            """, new { input = new { id = taskId } });

        deleteData!["deleteTask"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(taskId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task CreateModelConfiguration_Mutation_ShouldCreateModelConfiguration()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var configData = await SendMutation("""
            mutation CreateModelConfiguration($input: CreateModelConfigurationInput!) {
              createModelConfiguration(input: $input) {
                modelConfiguration { id url model }
                errors
              }
            }
            """,
            new { input = new { projectId, url = "https://api.example.com", model = "gpt-4", modelAlias = "GPT-4", apiKey = "test-api-key", maxComplexity = 8 } });

        configData!["createModelConfiguration"]!["errors"]!.AsArray().Should().BeEmpty();
        var configId = configData["createModelConfiguration"]!["modelConfiguration"]!["id"]!.GetValue<Guid>();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.ModelConfigurations.FindAsync(configId);
        fetched.Should().NotBeNull();
        fetched!.Url.Should().Be("https://api.example.com");
        fetched.Model.Should().Be("gpt-4");
    }

    [Fact]
    public async Task UpdateModelConfiguration_Mutation_ShouldUpdateModelConfiguration()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var configData = await SendMutation("""
            mutation CreateModelConfiguration($input: CreateModelConfigurationInput!) {
              createModelConfiguration(input: $input) { modelConfiguration { id } errors }
            }
            """, new { input = new { projectId, url = "https://api.example.com", model = "gpt-3.5", apiKey = "test-api-key", maxComplexity = 5 } });
        var configId = configData!["createModelConfiguration"]!["modelConfiguration"]!["id"]!.GetValue<Guid>();

        var updateData = await SendMutation("""
            mutation UpdateModelConfiguration($input: UpdateModelConfigurationInput!) {
              updateModelConfiguration(input: $input) {
                modelConfiguration { id url model }
                errors
              }
            }
            """,
            new { input = new { id = configId, url = "https://api.newexample.com", model = "gpt-4", maxComplexity = 10 } });

        updateData!["updateModelConfiguration"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.ModelConfigurations.FindAsync(configId);
        fetched!.Url.Should().Be("https://api.newexample.com");
        fetched.Model.Should().Be("gpt-4");
        fetched.MaxComplexity.Should().Be(10);
    }

    [Fact]
    public async Task DeleteModelConfiguration_Mutation_ShouldDeleteModelConfiguration()
    {
        var projectData = await SendMutation("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) { project { id } errors }
            }
            """, new { input = new { name = "Test Project" } });
        var projectId = projectData!["createProject"]!["project"]!["id"]!.GetValue<Guid>();

        var configData = await SendMutation("""
            mutation CreateModelConfiguration($input: CreateModelConfigurationInput!) {
              createModelConfiguration(input: $input) { modelConfiguration { id } errors }
            }
            """, new { input = new { projectId, url = "https://api.example.com", model = "gpt-4", apiKey = "test-api-key", maxComplexity = 5 } });
        var configId = configData!["createModelConfiguration"]!["modelConfiguration"]!["id"]!.GetValue<Guid>();

        var deleteData = await SendMutation("""
            mutation DeleteModelConfiguration($input: DeleteModelConfigurationInput!) {
              deleteModelConfiguration(input: $input) { modelConfiguration { id } errors }
            }
            """, new { input = new { id = configId } });

        deleteData!["deleteModelConfiguration"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _fixture.CreateDbContext();
        var fetched = await ctx.ModelConfigurations.FindAsync(configId);
        fetched.Should().BeNull();
    }
}
