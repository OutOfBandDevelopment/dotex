using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.System.IO.Messages;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Nmea.Tests;

[TestClass]
public class NmeaTests
{
    private const string Gga = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";
    private const string Rmc = "$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A";
    private const string Gsa = "$GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39";
    private const string Gsv = "$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74";
    private const string Vtg = "$GPVTG,054.7,T,034.4,M,005.5,N,010.2,K*48";

    private static NmeaSentence Decode(string line) =>
        new NmeaMessageDecoder().Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(line)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_Gga_SplitsTalkerTypeFieldsAndValidatesChecksum()
    {
        var s = Decode(Gga + "\r\n");
        Assert.AreEqual("GP", s.TalkerId);
        Assert.AreEqual("GGA", s.SentenceType);
        Assert.AreEqual(NmeaChecksum.Valid, s.Checksum);
        Assert.AreEqual(14, s.Fields.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WrongChecksum_IsMismatch() =>
        Assert.AreEqual(NmeaChecksum.Mismatch, Decode(Gga.Replace("*47", "*00", StringComparison.Ordinal)).Checksum);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_NoChecksum_IsAbsent() =>
        Assert.AreEqual(NmeaChecksum.Absent, Decode("$GPVTG,1,T").Checksum);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_NulPadding_IsDropped()
    {
        var bytes = Encoding.ASCII.GetBytes(Vtg + "\r\n").Concat(new byte[] { 0, 0, 0 }).ToArray();
        var s = new NmeaMessageDecoder().Decode(new ReadOnlySequence<byte>(bytes));
        Assert.AreEqual(NmeaChecksum.Valid, s.Checksum);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("")]
    [DataRow("GPGGA,1")]
    [DataRow("$GP")]
    [DataRow("$,1,2,3")]
    public void Decode_NotASentence_Throws(string line) =>
        Assert.ThrowsExactly<ArgumentException>(() => Decode(line));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Gga_TryParse_ReadsPositionAndFix()
    {
        Assert.IsTrue(NmeaGga.TryParse(Decode(Gga), out var gga));
        Assert.AreEqual(new TimeOnly(12, 35, 19), gga!.Value.Time);
        Assert.AreEqual(48.1173, gga.Value.Latitude!.Value, 1e-6);
        Assert.AreEqual(11.516667, gga.Value.Longitude!.Value, 1e-6);
        Assert.AreEqual(NmeaFixQuality.Gps, gga.Value.Quality);
        Assert.AreEqual(8, gga.Value.SatellitesUsed);
        Assert.AreEqual(0.9, gga.Value.Hdop);
        Assert.AreEqual(545.4, gga.Value.AltitudeMeters);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Gga_SouthWest_AreNegative()
    {
        Assert.IsTrue(NmeaGga.TryParse(Decode("$GPGGA,000000,3354.000,S,15112.000,W,0,00,,,"
            + ",,,,,,"), out var gga));
        Assert.IsTrue(gga!.Value.Latitude < 0);
        Assert.IsTrue(gga.Value.Longitude < 0);
        Assert.AreEqual(NmeaFixQuality.NoFix, gga.Value.Quality);
        Assert.IsNull(gga.Value.Hdop);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("0", NmeaFixQuality.NoFix)]
    [DataRow("2", NmeaFixQuality.Dgps)]
    [DataRow("3", NmeaFixQuality.Pps)]
    [DataRow("4", NmeaFixQuality.RtkFixed)]
    [DataRow("5", NmeaFixQuality.RtkFloat)]
    [DataRow("6", NmeaFixQuality.Estimated)]
    [DataRow("9", NmeaFixQuality.Unknown)]
    public void Gga_Quality_MapsCode(string code, NmeaFixQuality expected)
    {
        Assert.IsTrue(NmeaGga.TryParse(Decode($"$GPGGA,123519,4807.038,N,01131.000,E,{code},08,0.9,545.4,M"), out var gga));
        Assert.AreEqual(expected, gga!.Value.Quality);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rmc_TryParse_ReadsMinimumNavigationData()
    {
        Assert.IsTrue(NmeaRmc.TryParse(Decode(Rmc), out var rmc));
        Assert.IsTrue(rmc!.Value.Active);
        Assert.AreEqual(22.4, rmc.Value.SpeedKnots);
        Assert.AreEqual(84.4, rmc.Value.CourseTrue);
        Assert.AreEqual(new DateOnly(2094, 3, 23), rmc.Value.Date);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rmc_Void_IsNotActive()
    {
        Assert.IsTrue(NmeaRmc.TryParse(Decode("$GPRMC,123519,V,,,,,,,230326,,"), out var rmc));
        Assert.IsFalse(rmc!.Value.Active);
        Assert.IsNull(rmc.Value.Latitude);
        Assert.AreEqual(new DateOnly(2026, 3, 23), rmc.Value.Date);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Gsa_TryParse_CountsSatellitesUsed()
    {
        Assert.IsTrue(NmeaGsa.TryParse(Decode(Gsa), out var gsa));
        Assert.AreEqual(NmeaFixType.Fix3D, gsa!.Value.FixType);
        Assert.AreEqual(5, gsa.Value.SatellitesUsed);
        Assert.AreEqual(2.5, gsa.Value.Pdop);
        Assert.AreEqual(1.3, gsa.Value.Hdop);
        Assert.AreEqual(2.1, gsa.Value.Vdop);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Gsv_TryParse_ReadsHeader()
    {
        Assert.IsTrue(NmeaGsv.TryParse(Decode(Gsv), out var gsv));
        Assert.AreEqual(3, gsv!.Value.TotalMessages);
        Assert.AreEqual(1, gsv.Value.MessageNumber);
        Assert.AreEqual(11, gsv.Value.SatellitesInView);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Vtg_TryParse_ReadsCourseAndSpeed()
    {
        Assert.IsTrue(NmeaVtg.TryParse(Decode(Vtg), out var vtg));
        Assert.AreEqual(54.7, vtg!.Value.CourseTrue);
        Assert.AreEqual(5.5, vtg.Value.SpeedKnots);
        Assert.AreEqual(10.2, vtg.Value.SpeedKmh);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_WrongTypeOrTooFewFields_ReturnsFalse()
    {
        Assert.IsFalse(NmeaGga.TryParse(Decode(Vtg), out _));
        Assert.IsFalse(NmeaRmc.TryParse(Decode("$GPRMC,1,A"), out _));
        Assert.IsFalse(NmeaGsa.TryParse(Decode("$GPGSA,A,3"), out _));
        Assert.IsFalse(NmeaGsv.TryParse(Decode("$GPGSV,1"), out _));
        Assert.IsFalse(NmeaVtg.TryParse(Decode("$GPVTG,1"), out _));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_NullSentence_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => NmeaGga.TryParse(null!, out _));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Device_StreamWithJunkNulsAndSplitLines_YieldsEachSentence()
    {
        var pump = new SegmentPump<NmeaSentence>(new NmeaDevice());
        var bytes = Encoding.ASCII.GetBytes("junk\0\0" + Gga + "\r\n" + Vtg + "\r\n");
        await pump.PushAsync(bytes[..20]);
        await pump.PushAsync(bytes[20..]);

        CollectionAssert.AreEqual(new[] { "GGA", "VTG" }, pump.Messages.Select(m => m.SentenceType).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Device_UnterminatedGarbage_ResynchronisesOnNextDollar()
    {
        var pump = new SegmentPump<NmeaSentence>(new NmeaDevice());
        await pump.PushAsync(Encoding.ASCII.GetBytes("$" + new string('X', NmeaSegmenter.MaxLength + 10)));
        await pump.PushAsync(Encoding.ASCII.GetBytes(Vtg + "\r\n"));

        Assert.IsTrue(pump.Messages.Any(m => m.SentenceType == "VTG"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddNmea_RegistersDecoder()
    {
        using var provider = new ServiceCollection().TryAddNmea().BuildServiceProvider();
        Assert.IsInstanceOfType<NmeaMessageDecoder>(provider.GetRequiredService<IMessageDecoder<NmeaSentence>>());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Bt20_CarriesUsbHidIdentity()
    {
        var attribute = (OoBDev.System.IO.UsbHids.UsbHidAttribute)Attribute.GetCustomAttribute(typeof(NmeaEarthmateBt20Device), typeof(OoBDev.System.IO.UsbHids.UsbHidAttribute))!;
        Assert.IsNotNull(attribute);
    }
}
