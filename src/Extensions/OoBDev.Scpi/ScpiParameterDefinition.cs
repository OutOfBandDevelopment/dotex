using System.Collections.Generic;

namespace OoBDev.Scpi;

/// <summary>
/// A parameter that a <see cref="ScpiCommandDefinition.Template"/> substitutes by name (a <c>{Name}</c> token).
/// </summary>
public sealed class ScpiParameterDefinition
{
    /// <summary>The token name in the template.</summary>
    public required string Name { get; set; }

    /// <summary>The data type the value is validated and formatted as.</summary>
    public ScpiParameterKind Kind { get; set; } = ScpiParameterKind.Text;

    /// <summary>An optional widget hint; <see langword="null"/> keeps the widget the kind implies.</summary>
    public ScpiParameterControl? Control { get; set; }

    /// <summary>Numeric only: the lower bound, or <see langword="null"/> for none. Values are clamped to it.</summary>
    public double? Minimum { get; set; }

    /// <summary>Numeric only: the upper bound, or <see langword="null"/> for none. Values are clamped to it.</summary>
    public double? Maximum { get; set; }

    /// <summary>Numeric only: the unit shown beside the value.</summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Numeric only: format with exactly this many decimal places. Some parsers read a fixed number of characters (the Korad
    /// KA3005P and KA6003P <c>VSET</c> and <c>ISET</c> have no terminator), so <c>12</c> instead of <c>12.00</c> desyncs them.
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// Numeric only, with <see cref="DecimalPlaces"/>: zero-pad the integer part to this many digits (<c>05.00</c>, not
    /// <c>5.00</c>); confirmed necessary on the Korad supplies.
    /// </summary>
    public int? IntegerDigits { get; set; }

    /// <summary>Choice only: the allowed values.</summary>
    public IList<string> Options { get; set; } = [];

    /// <summary>The value used when none is supplied.</summary>
    public string? DefaultValue { get; set; }
}
