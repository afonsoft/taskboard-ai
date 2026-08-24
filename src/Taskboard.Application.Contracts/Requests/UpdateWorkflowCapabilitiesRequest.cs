using System.Text.Json;

namespace Taskboard.Requests;

public sealed record UpdateWorkflowCapabilitiesRequest(
    string DeviceId,
    JsonElement Capabilities);
