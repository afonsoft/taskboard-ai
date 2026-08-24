namespace Taskboard.Requests;

/// <summary>
/// Representa os dados de autenticação do administrador.
/// </summary>
/// <param name="Username">Nome de usuário do administrador.</param>
/// <param name="Password">Senha em texto plano.</param>
/// <param name="ReturnUrl">Caminho de redirecionamento após o login.</param>
public sealed record LoginRequest(string Username, string Password, string? ReturnUrl = "/");
