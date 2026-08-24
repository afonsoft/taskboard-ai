using Taskboard.Dtos;
using Taskboard.Requests;

namespace Taskboard.Server.Services;

public sealed class CloudSessionService
{
    private CloudSessionDto _session = new(false, null, null, null);

    public CloudSessionDto Get() => _session;

    public CloudSessionDto Update(UpdateCloudSessionRequest request)
    {
        var connected = request.Connected ?? _session.Connected;
        var companionUrl = request.CompanionUrl ?? _session.CompanionUrl;
        var username = request.Username ?? _session.Username;
        var projectId = request.ProjectId ?? _session.ProjectId;

        if (request.Connected.HasValue && !request.Connected.Value)
        {
            connected = false;
        }

        _session = new CloudSessionDto(connected, companionUrl, username, projectId);
        return _session;
    }
}
