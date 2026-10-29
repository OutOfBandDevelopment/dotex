using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Data.Vectors.Tests;

[TestClass]
public class AggregateTests
{
    private static readonly double[][] _rows = [[1, 8, 3], [4, 2, 9], [2, 5, 6]];

    private static CentroidAggregate Centroid(params double[][] rows) { var a = new CentroidAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVector(r)); return a; }
    private static MinimumAggregate Minimum(params double[][] rows) { var a = new MinimumAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVector(r)); return a; }
    private static MaximumAggregate Maximum(params double[][] rows) { var a = new MaximumAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVector(r)); return a; }
    private static CentroidFAggregate CentroidF(params double[][] rows) { var a = new CentroidFAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVectorF(Array.ConvertAll(r, v => (float)v))); return a; }
    private static MinimumFAggregate MinimumF(params double[][] rows) { var a = new MinimumFAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVectorF(Array.ConvertAll(r, v => (float)v))); return a; }
    private static MaximumFAggregate MaximumF(params double[][] rows) { var a = new MaximumFAggregate(); a.Init(); foreach (var r in rows) a.Accumulate(new SqlVectorF(Array.ConvertAll(r, v => (float)v))); return a; }

    private static void AreEqual(double[] expected, global::System.Collections.Generic.IReadOnlyList<double> actual)
    {
        CollectionAssert.AreEqual(expected, new global::System.Collections.Generic.List<double>(actual), "aggregate result");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Accumulate_ThreeVectors_ReturnsElementWiseResults()
    {
        AreEqual([1, 2, 3], Minimum(_rows).Terminate().Values);
        AreEqual([4, 8, 9], Maximum(_rows).Terminate().Values);
        AreEqual([7d / 3, 5, 6], Centroid(_rows).Terminate().Values);
        AreEqual([1, 2, 3], MinimumF(_rows).Terminate().Values);
        AreEqual([4, 8, 9], MaximumF(_rows).Terminate().Values);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_TwoPartialAggregates_EqualsOneAggregate()
    {
        // Stage: the engine runs a parallel plan as partial aggregates that are merged
        var left = _rows[..2];
        var right = _rows[2..];

        // Test
        var min = Minimum(left); min.Merge(Minimum(right));
        var max = Maximum(left); max.Merge(Maximum(right));
        var centroid = Centroid(left); centroid.Merge(Centroid(right));
        var minF = MinimumF(left); minF.Merge(MinimumF(right));
        var maxF = MaximumF(left); maxF.Merge(MaximumF(right));
        var centroidF = CentroidF(left); centroidF.Merge(CentroidF(right));

        // Assert
        AreEqual([1, 2, 3], min.Terminate().Values);
        AreEqual([4, 8, 9], max.Terminate().Values);
        AreEqual([.. Centroid(_rows).Terminate().Values], centroid.Terminate().Values);
        AreEqual([1, 2, 3], minF.Terminate().Values);
        AreEqual([4, 8, 9], maxF.Terminate().Values);
        Assert.AreEqual(3, centroidF.Terminate().Values.Count);
        Assert.AreEqual(5d, centroidF.Terminate().Values[1], 1e-6);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_EmptyPartial_KeepsTheResult()
    {
        var min = Minimum(_rows); min.Merge(Minimum());
        var maxF = MaximumF(_rows); maxF.Merge(MaximumF());
        var centroid = Centroid(_rows); centroid.Merge(Centroid());

        AreEqual([1, 2, 3], min.Terminate().Values);
        AreEqual([4, 8, 9], maxF.Terminate().Values);
        Assert.AreEqual(5d, centroid.Terminate().Values[1], 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Merge_DifferentLengths_Throws()
    {
        var min = Minimum([1, 2, 3]);
        Assert.ThrowsExactly<NotSupportedException>(() => min.Merge(Minimum([1, 2])));
    }
}
