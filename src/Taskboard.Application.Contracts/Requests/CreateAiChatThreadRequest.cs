namespace Taskboard.Requests;

public sealed record CreateAiChatThreadRequest(
    string Title,
    string? OriginProjectId,
    string Model,
    string ReasoningEffort,
    string Sandbox);
