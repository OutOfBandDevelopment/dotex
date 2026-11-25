using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class WorkQueueOptionsTests
{
    [TestMethod]
    [DataRow(1, 30)]
    [DataRow(2, 60)]
    [DataRow(3, 120)]
    [DataRow(8, 3600)]
    [DataRow(30, 3600)]
    [DataRow(0, 30)]
    public void Backoff_DoublesPerAttempt_AndIsCapped(int attempt, int expectedSeconds)
    {
        var options = new WorkQueueOptions();

        Assert.AreEqual(TimeSpan.FromSeconds(expectedSeconds), options.Backoff(attempt));
    }

    [TestMethod]
    public void For_ReturnsTheOverrideIgnoringCase_OtherwiseTheDefault()
    {
        var options = new WorkQueueOptions();
        var slow = new StageSla { Lease = TimeSpan.FromHours(1) };
        options.Stages["ocr"] = slow;

        Assert.AreSame(slow, options.For("OCR"));
        Assert.AreSame(options.Default, options.For("hash"));
    }
}
