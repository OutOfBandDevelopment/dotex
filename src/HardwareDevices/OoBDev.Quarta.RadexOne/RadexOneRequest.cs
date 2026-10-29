using System.Runtime.InteropServices;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// One request to the device.
/// </summary>
/// <param name="Kind">What to ask.</param>
/// <param name="PacketNumber">The packet number the reply will echo.</param>
/// <param name="Settings">The settings to write; only used by <see cref="RadexOneRequestKind.WriteSettings"/>.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct RadexOneRequest(RadexOneRequestKind Kind, ushort PacketNumber = 1, RadexOneSettings Settings = default);
