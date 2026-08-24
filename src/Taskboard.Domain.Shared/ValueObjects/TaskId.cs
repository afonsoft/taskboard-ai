namespace Taskboard.ValueObjects;

public sealed record TaskId : StringIdBase
{
    public TaskId(string value)
        : base(value)
    {
    }

    public static TaskId From(string value) => new(value);

    public static TaskId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
