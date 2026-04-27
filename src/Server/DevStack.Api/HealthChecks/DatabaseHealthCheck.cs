using DevStack.Persistence;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevStack.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DevStackDbContext _dbContext;

    public DatabaseHealthCheck(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
