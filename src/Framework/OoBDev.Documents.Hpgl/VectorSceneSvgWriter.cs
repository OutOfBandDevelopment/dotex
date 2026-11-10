using System.Globalization;
using System.Text;

namespace OoBDev.Documents.Hpgl;

/// <summary>Writes a <see cref="VectorScene"/> as an SVG document.</summary>
public static class VectorSceneSvgWriter
{
    /// <summary>The SVG text for <paramref name="scene"/>.</summary>
    public static string Write(VectorScene scene)
    {
        System.ArgumentNullException.ThrowIfNull(scene);
        var builder = new StringBuilder();
        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        if (scene.Lines.Count == 0)
        {
            builder.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\" viewBox=\"0 0 100 100\"></svg>\n");
            return builder.ToString();
        }

        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{scene.Width:F2}\" height=\"{scene.Height:F2}\" viewBox=\"0 0 {scene.Width:F2} {scene.Height:F2}\">\n");
        foreach (var line in scene.Lines)
        {
            builder.Append("  <path d=\"");
            for (var i = 0; i < line.Points.Count; i++)
            {
                var point = line.Points[i];
                builder.Append(CultureInfo.InvariantCulture, $"{(i == 0 ? "M" : "L")} {point.X:F2},{point.Y:F2} ");
            }

            builder.Append(CultureInfo.InvariantCulture, $"\" fill=\"none\" stroke=\"{line.Color}\" stroke-width=\"1\" />\n");
        }

        builder.Append("</svg>\n");
        return builder.ToString();
    }
}
