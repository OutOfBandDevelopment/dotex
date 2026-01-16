using OoBDev.ComplexEvents.Contracts.Schedulers;
using OoBDev.Toolkit.Contracts.Common;
using NCrontab.Advanced;
using NCrontab.Advanced.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.ComplexEvents.Common.Schedulers
{
    public class ScheduleCalculations : IScheduleCalculations
    {
        private readonly IDateTools _date;

        public ScheduleCalculations(
            IDateTools date
            )
        {
            _date = date;
        }

        public DateTimeOffset? GetNextOccurrence(string schedule, params string[]? schedules) =>
             GetNextOccurrence((schedule ?? "").Split('|').Concat(schedules ?? Enumerable.Empty<string>()));

        public DateTimeOffset? GetNextOccurrence(params string[]? schedules) =>
            GetNextOccurrence(schedules?.AsEnumerable());

        public DateTimeOffset? GetNextOccurrence(IEnumerable<string>? schedules)
        {
            if (schedules == null) return null;

            var now = _date.UtcNow();
            var mapped = from schedule in schedules.SelectMany(s => s.Split('|'))
                         where !string.IsNullOrWhiteSpace(schedule)
                         let tz = schedule.Split('{', '}').Skip(1).FirstOrDefault()
                         let ncron = schedule.Split('{').FirstOrDefault()
                         where !string.IsNullOrWhiteSpace(ncron)
                         let parsed = CrontabSchedule.TryParse(ncron, CronStringFormat.WithSeconds)
                         where parsed != null
                         let @from = _date.ConvertToTimeZoneId(now, tz).DateTime
                         let next = parsed.GetNextOccurrence(@from)
                         let @out = _date.ConvertToTimeZoneId(next, tz)
                         orderby next
                         select (DateTimeOffset?)@out;
            var selectedValue = mapped.FirstOrDefault();
            return selectedValue;
        }
    }
}

