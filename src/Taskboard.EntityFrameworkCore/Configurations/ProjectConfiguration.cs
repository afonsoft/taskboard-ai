using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            .HasConversion(new ListStringJsonValueConverter())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList()));

        builder.Property(p => p.NextTaskNumber)
            .HasDefaultValue(1L);

        builder.Property(p => p.CreatedAt);
        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.Version)
            .IsConcurrencyToken();
    }
}
