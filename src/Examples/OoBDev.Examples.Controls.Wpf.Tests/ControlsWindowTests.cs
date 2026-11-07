using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.Controls.Wpf;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.Examples.Controls.Wpf.Tests;

/// <summary>The generated sample form on a real WPF window; needs an STA thread and nothing is shown.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ControlsWindowTests
{
    [TestMethod]
    public void Build_GeneratesOneRowPerSampleSetting_AdvancedHiddenUntilShown()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        var window = Run(() => { var w = ControlsWindow.Build(settings, out var f); return (w, f); });

        // Assert
        var ids = window.f.Sections.Where(s => s.HasVisibleRows).SelectMany(s => s.Rows).Where(r => r.IsVisible).Select(r => r.Id).ToList();
        Assert.AreEqual("Enabled,Name,Mode,ShowAdvanced,Count,Level", string.Join(",", ids));
    }

    [TestMethod]
    public void Toggle_ShowAdvanced_RevealsTheAdvancedSection()
    {
        // Stage
        var settings = new ControlsSampleSettings();
        var (_, form) = Run(() => { var w = ControlsWindow.Build(settings, out var f); return (w, f); });

        // Test
        settings.ShowAdvanced = true;
        form.Refresh();

        // Assert
        Assert.IsTrue(form.Sections.Single(s => s.Label == "Advanced").HasVisibleRows);
    }

    [TestMethod]
    public void Click_ShowAdvancedCheckBox_ShowsTheAdvancedGroupOnScreen()
    {
        // Test
        var settings = new ControlsSampleSettings();
        var (before, after) = Run(() =>
        {
            var window = ControlsWindow.Build(settings, out _);
            var view = (FrameworkElement)window.Content;
            view.Measure(new Size(480, 520));
            view.Arrange(new Rect(0, 0, 480, 520));
            view.UpdateLayout();
            var group = Descendants<GroupBox>(view).Single(g => (string?)g.Header == "Advanced");
            var box = Descendants<CheckBox>(view).Single(c => c.DataContext is ToggleViewModel { Id: "ShowAdvanced" });
            var hidden = group.Visibility;

            typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(box, null);
            view.UpdateLayout();
            return (hidden, group.Visibility);
        });

        // Assert
        Assert.AreEqual(Visibility.Collapsed, before);
        Assert.IsTrue(settings.ShowAdvanced, "the click did not reach the model");
        Assert.AreEqual(Visibility.Visible, after);
    }

    [TestMethod]
    public void Build_RendersAControlForEachVisibleRow()
    {
        // Test
        var count = Run(() =>
        {
            var window = ControlsWindow.Build(new ControlsSampleSettings(), out _);
            window.Content.GetType();
            var view = (FrameworkElement)window.Content;
            view.Measure(new Size(480, 520));
            view.Arrange(new Rect(0, 0, 480, 520));
            view.UpdateLayout();
            return Descendants<Control>(view).Count();
        });

        // Assert
        Assert.IsGreaterThan(5, count);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root)
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

    private static T Run<T>(Func<T> action)
    {
        T result = default!;
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }

        return result;
    }
}
