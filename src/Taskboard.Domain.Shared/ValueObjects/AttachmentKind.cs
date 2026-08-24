namespace Taskboard.ValueObjects;

public sealed record AttachmentKind : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "inline",
        "attachment"
    };

    public static readonly AttachmentKind Inline = new("inline");
    public static readonly AttachmentKind Attachment = new("attachment");

    public AttachmentKind(string value)
        : base(value, AllowedValues)
    {
    }

    public static AttachmentKind From(string value) => new(value);
}
