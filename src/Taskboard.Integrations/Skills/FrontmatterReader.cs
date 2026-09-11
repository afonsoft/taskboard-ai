namespace Taskboard.Integrations.Skills;

internal sealed record Frontmatter(string Name, string Description, IReadOnlyList<string> Tools);

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
        var tools = new List<string>();
        var inTools = false;
        for (var i = 1; i < endIndex; i++)
        {
            var line = lines[i];
            if (TryExtract(line, "name", out var value))
            {
                name = value;
                inTools = false;
            }
            else if (TryExtract(line, "description", out value))
            {
                description = value;
                inTools = false;
            }
            else if (TryExtract(line, "tools", out value))
            {
                inTools = true;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // inline array literal such as [Bash, Read]
                    tools.AddRange(ParseInlineArray(value));
                }
            }
            else if (inTools && line.TrimStart().StartsWith("- ", StringComparison.Ordinal))
            {
                var tool = line.TrimStart()[2..].Trim().Trim('"', '\'');
                if (!string.IsNullOrWhiteSpace(tool))
                {
                    tools.Add(tool);
                }
            }
            else if (inTools && TryExtractKey(line, out _))
            {
                inTools = false;
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new Frontmatter(name, description, tools.AsReadOnly());
    }

    private static IEnumerable<string> ParseInlineArray(string value)
    {
        value = value.Trim();
        if (value.StartsWith("[", StringComparison.Ordinal) && value.EndsWith("]", StringComparison.Ordinal))
        {
            value = value[1..^1];
        }

        foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = part.Trim().Trim('"', '\'');
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                yield return trimmed;
            }
        }
    }

    private static bool TryExtractKey(string line, out string key)
    {
        key = string.Empty;
        var trimmed = line.TrimStart();
        var colon = trimmed.IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        key = trimmed[..colon].Trim();
        return !string.IsNullOrWhiteSpace(key);
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
