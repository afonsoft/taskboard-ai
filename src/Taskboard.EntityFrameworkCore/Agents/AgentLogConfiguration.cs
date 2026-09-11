using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Agents;

namespace Taskboard.EntityFrameworkCore.Agents;

public sealed class AgentLogConfiguration : IEntityTypeConfiguration<AgentLog>
{
    public void Configure(EntityTypeBuilder<AgentLog> builder)
    {
        builder.ToTable("AgentLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Timestamp)
            .IsRequired()
            .HasConversion(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v));
        builder.Property(x => x.IssueId).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Stream).IsRequired();
        builder.Property(x => x.Content).IsRequired();
    }
}
