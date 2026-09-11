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
                var skillFile = Path.Join(directory, "SKILL.md");
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

    public Task<SkillDetailDto?> GetDetailAsync(string sourceName, string name, CancellationToken cancellationToken = default)
    {
        var source = _sources.FirstOrDefault(s => s.Source.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        if (source is null || !Directory.Exists(source.Path))
        {
            return Task.FromResult<SkillDetailDto?>(null);
        }

        foreach (var directory in Directory.EnumerateDirectories(source.Path))
        {
            var skillFile = Path.Join(directory, "SKILL.md");
            if (!File.Exists(skillFile))
            {
                continue;
            }

            var frontmatter = FrontmatterReader.Read(skillFile);
            if (frontmatter is null || !frontmatter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var content = File.ReadAllText(skillFile);
            var (references, scripts) = ExtractSections(content);

            return Task.FromResult<SkillDetailDto?>(new SkillDetailDto(
                frontmatter.Name,
                frontmatter.Description,
                source.Source,
                directory,
                frontmatter.Tools,
                references,
                scripts,
                content));
        }

        return Task.FromResult<SkillDetailDto?>(null);
    }

    private static (string? References, string? Scripts) ExtractSections(string content)
    {
        var referencesHeading = "## References";
        var scriptsHeading = "## Scripts";

        var references = ExtractSection(content, referencesHeading);
        var scripts = ExtractSection(content, scriptsHeading);

        return (references, scripts);
    }

    private static string? ExtractSection(string content, string heading)
    {
        var index = content.IndexOf(heading, StringComparison.Ordinal);
        if (index == -1)
        {
            return null;
        }

        var start = index + heading.Length;
        var end = content.Length;

        for (var i = start; i < content.Length; i++)
        {
            if (i + 1 < content.Length && content[i] == '\r' && content[i + 1] == '\n')
            {
                continue;
            }

            if (content[i] == '#' && (i == start || content[i - 1] == '\n'))
            {
                end = i;
                break;
            }
        }

        var value = content[start..end].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
