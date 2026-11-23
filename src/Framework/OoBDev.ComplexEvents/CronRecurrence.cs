using System;
using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.ComplexEvents;

/// <summary>
/// A five-field cron expression evaluated in UTC: minute, hour, day of month, month, day of week (0 or 7 is Sunday).
/// Each field takes <c>*</c>, a number, a range (<c>1-5</c>), a list (<c>1,3</c>) and a step (<c>*/15</c>, <c>10-30/5</c>).
/// When both day fields are restricted a day matches if either does, as in Vixie cron.
/// </summary>
public sealed class CronRecurrence : IRecurrence
{
    private readonly HashSet<int> _minutes;
    private readonly HashSet<int> _hours;
    private readonly HashSet<int> _days;
    private readonly HashSet<int> _months;
    private readonly HashSet<int> _weekdays;
    private readonly bool _dayRestricted;
    private readonly bool _weekdayRestricted;

    /// <summary>Parses <paramref name="expression"/>.</summary>
    /// <param name="expression">The five-field expression.</param>
    /// <exception cref="FormatException">The expression is not valid.</exception>
    public CronRecurrence(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var fields = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 5)
        {
            throw new FormatException($"A cron expression has five fields, but '{expression}' has {fields.Length}.");
        }

        _minutes = Parse(fields[0], 0, 59);
        _hours = Parse(fields[1], 0, 23);
        _days = Parse(fields[2], 1, 31);
        _months = Parse(fields[3], 1, 12);
        _weekdays = Parse(fields[4], 0, 7);
        if (_weekdays.Remove(7))
        {
            _weekdays.Add(0);
        }

        _dayRestricted = fields[2] != "*";
        _weekdayRestricted = fields[4] != "*";
    }

    /// <inheritdoc />
    public DateTimeOffset? Next(DateTimeOffset after)
    {
        var utc = after.UtcDateTime;
        var t = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc).AddMinutes(1);
        var limit = t.AddYears(5);
        while (t < limit)
        {
            if (!_months.Contains(t.Month))
            {
                t = new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            }
            else if (!DayMatches(t))
            {
                t = t.Date.AddDays(1);
            }
            else if (!_hours.Contains(t.Hour))
            {
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
            }
            else if (!_minutes.Contains(t.Minute))
            {
                t = t.AddMinutes(1);
            }
            else
            {
                return new DateTimeOffset(t, TimeSpan.Zero);
            }
        }

        return null;
    }

    private bool DayMatches(DateTime t)
    {
        var day = _days.Contains(t.Day);
        var weekday = _weekdays.Contains((int)t.DayOfWeek);
        return _dayRestricted && _weekdayRestricted ? day || weekday : day && weekday;
    }

    private static HashSet<int> Parse(string field, int min, int max)
    {
        var values = new HashSet<int>();
        foreach (var part in field.Split(','))
        {
            var stepAt = part.IndexOf('/', StringComparison.Ordinal);
            var step = 1;
            var range = part;
            if (stepAt >= 0)
            {
                range = part[..stepAt];
                if (!int.TryParse(part[(stepAt + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out step) || step < 1)
                {
                    throw new FormatException($"Invalid step in '{part}'.");
                }
            }

            int from, to;
            if (range == "*")
            {
                (from, to) = (min, max);
            }
            else if (range.Split('-') is [var a, var b])
            {
                (from, to) = (Number(a, min, max), Number(b, min, max));
            }
            else
            {
                from = Number(range, min, max);
                to = stepAt >= 0 ? max : from;
            }

            if (from > to)
            {
                throw new FormatException($"Invalid range '{part}'.");
            }

            for (var v = from; v <= to; v += step)
            {
                values.Add(v);
            }
        }

        return values.Count == 0 ? throw new FormatException($"Empty field '{field}'.") : values;
    }

    private static int Number(string text, int min, int max) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var v) && v >= min && v <= max
            ? v
            : throw new FormatException($"'{text}' is not a number from {min} to {max}.");
}
