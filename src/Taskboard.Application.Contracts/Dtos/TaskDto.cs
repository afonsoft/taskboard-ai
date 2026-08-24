using Taskboard.ValueObjects;

namespace Taskboard.Dtos;

public sealed record TaskDto(
    string Id,
    string Identifier,
    string ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    IReadOnlyList<string> Labels,
    double? SortOrder,
    ThreadBinding? ThreadBinding,
    Actor Creator,
    Actor? Assignee,
    string? WorkflowId,
    string? GitBranch,
    string? WorktreePath,
    string? WorktreeBranch,
    DateTime? StartDate,
    DateTime? DueDate,
    Recurrence? Recurrence,
    string? ExternalSource,
    string? ExternalOrigin,
    string? ExternalId,
    string? ExternalKey,
    string? ExternalUrl,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long Version);
