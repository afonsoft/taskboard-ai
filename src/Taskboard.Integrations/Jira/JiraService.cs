using Taskboard.Dtos;
using Taskboard.Requests;

namespace Taskboard.Integrations.Jira;

public sealed class JiraService : IJiraService
{
    private readonly HttpClient _httpClient;
    private JiraConnectionDto _connection = new(false, null, null, null);
    private string? _token;

    public JiraService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public JiraConnectionDto GetConnection() => _connection;

    public JiraConnectionDto UpdateConnection(UpdateJiraConnectionRequest request)
    {
        var url = request.Url ?? _connection.Url;
        var email = request.Email ?? _connection.Email;
        var projectKey = request.ProjectKey ?? _connection.ProjectKey;
        _token = request.Token ?? _token;

        var connected = !string.IsNullOrWhiteSpace(url)
                          && !string.IsNullOrWhiteSpace(email)
                          && !string.IsNullOrWhiteSpace(_token);

        _connection = new JiraConnectionDto(connected, url, email, projectKey);
        return _connection;
    }

    public async Task<JiraConnectionDto> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!_connection.Connected)
        {
            return new JiraConnectionDto(false, _connection.Url, _connection.Email, _connection.ProjectKey);
        }

        try
        {
            var url = _connection.Url!;
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{url.TrimEnd('/')}/rest/api/3/myself");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_connection.Email!}:{_token}")));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var connected = response.IsSuccessStatusCode;
            _connection = new JiraConnectionDto(connected, _connection.Url, _connection.Email, _connection.ProjectKey);
        }
        catch
        {
            _connection = new JiraConnectionDto(false, _connection.Url, _connection.Email, _connection.ProjectKey);
        }

        return _connection;
    }

    public Task<JiraSyncResultDto> SyncAsync(CancellationToken cancellationToken = default)
    {
        if (!_connection.Connected)
        {
            return Task.FromResult(new JiraSyncResultDto(false, 0, 0, 0));
        }

        // Sync real com Jira fica fora do escopo desta implementação inicial.
        return Task.FromResult(new JiraSyncResultDto(true, 0, 0, 0));
    }
}
