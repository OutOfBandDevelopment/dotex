using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.System.Text.Json;

/// <summary>
/// Custom JSON converter for dictionaries with string keys and object values.
/// </summary>
public class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object?>>
{
    // Reference: https://josef.codes/custom-dictionary-string-object-jsonconverter-for-system-text-json/

    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <returns>true if the converter can convert the specified type; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Dictionary<string, object>)
               || typeToConvert == typeof(Dictionary<string, object?>);
    }

    /// <summary>
    /// Reads the JSON representation of a <see cref="Dictionary{TKey, TValue}"/> with string keys and object values.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted dictionary.</returns>
    public override Dictionary<string, object?> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
        }

        var dictionary = new Dictionary<string, object?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("JsonTokenType was not PropertyName");
            }

            var propertyName = reader.GetString();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new JsonException("Failed to get property name");
            }

            reader.Read();

            dictionary.Add(propertyName!, ExtractValue(ref reader, options));
        }

        return dictionary;
    }

    /// <summary>
    /// Writes the JSON representation of a <see cref="Dictionary{TKey, TValue}"/> with string keys and object values.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The dictionary to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(
        Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options) =>
        // We don't need any custom serialization logic for writing the JSON.
        // Ideally, this method should not be called at all. It's only called if you
        // supply JsonSerializerOptions that contains this JsonConverter in its Converters list.
        // Don't do that, you will lose performance because of the cast needed below.
        // Cast to avoid infinite loop: https://github.com/dotnet/docs/issues/19268
        JsonSerializer.Serialize(writer, (IDictionary<string, object?>)value, options);

    private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                if (reader.TryGetDateTime(out var date))
                {
                    return date;
                }
                return reader.GetString();
            case JsonTokenType.False:
                return false;
            case JsonTokenType.True:
                return true;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var result))
                {
                    return result;
                }
                return reader.GetDecimal();
            case JsonTokenType.StartObject:
                return Read(ref reader, null!, options);
            case JsonTokenType.StartArray:
                var list = new List<object?>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    list.Add(ExtractValue(ref reader, options));
                }
                return list;
            default:
                throw new JsonException($"'{reader.TokenType}' is not supported");
        }
    }
}
