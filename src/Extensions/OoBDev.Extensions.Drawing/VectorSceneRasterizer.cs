using System;
using OoBDev.Documents.Hpgl;
using SkiaSharp;

namespace OoBDev.Extensions.Drawing;

/// <summary>Draws a <see cref="VectorScene"/> to PNG or JPEG bytes: white background, anti-aliased 1 px strokes, scaled to the requested width with the aspect ratio kept.</summary>
public static class VectorSceneRasterizer
{
    /// <summary>The widest bitmap <see cref="Rasterize"/> will make.</summary>
    public const int MaxWidthPixels = 8192;

    /// <summary>Renders <paramref name="scene"/> at <paramref name="widthPixels"/> wide.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="widthPixels"/> is below 1 or above <see cref="MaxWidthPixels"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="format"/> is not PNG or JPEG.</exception>
    public static byte[] Rasterize(VectorScene scene, int widthPixels, SKEncodedImageFormat format = SKEncodedImageFormat.Png)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentOutOfRangeException.ThrowIfLessThan(widthPixels, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(widthPixels, MaxWidthPixels);
        if (format is not (SKEncodedImageFormat.Png or SKEncodedImageFormat.Jpeg))
        {
            throw new ArgumentException("Only PNG and JPEG are supported.", nameof(format));
        }

        var scale = widthPixels / scene.Width;
        var heightPixels = Math.Max(1, (int)Math.Round(scene.Height * scale));
        using var bitmap = new SKBitmap(widthPixels, heightPixels);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint { IsAntialias = true, IsStroke = true, StrokeWidth = 1, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round };
        foreach (var line in scene.Lines)
        {
            if (line.Points.Count < 2)
            {
                continue;
            }

            paint.Color = SKColor.Parse(line.Color);
            for (var i = 1; i < line.Points.Count; i++)
            {
                var from = line.Points[i - 1];
                var to = line.Points[i];
                canvas.DrawLine((float)(from.X * scale), (float)(from.Y * scale), (float)(to.X * scale), (float)(to.Y * scale), paint);
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }
}
