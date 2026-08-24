namespace Taskboard.ValueObjects;

public sealed record AttachmentId : StringIdBase
{
    public AttachmentId(string value)
        : base(value)
    {
    }

    public static AttachmentId From(string value) => new(value);

    public static AttachmentId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
