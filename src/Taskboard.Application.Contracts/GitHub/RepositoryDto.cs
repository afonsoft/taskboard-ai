namespace Taskboard.GitHub;

/// <summary>
/// DTO que representa um repositório do GitHub.
/// </summary>
public sealed record RepositoryDto(
    long Id,
    string FullName,
    string Name,
    string? Description,
    string Url,
    bool IsPrivate);
