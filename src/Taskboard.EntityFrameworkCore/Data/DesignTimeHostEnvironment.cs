using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Taskboard.EntityFrameworkCore.Data;

/// <summary>
/// Minimal implementation of <see cref="IHostEnvironment"/> for design-time database context creation.
/// </summary>
internal sealed class DesignTimeHostEnvironment : IHostEnvironment
{
    public DesignTimeHostEnvironment(string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(contentRootPath);
        ContentRootPath = contentRootPath;
        ApplicationName = "Taskboard.DbContextFactory";
        EnvironmentName = "Development";
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
    }

    public string ApplicationName { get; set; }
    public string EnvironmentName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
}
