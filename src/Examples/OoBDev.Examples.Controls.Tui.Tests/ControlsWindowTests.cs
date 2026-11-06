using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terminal.Gui.App;

namespace OoBDev.Examples.Controls.Tui.Tests;

/// <summary>The generated sample form on a headless Terminal.Gui application; nothing is shown.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class ControlsWindowTests
{
    private static void Run(ControlsSampleSettings settings, Action<IApplication, OoBDev.UiDefinitions.Tui.TuiFormParts> body)
    {
        using var app = Application.Create().Init("dotnet");
        using var window = ControlsWindow.Build(app, settings, out var binding, out var parts);
        using (binding)
        {
            var token = app.Begin(window) ?? throw new NotSupportedException();
            app.LayoutAndDraw(true);
            try
            {
                body(app, parts);
            }
            finally
            {
                app.End(token);
            }
        }
    }

    [TestMethod]
    public void Build_RendersAWidgetForEachBasicAndRangeSetting_AdvancedHidden()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        Run(settings, (_, parts) =>
        {
            // Assert
            var ids = parts.ControlViews.Where(p => p.Value.Visible).Select(p => p.Key).OrderBy(i => i, StringComparer.Ordinal).ToList();
            CollectionAssert.AreEqual(new[] { "Count", "Enabled", "Level", "Mode", "Name", "ShowAdvanced" }, ids);
            Assert.IsFalse(parts.ControlViews.TryGetValue("Notes", out var notes) && notes.Visible);
        });
    }

    [TestMethod]
    public void ModelChange_ShowAdvanced_RevealsTheAdvancedSection()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        Run(settings, (app, parts) =>
        {
            // Test
            parts.Binding.SetBool("ShowAdvanced", true);
            app.LayoutAndDraw(true);

            // Assert
            Assert.IsTrue(parts.ControlViews["Notes"].Visible);
        });
    }
}
