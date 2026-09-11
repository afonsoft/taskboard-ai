namespace Taskboard.Requests;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
