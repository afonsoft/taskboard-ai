using Shouldly;
using Taskboard.GitHub;
using Xunit;

namespace Taskboard.Tests.Unit.Application.Contracts.GitHub;

public class GitHubBoardColumnExtensionsTests
{
    [Theory]
    [InlineData(GitHubBoardColumn.Backlog, "backlog")]
    [InlineData(GitHubBoardColumn.InProgress, "in-progress")]
    [InlineData(GitHubBoardColumn.Review, "review")]
    [InlineData(GitHubBoardColumn.Done, "done")]
    public void Dado_ColunaValida_Quando_ConverterParaLabel_Entao_RetornaLabelCorrespondente(
        GitHubBoardColumn column,
        string expectedLabel)
    {
        column.ToLabel().ShouldBe(expectedLabel);
    }

    [Theory]
    [InlineData("backlog", GitHubBoardColumn.Backlog)]
    [InlineData("in-progress", GitHubBoardColumn.InProgress)]
    [InlineData("review", GitHubBoardColumn.Review)]
    [InlineData("done", GitHubBoardColumn.Done)]
    [InlineData("BACKLOG", GitHubBoardColumn.Backlog)]
    [InlineData("In-Progress", GitHubBoardColumn.InProgress)]
    public void Dado_LabelValida_Quando_IdentificarColuna_Entao_RetornaColunaCorrespondente(
        string label,
        GitHubBoardColumn expectedColumn)
    {
        GitHubBoardColumnExtensions.FromLabel(label).ShouldBe(expectedColumn);
    }

    [Fact]
    public void Dado_LabelNaoMapeada_Quando_IdentificarColuna_Entao_RetornaNulo()
    {
        GitHubBoardColumnExtensions.FromLabel("invalid-label").ShouldBeNull();
    }

    [Fact]
    public void Dado_MapeamentoDeColunas_Quando_ObterTodasLabels_Entao_RetornaLabelsEsperadas()
    {
        var labels = GitHubBoardColumnExtensions.GetAllLabels();

        labels.Count.ShouldBe(4);
        labels.ShouldContain("backlog");
        labels.ShouldContain("in-progress");
        labels.ShouldContain("review");
        labels.ShouldContain("done");
    }
}
