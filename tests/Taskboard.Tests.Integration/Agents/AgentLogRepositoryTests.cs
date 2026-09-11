using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using Taskboard.Agents;
using Taskboard.EntityFrameworkCore.Agents;
using Taskboard.EntityFrameworkCore.Data;

namespace Taskboard.Tests.Integration.Agents;

public class AgentLogRepositoryTests
{
    [Fact]
    public async Task Dado_LogInserido_Quando_BuscarPorIssueId_Entao_RetornaLogNaOrdemCronologica()
    {
        // Covers FR-002: Repositório
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TaskboardDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new TaskboardDbContext(options);
        await context.Database.EnsureCreatedAsync();

        IAgentLogRepository repository = new EfCoreAgentLogRepository(context);

        var message = new AgentLogMessage(
            new DateTimeOffset(2026, 9, 11, 12, 0, 0, TimeSpan.Zero),
            "issue-1",
            AgentLogStream.StdOut,
            "hello");

        await repository.AppendAsync(message);

        var logs = await repository.GetByIssueIdAsync("issue-1");

        logs.Count.ShouldBe(1);
        logs[0].Content.ShouldBe("hello");
        logs[0].Stream.ShouldBe(AgentLogStream.StdOut);
    }
}
