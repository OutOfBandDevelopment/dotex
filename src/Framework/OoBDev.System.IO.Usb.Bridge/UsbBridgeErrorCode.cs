namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>The code byte of an <see cref="UsbBridgeOp.Error"/> frame.</summary>
public enum UsbBridgeErrorCode : byte
{
    /// <summary>The device did not answer within its timeout.</summary>
    Timeout = 1,

    /// <summary>The device is missing or could not be opened.</summary>
    DeviceGone = 2,

    /// <summary>Another client has the device.</summary>
    Busy = 3,

    /// <summary>The request is not valid for this bridge or state.</summary>
    BadRequest = 4,
}
