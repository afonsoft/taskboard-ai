using System.Net.Http.Json;
using Taskboard.Dtos;

namespace Taskboard.Blazor.Services;

public sealed class TaskboardClient
{
    private readonly HttpClient _httpClient;

    public TaskboardClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<ProjectListResponse>("/api/projects", cancellationToken);
        return response?.Projects ?? [];
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetTasksAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<TaskListDto>($"/api/tasks?projectId={projectId}", cancellationToken);
        return response?.Tasks ?? [];
    }

    public async Task<IReadOnlyCollection<AiChatThreadDto>> GetAiChatThreadsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<AiChatThreadListResponse>("/api/local/ai/threads", cancellationToken);
        return response?.Threads ?? [];
    }

    private sealed record ProjectListResponse(List<ProjectDto> Projects);
    private sealed record AiChatThreadListResponse(List<AiChatThreadDto> Threads);
}
