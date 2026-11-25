using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Imaging.People.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class CentroidClustererTests
{
    public required TestContext TestContext { get; set; }

    private readonly PeopleClusteringOptions _options = new();

    private static float[] V(params float[] v) => v;

    [TestMethod]
    public void Faces_NearCentroid_JoinsExistingCluster()
    {
        var result = CentroidClusterer.Faces(
            [new(1, V(1, 0), 7), new(2, V(0.95f, 0.1f), null)], _options);

        TestContext.WriteLine($"assignments={string.Join(";", result.Assignments)}");
        Assert.AreEqual(7, result.Assignments.Single().Target);
        Assert.AreEqual("threshold met", result.Assignments.Single().Why);
        Assert.AreEqual(0, result.NewClusterSpecies.Count);
    }

    [TestMethod]
    public void Faces_FarFromEveryCentroid_SeedsNewClusters()
    {
        var result = CentroidClusterer.Faces(
            [new(1, V(1, 0), 7), new(2, V(0, 1), null), new(3, V(0, 1), null)], _options);

        CollectionAssert.AreEqual(new[] { -1, -1 }, result.Assignments.Select(a => a.Target).ToArray());
        Assert.AreEqual(1, result.NewClusterSpecies.Count);
        StringAssert.Contains(result.Assignments[0].Why, "seeded");
    }

    [TestMethod]
    public void Faces_NoClustersAtAll_FirstSeedsSecondJoinsIt()
    {
        var result = CentroidClusterer.Faces([new(1, V(1, 0), null), new(2, V(1, 0.05f), null), new(3, V(0, 1), null)], _options);

        CollectionAssert.AreEqual(new[] { -1, -1, -2 }, result.Assignments.Select(a => a.Target).ToArray());
    }

    [TestMethod]
    public void Faces_AlreadyClusteredItems_AreNeverReassigned()
    {
        var result = CentroidClusterer.Faces([new(1, V(1, 0), 3), new(2, V(0, 1), 4)], _options);

        Assert.AreEqual(0, result.Assignments.Count);
    }

    [TestMethod]
    public void Faces_TieBetweenClusters_GoesToTheLowestId()
    {
        var result = CentroidClusterer.Faces([new(1, V(1, 0), 9), new(2, V(1, 0), 2), new(3, V(1, 0), null)], _options);

        Assert.AreEqual(2, result.Assignments.Single().Target);
    }

    [TestMethod]
    public void Pets_OtherSpecies_NeverMatches()
    {
        var result = CentroidClusterer.Pets([new(1, V(1, 0), 5, "dog"), new(2, V(1, 0), null, "cat")], new HashSet<int>(), _options);

        Assert.AreEqual(-1, result.Assignments.Single().Target);
        CollectionAssert.AreEqual(new[] { "cat" }, result.NewClusterSpecies.ToArray());
    }

    [TestMethod]
    public void Pets_SameSpeciesAboveThreshold_Joins()
    {
        var result = CentroidClusterer.Pets([new(1, V(1, 0), 5, "dog"), new(2, V(0.9f, 0.3f), null, "dog")], new HashSet<int>(), _options);

        Assert.AreEqual(5, result.Assignments.Single().Target);
    }

    [TestMethod]
    public void Pets_NamedCluster_NeedsTheHigherThreshold()
    {
        // cosine of (1,0) and (0.6,0.8) is 0.6: above 0.55, below 0.65
        var items = new List<ClusterItem> { new(1, V(1, 0), 5, "dog"), new(2, V(0.6f, 0.8f), null, "dog") };

        var plain = CentroidClusterer.Pets(items, new HashSet<int>(), _options);
        var named = CentroidClusterer.Pets(items, new HashSet<int> { 5 }, _options);

        TestContext.WriteLine($"plain={plain.Assignments[0].Target} named={named.Assignments[0].Target}");
        Assert.AreEqual(5, plain.Assignments[0].Target);
        Assert.AreEqual(-1, named.Assignments[0].Target);
    }

    [TestMethod]
    public void Pets_ConfirmedMember_PullsTheCentroid()
    {
        // centroid of an unconfirmed (1,0) and a confirmed (0,1) leans to the confirmed one
        var items = new List<ClusterItem>
        {
            new(1, V(1, 0), 5, "dog"),
            new(2, V(0, 1), 5, "dog", Confirmed: true),
            new(3, V(0, 1), null, "dog"),
        };

        var result = CentroidClusterer.Pets(items, new HashSet<int> { 5 }, _options);

        Assert.AreEqual(5, result.Assignments.Single().Target);
        Assert.IsTrue(result.Assignments.Single().Similarity > 0.9);
        Assert.AreEqual("named group, higher threshold met", result.Assignments.Single().Why);
    }

    [TestMethod]
    public void Pets_MissingSpecies_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => CentroidClusterer.Pets([new(1, V(1, 0), null)], new HashSet<int>(), _options));

    [TestMethod]
    public void Arguments_Null_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => CentroidClusterer.Faces(null!, _options));
        Assert.ThrowsExactly<ArgumentNullException>(() => CentroidClusterer.Pets([], null!, _options));
    }
}
