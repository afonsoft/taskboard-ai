namespace Taskboard.ValueObjects;

public sealed record TaskPriority : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "none",
        "urgent",
        "high",
        "medium",
        "low"
    };

    public static readonly TaskPriority None = new("none");
    public static readonly TaskPriority Urgent = new("urgent");
    public static readonly TaskPriority High = new("high");
    public static readonly TaskPriority Medium = new("medium");
    public static readonly TaskPriority Low = new("low");

    public TaskPriority(string value)
        : base(value, AllowedValues)
    {
    }

    public static TaskPriority From(string value) => new(value);
}
