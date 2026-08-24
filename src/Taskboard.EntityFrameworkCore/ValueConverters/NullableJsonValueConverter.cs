using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Taskboard.EntityFrameworkCore.ValueConverters;

public sealed class NullableJsonValueConverter<T> : ValueConverter<T?, string?>
    where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public NullableJsonValueConverter()
        : base(ToProvider(), FromProvider())
    {
    }

    private static Expression<Func<T?, string?>> ToProvider()
        => v => v == null ? null : JsonSerializer.Serialize(v, Options);

    private static Expression<Func<string?, T?>> FromProvider()
        => v => v == null ? null : JsonSerializer.Deserialize<T>(v, Options);
}
