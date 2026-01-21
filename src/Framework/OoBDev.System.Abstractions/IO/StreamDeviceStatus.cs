namespace OoBDev.System.IO;

/// <summary>
/// Represents the operational status of a streaming device.
/// </summary>
public enum StreamDeviceStatus
{
    /// <summary>
    /// The device status is unknown or undefined.
    /// </summary>
    Unknown,

    /// <summary>
    /// The device is currently initializing.
    /// </summary>
    Initializing,

    /// <summary>
    /// The device has been initialized and is ready for operation.
    /// </summary>
    Initialized,

    /// <summary>
    /// The device is currently transmitting data.
    /// </summary>
    Transmitting,

    /// <summary>
    /// The device has completed transmitting data.
    /// </summary>
    Transmitted,

    /// <summary>
    /// The device is currently receiving data.
    /// </summary>
    Receiving,

    /// <summary>
    /// The device has completed receiving data.
    /// </summary>
    Received,
}