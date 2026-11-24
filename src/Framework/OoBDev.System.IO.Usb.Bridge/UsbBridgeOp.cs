namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>The operation of a bridge frame (see <c>docs/design/Ser2NetWin/usb-bridge.md</c>).</summary>
public enum UsbBridgeOp : byte
{
    /// <summary>Client: protocol version and device kind. Server: its version and a device description.</summary>
    Hello = 0x01,

    /// <summary>Client: open the device. The reply carries the transfer sizes for USBTMC.</summary>
    Open = 0x02,

    /// <summary>Client: close the device.</summary>
    Close = 0x03,

    /// <summary>USBTMC: one framed bulk-OUT transfer.</summary>
    WriteBulkOut = 0x10,

    /// <summary>USBTMC: one bulk-IN read; the payload is the buffer size.</summary>
    ReadBulkIn = 0x11,

    /// <summary>USBTMC: abort the bulk-IN transfer tagged by the payload byte.</summary>
    AbortBulkIn = 0x12,

    /// <summary>USBTMC: abort the bulk-OUT transfer tagged by the payload byte.</summary>
    AbortBulkOut = 0x13,

    /// <summary>USBTMC: clear the device's buffers.</summary>
    Clear = 0x14,

    /// <summary>USBTMC: remote (1) or local (0) control.</summary>
    SetRemote = 0x15,

    /// <summary>HID: write one output report.</summary>
    HidWrite = 0x20,

    /// <summary>HID, server to client without a request: one input report.</summary>
    HidReport = 0x21,

    /// <summary>Server reply: done.</summary>
    Ok = 0x80,

    /// <summary>Server reply: data follows.</summary>
    Data = 0x81,

    /// <summary>Server reply: failed; one code byte and the text.</summary>
    Error = 0x82,
}
