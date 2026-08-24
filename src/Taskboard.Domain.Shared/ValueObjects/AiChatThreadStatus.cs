namespace Taskboard.ValueObjects;

public sealed record AiChatThreadStatus : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "idle",
        "running",
        "failed"
    };

    public static readonly AiChatThreadStatus Idle = new("idle");
    public static readonly AiChatThreadStatus Running = new("running");
    public static readonly AiChatThreadStatus Failed = new("failed");

    public AiChatThreadStatus(string value)
        : base(value, AllowedValues)
    {
    }

    public static AiChatThreadStatus From(string value) => new(value);
}
