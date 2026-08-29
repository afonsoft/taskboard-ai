using Taskboard;
using Taskboard.ValueObjects;

namespace Taskboard.Domain.Entities;

public sealed class AiChatEvent : Entity<AiChatEventId>
{
    public AiChatThreadId ThreadId { get; private set; } = default!;
    public AiChatEventRole Role { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private AiChatEvent()
    {
    }

    private AiChatEvent(AiChatEventId id, AiChatThreadId threadId, AiChatEventRole role, string content, DateTime createdAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content cannot be empty.", nameof(content));
        }

        ThreadId = threadId;
        Role = role;
        Content = content;
        CreatedAt = createdAt;
    }

    public static AiChatEvent Create(
        AiChatEventId id,
        AiChatThreadId threadId,
        AiChatEventRole role,
        string content,
        DateTime? now = null)
        => new(id, threadId, role, content, now ?? DateTime.UtcNow);
}
