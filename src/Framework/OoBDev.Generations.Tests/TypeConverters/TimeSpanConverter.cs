using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OoBDev.Generations.Tests.TypeConverters
{
    // https://stackoverflow.com/questions/39876232/newtonsoft-json-serialize-timespan-format
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Format: Days.Hours:Minutes:Seconds:Milliseconds
        /// </summary>
        public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TimeSpan.TryParseExact(reader.GetString(), TimeSpanFormatString, null, out var parsedTimeSpan);
            return parsedTimeSpan;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
            writer.WriteStringValue(timespanFormatted);
        }
    }
}