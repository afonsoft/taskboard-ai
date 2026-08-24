using System.Collections.Concurrent;
using System.Text.Json;
using Taskboard.Dtos;
using Taskboard.Requests;

namespace Taskboard.Server.Services;

public sealed class WorkflowCapabilityService
{
    private readonly ConcurrentDictionary<string, JsonElement> _capabilities = new();

    public IReadOnlyCollection<WorkflowCapabilityDto> List()
        => _capabilities
            .Select(kvp => new WorkflowCapabilityDto(kvp.Key, kvp.Value))
            .ToList();

    public WorkflowCapabilityDto Get(string deviceId)
        => _capabilities.TryGetValue(deviceId, out var capabilities)
            ? new WorkflowCapabilityDto(deviceId, capabilities)
            : new WorkflowCapabilityDto(deviceId, JsonSerializer.SerializeToElement(new { }));

    public WorkflowCapabilityDto Upsert(UpdateWorkflowCapabilitiesRequest request)
    {
        _capabilities[request.DeviceId] = request.Capabilities;
        return new WorkflowCapabilityDto(request.DeviceId, request.Capabilities);
    }
}
