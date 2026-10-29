using System.Collections.Generic;

namespace OoBDev.Scpi;

/// <summary>
/// One outbound command, declared as data. Covers SCPI and SCPI-adjacent protocols such as the Korad plain-ASCII one.
/// </summary>
public sealed class ScpiCommandDefinition
{
    /// <summary>The identifier a caller uses to invoke the command.</summary>
    public required string Id { get; set; }

    /// <summary>A display label.</summary>
    public required string Label { get; set; }

    /// <summary>Groups related commands, for example Measure, Source or Output.</summary>
    public string Category { get; set; } = "Commands";

    /// <summary>
    /// The command text with <c>{Name}</c> tokens substituted from <see cref="Parameters"/>, for example
    /// <c>SOUR1:FREQ {Frequency}</c>. No terminator: <see cref="ScpiInstrumentProfile.Terminator"/> is appended on build.
    /// </summary>
    public required string Template { get; set; }

    /// <summary>Whether the command expects a reply.</summary>
    public bool IsQuery { get; set; }

    /// <summary>What the reply contains when it is not an ordinary text line.</summary>
    public ScpiResponseFormat ExpectedResponseFormat { get; set; } = ScpiResponseFormat.Text;

    /// <summary>The template's parameters, in the order values are supplied.</summary>
    public IList<ScpiParameterDefinition> Parameters { get; set; } = [];
}
