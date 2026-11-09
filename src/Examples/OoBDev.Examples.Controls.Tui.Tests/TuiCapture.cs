using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Terminal.Gui.App;
using OoBDev.Examples.TestUtilities;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.Examples.Controls.Tui.Tests;

/// <summary>Renders the headless Terminal.Gui screen buffer to a PNG, cell by cell. The headless driver reports white-on-white for cells on the default scheme, so equal colors fall back to black on white.</summary>
internal static class TuiCapture
{
    private const int _cellWidth = 9;
    private const int _cellHeight = 18;

    public static bool Save(IApplication app, string imageName) =>
        ExampleGuide.Write(imageName, path => Render(app, path));

    private static void Render(IApplication app, string path)
    {
        var buffer = app.Driver!.GetOutputBuffer();
        using var bitmap = new Bitmap(buffer.Cols * _cellWidth, buffer.Rows * _cellHeight);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font("Cascadia Mono", _cellHeight - 4f, FontStyle.Regular, GraphicsUnit.Pixel);
        graphics.TextRenderingHint = global::System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.Clear(Color.White);
        if (buffer.Contents is null)
        {
            bitmap.Save(path, ImageFormat.Png);
            return;
        }

        for (var row = 0; row < buffer.Rows; row++)
        {
            for (var col = 0; col < buffer.Cols; col++)
            {
                var cell = buffer.Contents[row, col];
                var (background, foreground) = Colors(cell.Attribute);
                using (var brush = new SolidBrush(background))
                {
                    graphics.FillRectangle(brush, col * _cellWidth, row * _cellHeight, _cellWidth, _cellHeight);
                }

                if (!string.IsNullOrWhiteSpace(cell.Grapheme))
                {
                    using var text = new SolidBrush(foreground);
                    graphics.DrawString(cell.Grapheme, font, text, col * _cellWidth, (row * _cellHeight) - 2);
                }
            }
        }

        bitmap.Save(path, ImageFormat.Png);
    }

    private static (Color Background, Color Foreground) Colors(Terminal.Gui.Drawing.Attribute? attribute)
    {
        if (attribute is not { } value)
        {
            return (Color.White, Color.Black);
        }

        var background = Color.FromArgb(value.Background.R, value.Background.G, value.Background.B);
        var foreground = Color.FromArgb(value.Foreground.R, value.Foreground.G, value.Foreground.B);
        return background == foreground ? (Color.White, Color.Black) : (background, foreground);
    }
}
