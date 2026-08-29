using Taskboard;
using Taskboard.ValueObjects;

namespace Taskboard.Domain.Entities;

public sealed class Attachment : Entity<AttachmentId>
{
    public TaskId TaskId { get; private set; } = default!;
    public CommentId? CommentId { get; private set; }
    public AttachmentKind Kind { get; private set; } = default!;
    public string Filename { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long Size { get; private set; }
    public string Path { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Attachment()
    {
    }

    private Attachment(
        AttachmentId id,
        TaskId taskId,
        CommentId? commentId,
        AttachmentKind kind,
        string filename,
        string contentType,
        long size,
        string path,
        DateTime now)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Filename cannot be empty.", nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type cannot be empty.", nameof(contentType));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        if (size < 0)
        {
            throw new DomainException(
                TaskboardDomainErrorCodes.NegativeAttachmentSize,
                "Attachment size cannot be negative.");
        }

        TaskId = taskId;
        CommentId = commentId;
        Kind = kind;
        Filename = filename;
        ContentType = contentType;
        Size = size;
        Path = path;
        CreatedAt = now;
    }

    public static Attachment Create(
        AttachmentId id,
        TaskId taskId,
        AttachmentKind kind,
        string filename,
        string contentType,
        long size,
        string path,
        DateTime? now = null,
        CommentId? commentId = null)
        => new(id, taskId, commentId, kind, filename, contentType, size, path, now ?? DateTime.UtcNow);
}
