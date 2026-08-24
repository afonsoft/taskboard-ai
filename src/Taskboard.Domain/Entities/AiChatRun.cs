using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class AiChatRun : Entity<AiChatRunId>
{
    public AiChatThreadId ThreadId { get; private set; } = default!;
    public string Status { get; private set; } = "running";
    public int? ExitCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    private AiChatRun()
    {
    }

    private AiChatRun(AiChatRunId id, AiChatThreadId threadId, DateTime createdAt)
        : base(id)
    {
        ThreadId = threadId;
        CreatedAt = createdAt;
    }

    public static AiChatRun Create(AiChatRunId id, AiChatThreadId threadId, DateTime? now = null)
        => new(id, threadId, now ?? DateTime.UtcNow);

    public void Complete(int exitCode, DateTime? now = null)
    {
        Status = "completed";
        ExitCode = exitCode;
        FinishedAt = now ?? DateTime.UtcNow;
    }

    public void Fail(int? exitCode, DateTime? now = null)
    {
        Status = "failed";
        ExitCode = exitCode;
        FinishedAt = now ?? DateTime.UtcNow;
    }
}
