using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.System.Text.Json;

/// <summary>
/// System.Text.Json converter to support BsonDatetimeOffset
/// </summary>
public class BsonDateTimeOffsetConverter : JsonConverter<object>
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns><c>true</c> if the converter can convert the specified type; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert) =>
        new[] {
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(DateTime),
            typeof(DateTime?),
        }.Contains(typeToConvert);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">The serializer options to use during conversion.</param>
    /// <returns>The deserialized object value.</returns>
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.TokenType;

        if (type == JsonTokenType.StartObject)
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$date")
            {
                if (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTimeOffset(out var dt))
                    {
                        reader.Read();
                        return dt;
                    }
                }
            }
        }
        else if (type == JsonTokenType.String && reader.TryGetDateTimeOffset(out var dt))
        {
            return dt;
        }
        else if (type == JsonTokenType.StartArray)
        {
            DateTimeOffset? dateTime = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    if (DateTimeOffset.TryParse(value, out dt))
                    {
                        dateTime = dt;
                    }
                    else if (long.TryParse(value, out var ticks))
                    {
                        dateTime = new DateTime(ticks, DateTimeKind.Utc);
                    }
                }
                else if (reader.TokenType == JsonTokenType.Number && dateTime.HasValue)
                {
                    dateTime = new DateTimeOffset(dateTime.Value.DateTime, TimeSpan.FromMinutes(reader.GetInt32()));
                }
            }
            if (dateTime.HasValue)
                return dateTime.Value;
        }

        throw new NotSupportedException($"element of type {type} is not supported");
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="options">The serializer options to use during conversion.</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value is DateTimeOffset dto)
        {
            writer.WriteString("$date", dto);
        }
        else if (value is DateTime dt)
        {
            writer.WriteString("$date", dt);
        }
        else
        {
            throw new NotSupportedException();
        }
        writer.WriteEndObject();
    }
}
