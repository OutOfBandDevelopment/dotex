using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Captures the tool windows for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ToolWindowScreenshotTests
{
    [TestMethod]
    public void ConverterTools_Empty_IsCaptured() =>
        WpfScreenshot.Capture(() => new ConverterToolsWindow(new ConverterToolsDialogViewModel(new ConverterToolsEditor([]))), "wpf-converter-tools.png", 700, 450);

    [TestMethod]
    public void StreamMonitor_Empty_IsCaptured() =>
        WpfScreenshot.Capture(() => new StreamMonitorWindow(new StreamMonitorDialogViewModel(new StreamMonitor())), "wpf-stream-monitor.png");
}
