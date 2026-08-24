namespace Taskboard.ValueObjects;

public sealed record Recurrence
{
    private static readonly IReadOnlyCollection<string> AllowedUnits = new HashSet<string>(StringComparer.Ordinal)
    {
        "day",
        "week",
        "month",
        "year"
    };

    public int Interval { get; }
    public string Unit { get; }

    public Recurrence(int interval, string unit)
    {
        if (interval <= 0)
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidValue, "Recurrence interval must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(unit) || !AllowedUnits.Contains(unit))
        {
            throw new DomainException(TaskboardDomainErrorCodes.InvalidRecurrenceUnit, $"'{unit}' is not a valid recurrence unit.");
        }

        Interval = interval;
        Unit = unit;
    }
}
