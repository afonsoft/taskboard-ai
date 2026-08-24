using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class AiChatRunConfiguration : IEntityTypeConfiguration<AiChatRun>
{
    public void Configure(EntityTypeBuilder<AiChatRun> builder)
    {
        builder.ToTable("AiChatRuns");

        builder.Property(r => r.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<AiChatRunId>());

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ThreadId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<AiChatThreadId>());

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(r => r.ExitCode);

        builder.Property(r => r.CreatedAt);
        builder.Property(r => r.FinishedAt);
    }
}
