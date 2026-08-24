using Taskboard.ValueObjects;

namespace Taskboard.Requests;

public sealed record CreateTaskRequest(
    string ProjectId,
    string Title,
    string? Description = null,
    string? Status = null,
    string? Priority = null,
    IReadOnlyList<string>? Labels = null,
    Actor? Creator = null,
    double? SortOrder = null,
    DateTime? StartDate = null,
    DateTime? DueDate = null);
