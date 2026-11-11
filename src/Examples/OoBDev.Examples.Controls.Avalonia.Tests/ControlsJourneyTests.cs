using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Examples.Controls.Avalonia.Tests;

/// <summary>The user journey on the real Avalonia window, headless: click the controls, check what appears, and attach a screenshot of each step.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ControlsJourneyTests
{
    private static HeadlessUnitTestSession _session => HeadlessSession.Instance;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Journey_ClickShowAdvanced_RevealsTheNotesRowAndCapturesBothSteps()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        var (before, after) = _session.Dispatch(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var notesBefore = window.GetVisualDescendants().OfType<TextBox>().Count(t => t.IsEffectivelyVisible && t.DataContext is TextViewModel { Id: "Notes" });
            Screenshots.Save(TestContext, window, "initial");

            var check = window.GetVisualDescendants().OfType<CheckBox>().Single(c => c.DataContext is ToggleViewModel { Id: "ShowAdvanced" });
            check.Command!.Execute(check.CommandParameter);
            Dispatcher.UIThread.RunJobs();
            var notesAfter = window.GetVisualDescendants().OfType<TextBox>().Count(t => t.IsEffectivelyVisible && t.DataContext is TextViewModel { Id: "Notes" });
            Screenshots.Save(TestContext, window, "advanced-shown");
            window.Close();
            return (notesBefore, notesAfter);
        }, default).GetAwaiter().GetResult();

        // Assert
        Assert.AreEqual(0, before);
        Assert.AreEqual(1, after);
        Assert.IsTrue(settings.ShowAdvanced);
    }

    [TestMethod]
    public void Journey_ClickEnabled_WritesTheModelThroughTheRealCheckBox()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        _session.Dispatch(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var check = window.GetVisualDescendants().OfType<CheckBox>().Single(c => c.DataContext is ToggleViewModel { Id: "Enabled" });
            check.Command!.Execute(check.CommandParameter);
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }, default).GetAwaiter().GetResult();

        // Assert
        Assert.IsFalse(settings.Enabled);
    }
}
