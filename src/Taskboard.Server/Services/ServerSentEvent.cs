namespace Taskboard.Server.Services;

public sealed record ServerSentEvent(string Type, object Payload);
