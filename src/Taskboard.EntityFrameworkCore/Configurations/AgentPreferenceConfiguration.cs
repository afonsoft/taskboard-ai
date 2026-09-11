using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskboard.Domain.Entities;

namespace Taskboard.EntityFrameworkCore.Configurations;

public sealed class AgentPreferenceConfiguration : IEntityTypeConfiguration<AgentPreference>
{
    public void Configure(EntityTypeBuilder<AgentPreference> builder)
    {
        builder.ToTable("AgentPreferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AgentType)
            .IsRequired();

        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.HasIndex(x => x.AgentType)
            .IsUnique();
    }
}
