using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Theme)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.GitHubToken)
            .HasMaxLength(256);
    }
}
