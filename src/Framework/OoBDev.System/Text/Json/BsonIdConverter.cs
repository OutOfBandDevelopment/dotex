using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.System.Text.Json;

/// <summary>
/// This type converter for System.Text.Json to support BSON ObjectID to JSON safe export/import
/// </summary>
public class BsonIdConverter : JsonConverter<string>
{
    /// <summary>
    /// read value from Bson and if object id is bson object id convert it to string
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.TokenType;

        if (type == JsonTokenType.StartObject)
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$oid")
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.String)
                {
                    var ret = reader.GetString();
                    reader.Read();
                    return ret;
                }
            }
        }
        else if (type == JsonTokenType.String)
        {
            return reader.GetString();
        }

        throw new NotSupportedException($"element of type {type} is not supported");
    }

    /// <summary>
    /// write object id to Bson object
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$oid", value);
        writer.WriteEndObject();
    }
}
