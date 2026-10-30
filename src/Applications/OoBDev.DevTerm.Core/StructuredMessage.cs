using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.DevTerm;

/// <summary>
/// The small, optional structured form of one decoded message: named fields with units,
/// alongside the text a presenter renders. A decoder may emit it; none must.
/// </summary>
public sealed record StructuredMessage(string Source, IReadOnlyList<StructuredField> Fields)
{
    /// <summary>The first field called <paramref name="name"/> (case-insensitive), or null.</summary>
    public StructuredField? Find(string name) =>
        Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>The field's value as a number (invariant culture), or null when absent or not numeric.</summary>
    public double? Number(string name) =>
        Find(name) is { } field && double.TryParse(field.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    public override string ToString() => string.Join(' ', Fields);
}
