using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.Iso9660;
using OoBDev.TestUtilities;
using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace OoBDev.System.Tests.IO.Iso9660;

[TestClass]
public class DateTimeTests
{
    private static byte[] Volume(string digits, sbyte offset)
    {
        var buffer = new byte[17];
        Encoding.ASCII.GetBytes(digits).CopyTo(buffer, 0);
        buffer[16] = (byte)offset;
        return buffer;
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void GetDateTime_ZeroOffset_ReturnsDigitsAsUtcInAnyTimeZone()
    {
        // Stage
        var buffer = Volume("2020060712304500", 0);
        var offset = 0;

        // Test
        var result = buffer.GetDateTime(ref offset, 17);

        // Assert
        Assert.AreEqual(new DateTime(2020, 6, 7, 12, 30, 45, DateTimeKind.Utc), result);
        Assert.AreEqual(17, offset);
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void GetDateTime_PositiveOffset_SubtractsOffsetToGetUtc()
    {
        // Stage: 12:00 local at GMT+1 (4 quarter hours) is 11:00 UTC.
        var buffer = Volume("2020060712000000", 4);
        var offset = 0;

        // Test
        var result = buffer.GetDateTime(ref offset, 17);

        // Assert
        Assert.AreEqual(new DateTime(2020, 6, 7, 11, 0, 0, DateTimeKind.Utc), result);
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void GetDateTime_ThaiBuddhistCulture_StillParsesGregorianDigits()
    {
        // Stage
        var saved = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
        try
        {
            var buffer = Volume("2020060712000000", 0);
            var offset = 0;

            // Test
            var result = buffer.GetDateTime(ref offset, 17);

            // Assert
            Assert.AreEqual(new DateTime(2020, 6, 7, 12, 0, 0, DateTimeKind.Utc), result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = saved;
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetDateTime_UnsetValue_ReturnsDefault()
    {
        // Stage
        var buffer = Volume("0000000000000000", 0);
        var offset = 0;

        // Test
        var result = buffer.GetDateTime(ref offset, 17);

        // Assert
        Assert.AreEqual(default, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void ReadDateTime_DayAndMonthDiffer_UsesTheDayByte()
    {
        // Stage: 2023-11-24 15:53:00, offset 0 (bytes as in the 7-byte directory record date).
        byte[] buffer = [123, 11, 24, 15, 53, 0, 0];
        var offset = 0;

        // Test
        var result = DirectoryRecord.ReadDateTime(buffer, ref offset);

        // Assert
        Assert.AreEqual(new DateTime(2023, 11, 24, 15, 53, 0, DateTimeKind.Utc), result);
        Assert.AreEqual(7, offset);
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void ReadDateTime_NegativeOffset_AddsOffsetToGetUtc()
    {
        // Stage: 12:00 local at GMT-5 (-20 quarter hours) is 17:00 UTC.
        byte[] buffer = [123, 11, 24, 12, 0, 0, unchecked((byte)(sbyte)-20)];
        var offset = 0;

        // Test
        var result = DirectoryRecord.ReadDateTime(buffer, ref offset);

        // Assert
        Assert.AreEqual(new DateTime(2023, 11, 24, 17, 0, 0, DateTimeKind.Utc), result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReadDateTime_ImpossibleFields_ReturnsMinValueInsteadOfThrowing()
    {
        // Stage
        byte[] buffer = [123, 13, 40, 99, 99, 99, 0];
        var offset = 0;

        // Test
        var result = DirectoryRecord.ReadDateTime(buffer, ref offset);

        // Assert
        Assert.AreEqual(DateTime.MinValue, result);
    }
}
