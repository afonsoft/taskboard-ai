namespace Taskboard.ValueObjects;

public sealed record Sandbox : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "read-only",
        "workspace-write",
        "danger-full-access"
    };

    public static readonly Sandbox ReadOnly = new("read-only");
    public static readonly Sandbox WorkspaceWrite = new("workspace-write");
    public static readonly Sandbox DangerFullAccess = new("danger-full-access");

    public Sandbox(string value)
        : base(value, AllowedValues)
    {
    }

    public static Sandbox From(string value) => new(value);
}
