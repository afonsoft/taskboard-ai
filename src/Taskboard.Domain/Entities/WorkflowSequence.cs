using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class WorkflowSequence : Entity<WorkflowSequenceId>
{
    public ProjectId ProjectId { get; private set; } = default!;
    public WorkflowNodeId? SourceNodeId { get; private set; }
    public WorkflowNodeId TargetNodeId { get; private set; } = default!;
    public string? Condition { get; private set; }
    public int Order { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private WorkflowSequence()
    {
    }

    private WorkflowSequence(
        WorkflowSequenceId id,
        ProjectId projectId,
        WorkflowNodeId? sourceNodeId,
        WorkflowNodeId targetNodeId,
        string? condition,
        int order,
        DateTime createdAt)
        : base(id)
    {
        if (sourceNodeId == targetNodeId)
        {
            throw new DomainException(TaskboardDomainErrorCodes.SelfRelation, "A sequence cannot target its own source node.");
        }

        ProjectId = projectId;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        Condition = condition;
        Order = order;
        CreatedAt = createdAt;
    }

    public static WorkflowSequence Create(
        WorkflowSequenceId id,
        ProjectId projectId,
        WorkflowNodeId? sourceNodeId,
        WorkflowNodeId targetNodeId,
        string? condition = null,
        int order = 0,
        DateTime? now = null)
        => new(id, projectId, sourceNodeId, targetNodeId, condition, order, now ?? DateTime.UtcNow);
}
