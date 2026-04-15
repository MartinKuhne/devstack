using DevStack.Domain.Entities;
using DevStack.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.Persistence;

public class DevStackDbContext : DbContext
{
    public DevStackDbContext(DbContextOptions<DevStackDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Feature> Features { get; set; } = null!;
    public DbSet<Defect> Defects { get; set; } = null!;
    public DbSet<DevStack.Domain.Entities.AgentTask> Tasks { get; set; } = null!;
    public DbSet<ModelConfiguration> ModelConfigurations { get; set; } = null!;
    public DbSet<WorkflowRun> WorkflowRuns { get; set; } = null!;
    public DbSet<AuditEvent> AuditEvents { get; set; } = null!;
    public DbSet<Epic> Epics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevStackDbContext).Assembly);

        var isSqlite = Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

        // Configure ConcurrencyToken
        // For SQL Server/PostgreSQL: Use IsRowVersion for automatic timestamp generation
        // For SQLite: Use IsConcurrencyToken only (application must provide value)

        // Add indexes for common queries
        modelBuilder.Entity<Feature>()
                    .HasIndex(f => f.Status);
                    
        modelBuilder.Entity<DevStack.Domain.Entities.AgentTask>()
                    .HasIndex(t => t.Status);
                    
        modelBuilder.Entity<AuditEvent>()
                    .HasIndex(a => a.EntityId);
                    
        modelBuilder.Entity<Feature>()
                    .HasIndex(f => new { f.ProjectId, f.Status });
                    
        modelBuilder.Entity<DevStack.Domain.Entities.AgentTask>()
                    .HasIndex(t => new { t.FeatureId, t.Status });

        modelBuilder.Entity<Feature>()
                    .HasIndex(f => f.EpicId);

        modelBuilder.Entity<Epic>()
                    .HasIndex(e => e.Title);
    }
}