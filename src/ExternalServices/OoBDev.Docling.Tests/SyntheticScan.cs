using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SkiaSharp;

namespace OoBDev.Docling.Tests;

/// <summary>Renders known text as a rotated, noisy page image inside a one-page PDF, so OCR accuracy can be measured against the truth.</summary>
internal static class SyntheticScan
{
    public const int Width = 1240;
    public const int Height = 1754;

    /// <summary>The known text, one line per entry.</summary>
    public static readonly IReadOnlyList<string> Lines =
    [
        "Invoice number 20481 for the month of March",
        "Please remit payment within thirty days of receipt",
        "Total amount due is one thousand two hundred dollars",
        "Questions about this statement may be sent to billing",
    ];

    /// <summary>Renders the page; the same seed gives the same bytes on every host.</summary>
    /// <param name="degrees">Rotation of the text, as a crooked scan has.</param>
    /// <param name="noise">Number of grey specks.</param>
    /// <param name="seed">Seed of the noise.</param>
    public static byte[] RenderJpeg(float degrees = 1.5f, int noise = 4000, int seed = 7)
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Save();
        canvas.RotateDegrees(degrees, Width / 2f, Height / 2f);
        using var font = new SKFont(SKTypeface.Default, 40);
        using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        var y = 300f;
        foreach (var line in Lines)
        {
            canvas.DrawText(line, 100, y, font, paint);
            y += 90;
        }

        canvas.Restore();
        var random = new Random(seed);
        using var speck = new SKPaint { Color = new SKColor(90, 90, 90) };
        for (var i = 0; i < noise; i++)
        {
            canvas.DrawRect(random.Next(Width), random.Next(Height), 2, 2, speck);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }

    /// <summary>Wraps a JPEG in a one-page PDF (A4) with no text layer, so OCR has to read the picture.</summary>
    /// <param name="jpeg">The page image.</param>
    public static byte[] Pdf(byte[] jpeg)
    {
        ArgumentNullException.ThrowIfNull(jpeg);
        var content = "q 595 0 0 842 0 0 cm /Im0 Do Q";
        using var output = new MemoryStream();
        var offsets = new List<long>();
        void Text(string s) => output.Write(Encoding.ASCII.GetBytes(s));
        void Object(int number, Action body)
        {
            offsets.Add(output.Position);
            Text(number.ToString(CultureInfo.InvariantCulture) + " 0 obj\n");
            body();
            Text("\nendobj\n");
        }

        Text("%PDF-1.4\n");
        Object(1, () => Text("<< /Type /Catalog /Pages 2 0 R >>"));
        Object(2, () => Text("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"));
        Object(3, () => Text("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /XObject << /Im0 5 0 R >> >> >>"));
        Object(4, () => Text($"<< /Length {content.Length} >>\nstream\n{content}\nendstream"));
        Object(5, () =>
        {
            Text($"<< /Type /XObject /Subtype /Image /Width {Width} /Height {Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpeg.Length} >>\nstream\n");
            output.Write(jpeg);
            Text("\nendstream");
        });
        var xref = output.Position;
        Text($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            Text(offset.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
        }

        Text($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return output.ToArray();
    }
}
