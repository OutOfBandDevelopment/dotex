using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Imaging.People.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ClusterSchedulePolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
    private readonly PeopleClusteringOptions _options = new();

    [TestMethod]
    public void ShouldRun_NeverRanAndEnoughItems_IsDue()
        => Assert.IsTrue(ClusterSchedulePolicy.ShouldRun(Now, null, 25, _options));

    [TestMethod]
    public void ShouldRun_TooFewItems_IsNotDue()
        => Assert.IsFalse(ClusterSchedulePolicy.ShouldRun(Now, null, 24, _options));

    [TestMethod]
    public void ShouldRun_RanRecently_IsNotDue()
        => Assert.IsFalse(ClusterSchedulePolicy.ShouldRun(Now, Now.AddMinutes(-14), 100, _options));

    [TestMethod]
    public void ShouldRun_IntervalElapsed_IsDue()
        => Assert.IsTrue(ClusterSchedulePolicy.ShouldRun(Now, Now.AddMinutes(-15), 100, _options));

    [TestMethod]
    public void ShouldRun_NullOptions_Throws()
        => Assert.ThrowsExactly<ArgumentNullException>(() => ClusterSchedulePolicy.ShouldRun(Now, null, 1, null!));
}
