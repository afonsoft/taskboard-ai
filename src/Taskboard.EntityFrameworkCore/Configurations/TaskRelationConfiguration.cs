using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class TaskRelationConfiguration : IEntityTypeConfiguration<TaskRelation>
{
    public void Configure(EntityTypeBuilder<TaskRelation> builder)
    {
        builder.ToTable("TaskRelations");

        builder.Property(r => r.Id);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SourceTaskId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.Property(r => r.TargetTaskId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.HasOne<Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(r => r.SourceTaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_TaskRelations_SourceTask");

        builder.HasOne<Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(r => r.TargetTaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_TaskRelations_TargetTask");

        builder.Property(r => r.RelationType)
            .IsRequired()
            .HasMaxLength(32)
            .HasColumnName("relation_type")
            .HasConversion(new StringValueObjectConverter<RelationType>());

        builder.Property(r => r.CreatedAt);

        builder.HasIndex(r => r.TargetTaskId)
            .IsUnique()
            .HasDatabaseName("UIX_TaskRelations_Parent")
            .HasFilter("relation_type = 'parent'");
    }
}
