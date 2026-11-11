using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.UiDefinitions.WinForms.Tests;

/// <summary>The colour picker's mapping between the <c>r,g,b</c> text and the dialog colour, without opening a dialog.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class WinFormsColorPickerTests
{
    [TestMethod]
    public void Pick_CurrentColour_StartsTheDialogThere()
    {
        // Stage
        Color? seen = null;
        var picker = new WinFormsColorPicker(start =>
        {
            seen = start;
            return null;
        });

        // Test
        picker.Pick("10,20,30");

        // Assert
        Assert.AreEqual(Color.FromArgb(10, 20, 30), seen);
    }

    [TestMethod]
    public void Pick_ChosenColour_ReturnsRgbText()
    {
        // Stage
        var picker = new WinFormsColorPicker(_ => Color.FromArgb(1, 2, 3));

        // Test
        var result = picker.Pick(null);

        // Assert
        Assert.AreEqual("1,2,3", result);
    }

    [TestMethod]
    public void Pick_Cancelled_ReturnsNull()
    {
        // Stage
        var picker = new WinFormsColorPicker(_ => null);

        // Test
        var result = picker.Pick("5,5,5");

        // Assert
        Assert.IsNull(result);
    }
}
