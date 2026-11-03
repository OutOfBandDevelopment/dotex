using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.TestUtilities;
using OoBDev.DevTerm.Wpf;

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

    [TestMethod]
    public void ConfigureDevice_AfterReadAndEdit_IsCaptured() =>
        WpfScreenshot.Capture(BuildConfigureDevice, "wpf-configure-device.png", 640, 520);

    private static ConfigureDeviceWindow BuildConfigureDevice()
    {
        var dialog = new ConfigureDeviceDialogViewModel([new FakeDeviceConfigEditor()], "192.168.0.50", "8899");
        dialog.ReadAsync().GetAwaiter().GetResult();
        ((ConfigTextFieldViewModel)dialog.Fields[0]).Text = "192.168.0.77";
        return new ConfigureDeviceWindow(dialog);
    }
}
