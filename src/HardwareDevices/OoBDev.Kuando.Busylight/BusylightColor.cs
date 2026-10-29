using System.Runtime.InteropServices;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// An RGB colour as the light takes it: one byte per channel.
/// </summary>
/// <param name="Red">The red channel.</param>
/// <param name="Green">The green channel.</param>
/// <param name="Blue">The blue channel.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct BusylightColor(byte Red, byte Green, byte Blue);
