using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class TaskActivity : Entity<TaskActivityId>
{
    public TaskId TaskId { get; private set; } = default!;
    public Actor Actor { get; private set; } = default!;
    public string Changes { get; private set; } = default!;
    public DateTime Timestamp { get; private set; }

    private TaskActivity()
    {
    }

    private TaskActivity(TaskActivityId id, TaskId taskId, Actor actor, string changes, DateTime timestamp)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(changes))
        {
            throw new ArgumentException("Changes cannot be empty.", nameof(changes));
        }

        TaskId = taskId;
        Actor = actor;
        Changes = changes;
        Timestamp = timestamp;
    }

    public static TaskActivity Create(
        TaskActivityId id,
        TaskId taskId,
        Actor actor,
        string changes,
        DateTime? timestamp = null)
        => new(id, taskId, actor, changes, timestamp ?? DateTime.UtcNow);
}
