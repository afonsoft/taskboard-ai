namespace Taskboard.ValueObjects;

public sealed record ProjectId : StringIdBase
{
    public ProjectId(string value)
        : base(value)
    {
    }

    public static ProjectId From(string value) => new(value);

    public static ProjectId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
