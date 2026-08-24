using Taskboard.ValueObjects;

namespace Taskboard.Dtos;

public sealed record CommentDto(
    string Id,
    string TaskId,
    string Body,
    Actor Author,
    string? ThreadId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
