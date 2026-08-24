namespace Taskboard.ValueObjects;

public sealed record CommentId : StringIdBase
{
    public CommentId(string value)
        : base(value)
    {
    }

    public static CommentId From(string value) => new(value);

    public static CommentId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
