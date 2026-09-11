using System;
using Taskboard.Agents;

namespace Taskboard.Domain.Entities;

public sealed class AgentPreference : Entity<Guid>
{
    public AgentType AgentType { get; set; }

    public bool Enabled { get; set; } = true;

    public AgentPreference(Guid id, AgentType agentType)
        : base(id)
    {
        AgentType = agentType;
    }
}
