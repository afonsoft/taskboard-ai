using System;
using System.IO;
using System.Threading.Tasks;
using Taskboard.Application.Contracts.Skills;
using Taskboard.Integrations.Skills;
using Shouldly;
using Xunit;

namespace Taskboard.Tests.Unit.Integrations.Skills;

public class SkillDiscoveryServiceTests
{
    [Fact]
    public async Task Dado_DiretorioComSkill_Quando_Descobrir_Entao_RetornaSkillComNomeEDescricao()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var skillDir = Path.Combine(temp, "manage-taskboard");
        Directory.CreateDirectory(skillDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillDir, "SKILL.md"),
            "---\nname: manage-taskboard\ndescription: Gerencia o taskboard.\n---\n");

        var service = new SkillDiscoveryService([new SkillDiscoverySource("custom", temp)]);

        var skills = await service.DiscoverAsync();

        skills.Count.ShouldBe(1);
        skills[0].Name.ShouldBe("manage-taskboard");
        skills[0].Description.ShouldBe("Gerencia o taskboard.");
        skills[0].Source.ShouldBe("custom");
        skills[0].Path.ShouldBe(skillDir);
    }
}
