using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Windows.Forms;

namespace OoBDev.Extensions.Windows.Forms.Tests;

/// <summary>The control is created without a handle or window; nothing is shown.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ValidTextBoxTests
{
    [TestMethod]
    public void IsValid_NoFilter_IsAlwaysTrue()
    {
        // Stage
        using var box = new ValidTextBox { Text = "anything" };

        // Assert
        Assert.IsTrue(box.IsValid);
        Assert.AreEqual(Color.LightGreen, box.BackColor);
    }

    [TestMethod]
    public void Text_NotMatchingTheFilter_ShowsTheInvalidColor()
    {
        // Stage
        using var box = new ValidTextBox { Filter = "^[0-9]+$" };

        // Test
        box.Text = "12a";

        // Assert
        Assert.IsFalse(box.IsValid);
        Assert.AreEqual(Color.LightPink, box.BackColor);
    }

    [TestMethod]
    public void Text_MatchingTheFilter_ShowsTheValidColor()
    {
        // Stage
        using var box = new ValidTextBox { Filter = "^[0-9]+$", ValidColor = Color.Lime };

        // Test
        box.Text = "123";

        // Assert
        Assert.IsTrue(box.IsValid);
        Assert.AreEqual(Color.Lime, box.BackColor);
    }

    [TestMethod]
    public void ValidateChanged_OnlyWhenTheStateChanges()
    {
        // Stage
        using var box = new ValidTextBox { Filter = "^[0-9]+$" };
        var events = new List<ValidationEventArgs>();
        box.ValidateChanged += (_, e) => events.Add(e);

        // Test
        box.Text = "1";
        box.Text = "12";
        box.Text = "x";

        // Assert
        Assert.HasCount(2, events);
        Assert.IsTrue(events[0].IsValid);
        Assert.AreEqual("1", events[0].Value);
        Assert.IsFalse(events[1].IsValid);
    }

    [TestMethod]
    public void Filter_Changed_Revalidates()
    {
        // Stage
        using var box = new ValidTextBox { Text = "abc" };

        // Test
        box.Filter = "^[0-9]+$";

        // Assert
        Assert.AreEqual(Color.LightPink, box.BackColor);
    }
}
