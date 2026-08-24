namespace Taskboard.ValueObjects;

public abstract record StringIdBase
{
    public string Value { get; }

    protected StringIdBase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identifier cannot be null or whitespace.", nameof(value));
        }

        if (value.Length > 128)
        {
            throw new ArgumentException("Identifier cannot exceed 128 characters.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
