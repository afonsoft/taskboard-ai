using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Taskboard.Application.Contracts.Configuration;

namespace Taskboard.EntityFrameworkCore.Data;

public sealed class TaskboardDbContextFactory : IDesignTimeDbContextFactory<TaskboardDbContext>
{
    public TaskboardDbContext CreateDbContext(string[] args)
    {
        var dataDir = TaskboardEnvironment.GetDataDir(Directory.GetCurrentDirectory());
        Directory.CreateDirectory(dataDir);
        var connectionString = $"Data Source={Path.Combine(dataDir, "taskboard.sqlite")}";

        var optionsBuilder = new DbContextOptionsBuilder<TaskboardDbContext>();
        optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly(typeof(TaskboardDbContext).Assembly.FullName));

        return new TaskboardDbContext(optionsBuilder.Options);
    }
}
