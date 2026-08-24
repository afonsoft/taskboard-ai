using System.Net.Http.Json;
using Taskboard.Dtos;

namespace Taskboard.Blazor.Services;

/// <summary>
/// Cliente HTTP para consumir a API REST do Taskboard a partir do frontend Blazor.
/// </summary>
public sealed class TaskboardClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Cria uma nova instância de <see cref="TaskboardClient"/>.
    /// </summary>
    public TaskboardClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retorna todos os projetos cadastrados.
    /// </summary>
    public async Task<IReadOnlyCollection<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<ProjectListResponse>("/api/projects", cancellationToken);
        return response?.Projects ?? [];
    }

    /// <summary>
    /// Retorna as tarefas do projeto especificado.
    /// </summary>
    public async Task<IReadOnlyCollection<TaskDto>> GetTasksAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<TaskListDto>($"/api/tasks?projectId={projectId}", cancellationToken);
        return response?.Tasks ?? [];
    }

    /// <summary>
    /// Retorna todas as threads de chat de IA.
    /// </summary>
    public async Task<IReadOnlyCollection<AiChatThreadDto>> GetAiChatThreadsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<AiChatThreadListResponse>("/api/local/ai/threads", cancellationToken);
        return response?.Threads ?? [];
    }

    private sealed record ProjectListResponse(List<ProjectDto> Projects);
    private sealed record AiChatThreadListResponse(List<AiChatThreadDto> Threads);
}
