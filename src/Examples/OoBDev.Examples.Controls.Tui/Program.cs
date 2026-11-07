using Terminal.Gui.App;

namespace OoBDev.Examples.Controls.Tui;

internal static class Program
{
    private static void Main()
    {
        using var app = Application.Create().Init();
        using var window = ControlsWindow.Build(app, new ControlsSampleSettings(), out var binding, out _, out var live);
        using (binding)
        {
            app.AddTimeout(LiveSampleFeed.Interval, () =>
            {
                live.Step();
                return true;
            });
            app.Run(window);
        }
    }
}
