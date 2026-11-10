using System.Runtime.Versioning;
using OoBDev.Tui.TestUtilities;

[assembly: SupportedOSPlatform("windows")]

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>Renders the current Terminal.Gui screen buffer to a real PNG (see <see cref="TuiScreenRenderer"/>), not the plain-text <see cref="TuiTestRunner.DumpBuffer"/> capture.</summary>
internal static class TuiScreenshot
{
    public static void Save(string path) => GuideImage.Write(path, p => TuiScreenRenderer.Save(TuiTestRunner.CurrentApp, p));
}
