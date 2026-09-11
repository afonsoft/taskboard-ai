using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Taskboard.Agents;
using Taskboard.Application.Contracts.Settings;
using Taskboard.Requests;
using Taskboard.Domain.Entities;
using Taskboard.Repositories;

namespace Taskboard.Application.Settings;

public sealed class SettingsService
{
    private readonly IRepository<UserPreference> _userPreferenceRepo;
    private readonly IRepository<AgentPreference> _agentPreferenceRepo;
    private readonly IAgentDiscoveryService _agentDiscovery;

    public SettingsService(
        IRepository<UserPreference> userPreferenceRepo,
        IRepository<AgentPreference> agentPreferenceRepo,
        IAgentDiscoveryService agentDiscovery)
    {
        _userPreferenceRepo = userPreferenceRepo;
        _agentPreferenceRepo = agentPreferenceRepo;
        _agentDiscovery = agentDiscovery;
    }

    public async Task<SettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userPreferenceRepo.ListAsync(cancellationToken);
        var user = users.FirstOrDefault() ?? new UserPreference(Guid.Empty);

        var enabled = await _agentPreferenceRepo.ListAsync(cancellationToken);
        var enabledByType = enabled.ToDictionary(a => a.AgentType);

        var discovered = await _agentDiscovery.DiscoverAsync(cancellationToken);

        var agents = discovered
            .Select(a => new AgentPreferenceDto(
                a.Type,
                a.Name,
                a.ExecutablePath,
                a.Version,
                enabledByType.TryGetValue(a.Type, out var preference) && preference.Enabled))
            .ToList()
            .AsReadOnly();

        return new SettingsDto(user.Theme, user.GitHubToken, agents);
    }

    public async Task SaveSettingsAsync(SaveSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var users = await _userPreferenceRepo.ListAsync(cancellationToken);
        var existing = users.FirstOrDefault();
        if (existing is null)
        {
            await _userPreferenceRepo.AddAsync(
                new UserPreference(Guid.Empty)
                {
                    Theme = request.Theme,
                    GitHubToken = request.GitHubToken
                },
                cancellationToken);
        }
        else
        {
            existing.Theme = request.Theme;
            existing.GitHubToken = request.GitHubToken;
            await _userPreferenceRepo.UpdateAsync(existing, cancellationToken);
        }

        var current = await _agentPreferenceRepo.ListAsync(cancellationToken);
        foreach (var preference in current)
        {
            await _agentPreferenceRepo.DeleteAsync(preference, cancellationToken);
        }

        var enabledTypes = new HashSet<AgentType>();
        foreach (var name in request.EnabledAgents)
        {
            if (Enum.TryParse<AgentType>(name, out var type))
            {
                enabledTypes.Add(type);
            }
        }

        foreach (var type in enabledTypes)
        {
            await _agentPreferenceRepo.AddAsync(
                new AgentPreference(Guid.NewGuid(), type) { Enabled = true },
                cancellationToken);
        }

        await _userPreferenceRepo.SaveChangesAsync(cancellationToken);
    }
}
