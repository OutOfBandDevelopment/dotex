using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.TestUtilities;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Examples.Controls.Wpf.Tests;

/// <summary>The user journey on the real WPF window: click the controls, check what appears, and capture a screenshot of each step for the controls guide. Needs a desktop session.</summary>
[TestCategory("DevLocal")]
[TestClass]
public sealed class ControlsJourneyTests
{
    [TestMethod]
    public void Journey_ClickShowAdvanced_RevealsTheAdvancedGroupAndCapturesBothSteps()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        var (collapsed, visible, notesRow) = WpfCapture.RunOnSta(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            WpfCapture.ShowOffScreen(window, 520, 780);
            var group = Descendants<GroupBox>(window).Single(g => (string?)g.Header == "Advanced");
            var before = group.Visibility;
            WpfCapture.Save(window, "wpf-1-initial.png");

            Click(Descendants<CheckBox>(window).Single(c => c.DataContext is ToggleViewModel { Id: "ShowAdvanced" }));
            WpfCapture.Settle(window);
            var after = group.Visibility;
            var notes = Descendants<TextBox>(group).Count();
            WpfCapture.Save(window, "wpf-2-advanced-shown.png");
            window.Close();
            return (before, after, notes);
        });

        // Assert
        Assert.AreEqual(Visibility.Collapsed, collapsed);
        Assert.AreEqual(Visibility.Visible, visible);
        Assert.AreEqual(1, notesRow);
        Assert.IsTrue(settings.ShowAdvanced);
    }

    [TestMethod]
    public void Journey_ClickEnabled_WritesTheModelThroughTheRealCheckBox()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        WpfCapture.RunOnSta(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            WpfCapture.ShowOffScreen(window, 520, 780);
            Click(Descendants<CheckBox>(window).Single(c => c.DataContext is ToggleViewModel { Id: "Enabled" }));
            WpfCapture.Settle(window);
            window.Close();
            return 0;
        });

        // Assert
        Assert.IsFalse(settings.Enabled);
    }

    private static void Click(ButtonBase button) =>
        typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, null);

    private static global::System.Collections.Generic.IEnumerable<T> Descendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var inner in Descendants<T>(child))
            {
                yield return inner;
            }
        }
    }
}
