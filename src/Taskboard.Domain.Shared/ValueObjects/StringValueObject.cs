namespace Taskboard.ValueObjects;

public abstract record StringValueObject
{
    public string Value { get; }

    protected StringValueObject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        Value = value;
    }

    protected StringValueObject(string value, IReadOnlyCollection<string> allowed)
        : this(value)
    {
        if (!allowed.Contains(value))
        {
            throw new DomainException(
                TaskboardDomainErrorCodes.InvalidValue,
                $"'{value}' is not an allowed value.");
        }
    }

    public override string ToString() => Value;
}
