using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Examples.Controls.Avalonia.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RealClickTests
{
    private static HeadlessUnitTestSession _session => HeadlessSession.Instance;

    [TestMethod]
    public void Click_ShowAdvancedTwice_RevealsThenHidesTheNotesRow()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        var (afterFirst, afterSecond) = _session.Dispatch(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickCheckBox(window, "ShowAdvanced");
            var first = NotesVisible(window);
            ClickCheckBox(window, "ShowAdvanced");
            var second = NotesVisible(window);
            window.Close();
            return (first, second);
        }, default).GetAwaiter().GetResult();

        // Assert
        Assert.IsTrue(afterFirst, "one real click should reveal Notes");
        Assert.IsFalse(afterSecond, "a second real click should hide Notes");
        Assert.IsFalse(settings.ShowAdvanced);
    }

    [TestMethod]
    public void Click_Enabled_WritesTheModelOnTheFirstRealClick()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        _session.Dispatch(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickCheckBox(window, "Enabled");
            window.Close();
        }, default).GetAwaiter().GetResult();

        // Assert
        Assert.IsFalse(settings.Enabled);
    }

    private static bool NotesVisible(Window window) =>
        window.GetVisualDescendants().OfType<TextBox>().Any(t => t.IsEffectivelyVisible && t.DataContext is TextViewModel { Id: "Notes" });

    private static void ClickCheckBox(Window window, string id)
    {
        var check = window.GetVisualDescendants().OfType<CheckBox>().Single(c => c.DataContext is ToggleViewModel vm && vm.Id == id);
        var point = check.TranslatePoint(new Point(8, 8), window) ?? throw new global::System.InvalidOperationException("not laid out");
        window.MouseMove(point);
        Dispatcher.UIThread.RunJobs();
        window.MouseDown(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }
}
