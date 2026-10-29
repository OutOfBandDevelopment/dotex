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
        CollectionAssert.AreEqual(new[] { nameof(LiveDisplayViewModel.Revision) }, raised);
        Assert.AreEqual(42d, ((BarGraphState)model.State).ValueOf("a"));
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
