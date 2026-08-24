namespace Taskboard.ValueObjects;

public sealed record TaskActivityId : StringIdBase
{
    public TaskActivityId(string value)
        : base(value)
    {
    }

    public static TaskActivityId From(string value) => new(value);

    public static TaskActivityId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
