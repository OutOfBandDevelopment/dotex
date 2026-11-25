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
        CollectionAssert.AreEqual(new[] { nameof(LiveDisplayViewModel.Revision), nameof(LiveDisplayViewModel.State), nameof(LiveDisplayViewModel.Text), nameof(LiveDisplayViewModel.Readout), nameof(LiveDisplayViewModel.PlotTrail), nameof(LiveDisplayViewModel.PlotX), nameof(LiveDisplayViewModel.PlotY), nameof(LiveDisplayViewModel.PlotColor) }, raised);
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
    public void Apply_StripChart_BuildsPointsInUnitBox()
    {
        // Stage
        var model = new LiveDisplayViewModel(new StripChartControl
        {
            Id = "chart",
            Label = "Chart",
            HistoryLength = 3,
            Minimum = 0,
            Maximum = 10,
            Channels = [new ChartChannel { Id = "a", Label = "A" }],
        });

        // Test
        model.Apply("a", "0");
        model.Apply("a", "10");

        // Assert
        Assert.IsTrue(model.IsStripChart);
        Assert.AreEqual("50,100 100,0", model.Lines[0].Points);
    }

    [TestMethod]
    public void Apply_VectorXY_ProjectsPointAndTrailIntoUnitBox()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y", Range = 2 });
        model.ApplyAll([new("x", "0"), new("y", "0")]);

        // Test
        model.ApplyAll([new("x", "1"), new("y", "-2")]);

        // Assert
        Assert.AreEqual(75d, model.PlotX);
        Assert.AreEqual(100d, model.PlotY);
        Assert.AreEqual("50,50 75,100", model.PlotTrail);
        Assert.AreEqual("#1F77B4", model.PlotColor);
    }

    [TestMethod]
    public void Apply_VectorPolar_PlotsRadiusAsFractionOfRange()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", Coordinates = CoordinateSystem.Polar, RadiusId = "r", AngleId = "a", Range = 4 });

        // Test
        model.ApplyAll([new("r", "2"), new("a", "0")]);

        // Assert
        Assert.AreEqual(75d, model.PlotX);
        Assert.AreEqual(50d, model.PlotY);
    }

    [TestMethod]
    public void Apply_VectorXYZ_ShiftsPointByDepthAndClamps()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", Coordinates = CoordinateSystem.XYZ, XId = "x", YId = "y", ZId = "z", Range = 1, TrailLength = 0 });

        // Test
        model.ApplyAll([new("x", "0"), new("y", "0"), new("z", "1")]);
        var shifted = (model.PlotX, model.PlotY);
        model.ApplyAll([new("x", "100"), new("y", "-100"), new("z", "0")]);

        // Assert
        Assert.IsLessThan(50d, shifted.Item1);
        Assert.IsGreaterThan(50d, shifted.Item2);
        Assert.AreEqual(100d, model.PlotX);
        Assert.AreEqual(100d, model.PlotY);
        Assert.AreEqual("100,100", model.PlotTrail);
    }

    [TestMethod]
    public void Apply_VectorHue_ColoursPoint()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y", HueId = "h" });

        // Test
        model.ApplyAll([new("x", "0"), new("y", "0"), new("h", "0")]);

        // Assert
        Assert.AreEqual("#FF0000", model.PlotColor);
    }

    [TestMethod]
    public void PlotProperties_NonVector_AreNeutral()
    {
        // Stage
        var model = new LiveDisplayViewModel(new IndicatorControl { Id = "i", Label = "I", Expression = "v" });

        // Assert
        Assert.AreEqual(string.Empty, model.PlotTrail);
        Assert.AreEqual(50d, model.PlotX);
        Assert.AreEqual(50d, model.PlotY);
    }

    [TestMethod]
    public void Apply_Vector_UpdatesReadout()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y" });

        // Test
        model.ApplyAll([new KeyValuePair<string, string>("x", "1"), new KeyValuePair<string, string>("y", "2")]);

        // Assert
        Assert.IsTrue(model.IsVector);
        Assert.AreEqual("x=1 y=2", model.Readout);
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
