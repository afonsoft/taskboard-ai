using System.Text.RegularExpressions;

namespace Taskboard.ValueObjects;

public sealed partial record TaskIdentifier : StringValueObject
{
    private static readonly Regex LocalTaskPattern = LocalRegex();

    public TaskIdentifier(string value)
        : base(value)
    {
        if (!LocalTaskPattern.IsMatch(value) && !value.StartsWith("JIRA:", StringComparison.Ordinal))
        {
            throw new DomainException(
                TaskboardDomainErrorCodes.InvalidTaskIdentifier,
                $"'{value}' is not a valid task identifier.");
        }
    }

    public static TaskIdentifier ForLocalTask(ProjectId projectId, long number)
    {
        if (projectId is null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        return new TaskIdentifier($"TASK-{projectId.Value}-{number}");
    }

    public static TaskIdentifier ForJira(string origin, string externalKey)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("Origin cannot be empty.", nameof(origin));
        }

        if (string.IsNullOrWhiteSpace(externalKey))
        {
            throw new ArgumentException("External key cannot be empty.", nameof(externalKey));
        }

        return new TaskIdentifier($"JIRA:{origin}:{externalKey}");
    }

    [GeneratedRegex("^TASK-.+-\\d+$")]
    private static partial Regex LocalRegex();
}
