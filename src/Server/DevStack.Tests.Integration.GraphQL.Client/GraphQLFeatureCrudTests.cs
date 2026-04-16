using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[Collection("Integration")]
public class GraphQLFeatureCrudTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public GraphQLFeatureCrudTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateFeature_ShouldCreateFeatureInDatabase()
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
            Plan = "Plan details",
            SecurityImpact = "Low",
            PerformanceImpact = "Medium",
            TestPlan = "Test plan",
            DeploymentPlan = "Deployment plan",
            OpenQuestions = "Open questions",
            Status = FeatureStatus.Planning
        };

        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Feature");
        fetched.Description.Should().Be("Feature description");
        fetched.Status.Should().Be(FeatureStatus.Planning);
    }

    [Fact]
    public async Task UpdateFeature_ShouldUpdateFeatureFields()
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
            Title = "Original Title",
            Description = "Original Description"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        feature.Title = "Updated Title";
        feature.Description = "Updated Description";
        feature.AcceptanceCriteria = "Updated criteria";
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched!.Title.Should().Be("Updated Title");
        fetched.Description.Should().Be("Updated Description");
        fetched.AcceptanceCriteria.Should().Be("Updated criteria");
    }

    [Fact]
    public async Task GetFeatures_ShouldReturnFilteredFeatures()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var testGuid = Guid.NewGuid();
        var project1 = new Project { Name = $"Project 1 {testGuid}" };
        var project2 = new Project { Name = $"Project 2 {testGuid}" };
        context.Projects.AddRange(project1, project2);
        await context.SaveChangesAsync();

        var feature1 = new Feature
        {
            ProjectId = project1.Id,
            Title = $"Feature 1 {testGuid}",
            Status = FeatureStatus.Planning
        };
        var feature2 = new Feature
        {
            ProjectId = project1.Id,
            Title = $"Feature 2 {testGuid}",
            Status = FeatureStatus.InProgress
        };
        var feature3 = new Feature
        {
            ProjectId = project2.Id,
            Title = $"Feature 3 {testGuid}",
            Status = FeatureStatus.Planning
        };
        context.Features.AddRange(feature1, feature2, feature3);
        await context.SaveChangesAsync();

        var project1Features = await context.Features
            .Where(f => f.ProjectId == project1.Id)
            .ToListAsync();

        project1Features.Should().HaveCount(2);
        project1Features.Should().Contain(f => f.Title == $"Feature 1 {testGuid}");
        project1Features.Should().Contain(f => f.Title == $"Feature 2 {testGuid}");

        var planningFeatures = await context.Features
            .Where(f => f.ProjectId == project1.Id || f.ProjectId == project2.Id)
            .Where(f => f.Status == FeatureStatus.Planning)
            .ToListAsync();

        planningFeatures.Should().HaveCount(2);
        planningFeatures.Should().Contain(f => f.Title == $"Feature 1 {testGuid}");
        planningFeatures.Should().Contain(f => f.Title == $"Feature 3 {testGuid}");
    }

    [Fact]
    public async Task GetFeatureById_ShouldReturnFeature()
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
            Description = "Test description"
        };
        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Test Feature");
        fetched.Description.Should().Be("Test description");
    }

    [Fact]
    public async Task GetFeatureById_ShouldReturnNull_WhenFeatureNotFound()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var nonExistentId = Guid.NewGuid();
        var fetched = await context.Features.FindAsync(nonExistentId);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task TransitionFeatureStatus_ShouldUpdateStatus()
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
        feature.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched!.Status.Should().Be(FeatureStatus.InProgress);
    }

    [Fact]
    public async Task DeleteFeature_ShouldRemoveFromDatabase()
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
            Title = "Feature to Delete"
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
    public async Task CreateFeature_WithAllFields_ShouldSaveAllData()
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
            Title = "Complete Feature",
            Description = "Full description",
            AcceptanceCriteria = "All criteria met",
            Plan = "Detailed plan",
            SecurityImpact = "High",
            PerformanceImpact = "Low",
            TestPlan = "Comprehensive testing",
            DeploymentPlan = "Rollout strategy",
            OpenQuestions = "Pending clarifications",
            Status = FeatureStatus.Planning,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Features.Add(feature);
        await context.SaveChangesAsync();

        var fetched = await context.Features.FindAsync(feature.Id);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("Complete Feature");
        fetched.Description.Should().Be("Full description");
        fetched.AcceptanceCriteria.Should().Be("All criteria met");
        fetched.Plan.Should().Be("Detailed plan");
        fetched.SecurityImpact.Should().Be("High");
        fetched.PerformanceImpact.Should().Be("Low");
        fetched.TestPlan.Should().Be("Comprehensive testing");
        fetched.DeploymentPlan.Should().Be("Rollout strategy");
        fetched.OpenQuestions.Should().Be("Pending clarifications");
    }

    [Fact]
    public async Task GetFeatures_WithPagination_ShouldReturnCorrectPage()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var testGuid = Guid.NewGuid();
        var project = new Project { Name = $"Test Project {testGuid}" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        for (int i = 0; i < 10; i++)
        {
            context.Features.Add(new Feature
            {
                ProjectId = project.Id,
                Title = $"Feature {i} {testGuid}",
                Status = FeatureStatus.Planning,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await context.SaveChangesAsync();

        var allFeatures = await context.Features
            .Where(f => f.ProjectId == project.Id)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync();

        allFeatures.Should().HaveCount(10);

        var firstPage = await context.Features
            .Where(f => f.ProjectId == project.Id)
            .OrderBy(f => f.CreatedAt)
            .Skip(0)
            .Take(5)
            .ToListAsync();

        firstPage.Should().HaveCount(5);
        firstPage.First().Title.Should().Be($"Feature 0 {testGuid}");

        var secondPage = await context.Features
            .Where(f => f.ProjectId == project.Id)
            .OrderBy(f => f.CreatedAt)
            .Skip(5)
            .Take(5)
            .ToListAsync();

        secondPage.Should().HaveCount(5);
        secondPage.First().Title.Should().Be($"Feature 5 {testGuid}");
    }
}
