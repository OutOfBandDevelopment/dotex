using System;
using System.Text.Json.Serialization;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>One parameter of an <see cref="OutboundCommand"/>, substituted for <c>{Name}</c> in its template.</summary>
public sealed class CommandParameter
{
    public required string Name { get; set; }

    /// <summary><c>string</c> (sent as typed), or <c>number</c>/<c>integer</c> (parsed, clamped to [Minimum, Maximum], formatted).</summary>
    public string Type { get; set; } = "string";

    public double? Minimum { get; set; }

    public double? Maximum { get; set; }

    public string? Unit { get; set; }

    /// <summary>Used when no value arrives for this parameter (or a number doesn't parse).</summary>
    public string? DefaultValue { get; set; }

    /// <summary>A .NET numeric format string for a number (e.g. <c>"00.00"</c> for a Korad-style <c>VSET1:05.00</c>); invariant-culture shortest form when unset.</summary>
    public string? Format { get; set; }

    /// <summary>Whether <see cref="Type"/> is a numeric kind.</summary>
    [JsonIgnore]
    public bool IsNumeric =>
        Type.Equals("number", StringComparison.OrdinalIgnoreCase)
        || Type.Equals("numeric", StringComparison.OrdinalIgnoreCase)
        || Type.Equals("integer", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool IsInteger => Type.Equals("integer", StringComparison.OrdinalIgnoreCase);
}
