using System.ComponentModel.DataAnnotations;

namespace Taskboard.Application.Contracts.Configuration;

/// <summary>
/// Configuration options for the administrator account.
/// </summary>
public sealed class AdminOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Admin username is required.")]
    public string Username { get; set; } = "admin";

    [Required(AllowEmptyStrings = false, ErrorMessage = "Admin password is required in production.")]
    public string? Password { get; set; }
}
