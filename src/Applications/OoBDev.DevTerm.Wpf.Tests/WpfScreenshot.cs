using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OoBDev.DevTerm.TestUtilities;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Renders a laid-out WPF <see cref="Window"/> to a guide PNG through <see cref="GuideImage"/>, so the committed image changes only when the masked capture differs.</summary>
internal static class WpfScreenshot
{
    /// <summary>Shows <paramref name="window"/> far off-screen: a window that was never shown has no compositor target and renders blank.</summary>
    public static void ShowOffScreen(Window window, double width = 900, double height = 650)
    {
        window.Width = width;
        window.Height = height;
        window.WindowStyle = WindowStyle.None;
        window.ShowInTaskbar = false;
        window.Left = -10000;
        window.Top = -10000;
        window.Show();
        window.UpdateLayout();
    }

    /// <summary>Renders <paramref name="window"/> and syncs it over <paramref name="path"/>; returns true when the committed image was replaced.</summary>
    public static bool Save(Window window, string path) => GuideImage.Write(path, temp => Render(window, temp));

    private static void Render(Window window, string path)
    {
        var width = (int)Math.Max(1, window.ActualWidth);
        var height = (int)Math.Max(1, window.ActualHeight);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
