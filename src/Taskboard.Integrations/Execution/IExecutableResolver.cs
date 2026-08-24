namespace Taskboard.Integrations.Execution;

public interface IExecutableResolver
{
    string? Resolve(string module);
}
