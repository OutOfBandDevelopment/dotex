using System.Collections.Generic;

namespace OoBDev.Documents.Hpgl;

/// <summary>Parsed vector drawing, ready for any writer (SVG here, raster in the drawing extension).</summary>
/// <param name="Width">Scene width including the margin.</param>
/// <param name="Height">Scene height including the margin.</param>
/// <param name="Lines">The stroked lines, in drawing order.</param>
public sealed record VectorScene(double Width, double Height, IReadOnlyList<VectorPolyline> Lines)
{
    /// <summary>An empty 100 by 100 scene.</summary>
    public static VectorScene Empty { get; } = new(100, 100, []);
}
