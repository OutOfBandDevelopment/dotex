using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OoBDev.Examples.TestUtilities;

/// <summary>Shows a WPF window far off-screen (a window never shown has no compositor target and renders blank) and renders it to a PNG.</summary>
public static class WpfCapture
{
    /// <summary>Shows <paramref name="window"/> off-screen at the given size and lets pending layout and binding work finish.</summary>
    public static void ShowOffScreen(Window window, double width, double height)
    {
        ArgumentNullException.ThrowIfNull(window);
        window.Width = width;
        window.Height = height;
        window.WindowStyle = WindowStyle.None;
        window.ShowInTaskbar = false;
        window.Left = -10000;
        window.Top = -10000;
        window.Show();
        Settle(window);
    }

    /// <summary>Runs queued dispatcher work and layout so a bound change is on screen before a capture.</summary>
    public static void Settle(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
        window.UpdateLayout();
    }

    /// <summary>Captures <paramref name="window"/> as <paramref name="imageName"/> in the controls guide; true when the committed image was replaced.</summary>
    public static bool Save(Window window, string imageName)
    {
        ArgumentNullException.ThrowIfNull(window);
        return ExampleGuide.Write(imageName, path =>
        {
            var bitmap = new RenderTargetBitmap((int)Math.Max(1, window.ActualWidth), (int)Math.Max(1, window.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        });
    }

    /// <summary>Runs <paramref name="action"/> on a new STA thread and rethrows its failure.</summary>
    public static T RunOnSta<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        T result = default!;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
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
            throw new InvalidOperationException(failure.Message, failure);
        }

        return result;
    }
}
