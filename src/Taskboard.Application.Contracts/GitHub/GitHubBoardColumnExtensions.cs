namespace Taskboard.GitHub;

/// <summary>
/// Extensões para converter <see cref="GitHubBoardColumn"/> em labels do GitHub.
/// </summary>
public static class GitHubBoardColumnExtensions
{
    private static readonly IReadOnlyDictionary<GitHubBoardColumn, string> LabelMap = new Dictionary<GitHubBoardColumn, string>
    {
        [GitHubBoardColumn.Backlog] = "backlog",
        [GitHubBoardColumn.InProgress] = "in-progress",
        [GitHubBoardColumn.Review] = "review",
        [GitHubBoardColumn.Done] = "done"
    };

    /// <summary>
    /// Retorna a label do GitHub correspondente à coluna.
    /// </summary>
    public static string ToLabel(this GitHubBoardColumn column) => LabelMap[column];

    /// <summary>
    /// Tenta identificar a coluna do Kanban a partir de uma label do GitHub.
    /// </summary>
    public static GitHubBoardColumn? FromLabel(string label)
    {
        foreach (var pair in LabelMap)
        {
            if (string.Equals(pair.Value, label, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Retorna todas as labels de coluna suportadas.
    /// </summary>
    public static IReadOnlyCollection<string> GetAllLabels() => LabelMap.Values.ToList().AsReadOnly();
}
