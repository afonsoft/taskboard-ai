using Taskboard;
using Taskboard.ValueObjects;

namespace Taskboard.Domain.Entities;

public sealed class TaskRelation : Entity<Guid>
{
    public TaskId SourceTaskId { get; private set; } = default!;
    public TaskId TargetTaskId { get; private set; } = default!;
    public RelationType RelationType { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private TaskRelation()
    {
    }

    private TaskRelation(Guid id, TaskId sourceTaskId, TaskId targetTaskId, RelationType relationType, DateTime now)
        : base(id)
    {
        if (sourceTaskId == targetTaskId)
        {
            throw new DomainException(
                TaskboardDomainErrorCodes.SelfRelation,
                "A task cannot be related to itself.");
        }

        SourceTaskId = sourceTaskId;
        TargetTaskId = targetTaskId;
        RelationType = relationType;
        CreatedAt = now;
    }

    public static TaskRelation Create(
        TaskId sourceTaskId,
        TaskId targetTaskId,
        RelationType relationType,
        DateTime? now = null)
        => new(Guid.NewGuid(), sourceTaskId, targetTaskId, relationType, now ?? DateTime.UtcNow);

    public bool Matches(TaskId source, TaskId target, RelationType relationType)
        => SourceTaskId == source && TargetTaskId == target && RelationType == relationType;

    public bool IsEquivalent(TaskId source, TaskId target, RelationType relationType)
    {
        if (RelationType != relationType)
        {
            return false;
        }

        if (relationType == RelationType.Parent || relationType == RelationType.Blocks)
        {
            return SourceTaskId == source && TargetTaskId == target;
        }

        return (SourceTaskId == source && TargetTaskId == target)
               || (SourceTaskId == target && TargetTaskId == source);
    }
}
