namespace Taskboard.Dtos;

public sealed record AiChatModelDto(
    string Id,
    string Provider,
    string Name,
    bool ReasoningEffortSupported);
