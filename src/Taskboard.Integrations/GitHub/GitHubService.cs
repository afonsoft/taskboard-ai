using Octokit;
using Taskboard.GitHub;

namespace Taskboard.Integrations.GitHub;

/// <summary>
/// Implementação do <see cref="IGitHubService"/> usando a biblioteca oficial Octokit.
/// </summary>
public sealed class GitHubService : IGitHubService
{
    private const string ProductName = "TaskBoardAI";
    private readonly GitHubClient _client;

    /// <summary>
    /// Cria uma nova instância do serviço de integração com GitHub.
    /// </summary>
    public GitHubService()
    {
        _client = new GitHubClient(new ProductHeaderValue(ProductName));

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.Credentials = new Credentials(token);
        }
    }

    /// <inheritdoc />
    public void SetToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        _client.Credentials = new Credentials(token);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RepositoryDto>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var apiOptions = new ApiOptions { PageSize = 100 };
        var repositories = await _client.Repository.GetAllForCurrent(apiOptions);

        return repositories
            .OrderBy(r => r.FullName)
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IssueDto>> GetIssuesAsync(
        string repositoryFullName,
        IReadOnlyCollection<string>? labels = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var (owner, name) = SplitRepositoryName(repositoryFullName);

        var request = new RepositoryIssueRequest
        {
            State = ItemStateFilter.Open,
            SortProperty = IssueSort.Created,
            SortDirection = SortDirection.Descending
        };

        if (labels is not null && labels.Count > 0)
        {
            foreach (var label in labels)
            {
                request.Labels.Add(label);
            }
        }

        var issues = await _client.Issue.GetAllForRepository(owner, name, request);
        return issues.Select(i => MapToDto(i, repositoryFullName)).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IssueDto> UpdateIssueColumnAsync(
        string repositoryFullName,
        int issueNumber,
        GitHubBoardColumn? oldColumn,
        GitHubBoardColumn newColumn,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var (owner, name) = SplitRepositoryName(repositoryFullName);
        var newLabel = newColumn.ToLabel();

        if (oldColumn.HasValue && oldColumn.Value != newColumn)
        {
            var oldLabel = oldColumn.Value.ToLabel();
            try
            {
                await _client.Issue.Labels.RemoveFromIssue(owner, name, issueNumber, oldLabel);
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Label já removida; prossegue.
            }
        }

        await _client.Issue.Labels.AddToIssue(owner, name, issueNumber, [newLabel]);

        var updatedIssue = await _client.Issue.Get(owner, name, issueNumber);
        return MapToDto(updatedIssue, repositoryFullName);
    }

    /// <inheritdoc />
    public async Task<IssueDto> CreateIssueAsync(
        string repositoryFullName,
        string title,
        string? body,
        GitHubBoardColumn initialColumn,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var (owner, name) = SplitRepositoryName(repositoryFullName);

        var newIssue = new NewIssue(title)
        {
            Body = body
        };
        newIssue.Labels.Add(initialColumn.ToLabel());

        var issue = await _client.Issue.Create(owner, name, newIssue);
        return MapToDto(issue, repositoryFullName);
    }

    /// <inheritdoc />
    public async Task AddLabelsToIssueAsync(
        string repositoryFullName,
        int issueNumber,
        IReadOnlyCollection<string> labels,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var (owner, name) = SplitRepositoryName(repositoryFullName);

        if (labels.Count == 0)
        {
            return;
        }

        foreach (var label in labels)
        {
            await EnsureLabelExistsAsync(owner, name, label, cancellationToken);
        }

        await _client.Issue.Labels.AddToIssue(owner, name, issueNumber, labels.ToArray());
    }

    private async Task EnsureLabelExistsAsync(string owner, string name, string label, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.Issue.Labels.Get(owner, name, label);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _client.Issue.Labels.Create(owner, name, new NewLabel(label, "ededed"));
        }
    }

    private static RepositoryDto MapToDto(Repository repository) => new(
        repository.Id,
        repository.FullName,
        repository.Name,
        repository.Description,
        repository.HtmlUrl,
        repository.Private);

    private static IssueDto MapToDto(Issue issue, string repositoryFullName) => new(
        issue.Id,
        issue.Number,
        issue.Title,
        issue.Body,
        issue.State.StringValue,
        issue.Url,
        issue.HtmlUrl,
        issue.Labels?.Select(l => l.Name).ToList() ?? [],
        ResolveColumn(issue.Labels?.Select(l => l.Name) ?? []),
        issue.Assignee?.Login,
        issue.CreatedAt,
        issue.UpdatedAt);

    private static GitHubBoardColumn ResolveColumn(IEnumerable<string> labels)
    {
        foreach (var column in new[]
                 {
                     GitHubBoardColumn.Done,
                     GitHubBoardColumn.Review,
                     GitHubBoardColumn.InProgress,
                     GitHubBoardColumn.Backlog
                 })
        {
            var label = column.ToLabel();
            if (labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase)))
            {
                return column;
            }
        }

        return GitHubBoardColumn.Backlog;
    }

    private static (string Owner, string Name) SplitRepositoryName(string repositoryFullName)
    {
        var parts = repositoryFullName.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Repository name must be in the format 'owner/name'. Value: '{repositoryFullName}'", nameof(repositoryFullName));
        }

        return (parts[0], parts[1]);
    }

    private void EnsureAuthenticated()
    {
        if (_client.Credentials is null || string.IsNullOrWhiteSpace(_client.Credentials.Password))
        {
            throw new InvalidOperationException("GitHub token not configured. Set the GITHUB_TOKEN environment variable or call SetToken.");
        }
    }
}
