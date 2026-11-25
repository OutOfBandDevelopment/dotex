using System.Runtime.InteropServices;

namespace OoBDev.Documents.Hpgl;

/// <summary>A point in scene units; Y already points down.</summary>
/// <param name="X">Horizontal position.</param>
/// <param name="Y">Vertical position.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct VectorPoint(double X, double Y);
