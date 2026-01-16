using OoBDev.Generations.Rules;
using System;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateDateTimeAttribute : GenerateLongAttribute
    {
        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            new[] {
                typeof(TimeSpan), typeof(TimeSpan?),
                typeof(DateTime), typeof(DateTime?),
                typeof(DateTimeOffset), typeof(DateTimeOffset?),
            }.Contains(context.TargetType);

        protected override object? OnGenerateValue(IProcedualGenerationContext context)
        {
            var result = (long?)base.OnGenerateValue(context);
            if (!result.HasValue) return null;

            var ruleAttribute = context.GetRule<DateTimeAttribute>();
            var rule = new
            {
                MaximumDateTime = DateTime.TryParse(ruleAttribute.MaximumDateTime, out var mxdt) ? mxdt : DateTime.MaxValue,
                MinimumDateTime = DateTime.TryParse(ruleAttribute.MinimumDateTime, out var mndt) ? mndt : DateTime.MinValue,

                MaximumTime = TimeSpan.TryParse(ruleAttribute.MaximumTime, out var mxts) ? mxts : TimeSpan.MaxValue,
                MinimumTime = TimeSpan.TryParse(ruleAttribute.MinimumTime, out var mnts) ? mnts : TimeSpan.MinValue,
            };


            if (context.TargetType == typeof(TimeSpan) || context.TargetType == typeof(TimeSpan?))
                return new TimeSpan(rule.MinimumTime.Ticks + (result.Value % (rule.MaximumTime.Ticks - rule.MinimumTime.Ticks)));

            var dateTime = new DateTime(rule.MinimumDateTime.Ticks + (result.Value % (rule.MaximumDateTime.Ticks - rule.MinimumDateTime.Ticks)));

            if (context.TargetType == typeof(DateTime) || context.TargetType == typeof(DateTime?)) return dateTime;
            else if (context.TargetType == typeof(DateTimeOffset) || context.TargetType == typeof(DateTimeOffset?))
            {
                var tzi = TimeZoneInfo.GetSystemTimeZones()[context.Random.Next() % TimeZoneInfo.GetSystemTimeZones().Count];
                var tz = tzi.GetUtcOffset(dateTime);
                return new DateTimeOffset(dateTime, tz);
            }

            throw new NotSupportedException($"type {context.TargetType} is not supported");
        }
    }
}
