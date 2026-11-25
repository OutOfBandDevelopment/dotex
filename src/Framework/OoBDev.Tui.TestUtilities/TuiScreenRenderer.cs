using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.Versioning;
using Terminal.Gui.App;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.Tui.TestUtilities;

/// <summary>
/// Renders the headless Terminal.Gui screen buffer to a PNG, cell by cell (windows only, like the rest of the repository's
/// image test infrastructure). The headless "dotnet" driver reports white-on-white for any cell still on the default
/// color scheme because no real terminal resolves a theme, so a cell whose foreground equals its background falls back to
/// black on white.
/// </summary>
public static class TuiScreenRenderer
{
    private const int _cellWidth = 9;
    private const int _cellHeight = 18;

    /// <summary>Writes the application's current screen buffer as a PNG.</summary>
    /// <param name="app">The running headless application.</param>
    /// <param name="path">The PNG file to write; its folder is created when missing.</param>
    public static void Save(IApplication app, string path)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrEmpty(path);
        var buffer = app.Driver!.GetOutputBuffer();
        using var bitmap = new Bitmap(buffer.Cols * _cellWidth, buffer.Rows * _cellHeight);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font("Cascadia Mono", _cellHeight - 4f, FontStyle.Regular, GraphicsUnit.Pixel);
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        graphics.Clear(Color.White);
        if (buffer.Contents is not null)
        {
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
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
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
