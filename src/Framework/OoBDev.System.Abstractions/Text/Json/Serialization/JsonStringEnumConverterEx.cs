using OoBDev.System.Reflection;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.System.Text.Json.Serialization;

/// <summary>
/// A custom JSON converter for serializing and deserializing enums as strings or numbers.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
public class JsonStringEnumConverterEx<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    /// <summary>
    /// Reads the JSON representation of the enum value and converts it to the specified enum type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized enum value.</returns>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.TokenType;
        if (type == JsonTokenType.String)
        {
            var value = reader.GetString();
            var enumValue = value.ToEnum<TEnum>();
            return enumValue ?? default;
        }
        else if (type == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            var enumValue = value.ToEnum<TEnum>();
            return enumValue;
        }

        return default;
    }

    /// <summary>
    /// Writes the JSON representation of the enum value.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The enum value to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) => writer.WriteStringValue(EnumExtensions.AsString<TEnum>(value));
}
