using Taskboard.Application.Contracts.Skills;

namespace Taskboard.Integrations.Skills;

public sealed class SkillDiscoveryService : ISkillDiscoveryService
{
    private readonly IReadOnlyList<SkillDiscoverySource> _sources;

    public SkillDiscoveryService(IEnumerable<SkillDiscoverySource> sources)
    {
        _sources = sources.ToList().AsReadOnly();
    }

    public Task<IReadOnlyList<SkillDto>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var skills = new List<SkillDto>();

        foreach (var source in _sources)
        {
            if (!Directory.Exists(source.Path))
            {
                continue;
            }

            foreach (var directory in Directory.EnumerateDirectories(source.Path))
            {
                var skillFile = Path.Combine(directory, "SKILL.md");
                if (!File.Exists(skillFile))
                {
                    continue;
                }

                var frontmatter = FrontmatterReader.Read(skillFile);
                if (frontmatter is null)
                {
                    continue;
                }

                skills.Add(new SkillDto(
                    frontmatter.Name,
                    frontmatter.Description,
                    source.Source,
                    directory));
            }
        }

        return Task.FromResult<IReadOnlyList<SkillDto>>(skills.AsReadOnly());
    }
}
