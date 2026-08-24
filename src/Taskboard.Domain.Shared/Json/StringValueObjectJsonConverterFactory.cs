using System.Text.Json;
using System.Text.Json.Serialization;
using Taskboard.ValueObjects;

namespace Taskboard.Json;

public sealed class StringValueObjectJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(StringValueObject).IsAssignableFrom(typeToConvert)
               && !typeToConvert.IsAbstract;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StringValueObjectJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class StringValueObjectJsonConverter<TValue> : JsonConverter<TValue>
        where TValue : StringValueObject
    {
        public override TValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return (TValue)Activator.CreateInstance(typeToConvert, value)!;
        }

        public override void Write(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
