using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.ComplexEvents;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class ScheduleDemoTests
{
    public required TestContext TestContext { get; set; }

    private static (ScheduleDemo Demo, ScheduleRunner Runner, FakeTimeProvider Time) Create()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 20, 8, 0, 0, TimeSpan.Zero));
        var store = new InMemoryScheduleStore(time);
        var demo = new ScheduleDemo(store, time);
        return (demo, new ScheduleRunner(store, time, demo.RunAsync, TimeSpan.FromMinutes(1)), time);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Schedule_NotYetDue_IsPendingAndNotRun()
    {
        // Stage
        var (demo, runner, _) = Create();

        // Test
        await demo.ScheduleAsync("a", 30, "hello", CancellationToken.None);
        var ran = await runner.RunDueAsync(10, CancellationToken.None);
        var state = demo.State();
        TestContext.WriteLine($"ran {ran}, pending {state.Pending.Count}, fired {state.Fired.Count}");

        // Assert
        Assert.AreEqual(0, ran);
        Assert.AreEqual("a", state.Pending.Single().Key);
        Assert.AreEqual(0, state.Fired.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Run_AfterDue_MovesTheItemToFiredWithItsMessage()
    {
        // Stage
        var (demo, runner, time) = Create();
        await demo.ScheduleAsync("a", 30, "hello", CancellationToken.None);

        // Test
        time.Advance(TimeSpan.FromSeconds(31));
        var ran = await runner.RunDueAsync(10, CancellationToken.None);
        var state = demo.State();
        TestContext.WriteLine($"ran {ran}, fired at {state.Fired.SingleOrDefault()?.FiredAt}");

        // Assert
        Assert.AreEqual(1, ran);
        Assert.AreEqual(0, state.Pending.Count);
        Assert.AreEqual("hello", state.Fired.Single().Message);
        Assert.IsNotNull(state.Fired.Single().FiredAt);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Schedule_SameKeyAgain_MovesTheDueTime()
    {
        // Stage
        var (demo, runner, time) = Create();
        await demo.ScheduleAsync("a", 10, "first", CancellationToken.None);

        // Test
        await demo.ScheduleAsync("a", 100, "second", CancellationToken.None);
        time.Advance(TimeSpan.FromSeconds(11));
        var ran = await runner.RunDueAsync(10, CancellationToken.None);
        TestContext.WriteLine($"ran {ran} after the first due time, pending {demo.State().Pending.Count}");

        // Assert
        Assert.AreEqual(0, ran);
        Assert.AreEqual("second", demo.State().Pending.Single().Message);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Release_Pending_RemovesItAndItNeverFires()
    {
        // Stage
        var (demo, runner, time) = Create();
        await demo.ScheduleAsync("a", 5, "x", CancellationToken.None);

        // Test
        var released = await demo.ReleaseAsync("a", CancellationToken.None);
        var missing = await demo.ReleaseAsync("zzz", CancellationToken.None);
        time.Advance(TimeSpan.FromSeconds(10));
        var ran = await runner.RunDueAsync(10, CancellationToken.None);
        TestContext.WriteLine($"released {released}, missing {missing}, ran {ran}");

        // Assert
        Assert.IsTrue(released);
        Assert.IsFalse(missing);
        Assert.AreEqual(0, ran);
        Assert.AreEqual(0, demo.State().Pending.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Controller_BlankKeyOrBadDelay_ReturnsBadRequest()
    {
        // Stage
        var (demo, _, _) = Create();
        var controller = new ScheduleController(demo);

        // Test
        var blank = await controller.Schedule(new ScheduleRequestModel { Key = " ", InSeconds = 5 }, CancellationToken.None);
        var late = await controller.Schedule(new ScheduleRequestModel { Key = "k", InSeconds = 99999 }, CancellationToken.None);
        TestContext.WriteLine($"statuses {(blank.Result as ObjectResult)?.StatusCode}, {(late.Result as ObjectResult)?.StatusCode}");

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, (blank.Result as ObjectResult)?.StatusCode);
        Assert.AreEqual(StatusCodes.Status400BadRequest, (late.Result as ObjectResult)?.StatusCode);
        Assert.AreEqual(0, demo.State().Pending.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Controller_ReleaseUnknownKey_ReturnsNotFound()
    {
        // Stage
        var (demo, _, _) = Create();

        // Test
        var result = await new ScheduleController(demo).Release("nope", CancellationToken.None);
        TestContext.WriteLine($"result {result.Result?.GetType().Name}");

        // Assert
        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }
}
