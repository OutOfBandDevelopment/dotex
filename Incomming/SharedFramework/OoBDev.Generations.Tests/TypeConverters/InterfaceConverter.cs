#if NET5_0_OR_GREATER

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.Generations.Tests.TypeConverters
{
    public class InterfaceConverter<I> : JsonConverter<I>
    {
        public override I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, I value, JsonSerializerOptions options)
        {
            if (value == null) return;

            writer.WriteStartObject();
            //writer.WriteCommentValue($"clr_type::{ value.GetType().FullName}");

            var properties = from p in value.GetType().GetProperties()
                             where !p.GetIndexParameters().Any()
                             where p.GetMethod != null
                             //orderby p.Name
                             select new
                             {
                                 p.Name,
                                 Value = p.GetValue(value),
                             };
            foreach (var p in properties)
            {
                writer.WritePropertyName(p.Name);
                JsonSerializer.Serialize(writer, p.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
#endif