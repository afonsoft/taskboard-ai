namespace Taskboard.ValueObjects;

public sealed record AiChatEventRole : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "user",
        "assistant",
        "activity",
        "error"
    };

    public static readonly AiChatEventRole User = new("user");
    public static readonly AiChatEventRole Assistant = new("assistant");
    public static readonly AiChatEventRole Activity = new("activity");
    public static readonly AiChatEventRole Error = new("error");

    public AiChatEventRole(string value)
        : base(value, AllowedValues)
    {
    }

    public static AiChatEventRole From(string value) => new(value);
}
