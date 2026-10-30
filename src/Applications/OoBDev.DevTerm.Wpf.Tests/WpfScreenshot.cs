using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
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

    /// <summary>Builds a window on a fresh STA thread, shows it off-screen, captures it as <paramref name="imageName"/> in the DevTerm user guide images folder, and closes it.</summary>
    public static void Capture(Func<Window> build, string imageName, double width = 900, double height = 650)
    {
        var path = Path.Combine(RepoRoot(), "docs", "applications", "devterm", "user-guide", "images", imageName);
        RunOnSta(() =>
        {
            var window = build();
            ShowOffScreen(window, width, height);
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            window.UpdateLayout();
            Save(window, path);
            window.Close();
        });
    }

    /// <summary>Runs <paramref name="action"/> on a new STA thread and rethrows its failure as an assertion failure.</summary>
    public static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(failure.Message, failure);
        }
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }

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
