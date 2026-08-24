namespace Taskboard.ValueObjects;

public sealed record TaskStatus : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "backlog",
        "todo",
        "in_progress",
        "in_review",
        "blocked",
        "done",
        "canceled"
    };

    public static readonly TaskStatus Backlog = new("backlog");
    public static readonly TaskStatus Todo = new("todo");
    public static readonly TaskStatus InProgress = new("in_progress");
    public static readonly TaskStatus InReview = new("in_review");
    public static readonly TaskStatus Blocked = new("blocked");
    public static readonly TaskStatus Done = new("done");
    public static readonly TaskStatus Canceled = new("canceled");

    public TaskStatus(string value)
        : base(value, AllowedValues)
    {
    }

    public static TaskStatus From(string value) => new(value);
}
