using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>What <see cref="DeviceManifestValidator.Validate"/> found: errors a manifest can't load with, and warnings it can (but that would misbehave).</summary>
public sealed class DeviceManifestValidation
{
    public DeviceManifestValidation(IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
    {
        Errors = errors;
        Warnings = warnings;
    }

    public IReadOnlyList<string> Errors { get; }

    public IReadOnlyList<string> Warnings { get; }

    public bool IsValid => Errors.Count == 0;
}
