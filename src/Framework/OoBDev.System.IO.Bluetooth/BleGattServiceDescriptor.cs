using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>One GATT service under a device, as seen during profile enumeration (see <see cref="IBleGattProfileExplorer"/>).</summary>
public sealed record BleGattServiceDescriptor(string Uuid, string? Name, IReadOnlyList<BleGattCharacteristicDescriptor> Characteristics);
