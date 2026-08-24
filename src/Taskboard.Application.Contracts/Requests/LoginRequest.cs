namespace Taskboard.Requests;

public sealed record LoginRequest(string Username, string Password, string? ReturnUrl = "/");
