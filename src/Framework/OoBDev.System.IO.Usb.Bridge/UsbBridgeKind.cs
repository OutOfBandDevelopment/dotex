namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>Which class of USB device a bridge shares.</summary>
public enum UsbBridgeKind : byte
{
    /// <summary>A USB Test and Measurement Class instrument.</summary>
    UsbTmc = 1,

    /// <summary>A HID device.</summary>
    Hid = 2,
}
