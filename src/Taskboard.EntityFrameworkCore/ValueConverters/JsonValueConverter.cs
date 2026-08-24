using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Taskboard.EntityFrameworkCore.ValueConverters;

public sealed class JsonValueConverter<T> : ValueConverter<T, string>
    where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<T>(v, Options)!)
    {
    }
}
