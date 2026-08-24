using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;
using Taskboard.EntityFrameworkCore.ValueConverters;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.Property(c => c.Id)
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<CommentId>());

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TaskId)
            .IsRequired()
            .HasMaxLength(128)
            .HasConversion(new StringIdValueConverter<TaskId>());

        builder.HasOne<Domain.Entities.Task>()
            .WithMany()
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Body)
            .IsRequired();

        builder.Property(c => c.Author)
            .IsRequired()
            .HasColumnName("author")
            .HasConversion(new JsonValueConverter<Actor>());

        builder.Property(c => c.ThreadId)
            .HasMaxLength(128);

        builder.Property(c => c.CreatedAt);
        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => new { c.TaskId, c.CreatedAt, c.Id })
            .HasDatabaseName("IX_Comments_Task_Created");
    }
}
