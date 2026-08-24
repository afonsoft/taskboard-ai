using System.Security.Cryptography;
using System.Text;

namespace Taskboard.Server.Services;

public sealed class AdminUser
{
    public string Username { get; }
    public string PasswordHash { get; }
    public string? PasswordFilePath { get; }

    public AdminUser(string username, string passwordHash, string? passwordFilePath = null)
    {
        Username = username;
        PasswordHash = passwordHash;
        PasswordFilePath = passwordFilePath;
    }

    public bool Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var hash = HashPassword(password);
        return PasswordHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static AdminUser CreateFromConfiguration(IConfiguration configuration, string dataDirectory)
    {
        var username = configuration["Admin:Username"]
                       ?? Environment.GetEnvironmentVariable("TASKBOARD_ADMIN_USERNAME")
                       ?? "admin";

        var password = configuration["Admin:Password"]
                       ?? Environment.GetEnvironmentVariable("TASKBOARD_ADMIN_PASSWORD");

        string? passwordFilePath = null;

        if (string.IsNullOrWhiteSpace(password))
        {
            password = GenerateRandomPassword();
            Directory.CreateDirectory(dataDirectory);
            passwordFilePath = Path.Combine(dataDirectory, ".admin-password");
            File.WriteAllText(passwordFilePath, password);

            Console.WriteLine($"[Taskboard] Admin password generated and saved to: {passwordFilePath}");

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(passwordFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }

        var passwordHash = HashPassword(password);
        return new AdminUser(username, passwordHash, passwordFilePath);
    }

    private static string GenerateRandomPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
