using Taskboard;
using Taskboard.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace Taskboard.Domain.Entities;

public sealed class WorkflowNode : Entity<WorkflowNodeId>
{
    public ProjectId ProjectId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string Config { get; private set; } = "{}";
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private WorkflowNode()
    {
    }

    private WorkflowNode(
        WorkflowNodeId id,
        ProjectId projectId,
        string type,
        string config,
        double positionX,
        double positionY,
        DateTime createdAt)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidValue, "Node type cannot be empty.");
        }

        ProjectId = projectId;
        Type = type;
        Config = config;
        PositionX = positionX;
        PositionY = positionY;
        CreatedAt = createdAt;
    }

    public static WorkflowNode Create(
        WorkflowNodeId id,
        ProjectId projectId,
        string type,
        string config,
        double positionX,
        double positionY,
        DateTime? now = null)
        => new(id, projectId, type, config, positionX, positionY, now ?? DateTime.UtcNow);
}
