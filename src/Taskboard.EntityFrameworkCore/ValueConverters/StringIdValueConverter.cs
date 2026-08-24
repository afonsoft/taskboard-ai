using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Taskboard.ValueObjects;

namespace Taskboard.EntityFrameworkCore.ValueConverters;

public sealed class StringIdValueConverter<TId> : ValueConverter<TId, string>
    where TId : StringIdBase
{
    public StringIdValueConverter()
        : base(ToProvider(), FromProvider())
    {
    }

    private static Expression<Func<TId, string>> ToProvider()
        => v => v.Value;

    private static Expression<Func<string, TId>> FromProvider()
        => v => (TId)Activator.CreateInstance(typeof(TId), v)!;
}

public sealed class NullableStringIdValueConverter<TId> : ValueConverter<TId?, string?>
    where TId : StringIdBase
{
    public NullableStringIdValueConverter()
        : base(ToProvider(), FromProvider())
    {
    }

    private static Expression<Func<TId?, string?>> ToProvider()
        => v => v == null ? null : v.Value;

    private static Expression<Func<string?, TId?>> FromProvider()
        => v => v == null ? null : (TId)Activator.CreateInstance(typeof(TId), v)!;
}
