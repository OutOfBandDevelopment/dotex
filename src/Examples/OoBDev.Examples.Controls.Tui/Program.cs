using Terminal.Gui.App;

namespace OoBDev.Examples.Controls.Tui;

internal static class Program
{
    private static void Main()
    {
        using var app = Application.Create().Init();
        using var window = ControlsWindow.Build(app, new ControlsSampleSettings(), out var binding, out _);
        using (binding)
        {
            app.Run(window);
        }
    }
}
