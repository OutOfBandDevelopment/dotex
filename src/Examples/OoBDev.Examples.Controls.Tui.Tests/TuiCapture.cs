using OoBDev.Examples.TestUtilities;
using OoBDev.Tui.TestUtilities;
using System.Runtime.Versioning;
using Terminal.Gui.App;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.Examples.Controls.Tui.Tests;

/// <summary>Writes the headless Terminal.Gui screen buffer into the example guide as a PNG.</summary>
internal static class TuiCapture
{
    public static bool Save(IApplication app, string imageName) =>
        ExampleGuide.Write(imageName, path => TuiScreenRenderer.Save(app, path));
}
