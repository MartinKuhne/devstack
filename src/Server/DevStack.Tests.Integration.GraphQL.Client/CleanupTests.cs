using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[Collection("Integration")]
public class CleanupTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;
    private readonly TestDataCleanup _cleanup;

    public CleanupTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
        _cleanup = new TestDataCleanup();
    }

    [Fact]
    public async Task Data_CanBeCreated_And_CleanedUp()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        var project = new Project { Name = "Test Project" };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var feature = new Item { Subtype = ItemSubtype.Feature,
            ProjectId = project.Id,
            Title = "Test Feature",
            Status = FeatureStatus.Planning
        };
        context.Items.Add(feature);
        await context.SaveChangesAsync();

        var defect = new Item
        {
            ProjectId = project.Id,
            Severity = Severity.Medium,
            Title = "Test Defect",
            Status = FeatureStatus.Planning,
            Subtype = ItemSubtype.Defect
        };
        context.Items.Add(defect);
        await context.SaveChangesAsync();

        var task = new AgentTask
        {
            ProjectId = project.Id,
            ItemId = feature.Id,
            Title = "Test Task",
            ComplexityRating = 5,
            Status = Domain.Enums.TaskStatus.Planning
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var modelConfig = new ModelConfiguration
        {
            Url = "https://api.example.com",
            Model = "gpt-4",
            MaxComplexity = 8
        };
        context.ModelConfigurations.Add(modelConfig);
        await context.SaveChangesAsync();

        await _cleanup.CleanupAsync(context);
    }

    [Fact]
    public async Task VerifyCleanup_ShouldConfirmNoOrphanedData()
    {
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        await _cleanup.VerifyCleanupAsync(context);
    }
}
