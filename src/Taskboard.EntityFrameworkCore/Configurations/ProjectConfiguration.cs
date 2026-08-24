using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.Property(p => p.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.WorkspacePath)
            .HasMaxLength(1024);

        builder.Property<List<string>>("_labels")
            .HasColumnName("labels")
            .HasConversion(new ListStringJsonValueConverter());

        builder.Property(p => p.NextTaskNumber)
            .HasDefaultValue(1L);

        builder.Property(p => p.CreatedAt);
        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.Version)
            .IsConcurrencyToken();
    }
}
