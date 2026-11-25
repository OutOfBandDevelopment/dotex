using System;
using System.Windows;
using System.Windows.Threading;

namespace OoBDev.Examples.Controls.Wpf;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var window = ControlsWindow.Build(new ControlsSampleSettings(), out _, out var live);
        var timer = new DispatcherTimer { Interval = LiveSampleFeed.Interval };
        timer.Tick += (_, _) => live.Step();
        timer.Start();
        new Application().Run(window);
    }
}
