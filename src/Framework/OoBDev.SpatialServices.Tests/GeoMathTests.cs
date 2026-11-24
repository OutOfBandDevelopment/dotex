using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace OoBDev.SpatialServices.Tests;

/// <summary>Great-circle distance, bearing and destination.</summary>
[TestClass]
[TestCategory("Unit")]
public class GeoMathTests
{
    private static readonly GlobalPosition _london = new(51.5074, -0.1278);
    private static readonly GlobalPosition _paris = new(48.8566, 2.3522);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void DistanceMeters_LondonToParis_IsAbout344Km()
    {
        var meters = GeoMath.DistanceMeters(_london, _paris);
        TestContext.WriteLine($"London to Paris: {meters:F0} m");
        Assert.AreEqual(343_600, meters, 1_500);
    }

    [TestMethod]
    public void DistanceMeters_SamePoint_IsZero() =>
        Assert.AreEqual(0, GeoMath.DistanceMeters(_paris, _paris), 1e-6);

    [TestMethod]
    public void DistanceMeters_AntipodalPoints_IsHalfTheCircumference() =>
        Assert.AreEqual(Math.PI * GeoMath.EarthRadiusMeters, GeoMath.DistanceMeters(new(0, 0), new(0, 180)), 1);

    [TestMethod]
    public void BearingDegrees_DueEastOnTheEquator_Is90() =>
        Assert.AreEqual(90, GeoMath.BearingDegrees(new(0, 0), new(0, 10)), 1e-9);

    [TestMethod]
    public void BearingDegrees_DueWest_IsNormalisedTo270() =>
        Assert.AreEqual(270, GeoMath.BearingDegrees(new(0, 0), new(0, -10)), 1e-9);

    [TestMethod]
    public void BearingDegrees_LondonToParis_IsSouthEast() =>
        Assert.AreEqual(148.1, GeoMath.BearingDegrees(_london, _paris), 0.5);

    [TestMethod]
    public void Destination_ByDistanceAndBearing_ReachesTheTarget()
    {
        var meters = GeoMath.DistanceMeters(_london, _paris);
        var reached = GeoMath.Destination(_london, GeoMath.BearingDegrees(_london, _paris), meters);
        Assert.AreEqual(_paris.Latitude, reached.Latitude, 1e-6);
        Assert.AreEqual(_paris.Longitude, reached.Longitude, 1e-6);
    }

    [TestMethod]
    public void Destination_CrossingTheDateLine_WrapsLongitude()
    {
        var reached = GeoMath.Destination(new(0, 179), 90, 222_000);
        Assert.IsLessThan(0, reached.Longitude);
        Assert.IsGreaterThanOrEqualTo(-180, reached.Longitude);
    }

    [TestMethod]
    public void Methods_NullPosition_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => GeoMath.DistanceMeters(null!, _paris));
        Assert.ThrowsExactly<ArgumentNullException>(() => GeoMath.BearingDegrees(_paris, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => GeoMath.Destination(null!, 0, 1));
    }
}
