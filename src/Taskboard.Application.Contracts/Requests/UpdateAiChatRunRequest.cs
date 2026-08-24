namespace Taskboard.Requests;

public sealed record UpdateAiChatRunRequest(
    string Status,
    int? ExitCode);
