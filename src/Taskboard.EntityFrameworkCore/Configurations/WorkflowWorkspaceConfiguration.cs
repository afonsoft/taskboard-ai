using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class WorkflowWorkspaceConfiguration : IEntityTypeConfiguration<WorkflowWorkspace>
{
    public void Configure(EntityTypeBuilder<WorkflowWorkspace> builder)
    {
        builder.ToTable("WorkflowWorkspaces");

        builder.Property(w => w.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<ProjectId>());

        builder.HasKey(w => w.Id);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(w => w.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.Workspace)
            .IsRequired();

        builder.Property(w => w.UpdatedAt);
    }
}
