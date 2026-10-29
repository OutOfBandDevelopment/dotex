using System.Runtime.InteropServices;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// The alarm settings of the device.
/// </summary>
/// <param name="AlarmMode">How the device alarms.</param>
/// <param name="Threshold">The alarm threshold.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct RadexOneSettings(RadexOneAlarmMode AlarmMode, ushort Threshold);
