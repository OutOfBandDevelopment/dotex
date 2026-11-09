using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Examples.Controls.WinForms.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class ControlsFormTests
{
    [TestMethod]
    public void Constructor_Samples_BecomeOneBoxEachPlusTheTypedFilterBox()
    {
        using var form = new ControlsForm();

        Assert.HasCount(ControlsForm.Samples.Count + 1, form.Boxes);
        Assert.AreEqual(ControlsForm.Samples[0].Filter, form.Boxes[0].Filter);
    }

    [TestMethod]
    public void Constructor_SampleTexts_ShowTheirValidity()
    {
        using var form = new ControlsForm();

        Assert.IsTrue(form.Boxes[0].IsValid);
        Assert.AreEqual(Color.LightGreen, form.Boxes[0].BackColor);
        Assert.IsFalse(form.Boxes[2].IsValid);
        Assert.AreEqual(Color.LightPink, form.Boxes[2].BackColor);
    }

    [TestMethod]
    public void FilterBox_NewPattern_RevalidatesTheTypedBox()
    {
        using var form = new ControlsForm();
        var typed = form.Boxes[^1];
        Assert.IsTrue(typed.IsValid);

        form.FilterBox.Text = "^[0-9]+$";

        Assert.AreEqual("^[0-9]+$", typed.Filter);
        Assert.IsFalse(typed.IsValid);
    }

    [TestMethod]
    public void FilterBox_InvalidPattern_ClearsTheFilterInsteadOfThrowing()
    {
        using var form = new ControlsForm();

        form.FilterBox.Text = "([";

        Assert.IsNull(form.Boxes[^1].Filter);
        Assert.IsTrue(form.Boxes[^1].IsValid);
    }
}
