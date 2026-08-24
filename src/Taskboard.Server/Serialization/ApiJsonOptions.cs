using System.Text.Json;
using System.Text.Json.Serialization;
using Taskboard.Json;

namespace Taskboard.Server.Serialization;

public static class ApiJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new StringIdJsonConverterFactory(),
            new StringValueObjectJsonConverterFactory()
        }
    };
}
