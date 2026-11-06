using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System;
using System.Globalization;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Formats a date: <c>{{format_date value "yyyy-MM-dd"}}</c>. The format defaults to <c>yyyy-MM-dd</c> and the culture is invariant.
/// Writes nothing when the value is not a date or a parsable date text.
/// </summary>
public class FormatDateHelperDescriptor : HelperDescriptorBase
{
    /// <inheritdoc />
    public override PathInfo Name => "format_date";

    /// <inheritdoc />
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var format = arguments.ElementAtOrDefault(1)?.ToString();
        if (string.IsNullOrWhiteSpace(format))
            format = "yyyy-MM-dd";
        var text = arguments.ElementAtOrDefault(0) switch
        {
            DateTime date => date.ToString(format, CultureInfo.InvariantCulture),
            DateTimeOffset offset => offset.ToString(format, CultureInfo.InvariantCulture),
            string s when DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed) => parsed.ToString(format, CultureInfo.InvariantCulture),
            _ => null,
        };
        if (text is not null)
            output.WriteSafeString(text);
    };
}
