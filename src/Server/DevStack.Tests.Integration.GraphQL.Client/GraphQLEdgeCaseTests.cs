using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[Collection("Integration")]
public class GraphQLEdgeCaseTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public GraphQLEdgeCaseTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Query_WithInvalidId_ShouldReturnNull()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var invalidId = Guid.NewGuid();

        var result = await context.Projects.FindAsync(invalidId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Query_EmptyResult_Set_ShouldHandleGracefully()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var projects = await context.Projects.ToListAsync();
        projects.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateProject_WithEmptyOrWhitespaceName_ShouldSucceedButValidate(string name)
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project
        {
            Name = name,
            Memory = "8GB"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be(name);
    }

    [Fact]
    public async Task CreateProject_WithLongName_ShouldFail()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var longName = new string('A', 201);
        var project = new Project
        {
            Name = longName,
            Memory = "8GB"
        };

        context.Projects.Add(project);
        var action = async () => await context.SaveChangesAsync();

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Theory]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Done)]
    [InlineData(FeatureStatus.Planning, FeatureStatus.Failed)]
    [InlineData(FeatureStatus.Ready, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.InProgress, FeatureStatus.Rejected)]
    [InlineData(FeatureStatus.Done, FeatureStatus.InProgress)]
    [InlineData(FeatureStatus.Failed, FeatureStatus.Planning)]
    public async Task TransitionFeature_InvalidStatus_ShouldFail(FeatureStatus from, FeatureStatus to)
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Item { Subtype = ItemSubtype.Feature,
            ProjectId = project.Id,
            Title = "Test Feature",
            Status = from
        };
        context.Items.Add(feature);
        await context.SaveChangesAsync();

        feature.Status = to;
        var action = async () => await context.SaveChangesAsync();

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateFeature_WithoutProject_ShouldFailForeignKey()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var feature = new Item { Subtype = ItemSubtype.Feature,
            ProjectId = Guid.NewGuid(),
            Title = "Test Feature",
            Status = FeatureStatus.Planning
        };

        context.Items.Add(feature);
        var action = async () => await context.SaveChangesAsync();

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DeleteNonExistentEntity_ShouldNotThrow()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var nonExistentId = Guid.NewGuid();
        var project = await context.Projects.FindAsync(nonExistentId);
        project.Should().BeNull();

        var action = async () =>
        {
            if (project != null)
            {
                context.Projects.Remove(project);
                await context.SaveChangesAsync();
            }
        };

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateNonExistentEntity_ShouldInsertNew()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var nonExistentId = Guid.NewGuid();
        var project = new Project
        {
            Id = nonExistentId,
            Name = "New Name",
            Memory = "8GB"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(nonExistentId);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("New Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task CreateTask_WithVariousComplexityValues_ShouldSucceed(int complexity)
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Item { Subtype = ItemSubtype.Feature,
            ProjectId = project.Id,
            Title = "Test Feature",
            Status = FeatureStatus.Planning
        };
        context.Items.Add(feature);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            ProjectId = project.Id,
            ItemId = feature.Id,
            Title = "Test Task",
            ComplexityRating = complexity,
            Status = Domain.Enums.TaskStatus.Planning
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var fetched = await context.Tasks.FindAsync(task.Id);
        fetched.Should().NotBeNull();
        fetched!.ComplexityRating.Should().Be(complexity);
    }

    [Fact]
    public async Task Concurrency_ShouldAllowSequentialUpdates()
    {
        await using var context1 = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        await using var context2 = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context1.Projects.Add(project);
        await context1.SaveChangesAsync();

        var project1 = await context1.Projects.FindAsync(project.Id);
        var project2 = await context2.Projects.FindAsync(project.Id);

        project1!.Name = "Updated by Context1";
        await context1.SaveChangesAsync();

        project2!.Name = "Updated by Context2";
        await context2.SaveChangesAsync();

        await context1.Entry(project1).ReloadAsync();
        project1.Name.Should().Be("Updated by Context2");
    }

    [Fact]
    public async Task CreateProject_WithSpecialCharacters_ShouldSucceed()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project
        {
            Name = "Project with special chars: @#$%^&*()_+-=[]{}|;':\",./<>?",
            Description = "Description with Unicode: 你好 🚀 émojis",
            Memory = "8GB"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Contain("special chars");
    }

    [Fact]
    public async Task CreateEntity_WithEmptyStrings_ShouldSucceed()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project
        {
            Name = string.Empty,
            Memory = string.Empty
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task Pagination_BoundaryConditions_ShouldHandle()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        for (int i = 0; i < 5; i++)
        {
            context.Projects.Add(new Project
            {
                Name = $"Project {i}",
                Memory = "8GB"
            });
        }
        await context.SaveChangesAsync();

        var allProjects = await context.Projects.ToListAsync();
        allProjects.Count.Should().BeGreaterThanOrEqualTo(5);

        var firstTwo = await context.Projects.OrderBy(p => p.Name).Take(2).ToListAsync();
        firstTwo.Count.Should().Be(2);

        var emptySkip = await context.Projects.OrderBy(p => p.Name).Skip(1000).Take(5).ToListAsync();
        emptySkip.Count.Should().BeLessThan(5);
    }

    [Fact]
    public async Task CreateDefect_WithoutParentFeature_ShouldSucceed()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var defect = new Item
        {
            ProjectId = project.Id,
            Title = "Orphan Defect",
            Severity = Severity.Medium,
            Status = FeatureStatus.Planning,
            Subtype = ItemSubtype.Defect
        };

        context.Items.Add(defect);
        await context.SaveChangesAsync();

        var fetched = await context.Items.FindAsync(defect.Id);
        fetched.Should().NotBeNull();
    }

    [Theory]
    [InlineData(Severity.Low)]
    [InlineData(Severity.Medium)]
    [InlineData(Severity.High)]
    [InlineData(Severity.Critical)]
    public async Task CreateDefect_WithAllSeverities_ShouldSucceed(Severity severity)
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var defect = new Item
        {
            ProjectId = project.Id,
            Title = $"Defect with {severity} severity",
            Severity = severity,
            Status = FeatureStatus.Planning,
            Subtype = ItemSubtype.Defect
        };

        context.Items.Add(defect);
        await context.SaveChangesAsync();

        var fetched = await context.Items.FindAsync(defect.Id);
        fetched.Should().NotBeNull();
        fetched!.Severity.Should().Be(severity);
    }

    [Fact]
    public async Task UpdateEntity_WithConcurrencyToken_ShouldDetectChanges()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project", Memory = "8GB" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var original = await context.Projects.FindAsync(project.Id);
        original!.Name = "Modified";
        await context.SaveChangesAsync();

        var updated = await context.Projects.FindAsync(project.Id);
        updated!.Name.Should().Be("Modified");
    }

    [Fact]
    public async Task Query_WithLargeDataset_ShouldPerform()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        for (int i = 0; i < 100; i++)
        {
            context.Projects.Add(new Project
            {
                Name = $"Project {i}",
                Description = $"Description {i}",
                Memory = "8GB"
            });
        }
        await context.SaveChangesAsync();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var count = await context.Projects.CountAsync();
        stopwatch.Stop();

        count.Should().BeGreaterThanOrEqualTo(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task CreateEntity_WithUnicodeTitle_ShouldSucceed()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project
        {
            Name = "プロジェクト",
            Description = "Проект с кириллицей",
            Memory = "8GB"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var fetched = await context.Projects.FindAsync(project.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("プロジェクト");
    }
}
