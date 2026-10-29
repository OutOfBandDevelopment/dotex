using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Scpi;

/// <summary>
/// A named command set for one instrument family. Profiles load from JSON (<see cref="ScpiProfileCatalog"/>), so supporting another
/// instrument is a new file, not new code.
/// </summary>
public sealed class ScpiInstrumentProfile
{
    /// <summary>The display name.</summary>
    public required string Name { get; set; }

    /// <summary>A regular expression matched against an <c>*IDN?</c> reply; empty for a profile that is never auto-selected.</summary>
    public string? IdnPattern { get; set; }

    /// <summary>Appended to every command; most gear wants <c>\n</c>, some want <c>\r\n</c> or <c>\r</c>, and Korad wants none.</summary>
    public string Terminator { get; set; } = "\n";

    /// <summary>Operational knowledge from real hardware: required connection settings, a mandatory preamble, quirks.</summary>
    public string? Notes { get; set; }

    /// <summary>The commands.</summary>
    public IList<ScpiCommandDefinition> Commands { get; set; } = [];

    /// <summary>
    /// Builds the request for one command of this profile.
    /// </summary>
    /// <param name="commandId">A command <see cref="ScpiCommandDefinition.Id"/>.</param>
    /// <param name="values">The parameter values in <see cref="ScpiCommandDefinition.Parameters"/> order; missing or empty ones use the default.</param>
    /// <returns>The request with its terminator appended.</returns>
    /// <exception cref="ArgumentException">The profile has no such command.</exception>
    public ScpiRequest Build(string commandId, IReadOnlyList<string>? values = null)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        var command = Commands.FirstOrDefault(c => c.Id == commandId)
            ?? throw new ArgumentException($"Unknown SCPI command '{commandId}'.", nameof(commandId));
        return new ScpiRequest(ScpiCommandBuilder.BuildText(command, values) + Terminator, command.IsQuery, command.ExpectedResponseFormat);
    }

    /// <summary>
    /// Builds a request from text sent verbatim, with this profile's terminator.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="isQuery">Whether a reply is expected.</param>
    /// <returns>The request.</returns>
    public ScpiRequest BuildCustom(string text, bool isQuery = false)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new ScpiRequest(text + Terminator, isQuery, ScpiResponseFormat.Text);
    }
}
