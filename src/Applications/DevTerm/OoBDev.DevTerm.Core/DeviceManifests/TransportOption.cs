namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>One key/value pair in a <see cref="TransportHint"/>.</summary>
public sealed class TransportOption
{
    public required string Key { get; set; }

    public required string Value { get; set; }
}
