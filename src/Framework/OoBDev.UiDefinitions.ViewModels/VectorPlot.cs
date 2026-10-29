using System.Globalization;
using System.Linq;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Maps a vector's coordinates into a 100 by 100 plot box (origin top left, axes spanning the control's range).</summary>
internal static class VectorPlot
{
    internal const string DefaultColor = "#1F77B4";

    // Oblique projection: z shifts a point down-left by this share of itself.
    private const double ObliqueShare = 0.35;

    internal static (double X, double Y) Project((double X, double Y, double Z) point, VectorControl control)
    {
        var extent = control.Range <= 0 ? 1 : control.Range;
        if (control.Coordinates == CoordinateSystem.XYZ)
        {
            extent *= 1 + ObliqueShare;
        }

        var u = (point.X - (ObliqueShare * point.Z)) / extent;
        var v = (point.Y - (ObliqueShare * point.Z)) / extent;
        return (Clamp(50 + (u * 50)), Clamp(50 - (v * 50)));
    }

    internal static (double X, double Y) Project(VectorState state, (double X, double Y, double Z) point) => Project(point, state.Control);

    internal static string Trail(VectorState state)
    {
        var points = state.Trail.Select(p => Project(state, p)).ToList();
        if (state.HasPoint)
        {
            points.Add(Project(state, state.Point));
        }

        return string.Join(' ', points.Select(p => string.Create(CultureInfo.InvariantCulture, $"{p.X:0.##},{p.Y:0.##}")));
    }

    internal static string ColorOf(VectorState state) => state.Color is { } c ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : DefaultColor;

    private static double Clamp(double value) => global::System.Math.Clamp(value, 0, 100);
}
