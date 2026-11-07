using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terminal.Gui.App;
using Terminal.Gui.Input;

namespace OoBDev.Examples.Controls.Tui.Tests;

/// <summary>The user journey on a headless Terminal.Gui screen: focus the Show advanced check box, press Space, see the Advanced section appear, with a screenshot of each step for the controls guide.</summary>
[TestCategory("DevLocal")]
[TestClass]
[DoNotParallelize]
public sealed class ControlsJourneyTests
{
    [TestMethod]
    public void Journey_PressSpaceOnShowAdvanced_RevealsTheAdvancedSectionAndCapturesBothSteps()
    {
        // Stage
        var settings = new ControlsSampleSettings();
        using var app = Application.Create().Init("dotnet");
        using var window = ControlsWindow.Build(app, settings, out var binding, out var parts, out _);
        using (binding)
        {
            var token = app.Begin(window) ?? throw new NotSupportedException();
            app.LayoutAndDraw(true);
            try
            {
                var hiddenBefore = !parts.ControlViews["Notes"].Visible;
                TuiCapture.Save(app, "tui-1-initial.png");

                // Test
                parts.ControlViews["ShowAdvanced"].SetFocus();
                app.Keyboard.RaiseKeyDownEvent(Key.Space);
                app.LayoutAndDraw(true);
                TuiCapture.Save(app, "tui-2-advanced-shown.png");

                // Assert
                Assert.IsTrue(hiddenBefore);
                Assert.IsTrue(settings.ShowAdvanced, "the key press did not reach the model");
                Assert.IsTrue(parts.ControlViews["Notes"].Visible);
            }
            finally
            {
                app.End(token);
            }
        }
    }
}
