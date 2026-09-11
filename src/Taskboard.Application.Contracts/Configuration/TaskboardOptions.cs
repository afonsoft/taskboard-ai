using System.ComponentModel.DataAnnotations;

namespace Taskboard.Application.Contracts.Configuration;

/// <summary>
/// Configuration options for the Taskboard server.
/// </summary>
public sealed class TaskboardOptions
{
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; set; } = 47823;

    [Required(AllowEmptyStrings = false, ErrorMessage = "DataDir is required.")]
    public string DataDir { get; set; } = ".data";

    public DatabaseOptions Database { get; set; } = new();
}

/// <summary>
/// Database configuration options.
/// </summary>
public sealed class DatabaseOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ConnectionStringName is required.")]
    public string ConnectionStringName { get; set; } = "Taskboard";
}
