using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.EcoWitt.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class EcoWittLiveDataDecoderTests
{
    private static byte[] LivePacket(params byte[] data)
    {
        var size = 2 + 1 + data.Length + 1;
        var packet = new List<byte> { 0xFF, 0xFF, EcoWittLocalApi.LiveDataCommand, (byte)(size >> 8), (byte)size };
        packet.AddRange(data);
        packet.Add(EcoWittLocalApi.Checksum([.. packet.GetRange(2, packet.Count - 2)]));
        return [.. packet];
    }

    [TestMethod]
    public void BuildRequest_LiveData_IsTheDocumentedFiveBytes()
    {
        CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0x27, 0x03, 0x2A }, EcoWittLocalApi.BuildRequest(EcoWittLocalApi.LiveDataCommand));
    }

    [TestMethod]
    public void Decode_LivePacket_ReturnsMetricReadings()
    {
        var packet = LivePacket(
            0x01, 0x00, 0xE6,
            0x02, 0xFF, 0x9C,
            0x06, 0x30,
            0x07, 0x41,
            0x08, 0x27, 0x74,
            0x09, 0x27, 0x9A,
            0x0A, 0x01, 0x0E,
            0x0B, 0x00, 0x64,
            0x0C, 0x00, 0x9B,
            0x10, 0x00, 0x78,
            0x13, 0x00, 0x00, 0x04, 0xD2,
            0x15, 0x00, 0x00, 0x09, 0xC9,
            0x17, 0x03);

        var reading = new EcoWittLiveDataDecoder().Decode(packet);

        Assert.AreEqual(23.0, reading.IndoorTemperatureC);
        Assert.AreEqual(-10.0, reading.OutdoorTemperatureC);
        Assert.AreEqual(48, reading.IndoorHumidity);
        Assert.AreEqual(65, reading.OutdoorHumidity);
        Assert.AreEqual(1010.0, reading.AbsolutePressureHpa);
        Assert.AreEqual(1013.8, reading.RelativePressureHpa);
        Assert.AreEqual(270, reading.WindDirectionDegrees);
        Assert.AreEqual(10.0, reading.WindSpeedMs);
        Assert.AreEqual(15.5, reading.WindGustMs);
        Assert.AreEqual(12.0, reading.RainDayMm);
        Assert.AreEqual(123.4, reading.RainYearMm);
        Assert.AreEqual(250.5, reading.LightLux);
        Assert.AreEqual(3, reading.UvIndex);
        Assert.IsNull(reading.RainWeekMm);
        Assert.IsEmpty(reading.Problems);
    }

    [TestMethod]
    public void Decode_UnknownField_IsReportedAndEarlierValuesKept()
    {
        var packet = LivePacket(0x06, 0x30, 0x7E, 0x11, 0x22, 0x07, 0x41);

        var reading = new EcoWittLiveDataDecoder().Decode(packet);

        Assert.AreEqual(48, reading.IndoorHumidity);
        Assert.IsNull(reading.OutdoorHumidity);
        Assert.HasCount(1, reading.Problems);
        StringAssert.Contains(reading.Problems[0], "0x7E");
    }

    [TestMethod]
    public void Decode_FieldCutShort_IsReportedAndEarlierValuesKept()
    {
        var packet = LivePacket(0x06, 0x30, 0x01, 0x00);

        var reading = new EcoWittLiveDataDecoder().Decode(packet);

        Assert.AreEqual(48, reading.IndoorHumidity);
        Assert.IsNull(reading.IndoorTemperatureC);
        Assert.HasCount(1, reading.Problems);
    }

    [TestMethod]
    public void Decode_BadFraming_ThrowsInvalidData()
    {
        var good = LivePacket(0x06, 0x30);
        var badChecksum = (byte[])good.Clone();
        badChecksum[^1]++;
        var badHeader = (byte[])good.Clone();
        badHeader[0] = 0;
        var decoder = new EcoWittLiveDataDecoder();

        Assert.ThrowsExactly<InvalidDataException>(() => decoder.Decode(badChecksum));
        Assert.ThrowsExactly<InvalidDataException>(() => decoder.Decode(badHeader));
        Assert.ThrowsExactly<InvalidDataException>(() => decoder.Decode(good[..^2]));
        Assert.ThrowsExactly<InvalidDataException>(() => decoder.Decode([]));
    }

    [TestMethod]
    public void Decode_OtherCommand_ThrowsInvalidData()
    {
        byte[] packet = [0xFF, 0xFF, 0x26, 0x03, 0x29];

        Assert.ThrowsExactly<InvalidDataException>(() => new EcoWittLiveDataDecoder().Decode(packet));
    }
}
