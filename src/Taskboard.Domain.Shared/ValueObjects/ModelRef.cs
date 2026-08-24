namespace Taskboard.ValueObjects;

public sealed record ModelRef : StringValueObject
{
    public ModelRef(string value)
        : base(value)
    {
    }

    public static ModelRef From(string value) => new(value);
}
