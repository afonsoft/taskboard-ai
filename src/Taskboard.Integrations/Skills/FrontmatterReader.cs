namespace Taskboard.Integrations.Skills;

internal sealed record Frontmatter(string Name, string Description);

internal static class FrontmatterReader
{
    public static Frontmatter? Read(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2 || !lines[0].Trim().Equals("---", StringComparison.Ordinal))
        {
            return null;
        }

        var endIndex = Array.FindIndex(lines, 1, l => l.Trim().Equals("---", StringComparison.Ordinal));
        if (endIndex == -1)
        {
            return null;
        }

        var name = string.Empty;
        var description = string.Empty;
        for (var i = 1; i < endIndex; i++)
        {
            var line = lines[i];
            if (TryExtract(line, "name", out var value))
            {
                name = value;
            }
            else if (TryExtract(line, "description", out value))
            {
                description = value;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new Frontmatter(name, description);
    }

    private static bool TryExtract(string line, string key, out string value)
    {
        value = string.Empty;
        var prefix = key + ":";
        if (!line.TrimStart().StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        value = line.Substring(prefix.Length).Trim().Trim('"').Trim('\'');
        return true;
    }
}
