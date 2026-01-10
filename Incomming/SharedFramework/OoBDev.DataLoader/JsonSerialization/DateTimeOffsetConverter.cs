using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.DataLoader.JsonSerialization
{
    public class DateTimeOffsetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            new[] {
                typeof(DateTimeOffset), typeof(DateTimeOffset?),
                typeof(DateTime), typeof(DateTime?),
            }.Contains(objectType);

        public override bool CanWrite { get; } = false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
            throw new NotSupportedException();

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            reader.Value switch
            {
                DateTime dt => new DateTimeOffset(dt),
                string s => ParseString(s),
                _ => reader.Value
            };

        private static readonly Regex relativeDateStringToday = new Regex(@"TODAY([\+-])(\d+)@(\d+):(\d+)");
        private static readonly Regex relativeDateStringWeekStart = new Regex(@"WEEKSTART([\+-])(\d+)@(\d+):(\d+)");

        private DateTimeOffset ParseString(string s)
        {
            var todayMatch = relativeDateStringToday.Match(s);
            var weekStartMatch = relativeDateStringWeekStart.Match(s);
            if (todayMatch.Success)
            {
                var op = todayMatch.Groups[1].Value;
                var day = int.Parse(todayMatch.Groups[2].Value);
                var hour = int.Parse(todayMatch.Groups[3].Value);
                var minute = int.Parse(todayMatch.Groups[4].Value);

                var ts = DateTimeOffset.Now;
                var offset = new TimeSpan(
                    (op == "-") ? -day : day,
                    -ts.Hour + hour,
                    -ts.Minute + minute,
                    -ts.Second,
                    -ts.Millisecond);

                return ts + offset;
            }
            else if (weekStartMatch.Success)
            {
                var op = weekStartMatch.Groups[1].Value;
                var day = int.Parse(weekStartMatch.Groups[2].Value);
                var hour = int.Parse(weekStartMatch.Groups[3].Value);
                var minute = int.Parse(weekStartMatch.Groups[4].Value);

                var ts = DateTimeOffset.Now.AddDays(1 - (int)DateTimeOffset.Now.DayOfWeek);
                var offset = new TimeSpan(
                    (op == "-") ? -day : day,
                    -ts.Hour + hour,
                    -ts.Minute + minute,
                    -ts.Second,
                    -ts.Millisecond);

                return ts + offset;
            }

            return DateTimeOffset.Parse(s);
        }
    }
}
