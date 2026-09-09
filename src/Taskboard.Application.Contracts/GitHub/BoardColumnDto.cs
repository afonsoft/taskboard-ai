namespace Taskboard.GitHub;

/// <summary>
/// DTO que representa uma coluna do quadro Kanban com suas issues.
/// </summary>
public sealed record BoardColumnDto(
    GitHubBoardColumn Column,
    string Label,
    IReadOnlyList<IssueDto> Issues);
