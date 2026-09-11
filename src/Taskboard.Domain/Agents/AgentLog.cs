using Taskboard;
using Taskboard.Agents;

namespace Taskboard.Domain.Agents;

public sealed class AgentLog : Entity<long>
{
    public DateTimeOffset Timestamp { get; private set; }
    public string IssueId { get; private set; } = string.Empty;
    public AgentLogStream Stream { get; private set; }
    public string Content { get; private set; } = string.Empty;

    private AgentLog()
    {
    }

    public AgentLog(DateTimeOffset timestamp, string issueId, AgentLogStream stream, string content)
    {
        Timestamp = timestamp;
        IssueId = issueId;
        Stream = stream;
        Content = content;
    }
}
