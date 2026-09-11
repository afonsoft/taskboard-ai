namespace Taskboard.Application.Contracts.Skills;

public interface ISkillDiscoveryService
{
    Task<IReadOnlyList<SkillDto>> DiscoverAsync(CancellationToken cancellationToken = default);
}
