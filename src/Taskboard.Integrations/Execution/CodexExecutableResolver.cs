using System.Runtime.InteropServices;

namespace Taskboard.Integrations.Execution;

public sealed class CodexExecutableResolver : IExecutableResolver
{
    public string? Resolve(string module)
    {
        var executable = module switch
        {
            "node" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "node.exe" : "node",
            "npx" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npx.cmd" : "npx",
            _ => null,
        };

        if (executable is null)
        {
            return null;
        }

        return SearchInPath(executable);
    }

    private static string? SearchInPath(string executable)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';

        foreach (var directory in path.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory.Trim(), executable);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
