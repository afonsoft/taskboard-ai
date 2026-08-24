using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class WorkflowSequenceConfiguration : IEntityTypeConfiguration<WorkflowSequence>
{
    public void Configure(EntityTypeBuilder<WorkflowSequence> builder)
    {
        builder.ToTable("WorkflowSequences");

        builder.Property(s => s.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<WorkflowSequenceId>());

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProjectId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.SourceNodeId)
            .HasMaxLength(128)
            .HasConversion(new NullableStringIdValueConverter<WorkflowNodeId>());

        builder.HasOne<WorkflowNode>()
            .WithMany()
            .HasForeignKey(s => s.SourceNodeId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_WorkflowSequences_SourceNode");

        builder.Property(s => s.TargetNodeId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<WorkflowNodeId>());

        builder.HasOne<WorkflowNode>()
            .WithMany()
            .HasForeignKey(s => s.TargetNodeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_WorkflowSequences_TargetNode");

        builder.Property(s => s.Condition);

        builder.Property(s => s.Order);

        builder.Property(s => s.CreatedAt);
    }
}
