using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.TestUtilities;

namespace OoBDev.Examples.Controls.WinForms.Tests;

/// <summary>The user journey on the real form: type a filter and see the boxes revalidate, with a screenshot of each step for the controls guide.</summary>
[TestCategory("DevLocal")]
[TestClass]
public sealed class ControlsJourneyTests
{
    [TestMethod]
    public void Journey_TypeAFilter_RevalidatesTheTypedBoxAndCapturesBothSteps()
    {
        // Test
        using var form = new ControlsForm();
        WinFormsCapture.Save(form, "winforms-1-initial.png");
        var before = form.Boxes[^1].IsValid;

        form.FilterBox.Text = "^[0-9]+$";
        WinFormsCapture.Save(form, "winforms-2-filter-typed.png");

        // Assert
        Assert.IsTrue(before);
        Assert.IsFalse(form.Boxes[^1].IsValid);
    }

    [TestMethod]
    public void Journey_ClickShowAdvanced_RevealsTheNotesRowAndCapturesBothSteps()
    {
        // Stage
        var settings = new ControlsSampleSettings();

        // Test
        using var form = new SettingsForm(settings);
        form.Show();
        WinFormsCapture.Save(form, "winforms-3-settings-initial.png");
        var before = form.View.Editors["Notes"].Visible;

        ((global::System.Windows.Forms.CheckBox)form.View.Editors["ShowAdvanced"]).Checked = true;
        global::System.Windows.Forms.Application.DoEvents();
        WinFormsCapture.Save(form, "winforms-4-settings-advanced.png");

        // Assert
        Assert.IsFalse(before);
        Assert.IsTrue(form.View.Editors["Notes"].Visible);
        Assert.IsTrue(settings.ShowAdvanced);
    }
}
