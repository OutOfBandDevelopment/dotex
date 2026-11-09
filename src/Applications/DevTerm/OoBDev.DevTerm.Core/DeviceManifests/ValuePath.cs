using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// One value an expression may legally read, with whatever the manifest says about it.
/// <see cref="Type"/> for a regex capture is inferred from the capture's pattern, so it is a hint rather than a guarantee.
/// </summary>
public sealed record ValuePath(
    string Path,
    ValuePathType Type,
    ValuePathSource Source,
    string Origin,
    string? Unit = null,
    double? Minimum = null,
    double? Maximum = null,
    IReadOnlyList<string>? Choices = null,
    string? Example = null);
