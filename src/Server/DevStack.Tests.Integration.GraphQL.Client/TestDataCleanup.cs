using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Tests.Integration.GraphQL.Client;

public interface ITestDataCleanup
{
    Task CleanupAsync(DevStackDbContext context);
    Task VerifyCleanupAsync(DevStackDbContext context);
}

public class TestDataCleanup : ITestDataCleanup
{
    public async Task CleanupAsync(DevStackDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"Projects\", \"Features\", \"Defects\", \"Tasks\", \"ModelConfigurations\", \"WorkflowRuns\", \"AuditEvents\" RESTART IDENTITY CASCADE");
    }

    public async Task VerifyCleanupAsync(DevStackDbContext context)
    {
        var projectCount = await context.Projects.CountAsync();
        var featureCount = await context.Items.CountAsync();
        var defectCount = await context.Defects.CountAsync();
        var taskCount = await context.Tasks.CountAsync();
        var modelConfigCount = await context.ModelConfigurations.CountAsync();
        var workflowRunCount = await context.WorkflowRuns.CountAsync();
        var auditEventCount = await context.AuditEvents.CountAsync();

        if (projectCount > 0 || featureCount > 0 || defectCount > 0 || 
            taskCount > 0 || modelConfigCount > 0 || workflowRunCount > 0 || 
            auditEventCount > 0)
        {
            throw new InvalidOperationException(
                $"Data cleanup failed. Remaining: Projects={projectCount}, " +
                $"Features={featureCount}, Defects={defectCount}, Tasks={taskCount}, " +
                $"ModelConfigurations={modelConfigCount}, WorkflowRuns={workflowRunCount}, " +
                $"AuditEvents={auditEventCount}");
        }
    }
}
