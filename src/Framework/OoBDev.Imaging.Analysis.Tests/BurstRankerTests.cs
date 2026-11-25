using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Imaging.Analysis.Bursts;
using OoBDev.TestUtilities;

namespace OoBDev.Imaging.Analysis.Tests;

[TestClass]
public class BurstRankerTests
{
    private static readonly DateTime T0 = new(2024, 1, 1, 12, 0, 0);

    private static QualityMeasures Good(double focus = 1000) => new(focus, 0.8, 0.02, 120, 1, 1, 200, 2);

    private static readonly IReadOnlyDictionary<string, SubjectFocus> NoSubjects = new Dictionary<string, SubjectFocus>();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Pairs_FiveFramesWithinTwoSeconds_LooksAtFourNeighbours()
    {
        var taken = Enumerable.Range(0, 5).Select(i => T0.AddMilliseconds(i * 400)).ToList();

        var pairs = BurstRanker.Pairs(taken, 60).ToList();

        Assert.AreEqual(4 + 3 + 2 + 1, pairs.Count);
        Assert.IsTrue(pairs.Contains((0, 4)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Pairs_GapOverWindow_BreaksTheChain()
    {
        var taken = new[] { T0, T0.AddSeconds(10), T0.AddSeconds(200), T0.AddSeconds(205) };

        var pairs = BurstRanker.Pairs(taken, 60).ToList();

        CollectionAssert.AreEquivalent(new[] { (0, 1), (2, 3) }, pairs);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Pairs_SixFramesSameSecond_LooksAtFourOnly()
    {
        var taken = Enumerable.Repeat(T0, 6).ToList();

        Assert.IsFalse(BurstRanker.Pairs(taken, 60).Contains((0, 5)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Components_TransitiveLinks_GiveOneGroupAndDropSingletons()
    {
        var groups = BurstRanker.Components(6, [(0, 1), (1, 2), (4, 5)]);

        Assert.AreEqual(2, groups.Count);
        CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groups.Single(g => g.Contains(0)).ToArray());
        CollectionAssert.AreEquivalent(new[] { 4, 5 }, groups.Single(g => g.Contains(4)).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_SharpestFirstWithNote()
    {
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good(1000), ["b"] = Good(500) };

        var scores = BurstRanker.Score(["a", "b"], q, NoSubjects);

        Assert.AreEqual("a", scores[0].Id);
        Assert.AreEqual("sharpest", scores[0].Notes);
        Assert.AreEqual("frame", scores[0].Basis);
        Assert.AreEqual(-1.0, scores[1].Score, 1e-9);
        Assert.AreEqual("focus -1.0", scores[1].Notes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_PenaltiesAreApplied()
    {
        var shaky = Good(1000) with { Isotropy = 0.1, Noise = 9 };
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good(1000), ["b"] = shaky with { FocusTile = 1000 } };

        var scores = BurstRanker.Score(["a", "b"], q, NoSubjects);

        Assert.AreEqual("a", scores[0].Id);
        Assert.AreEqual(-0.5, scores[1].Score, 1e-9);
        Assert.AreEqual("noisy -0.5", scores[1].Notes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_SoftShakyFrameLosesItsPenalty()
    {
        var q = new Dictionary<string, QualityMeasures>
        {
            ["a"] = Good(1000),
            ["b"] = Good(400) with { Isotropy = 0.1 },
        };

        var b = BurstRanker.Score(["a", "b"], q, NoSubjects).Single(s => s.Id == "b");

        Assert.AreEqual(Math.Round(Math.Log2(0.4) - 1.0, 3), b.Score, 1e-9);
        Assert.IsTrue(b.Notes.Contains("shaky -1", StringComparison.Ordinal));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_MissingMeasurement_ScoresMinusNine()
    {
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good() };

        var scores = BurstRanker.Score(["a", "b"], q, NoSubjects);

        Assert.AreEqual("no measurement", scores[1].Notes);
        Assert.AreEqual(-9.0, scores[1].Score);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_NoPositiveFocus_AllHaveNoMeasurement()
    {
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good(0), ["b"] = Good(0) };

        Assert.IsTrue(BurstRanker.Score(["a", "b"], q, NoSubjects).All(s => s.Score == -9.0));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_WorstFocusIsFlooredAtMinusFour()
    {
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good(100000), ["b"] = Good(1) };

        var b = BurstRanker.Score(["a", "b"], q, NoSubjects).Single(s => s.Id == "b");

        Assert.IsTrue(b.Score <= -4.0);
        Assert.IsTrue(b.Notes.StartsWith("focus -4.0", StringComparison.Ordinal));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_SubjectBasis_OnlyWhenEveryMemberHasSubjects()
    {
        var q = new Dictionary<string, QualityMeasures> { ["a"] = Good(1000), ["b"] = Good(1000) };
        var all = new Dictionary<string, SubjectFocus> { ["a"] = new(1, 2.0), ["b"] = new(2, 1.0) };
        var some = new Dictionary<string, SubjectFocus> { ["a"] = new(1, 2.0), ["b"] = new(0, null) };

        var bySubject = BurstRanker.Score(["a", "b"], q, all);
        var byFrame = BurstRanker.Score(["a", "b"], q, some);

        Assert.AreEqual("subject", bySubject[0].Basis);
        Assert.AreEqual(-1.0, bySubject[1].Score, 1e-9);
        Assert.AreEqual("frame", byFrame[0].Basis);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Score_SubjectWithoutFocusValue_HasNoMeasurement()
    {
        var q = new Dictionary<string, QualityMeasures>();
        var s = new Dictionary<string, SubjectFocus> { ["a"] = new(1, 2.0), ["b"] = new(1, null) };

        var scores = BurstRanker.Score(["a", "b"], q, s);

        Assert.AreEqual("no measurement", scores.Single(x => x.Id == "b").Notes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Arguments_Null_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => BurstRanker.Pairs(null!, 1).ToList());
        Assert.ThrowsExactly<ArgumentNullException>(() => BurstRanker.Components(1, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => BurstRanker.Score(null!, new Dictionary<string, QualityMeasures>(), NoSubjects));
    }
}
