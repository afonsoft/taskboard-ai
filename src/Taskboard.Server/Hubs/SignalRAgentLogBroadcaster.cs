using Microsoft.AspNetCore.SignalR;
using Taskboard.Agents;

namespace Taskboard.Server.Hubs;

/// <summary>
/// Implementação do broadcast de logs usando SignalR.
/// </summary>
public sealed class SignalRAgentLogBroadcaster : IAgentLogBroadcaster
{
    private readonly IHubContext<AgentLogHub> _hubContext;

    public SignalRAgentLogBroadcaster(IHubContext<AgentLogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastAsync(AgentLogMessage message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(message.IssueId)
            .SendAsync("ReceiveLog", message, cancellationToken: cancellationToken);
    }
}
