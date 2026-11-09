using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using System;
using System.Globalization;
using System.Linq;

namespace OoBDev.Handlebars.Helpers;

/// <summary>
/// Formats a number: <c>{{format_number value "N2"}}</c> (default <c>N2</c>), or as currency with <c>{{currency value}}</c>
/// (format <c>C</c>). An optional last argument names the culture (for example <c>en-US</c>); the default is invariant.
/// A value that is not a number is written as its text.
/// </summary>
public class FormatNumberHelperDescriptor : HelperDescriptorBase
{
    private readonly string _name;
    private readonly string _defaultFormat;

    /// <summary>Initializes the number helper (<c>format_number</c>, <c>N2</c>).</summary>
    public FormatNumberHelperDescriptor() : this("format_number", "N2") { }

    /// <summary>Initializes a number helper with its name and default format.</summary>
    /// <param name="name">The helper name.</param>
    /// <param name="defaultFormat">The format used when the template gives none.</param>
    public FormatNumberHelperDescriptor(string name, string defaultFormat)
    {
        _name = name;
        _defaultFormat = defaultFormat;
    }

    /// <inheritdoc />
    public override PathInfo Name => _name;

    /// <inheritdoc />
    protected override HandlebarsHelper Helper => (output, context, arguments) =>
    {
        var value = arguments.ElementAtOrDefault(0);
        if (value is null)
            return;
        var format = arguments.ElementAtOrDefault(1)?.ToString();
        if (string.IsNullOrWhiteSpace(format))
            format = _defaultFormat;
        var cultureName = arguments.ElementAtOrDefault(2)?.ToString();
        var culture = string.IsNullOrWhiteSpace(cultureName) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);
        try
        {
            output.WriteSafeString(Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(format, culture));
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            output.WriteSafeString(value.ToString() ?? string.Empty);
        }
    };
}
