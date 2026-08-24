using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.ToTable("WorkflowNodes");

        builder.Property(n => n.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<WorkflowNodeId>());

        builder.HasKey(n => n.Id);

        builder.Property(n => n.ProjectId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(n => n.Config)
            .IsRequired();

        builder.Property(n => n.PositionX);
        builder.Property(n => n.PositionY);

        builder.Property(n => n.CreatedAt);
    }
}
