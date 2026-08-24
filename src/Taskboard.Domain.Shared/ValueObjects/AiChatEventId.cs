namespace Taskboard.ValueObjects;

public sealed record AiChatEventId : StringIdBase
{
    public AiChatEventId(string value)
        : base(value)
    {
    }

    public static AiChatEventId From(string value) => new(value);

    public static AiChatEventId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
