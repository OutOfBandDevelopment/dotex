using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.UsbTmc;

/// <summary>
/// Describes one USB device whose active configuration exposes a USBTMC
/// (USB Test &amp; Measurement Class, interface class 0xFE / sub-class 0x03) interface.
/// </summary>
public sealed record UsbTmcDeviceDescriptor(
    int VendorId,
    int ProductId,
    string? Manufacturer,
    string? Product,
    string? SerialNumber,
    string? DevicePath);
