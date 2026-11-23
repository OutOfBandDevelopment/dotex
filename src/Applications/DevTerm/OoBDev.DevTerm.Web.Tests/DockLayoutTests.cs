using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Web.Docking;
using OoBDev.TestUtilities;
namespace OoBDev.DevTerm.Web.Tests;

/// <summary>The dock tree model: operations, JSON and validation.</summary>
[TestClass]
[TestCategory("Unit")]
[TestCategory(TestCategories.Web)]
public class DockLayoutTests
{
    private static readonly string[] _ids = ["panel", "monitor", "connections"];

    [TestMethod]
    public void Default_HoldsEveryPanelInOnePane()
    {
        var layout = DockLayout.Default(_ids);
        Assert.IsInstanceOfType<DockPane>(layout.Root);
        CollectionAssert.AreEqual(_ids, layout.PanelIds().ToList());
    }

    [TestMethod]
    public void Split_MovesThePanelToANewPaneBesideTheTarget()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "row", before: false);
        var split = (DockSplit)layout.Root;
        Assert.AreEqual("row", split.Direction);
        CollectionAssert.AreEqual(new[] { "panel", "connections" }, ((DockPane)split.Children[0]).Tabs.ToList());
        CollectionAssert.AreEqual(new[] { "monitor" }, ((DockPane)split.Children[1]).Tabs.ToList());
        Assert.AreEqual(1.0, split.Ratios.Sum(), 1e-9);
    }

    [TestMethod]
    public void Split_BadDirection_Throws() =>
        Assert.ThrowsExactly<global::System.ArgumentException>(() => DockLayout.Default(_ids).Split("monitor", 0, "diagonal", true));

    [TestMethod]
    public void Close_LastTabOfAPane_CollapsesTheSplit()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "row", false).Close("monitor");
        Assert.IsInstanceOfType<DockPane>(layout.Root);
        CollectionAssert.AreEqual(new[] { "panel", "connections" }, layout.PanelIds().ToList());
    }

    [TestMethod]
    public void Move_PanelToAnotherPane_RemovesItFromTheOldOneAndActivatesIt()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "row", false).Move("connections", 1);
        var split = (DockSplit)layout.Root;
        CollectionAssert.AreEqual(new[] { "panel" }, ((DockPane)split.Children[0]).Tabs.ToList());
        var right = (DockPane)split.Children[1];
        CollectionAssert.AreEqual(new[] { "monitor", "connections" }, right.Tabs.ToList());
        Assert.AreEqual("connections", right.Active);
    }

    [TestMethod]
    public void Resize_ClampsToTheMinimumAndNormalises()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "row", false).Resize(0, [0.0, 3.0]);
        var ratios = ((DockSplit)layout.Root).Ratios;
        Assert.AreEqual(1.0, ratios.Sum(), 1e-9);
        Assert.IsGreaterThan(0.0, ratios[0]);
        Assert.IsLessThan(ratios[1], ratios[0]);
    }

    [TestMethod]
    public void Json_RoundTrips()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "column", true);
        var again = DockLayout.Load(layout.ToJson(), _ids);
        Assert.AreEqual(layout.ToJson(), again.ToJson());
    }

    [TestMethod]
    public void Load_UnknownPanelsAndCorruptText_AreDroppedOrFallBack()
    {
        var saved = new DockLayout(1, new DockPane(["panel", "gone"], "gone")).ToJson();
        var loaded = DockLayout.Load(saved, _ids);
        CollectionAssert.AreEqual(new[] { "panel" }, loaded.PanelIds().ToList());
        Assert.AreEqual("panel", ((DockPane)loaded.Root).Active);
        CollectionAssert.AreEqual(_ids, DockLayout.Load("{not json", _ids).PanelIds().ToList());
        CollectionAssert.AreEqual(_ids, DockLayout.Load(new DockLayout(2, new DockPane(["panel"])).ToJson(), _ids).PanelIds().ToList());
        CollectionAssert.AreEqual(_ids, DockLayout.Load(null, _ids).PanelIds().ToList());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Activate_ChangesTheActiveTabOfItsPane()
    {
        var layout = DockLayout.Default(_ids).Activate("monitor");
        Assert.AreEqual("monitor", ((DockPane)layout.Root).Active);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void IndexOf_FindsPanesAndSplitsByReference()
    {
        var layout = DockLayout.Default(_ids).Split("monitor", 0, "row", before: false);
        var split = (DockSplit)layout.Root;
        Assert.AreEqual(0, layout.IndexOf(split));
        Assert.AreEqual(0, layout.IndexOf(split.Children[0]));
        Assert.AreEqual(1, layout.IndexOf(split.Children[1]));
        Assert.AreEqual(-1, layout.IndexOf(new DockPane(["x"])));
    }
}
