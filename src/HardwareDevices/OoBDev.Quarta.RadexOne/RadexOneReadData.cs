using System.Runtime.InteropServices;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// The values of a Read Data reply. The unit of each is unconfirmed (BinaryDataDecoders scales them as hundredths of a
/// microsievert per hour and a microsievert, from 32-bit fields; dev-term reads 16-bit fields and states the unit is unconfirmed).
/// </summary>
/// <param name="Ambient">The ambient reading.</param>
/// <param name="Accumulated">The accumulated reading.</param>
/// <param name="Cpm">The counts per minute.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct RadexOneReadData(ushort Ambient, ushort Accumulated, ushort Cpm);
