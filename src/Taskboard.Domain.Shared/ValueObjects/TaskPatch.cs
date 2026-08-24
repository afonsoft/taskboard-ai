namespace Taskboard.ValueObjects;

public sealed record TaskPatch(
    string? Title = null,
    string? Description = null,
    string? Status = null,
    string? Priority = null,
    IReadOnlyList<string>? Labels = null,
    Actor? Assignee = null,
    double? SortOrder = null,
    DateTime? StartDate = null,
    DateTime? DueDate = null,
    string? WorkflowId = null,
    string? GitBranch = null,
    string? WorktreePath = null,
    string? WorktreeBranch = null,
    ThreadBinding? ThreadBinding = null
);
