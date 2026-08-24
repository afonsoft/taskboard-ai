namespace Taskboard.ValueObjects;

public sealed record ThreadBinding
{
    public string ThreadId { get; }
    public string? Source { get; }
    public string? Name { get; }
    public string? Url { get; }
    public string? References { get; }

    public ThreadBinding(string threadId, string? source = null, string? name = null, string? url = null, string? references = null)
    {
        if (string.IsNullOrWhiteSpace(threadId))
        {
            throw new ArgumentException("ThreadId cannot be empty.", nameof(threadId));
        }

        ThreadId = threadId;
        Source = source;
        Name = name;
        Url = url;
        References = references;
    }
}
