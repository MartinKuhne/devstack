using DevStack.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DevStack.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DevStackDbContext>
{
    public DevStackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DevStackDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DevStack;Username=devstack;Password=devstack123");

        return new DevStackDbContext(optionsBuilder.Options);
    }
}
