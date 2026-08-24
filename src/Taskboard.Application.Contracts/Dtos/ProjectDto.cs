namespace Taskboard.Dtos;

public sealed record ProjectDto(
    string Id,
    string Name,
    string? WorkspacePath,
    IReadOnlyList<string> Labels,
    long IssueCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
