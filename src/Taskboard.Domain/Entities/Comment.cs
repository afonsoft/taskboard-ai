using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class Comment : Entity<CommentId>
{
    public TaskId TaskId { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public Actor Author { get; private set; } = default!;
    public string? ThreadId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Comment()
    {
    }

    private Comment(CommentId id, TaskId taskId, string body, Actor author, DateTime now, string? threadId = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException(TaskboardDomainErrorCodes.EmptyCommentBody, "Comment body cannot be empty.");
        }

        TaskId = taskId;
        Body = body;
        Author = author;
        ThreadId = threadId;
        CreatedAt = UpdatedAt = now;
    }

    public static Comment Create(
        CommentId id,
        TaskId taskId,
        string body,
        Actor author,
        DateTime? now = null,
        string? threadId = null)
        => new(id, taskId, body, author, now ?? DateTime.UtcNow, threadId);

    public void Edit(string body, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException(TaskboardDomainErrorCodes.EmptyCommentBody, "Comment body cannot be empty.");
        }

        Body = body;
        UpdatedAt = now ?? DateTime.UtcNow;
    }
}
