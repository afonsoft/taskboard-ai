namespace Taskboard.Requests;

public sealed record AddAiChatEventRequest(
    string Role,
    string Content);
