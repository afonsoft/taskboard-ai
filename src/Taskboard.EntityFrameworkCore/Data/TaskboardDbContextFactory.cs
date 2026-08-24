using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Taskboard.EntityFrameworkCore.Data;

public sealed class TaskboardDbContextFactory : IDesignTimeDbContextFactory<TaskboardDbContext>
{
    public TaskboardDbContext CreateDbContext(string[] args)
    {
        var dataDir = Environment.GetEnvironmentVariable("CODEX_TASKBOARD_DATA_DIR")
                      ?? Path.Combine(Directory.GetCurrentDirectory(), ".data");
        Directory.CreateDirectory(dataDir);
        var connectionString = $"Data Source={Path.Combine(dataDir, "taskboard.sqlite")}";

        var optionsBuilder = new DbContextOptionsBuilder<TaskboardDbContext>();
        optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(TaskboardDbContext).Assembly.FullName));

        return new TaskboardDbContext(optionsBuilder.Options);
    }
}
