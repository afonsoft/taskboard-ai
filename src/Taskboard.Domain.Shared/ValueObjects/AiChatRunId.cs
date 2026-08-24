namespace Taskboard.ValueObjects;

public sealed record AiChatRunId : StringIdBase
{
    public AiChatRunId(string value)
        : base(value)
    {
    }

    public static AiChatRunId From(string value) => new(value);

    public static AiChatRunId NewGuid() => new(Guid.NewGuid().ToString("N"));
}
