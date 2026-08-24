using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Taskboard.Server.Services;

/// <summary>
/// Representa o usuário administrador carregado das configurações do servidor.
/// </summary>
public sealed class AdminUser
{
    /// <summary>Nome de usuário do administrador.</summary>
    public string Username { get; }

    /// <summary>Hash da senha gerado pelo <see cref="PasswordHasher{TUser}"/>.</summary>
    public string PasswordHash { get; }

    /// <summary>Caminho do arquivo onde a senha em texto plano foi persistida, quando gerada automaticamente.</summary>
    public string? PasswordFilePath { get; }

    /// <summary>
    /// Cria uma nova instância de <see cref="AdminUser"/>.
    /// </summary>
    public AdminUser(string username, string passwordHash, string? passwordFilePath = null)
    {
        Username = username;
        PasswordHash = passwordHash;
        PasswordFilePath = passwordFilePath;
    }

    /// <summary>
    /// Verifica se a senha informada corresponde ao hash armazenado.
    /// </summary>
    public bool Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var result = new PasswordHasher<AdminUser>().VerifyHashedPassword(this, PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    /// <summary>
    /// Carrega as credenciais do administrador a partir da configuração ou variáveis de ambiente,
    /// gerando uma senha aleatória quando nenhuma for fornecida.
    /// </summary>
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

        var user = new AdminUser(username, string.Empty, passwordFilePath);
        var passwordHash = new PasswordHasher<AdminUser>().HashPassword(user, password);
        return new AdminUser(username, passwordHash, passwordFilePath);
    }

    private static string GenerateRandomPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
