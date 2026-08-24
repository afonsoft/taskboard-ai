namespace Taskboard.Integrations.Execution;

public interface IProcessTreeSignaler
{
    void Terminate(int processId);
}
