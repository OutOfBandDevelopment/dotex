using System;
using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>A manifest <see cref="DeviceManifestLoader"/> refused because it failed <see cref="DeviceManifestValidator"/>'s checks.</summary>
public sealed class DeviceManifestValidationException : InvalidOperationException
{
    public DeviceManifestValidationException(IReadOnlyList<string> errors)
        : base("The device manifest isn't valid: " + string.Join(" ", errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
