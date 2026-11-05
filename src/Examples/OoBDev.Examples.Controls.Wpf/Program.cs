using System;
using System.Windows;

namespace OoBDev.Examples.Controls.Wpf;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var window = ControlsWindow.Build(new ControlsSampleSettings(), out _);
        new Application().Run(window);
    }
}
