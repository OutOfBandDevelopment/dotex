using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.EcoWitt.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class EcoWittPushDecoderTests
{
    private const string _push =
        "PASSKEY=ABCDEF0123456789&stationtype=GW1000_V1.7.3&dateutc=2026-11-12+10:30:00&tempinf=71.6&humidityin=48" +
        "&baromrelin=29.92&baromabsin=29.50&tempf=50.0&humidity=65&winddir=270&windspeedmph=10.0&windgustmph=15.5" +
        "&rainratein=0.000&dailyrainin=0.120&solarradiation=250.5&uv=3&model=GW1100B&wh65batt=0";

    [TestMethod]
    public void Decode_CapturedPush_ReturnsTypedReadings()
    {
        // Test
        var reading = new EcoWittPushDecoder().Decode(_push);

        // Assert
        Assert.AreEqual("GW1000_V1.7.3", reading.StationType);
        Assert.AreEqual("GW1100B", reading.Model);
        Assert.AreEqual(new DateTimeOffset(2026, 11, 12, 10, 30, 0, TimeSpan.Zero), reading.Timestamp);
        Assert.AreEqual(71.6, reading.IndoorTemperatureF);
        Assert.AreEqual(48, reading.IndoorHumidity);
        Assert.AreEqual(50.0, reading.OutdoorTemperatureF);
        Assert.AreEqual(65, reading.OutdoorHumidity);
        Assert.AreEqual(29.92, reading.RelativePressureInHg);
        Assert.AreEqual(270, reading.WindDirectionDegrees);
        Assert.AreEqual(15.5, reading.WindGustMph);
        Assert.AreEqual(0.12, reading.DailyRainIn);
        Assert.AreEqual(250.5, reading.SolarRadiationWm2);
        Assert.AreEqual(3, reading.UvIndex);
        Assert.IsNull(reading.WeeklyRainIn);
        Assert.IsEmpty(reading.Problems);
    }

    [TestMethod]
    public void Decode_UnmappedField_IsKeptAndPasskeyIsDropped()
    {
        var reading = new EcoWittPushDecoder().Decode(_push);

        Assert.AreEqual("0", reading.Unrecognised["wh65batt"]);
        Assert.IsFalse(reading.Unrecognised.ContainsKey("PASSKEY"));
    }

    [TestMethod]
    public void Decode_BadNumberAndBrokenPair_AreReportedAndTheRestDecodes()
    {
        var reading = new EcoWittPushDecoder().Decode("tempf=abc&humidity=40&tempinf=7x&oops&dateutc=yesterday");

        Assert.AreEqual(40, reading.OutdoorHumidity);
        Assert.IsNull(reading.OutdoorTemperatureF);
        Assert.HasCount(4, reading.Problems);
    }

    [TestMethod]
    public void Decode_DateNow_LeavesTheTimestampEmpty()
    {
        var reading = new EcoWittPushDecoder().Decode("dateutc=now&tempf=32");

        Assert.IsNull(reading.Timestamp);
        Assert.IsEmpty(reading.Problems);
    }

    [TestMethod]
    public void Decode_PercentEncodedValue_IsUnescaped()
    {
        var reading = new EcoWittPushDecoder().Decode("stationtype=GW%201000&x=a%2Bb");

        Assert.AreEqual("GW 1000", reading.StationType);
        Assert.AreEqual("a+b", reading.Unrecognised["x"]);
    }

    [TestMethod]
    public void MetricHelpers_ConvertFromTheSentUnits()
    {
        var reading = new EcoWittPushDecoder().Decode("tempf=212&windspeedmph=10&dailyrainin=1&baromrelin=30");

        Assert.AreEqual(100.0, reading.OutdoorTemperatureC!.Value, 1e-9);
        Assert.AreEqual(16.09344, reading.WindSpeedKph!.Value, 1e-9);
        Assert.AreEqual(25.4, reading.DailyRainMm!.Value, 1e-9);
        Assert.AreEqual(1015.917, reading.RelativePressureHpa!.Value, 1e-3);
        Assert.IsNull(reading.IndoorTemperatureC);
    }

    [TestMethod]
    public void Decode_Null_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new EcoWittPushDecoder().Decode(null!));

    [TestMethod]
    public void TryAddEcoWitt_RegistersTheDecoderOnce()
    {
        var services = new ServiceCollection().TryAddEcoWitt().TryAddEcoWitt();

        using var provider = services.BuildServiceProvider();

        Assert.AreSame(provider.GetRequiredService<EcoWittPushDecoder>(), provider.GetRequiredService<EcoWittPushDecoder>());
    }
}
