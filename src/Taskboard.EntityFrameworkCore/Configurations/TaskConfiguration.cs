using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using TaskStatus = Taskboard.ValueObjects.TaskStatus;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Domain.Entities.Task>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Task> builder)
    {
        builder.ToTable("Tasks");

        builder.Property(t => t.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Identifier)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringValueObjectConverter<TaskIdentifier>());

        builder.HasIndex(t => t.Identifier)
            .IsUnique()
            .HasDatabaseName("UIX_Tasks_Identifier");

        builder.Property(t => t.ProjectId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(240);

        builder.Property(t => t.Description);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion(new StringValueObjectConverter<TaskStatus>());

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion(new StringValueObjectConverter<TaskPriority>());

        builder.Property<List<string>>("_labels")
            .HasColumnName("labels")
            .HasConversion(new ListStringJsonValueConverter())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList()));

        builder.Property(t => t.SortOrder);

        builder.Property(t => t.ThreadBinding)
            .HasColumnName("thread_binding")
            .HasConversion(new NullableJsonValueConverter<ThreadBinding>());

        builder.Property(t => t.Creator)
            .IsRequired()
            .HasColumnName("creator")
            .HasConversion(new JsonValueConverter<Actor>());

        builder.Property(t => t.Assignee)
            .HasColumnName("assignee")
            .HasConversion(new NullableJsonValueConverter<Actor>());

        builder.Property(t => t.WorkflowId)
            .HasMaxLength(128);

        builder.Property(t => t.GitBranch)
            .HasMaxLength(256);

        builder.Property(t => t.WorktreePath)
            .HasMaxLength(1024);

        builder.Property(t => t.WorktreeBranch)
            .HasMaxLength(256);

        builder.Property(t => t.StartDate);
        builder.Property(t => t.DueDate);

        builder.Property(t => t.Recurrence)
            .HasColumnName("recurrence")
            .HasConversion(new NullableJsonValueConverter<Recurrence>());

        builder.Property(t => t.ExternalSource)
            .HasMaxLength(64);

        builder.Property(t => t.ExternalOrigin)
            .HasMaxLength(128);

        builder.Property(t => t.ExternalId)
            .HasMaxLength(256);

        builder.Property(t => t.ExternalKey)
            .HasMaxLength(256);

        builder.Property(t => t.ExternalUrl)
            .HasMaxLength(2048);

        builder.HasIndex(t => new { t.ExternalSource, t.ExternalOrigin, t.ExternalId })
            .IsUnique()
            .HasDatabaseName("UIX_Tasks_External");

        builder.Property(t => t.ArchivedAt);

        builder.Property(t => t.Version)
            .IsConcurrencyToken();

        builder.Property(t => t.CreatedAt);
        builder.Property(t => t.UpdatedAt);

        builder.HasIndex(t => new { t.ProjectId, t.ArchivedAt, t.Status, t.SortOrder, t.CreatedAt })
            .HasDatabaseName("IX_Tasks_Project_Status_Sort");
    }
}
