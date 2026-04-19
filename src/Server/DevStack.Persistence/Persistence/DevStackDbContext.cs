using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Persistence;

public class DevStackDbContext : DbContext
{
    public DevStackDbContext(DbContextOptions<DevStackDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Deliverable> Deliverables { get; set; } = null!;
    public DbSet<AgentTask> AgentTasks { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<LargeLanguageModel> LargeLanguageModels { get; set; } = null!;

    [Obsolete("Use Items with Subtype filter instead")]
    public IQueryable<Item> Features => Items.Where(i => i.ItemType == Domain.Enums.ItemSubtype.Feature);

    [Obsolete("Use Items with Subtype filter instead")]
    public IQueryable<Item> Defects => Items.Where(i => i.ItemType == Domain.Enums.ItemSubtype.Defect);

    [Obsolete("Use Items with Subtype filter instead")]
    public IQueryable<Item> Tasks => Items.Where(i => i.ItemType == Domain.Enums.ItemSubtype.Task);


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevStackDbContext).Assembly);

        var isSqlite = Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

        modelBuilder.Entity<Item>()
                    .HasIndex(f => f.Status);
                    
        modelBuilder.Entity<Item>()
                    .HasIndex(f => new { f.ProjectId, f.Status });
    }
}