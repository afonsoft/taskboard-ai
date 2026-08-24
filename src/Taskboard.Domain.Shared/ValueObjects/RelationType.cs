namespace Taskboard.ValueObjects;

public sealed record RelationType : StringValueObject
{
    private static readonly IReadOnlyCollection<string> AllowedValues = new HashSet<string>(StringComparer.Ordinal)
    {
        "parent",
        "blocks",
        "related"
    };

    public static readonly RelationType Parent = new("parent");
    public static readonly RelationType Blocks = new("blocks");
    public static readonly RelationType Related = new("related");

    public RelationType(string value)
        : base(value, AllowedValues)
    {
    }

    public static RelationType From(string value) => new(value);
}
