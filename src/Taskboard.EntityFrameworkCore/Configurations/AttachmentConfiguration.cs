using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.Property(a => a.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<AttachmentId>());

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TaskId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.HasOne<Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.CommentId)
            .HasMaxLength(128)
            .HasConversion(new NullableStringIdValueConverter<CommentId>());

        builder.Property(a => a.Kind)
            .IsRequired()
            .HasMaxLength(32)
            .HasConversion(new StringValueObjectConverter<AttachmentKind>());

        builder.Property(a => a.Filename)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Size);

        builder.Property(a => a.Path)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(a => a.CreatedAt);

        builder.HasIndex(a => new { a.TaskId, a.CreatedAt, a.Id })
            .HasDatabaseName("IX_Attachments_Task_Created");
    }
}
