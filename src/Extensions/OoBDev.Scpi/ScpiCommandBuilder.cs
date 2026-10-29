using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OoBDev.Scpi;

internal static partial class ScpiCommandBuilder
{
    // One pass over the template: a substituted value is never rescanned, so a value that contains another
    // parameter's placeholder text is inserted verbatim.
    public static string BuildText(ScpiCommandDefinition command, IReadOnlyList<string>? values)
    {
        if (command.Parameters.Count == 0) return command.Template;

        var substitutions = new Dictionary<string, string>(command.Parameters.Count, StringComparer.Ordinal);
        for (var i = 0; i < command.Parameters.Count; i++)
        {
            var parameter = command.Parameters[i];
            var raw = values is not null && i < values.Count && values[i].Length > 0 ? values[i] : parameter.DefaultValue ?? string.Empty;
            substitutions[parameter.Name] = parameter.Kind == ScpiParameterKind.Numeric ? FormatNumeric(raw, parameter) : raw;
        }

        return TemplatePlaceholder().Replace(command.Template, match =>
            substitutions.TryGetValue(match.Groups["name"].Value, out var substituted) ? substituted : match.Value);
    }

    private static string FormatNumeric(string raw, ScpiParameterDefinition parameter)
    {
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            number = double.TryParse(parameter.DefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var fallback) ? fallback : 0;

        var clamped = Math.Clamp(number, parameter.Minimum ?? double.NegativeInfinity, parameter.Maximum ?? double.PositiveInfinity);
        if (parameter.DecimalPlaces is not { } decimalPlaces) return clamped.ToString(CultureInfo.InvariantCulture);

        var integerDigits = Math.Max(parameter.IntegerDigits ?? 1, 1);
        var format = new string('0', integerDigits) + (decimalPlaces > 0 ? "." + new string('0', decimalPlaces) : string.Empty);
        return clamped.ToString(format, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"\{(?<name>[A-Za-z_](?:[A-Za-z0-9_.]|\[[0-9]+\])*)\}", RegexOptions.None, matchTimeoutMilliseconds: 250)]
    private static partial Regex TemplatePlaceholder();
}
