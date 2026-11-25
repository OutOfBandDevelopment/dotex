using System.Collections.Generic;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>One GATT service under a device, as seen during profile enumeration (see <see cref="IBleGattProfileExplorer"/>).</summary>
public sealed record BleGattServiceDescriptor(string Uuid, string? Name, IReadOnlyList<BleGattCharacteristicDescriptor> Characteristics);
