using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.ValueConverters;

public sealed class StringValueObjectConverter<TValue> : ValueConverter<TValue, string>
    where TValue : StringValueObject
{
    public StringValueObjectConverter()
        : base(ToProvider(), FromProvider())
    {
    }

    private static Expression<Func<TValue, string>> ToProvider()
        => v => v.Value;

    private static Expression<Func<string, TValue>> FromProvider()
        => v => (TValue)Activator.CreateInstance(typeof(TValue), v)!;
}

public sealed class NullableStringValueObjectConverter<TValue> : ValueConverter<TValue?, string?>
    where TValue : StringValueObject
{
    public NullableStringValueObjectConverter()
        : base(ToProvider(), FromProvider())
    {
    }

    private static Expression<Func<TValue?, string?>> ToProvider()
        => v => v == null ? null : v.Value;

    private static Expression<Func<string?, TValue?>> FromProvider()
        => v => v == null ? null : (TValue)Activator.CreateInstance(typeof(TValue), v)!;
}
