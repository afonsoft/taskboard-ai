namespace Taskboard.ValueObjects;

public sealed record WorkflowNodeId : StringIdBase
{
    public WorkflowNodeId(string value)
        : base(value)
    {
    }

    public static WorkflowNodeId From(string value) => new(value);

    public static WorkflowNodeId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
