namespace Taskboard.ValueObjects;

public sealed record WorkflowSequenceId : StringIdBase
{
    public WorkflowSequenceId(string value)
        : base(value)
    {
    }

    public static WorkflowSequenceId From(string value) => new(value);

    public static WorkflowSequenceId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
