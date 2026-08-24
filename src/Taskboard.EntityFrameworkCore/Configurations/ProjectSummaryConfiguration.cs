using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class ProjectSummaryConfiguration : IEntityTypeConfiguration<ProjectSummary>
{
    public void Configure(EntityTypeBuilder<ProjectSummary> builder)
    {
        builder.ToTable("ProjectSummaries");

        builder.Property(s => s.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasKey(s => s.Id);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Summary)
            .IsRequired();

        builder.Property(s => s.GeneratedAt);
    }
}
