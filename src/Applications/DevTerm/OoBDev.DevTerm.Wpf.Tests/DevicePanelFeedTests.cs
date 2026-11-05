using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Values a structured presenter decodes reach the live displays of a device panel.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class DevicePanelFeedTests
{
    private sealed class FakeSource : IStructuredPresenter
    {
        public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

        public bool HasSubscribers => ValuesChanged is not null;

        public void Publish(IReadOnlyDictionary<string, string> values) => ValuesChanged?.Invoke(this, values);
    }

    private sealed class Surface : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private static UiDefinition Definition() => new()
    {
        Name = "Fake",
        Sections = [new UiSection { Controls = [new IndicatorControl { Id = "led", Label = "Led", Expression = "led" }, new ToggleControl { Id = "d1", Label = "D1" }] }],
    };

    [TestMethod]
    public void ValuesChanged_PublishedValue_UpdatesTheIndicatorAndDisposeDetaches()
    {
        // Stage
        var source = new FakeSource();
        var surface = new Surface();
        var feed = new DevicePanelFeed(Definition(), source, surface);

        // Test
        source.Publish(new Dictionary<string, string> { ["led"] = "on" });
        var text = feed.Displays[0].Text;
        feed.Dispose();

        // Assert
        Assert.HasCount(1, feed.Displays);
        Assert.AreEqual("on", text);
        Assert.IsFalse(source.HasSubscribers);
        Assert.IsTrue(surface.Disposed);
    }
}
