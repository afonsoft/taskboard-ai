using System;

namespace Taskboard.Domain.Entities;

public sealed class UserPreference : Entity<Guid>
{
    public string Theme { get; set; } = "dark";

    public string? GitHubToken { get; set; }

    public UserPreference(Guid id)
        : base(id)
    {
    }
}
