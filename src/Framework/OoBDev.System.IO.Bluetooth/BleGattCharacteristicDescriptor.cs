using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// One GATT characteristic under a <see cref="BleGattServiceDescriptor"/>, as seen during profile
/// enumeration (a "sub-device" picker for <see cref="BleDeviceAdapterOptions.WriteCharacteristicUuid"/>/
/// <see cref="BleDeviceAdapterOptions.NotifyCharacteristicUuid"/>) - not a live subscription, just a
/// snapshot of what the peripheral advertises it supports.
/// </summary>
public sealed record BleGattCharacteristicDescriptor(
    string Uuid,
    string? Name,
    bool CanRead,
    bool CanWrite,
    bool CanWriteWithoutResponse,
    bool CanNotify,
    bool CanIndicate);
