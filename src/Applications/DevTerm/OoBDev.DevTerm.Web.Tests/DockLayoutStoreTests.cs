using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Web.Docking;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Web.Tests;

[TestClass]
public sealed class DockLayoutStoreTests
{
    private static readonly string[] _ids = ["a", "b"];

    [TestMethod]
    [TestCategory("Unit")]
    [TestCategory(TestCategories.Web)]
    public async Task SaveThenLoad_RoundTripsThroughStorage()
    {
        var js = new FakeJs();
        var store = new DockLayoutStore(js);
        await store.SaveAsync(DockLayout.Default(_ids).Activate("b"));
        var loaded = await store.LoadAsync(_ids);
        Assert.AreEqual("b", ((DockPane)loaded.Root).Active);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [TestCategory(TestCategories.Web)]
    public async Task Load_CorruptValue_GivesTheDefault()
    {
        var js = new FakeJs { Value = "{nope" };
        var loaded = await new DockLayoutStore(js).LoadAsync(_ids);
        CollectionAssert.AreEqual(_ids, loaded.PanelIds().ToList());
    }

    private sealed class FakeJs : IJSRuntime
    {
        public string? Value { get; set; }

        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);

        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "localStorage.setItem")
            {
                Value = (string?)args![1];
            }

            return new ValueTask<T>(identifier == "localStorage.getItem" ? (T)(object?)Value! : default!);
        }
    }
}
