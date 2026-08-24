using Taskboard.Dtos;
using Taskboard.Requests;

namespace Taskboard.Integrations.Jira;

public interface IJiraService
{
    JiraConnectionDto GetConnection();

    JiraConnectionDto UpdateConnection(UpdateJiraConnectionRequest request);

    Task<JiraConnectionDto> TestConnectionAsync(CancellationToken cancellationToken = default);

    Task<JiraSyncResultDto> SyncAsync(CancellationToken cancellationToken = default);
}
