using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.DevTerm.DeviceManifests.Editing;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Captures the expression picker for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ExpressionPickerScreenshotTests
{
    internal static readonly ValuePath[] Paths =
    [
        new("volts", ValuePathType.Number, ValuePathSource.Control, "Voltage", "V", 0, 30),
        new("amps", ValuePathType.Number, ValuePathSource.Control, "Current", "A", 0, 5),
        new("status", ValuePathType.Text, ValuePathSource.ResponsePattern, "Status"),
    ];

    [DataTestMethod]
    [DataRow("wpf-expression-picker", "round({volts} * {amps}, 1)", PickerMode.Expression, "")]
    [DataRow("wpf-expression-picker-error", "round({volts} * ", PickerMode.Expression, "")]
    [DataRow("wpf-expression-picker-warning", "{volts} * {watts}", PickerMode.Expression, "")]
    [DataRow("wpf-expression-picker-text", "matches({status}, 'READY') ? 100 : 0", PickerMode.Expression, "")]
    [DataRow("wpf-expression-picker-parameters", "round({volts} * 100, 0); {amps}", PickerMode.ExpressionList, "")]
    [DataRow("wpf-expression-picker-find", "{volts}", PickerMode.Expression, "amp")]
    [DataRow("wpf-expression-picker-channels", "volts:Volts:#FF6600; amps", PickerMode.Channels, "")]
    public void Window_Mode_IsCaptured(string name, string text, PickerMode mode, string filter) =>
        WpfScreenshot.Capture(
            () => new ExpressionPickerWindow(new ExpressionPickerViewModel(Paths, text, seed: 7, mode: mode) { Filter = filter }),
            name + ".png",
            560,
            460);
}
