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
}
