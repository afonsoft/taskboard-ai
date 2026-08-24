using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
{
    public void Configure(EntityTypeBuilder<TaskActivity> builder)
    {
        builder.ToTable("TaskActivities");

        builder.Property(a => a.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskActivityId>());

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TaskId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.HasOne<Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Actor)
            .IsRequired()
            .HasColumnName("actor")
            .HasConversion(new JsonValueConverter<Actor>());

        builder.Property(a => a.Changes)
            .IsRequired();

        builder.Property(a => a.Timestamp);

        builder.HasIndex(a => new { a.TaskId, a.Timestamp, a.Id })
            .HasDatabaseName("IX_TaskActivities_Task_Created");
    }
}
