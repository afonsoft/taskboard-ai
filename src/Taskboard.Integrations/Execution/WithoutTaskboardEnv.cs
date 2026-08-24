namespace Taskboard.Integrations.Execution;

public static class WithoutTaskboardEnv
{
    public static void RemoveFrom(IDictionary<string, string?> environment)
    {
        var keys = environment.Keys.Where(k => k.StartsWith("CODEX_TASKBOARD_", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keys)
        {
            environment.Remove(key);
        }
    }
}
