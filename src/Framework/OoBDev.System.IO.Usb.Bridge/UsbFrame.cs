using System;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>One frame: an operation, the id of the request it belongs to, and a payload.</summary>
public sealed record UsbFrame(UsbBridgeOp Op, uint Id, ReadOnlyMemory<byte> Payload);
