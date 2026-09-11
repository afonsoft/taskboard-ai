using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Taskboard.EntityFrameworkCore.Data;

namespace Taskboard.Server.HealthChecks;

/// <summary>
/// Health check that verifies the SQLite database is reachable.
/// </summary>
public sealed class TaskboardDbContextHealthCheck : IHealthCheck
{
    private readonly TaskboardDbContext _dbContext;

    public TaskboardDbContextHealthCheck(TaskboardDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }
    }
}
