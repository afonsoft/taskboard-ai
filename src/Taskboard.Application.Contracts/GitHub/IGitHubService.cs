namespace Taskboard.GitHub;

/// <summary>
/// Serviço de integração com a API do GitHub para sincronização do Taskboard.
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Configura o token de autenticação do GitHub (PAT ou token OAuth).
    /// </summary>
    void SetToken(string token);

    /// <summary>
    /// Lista todos os repositórios que o usuário autenticado pode acessar.
    /// </summary>
    Task<IReadOnlyList<RepositoryDto>> GetRepositoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna as issues abertas de um repositório, opcionalmente filtradas por labels.
    /// </summary>
    Task<IReadOnlyList<IssueDto>> GetIssuesAsync(string repositoryFullName, IReadOnlyCollection<string>? labels = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza a coluna (label) de uma issue, removendo a label anterior e adicionando a nova.
    /// </summary>
    Task<IssueDto> UpdateIssueColumnAsync(string repositoryFullName, int issueNumber, GitHubBoardColumn? oldColumn, GitHubBoardColumn newColumn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma nova issue no repositório e aplica a label inicial correspondente à coluna informada.
    /// </summary>
    Task<IssueDto> CreateIssueAsync(string repositoryFullName, string title, string? body, GitHubBoardColumn initialColumn, CancellationToken cancellationToken = default);
}
