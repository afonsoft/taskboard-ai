namespace Taskboard.ValueObjects;

public sealed record AiChatThreadId : StringIdBase
{
    public AiChatThreadId(string value)
        : base(value)
    {
    }

    public static AiChatThreadId From(string value) => new(value);

    public static AiChatThreadId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
