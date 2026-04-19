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
    public DbSet<LargeLanguageModel> LargeLanguageModels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevStackDbContext).Assembly);
    }
}