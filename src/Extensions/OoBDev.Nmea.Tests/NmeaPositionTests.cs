using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Nmea.Tests;

[TestClass]
public class NmeaBearingTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0d, NmeaCompassPoint.North)]
    [DataRow(11.24d, NmeaCompassPoint.North)]
    [DataRow(11.25d, NmeaCompassPoint.NorthNorthEast)]
    [DataRow(22.5d, NmeaCompassPoint.NorthNorthEast)]
    [DataRow(45d, NmeaCompassPoint.NorthEast)]
    [DataRow(90d, NmeaCompassPoint.East)]
    [DataRow(180d, NmeaCompassPoint.South)]
    [DataRow(270d, NmeaCompassPoint.West)]
    [DataRow(337.5d, NmeaCompassPoint.NorthNorthWest)]
    [DataRow(348.74d, NmeaCompassPoint.NorthNorthWest)]
    [DataRow(348.75d, NmeaCompassPoint.North)]
    [DataRow(359.9d, NmeaCompassPoint.North)]
    public void Point_Degrees_ReturnsCompassPoint(double degrees, NmeaCompassPoint expected) =>
        Assert.AreEqual(expected, new NmeaBearing(degrees).Point);

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(360d, 0d)]
    [DataRow(450d, 90d)]
    [DataRow(-90d, 270d)]
    [DataRow(-720d, 0d)]
    public void Constructor_OutOfRange_Wraps(double degrees, double expected) =>
        Assert.AreEqual(expected, new NmeaBearing(degrees).Degrees);

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    public void Constructor_NotFinite_Throws(double degrees) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new NmeaBearing(degrees));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToString_Bearing_ShowsPointAndAngle() =>
        Assert.AreEqual("NNE (22.5000°)", new NmeaBearing(22.5).ToString());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_Text_ReadsBearing()
    {
        Assert.IsTrue(NmeaBearing.TryParse("084.4", out var bearing));
        Assert.AreEqual(84.4, bearing.Degrees);
        Assert.AreEqual("E", bearing.Abbreviation);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("north")]
    [DataRow("NaN")]
    public void TryParse_Invalid_ReturnsFalse(string? text) =>
        Assert.IsFalse(NmeaBearing.TryParse(text, out _));
}

[TestClass]
public class NmeaCoordinateTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_NorthLatitude_ReadsDecimalDegrees()
    {
        Assert.IsTrue(NmeaCoordinate.TryParse("4807.038", "N", true, out var c));
        Assert.AreEqual(48.1173, (double)c.DecimalDegrees);
        Assert.AreEqual('N', c.Hemisphere);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_WestLongitude_IsNegative()
    {
        Assert.IsTrue(NmeaCoordinate.TryParse("01131.000", "W", false, out var c));
        Assert.IsTrue(c.Degrees < 0);
        Assert.AreEqual('W', c.Hemisphere);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null, "N", true)]
    [DataRow("4807.038", null, true)]
    [DataRow("4807.038", "", true)]
    [DataRow("4807.038", "E", true)]
    [DataRow("01131.000", "N", false)]
    [DataRow("abc", "N", true)]
    public void TryParse_Invalid_ReturnsFalse(string? value, string? hemisphere, bool isLatitude) =>
        Assert.IsFalse(NmeaCoordinate.TryParse(value, hemisphere, isLatitude, out _));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToDegreesMinutesSeconds_Latitude_FormatsWithHemisphere()
    {
        NmeaCoordinate.TryParse("4807.038", "N", true, out var c);

        Assert.AreEqual("48°7'2.280\"N", c.ToDegreesMinutesSeconds());
        Assert.AreEqual("48d7m2.280sN", c.ToDegreesMinutesSeconds(ascii: true));
        Assert.AreEqual(c.ToDegreesMinutesSeconds(), c.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToDegreesMinutes_Longitude_FormatsWithHemisphere()
    {
        NmeaCoordinate.TryParse("01131.000", "E", false, out var c);

        Assert.AreEqual("11°31.000'E", c.ToDegreesMinutes());
        Assert.AreEqual("11d31.000mE", c.ToDegreesMinutes(ascii: true));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToDegreesMinutesSeconds_NearlyAMinute_CarriesInsteadOfShowingSixty()
    {
        var c = new NmeaCoordinate(10 + (59.9996 / 3600d), true);

        Assert.AreEqual("10°1'0.000\"N", c.ToDegreesMinutesSeconds());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToDegreesMinutes_NegativeSouth_ShowsMagnitudeAndHemisphere() =>
        Assert.AreEqual("33°52.000'S", new NmeaCoordinate(-(33 + (52d / 60)), true).ToDegreesMinutes());
}
