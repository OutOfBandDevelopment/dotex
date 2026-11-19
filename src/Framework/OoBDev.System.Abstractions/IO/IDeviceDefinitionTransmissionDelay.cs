namespace OoBDev.System.IO;

/// <summary>
/// Optional capability of a device definition: the minimum pause between two transmissions the device tolerates.
/// </summary>
public interface IDeviceDefinitionTransmissionDelay
{
    /// <summary>
    /// Gets the minimum delay between message transmissions in milliseconds; 0 sends back to back.
    /// </summary>
    int MinimumTransmissionDelay { get; }
}
