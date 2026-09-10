using Microsoft.AspNetCore.SignalR;
using Taskboard.Agents;

namespace Taskboard.Server.Hubs;

/// <summary>
/// Hub SignalR que transmite logs de execução dos agentes CLI para os clientes.
/// </summary>
public sealed class AgentLogHub : Hub
{
    /// <summary>
    /// Método invocado pelo cliente para se inscrever nos logs de uma issue.
    /// </summary>
    public async Task SubscribeToIssue(string issueId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, issueId);
    }

    /// <summary>
    /// Método invocado pelo cliente para cancelar a inscrição nos logs de uma issue.
    /// </summary>
    public async Task UnsubscribeFromIssue(string issueId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, issueId);
    }
}
