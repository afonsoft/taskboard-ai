using System.Text.Json;
using System.Text.Json.Serialization;
using Taskboard.ValueObjects;

namespace Taskboard.Json;

public sealed class StringIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(StringIdBase).IsAssignableFrom(typeToConvert)
               && !typeToConvert.IsAbstract;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StringIdJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class StringIdJsonConverter<TId> : JsonConverter<TId>
        where TId : StringIdBase
    {
        public override TId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var value = reader.GetString();
            if (value is null)
            {
                return null;
            }

            return (TId)Activator.CreateInstance(typeToConvert, value)!;
        }

        public override void Write(Utf8JsonWriter writer, TId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
