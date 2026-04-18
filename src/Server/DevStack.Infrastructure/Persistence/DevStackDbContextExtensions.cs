using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.Persistence;

public static class DevStackDbContextExtensions
{
    private const string TestDataMarker = "[TestData]";

    public static async Task CleanupTestDataAsync(
        this DevStackDbContext context,
        CancellationToken cancellationToken = default)
    {
        await CleanupTasksAsync(context, cancellationToken);
        await CleanupItemsAsync(context, cancellationToken);
        await CleanupProjectsAsync(context, cancellationToken);
        await CleanupModelConfigurationsAsync(context, cancellationToken);
        await CleanupWorkflowRunsAsync(context, cancellationToken);
        await CleanupAuditEventsAsync(context, cancellationToken);
    }

    private static async Task CleanupAuditEventsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var auditEvents = await context.AuditEvents
            .Where(e => e.EntityType == "Project" ||
                       e.EntityType == "Item" ||
                       e.EntityType == "Task" ||
                       e.EntityType == "Feature" ||
                       e.EntityType == "Defect" ||
                       e.EntityType == "Epic")
            .ToListAsync(cancellationToken);

        foreach (var auditEvent in auditEvents)
        {
            if (ShouldDeleteEntity(auditEvent.EntityType, auditEvent.EntityId, context))
            {
                context.AuditEvents.Remove(auditEvent);
            }
        }

        if (context.AuditEvents.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupWorkflowRunsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var workflowRuns = await context.WorkflowRuns
            .Where(w => ShouldDeleteProject(w.ProjectId, context))
            .ToListAsync(cancellationToken);

        foreach (var workflowRun in workflowRuns)
        {
            if (ShouldDeleteProject(workflowRun.ProjectId, context))
            {
                context.WorkflowRuns.Remove(workflowRun);
            }
        }

        if (context.WorkflowRuns.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupModelConfigurationsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var modelConfigs = await context.ModelConfigurations
            .Where(m => m.Url.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (modelConfigs.Any())
        {
            context.ModelConfigurations.RemoveRange(modelConfigs);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupTasksAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var tasks = await context.Tasks.ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            if (ShouldDeleteEntity(nameof(AgentTask), task.Id, context))
            {
                context.Tasks.Remove(task);
            }
        }

        if (context.Tasks.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupItemsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var items = await context.Items.ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            if (ShouldDeleteEntity(nameof(Item), item.Id, context))
            {
                context.Items.Remove(item);
            }
        }

        if (context.Items.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupProjectsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var projects = await context.Projects
            .Where(p => p.Name.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (projects.Any())
        {
            context.Projects.RemoveRange(projects);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ShouldDeleteEntity(string entityType, Guid entityId, DevStackDbContext context)
    {
        return entityType switch
        {
            nameof(AgentTask) => context.Tasks.Any(t => t.Id == entityId && t.Title.Contains(TestDataMarker)),
            nameof(Item) => context.Items.Any(i => i.Id == entityId && i.Title.Contains(TestDataMarker)),
            "Feature" => context.Items.Any(i => i.Id == entityId && i.Title.Contains(TestDataMarker)),
            "Defect" => context.Items.Any(i => i.Id == entityId && i.Title.Contains(TestDataMarker)),
            "Epic" => context.Items.Any(i => i.Id == entityId && i.Title.Contains(TestDataMarker)),
            nameof(Project) => context.Projects.Any(p => p.Id == entityId && p.Name.Contains(TestDataMarker)),
            _ => false
        };
    }

    private static bool ShouldDeleteProject(Guid projectId, DevStackDbContext context)
    {
        var project = context.Projects.Find(projectId);
        return project != null && project.Name.Contains(TestDataMarker);
    }
}
