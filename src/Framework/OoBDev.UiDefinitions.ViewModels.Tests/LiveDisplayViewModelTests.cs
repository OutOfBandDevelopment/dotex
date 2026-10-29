using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.UiDefinitions.ViewModels.Tests;

/// <summary><see cref="LiveDisplayViewModel"/>.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveDisplayViewModelTests
{
    private static BarGraphControl Bar() => new()
    {
        Id = "bars",
        Label = "Bars",
        Channels = [new ChartChannel { Id = "a", Label = "A" }],
    };

    [TestMethod]
    public void Apply_PublishedValue_RaisesRevisionOnce()
    {
        // Stage
        var model = new LiveDisplayViewModel(Bar());
        var raised = new List<string?>();
        model.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        // Test
        var changed = model.Apply("a", "42");

        // Assert
        Assert.IsTrue(changed);
        Assert.AreEqual(1, model.Revision);
        CollectionAssert.AreEqual(new[] { nameof(LiveDisplayViewModel.Revision), nameof(LiveDisplayViewModel.State), nameof(LiveDisplayViewModel.Text) }, raised);
        Assert.AreEqual(42d, ((BarGraphState)model.State).ValueOf("a"));
    }

    [TestMethod]
    public void Apply_BarGraph_UpdatesBars()
    {
        // Stage
        var control = Bar();
        control.Maximum = 200;
        control.Unit = "V";
        var model = new LiveDisplayViewModel(control);

        // Test
        model.Apply("a", "50");

        // Assert
        Assert.IsTrue(model.IsBarGraph);
        Assert.IsFalse(model.IsIndicator);
        Assert.AreEqual(0.25, model.Bars[0].Fraction, 1e-9);
        Assert.AreEqual("50 V", model.Bars[0].ValueText);
    }

    [TestMethod]
    public void Apply_Indicator_UpdatesText()
    {
        // Stage
        var model = new LiveDisplayViewModel(new IndicatorControl { Id = "i", Label = "I", Expression = "a" });
        var before = model.Text;

        // Test
        model.Apply("a", "7");

        // Assert
        Assert.IsNull(before);
        Assert.IsTrue(model.IsIndicator);
        Assert.AreEqual("7", model.Text);
    }

    [TestMethod]
    public void Apply_UnknownId_LeavesRevision()
    {
        var model = new LiveDisplayViewModel(Bar());

        Assert.IsFalse(model.Apply("zzz", "1"));
        Assert.AreEqual(0, model.Revision);
    }

    [TestMethod]
    public void Constructor_FormControl_Throws()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => new LiveDisplayViewModel(new ToggleControl { Id = "t", Label = "T" }));
        Assert.ThrowsExactly<ArgumentNullException>(() => new LiveDisplayViewModel(null!));
    }
}
