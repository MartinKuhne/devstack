using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DevStack.Infrastructure.Persistence;

/// <summary>
/// Implements the design-time DbContext factory for EF Core migrations.
/// This class is used by EF Core tools at design time to create the DbContext
/// for migration generation and database updates.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DevStackDbContext>
{
    /// <summary>
    /// Creates a new instance of the DevStackDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command line arguments passed by the EF Core tools.</param>
    /// <returns>A configured DevStackDbContext instance.</returns>
    public DevStackDbContext CreateDbContext(string[] args)
    {
        // Configure the DbContext options for design-time use
        var optionsBuilder = new DbContextOptionsBuilder<DevStackDbContext>();
        
        // Use PostgreSQL provider with a connection string suitable for design-time
        // In practice, this would typically come from configuration or environment variables
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DevStack;Username=devstack;Password=devstack123");
        
        return new DevStackDbContext(optionsBuilder.Options);
    }
}