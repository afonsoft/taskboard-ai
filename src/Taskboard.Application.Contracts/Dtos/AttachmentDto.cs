namespace Taskboard.Dtos;

public sealed record AttachmentDto(
    string Id,
    string TaskId,
    string? CommentId,
    string Kind,
    string Filename,
    string ContentType,
    long Size,
    string Path,
    DateTime CreatedAt);
