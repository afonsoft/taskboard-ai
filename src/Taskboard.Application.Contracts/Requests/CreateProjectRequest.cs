namespace Taskboard.Requests;

public sealed record CreateProjectRequest(
    string? Id,
    string Name,
    string? WorkspacePath);
