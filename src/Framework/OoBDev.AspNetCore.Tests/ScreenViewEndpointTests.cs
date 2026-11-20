using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc;
using OoBDev.TestUtilities;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace OoBDev.AspNetCore.Tests;

[TestClass]
public class ScreenViewEndpointTests
{
    public required TestContext TestContext { get; set; }

    private static (List<(long value, string screen)> seen, MeterListener listener, Counter<long> counter) Setup()
    {
        var factory = new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>();
        var counter = factory.Create(ScreenViewEndpointRouteBuilderExtensions.MeterName).CreateCounter<long>("oobdev.ui.screen_views");
        var seen = new List<(long, string)>();
        var listener = new MeterListener();
        listener.InstrumentPublished = (i, l) => { if (i == counter) l.EnableMeasurementEvents(i); };
        listener.SetMeasurementEventCallback<long>((_, v, t, _) => seen.Add((v, (string)t.ToArray().Single(x => x.Key == "screen").Value!)));
        listener.Start();
        return (seen, listener, counter);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Record_KnownScreen_CountsAndReturnsNoContent()
    {
        var (seen, listener, counter) = Setup();
        using (listener)
        {
            var result = ScreenViewEndpointRouteBuilderExtensions.Record(new ScreenViewRequest("data.access"), new DefaultHttpContext(), new[] { "data.access" }.ToFrozenSet(), counter);

            TestContext.WriteLine($"{result.GetType().Name}, seen {seen.Count}");
            Assert.IsInstanceOfType<NoContent>(result);
            Assert.AreEqual((1L, "data.access"), seen.Single());
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Record_UnknownOrMissingScreen_IsRejectedAndNotCounted()
    {
        var (seen, listener, counter) = Setup();
        using (listener)
        {
            var known = new[] { "data.access" }.ToFrozenSet();
            var unknown = ScreenViewEndpointRouteBuilderExtensions.Record(new ScreenViewRequest("user-12345"), new DefaultHttpContext(), known, counter);
            var missing = ScreenViewEndpointRouteBuilderExtensions.Record(new ScreenViewRequest(null), new DefaultHttpContext(), known, counter);

            Assert.IsInstanceOfType<ProblemHttpResult>(unknown);
            Assert.IsInstanceOfType<ProblemHttpResult>(missing);
            Assert.IsEmpty(seen);
        }
    }
}
