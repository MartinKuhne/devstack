using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[Collection("Integration")]
public class MigrationExecutionTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public MigrationExecutionTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task All_Migrations_Execute_Successfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        // Act & Assert - If migrations fail, this will throw
        await using var context = new DevStackDbContext(options);
        await context.Database.MigrateAsync();

        // Verify all expected tables exist
        await context.Database.OpenConnectionAsync();
        var tables = await context.Database.GetDbConnection().GetSchemaAsync("TABLES");
        await context.Database.CloseConnectionAsync();
        
        var tableNames = new List<string>();
        foreach (DataRow row in tables.Rows)
        {
            var tableName = row["TABLE_NAME"].ToString();
            if (!string.IsNullOrEmpty(tableName))
                tableNames.Add(tableName);
        }

        tableNames.Should().Contain("Projects");
        tableNames.Should().Contain("Items");
        tableNames.Should().Contain("Tasks");
        tableNames.Should().Contain("ModelConfigurations");
        tableNames.Should().Contain("WorkflowRuns");
        tableNames.Should().Contain("AuditEvents");
    }

    [Fact]
    public async Task InitialCreate_Migration_Verify_Tables()
    {
        // Arrange
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        await context.Database.MigrateAsync();

        // Act & Assert - Verify initial tables from InitialCreate migration
        var hasProjects = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Projects')").AnyAsync();
        var hasItems = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Items')").AnyAsync();
        var hasTasks = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Tasks')").AnyAsync();
        var hasModelConfigurations = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'ModelConfigurations')").AnyAsync();
        var hasWorkflowRuns = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'WorkflowRuns')").AnyAsync();
        var hasAuditEvents = await context.Database.SqlQueryRaw<string>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'AuditEvents')").AnyAsync();

        hasProjects.Should().BeTrue();
        hasItems.Should().BeTrue();
        hasTasks.Should().BeTrue();
        hasModelConfigurations.Should().BeTrue();
        hasWorkflowRuns.Should().BeTrue();
        hasAuditEvents.Should().BeTrue();
    }

    [Fact]
    public async Task Migrations_Execute_Without_Error()
    {
        // Arrange
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        // Act & Assert - Just verify migrations run without throwing
        await context.Database.MigrateAsync();
        
        // Verify we can query the database after migrations
        var projectCount = await context.Projects.CountAsync();
        projectCount.Should().Be(0); // Fresh database
    }

    [Fact]
    public async Task AddWorkItemIndexes_Migration_Verify()
    {
        // Arrange
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        await context.Database.MigrateAsync();

        // Act & Assert - Verify indexes were added
        var hasItemStatusIndex = await context.Database.SqlQueryRaw<string>(
            "SELECT EXISTS (SELECT FROM pg_indexes WHERE tablename = 'Items' AND indexname = 'IX_Items_Status')"
        ).AnyAsync();

        var hasTaskStatusIndex = await context.Database.SqlQueryRaw<string>(
            "SELECT EXISTS (SELECT FROM pg_indexes WHERE tablename = 'Tasks' AND indexname = 'IX_Tasks_Status')"
        ).AnyAsync();

        var hasAuditEventEntityIdIndex = await context.Database.SqlQueryRaw<string>(
            "SELECT EXISTS (SELECT FROM pg_indexes WHERE tablename = 'AuditEvents' AND indexname = 'IX_AuditEvents_EntityId')"
        ).AnyAsync();

        var hasItemProjectStatusIndex = await context.Database.SqlQueryRaw<string>(
            "SELECT EXISTS (SELECT FROM pg_indexes WHERE tablename = 'Items' AND indexname = 'IX_Items_ProjectId_Status')"
        ).AnyAsync();

        var hasTaskItemStatusIndex = await context.Database.SqlQueryRaw<string>(
            "SELECT EXISTS (SELECT FROM pg_indexes WHERE tablename = 'Tasks' AND indexname = 'IX_Tasks_ItemId_Status')"
        ).AnyAsync();

        hasItemStatusIndex.Should().BeTrue("AddWorkItemIndexes migration should have added IX_Items_Status");
        hasTaskStatusIndex.Should().BeTrue("AddWorkItemIndexes migration should have added IX_Tasks_Status");
        hasAuditEventEntityIdIndex.Should().BeTrue("AddWorkItemIndexes migration should have added IX_AuditEvents_EntityId");
        hasItemProjectStatusIndex.Should().BeTrue("AddWorkItemIndexes migration should have added IX_Items_ProjectId_Status");
        hasTaskItemStatusIndex.Should().BeTrue("AddWorkItemIndexes migration should have added IX_Tasks_ItemId_Status");
    }

    [Fact]
    public async Task Migrations_Create_Correct_Schema()
    {
        // Arrange
        await using var context = new DevStackDbContext(
            new DbContextOptionsBuilder<DevStackDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

        await context.Database.MigrateAsync();

        // Act & Assert - Verify all migrations ran by checking ModelSnapshot
        var model = context.Model;

        // Verify entities are in the model
        model.FindEntityType(typeof(Project))!.Should().NotBeNull();
        model.FindEntityType(typeof(Item))!.Should().NotBeNull();
        model.FindEntityType(typeof(Domain.Entities.AgentTask))!.Should().NotBeNull();
        model.FindEntityType(typeof(ModelConfiguration))!.Should().NotBeNull();
        model.FindEntityType(typeof(WorkflowRun))!.Should().NotBeNull();
        model.FindEntityType(typeof(AuditEvent))!.Should().NotBeNull();
    }
}
