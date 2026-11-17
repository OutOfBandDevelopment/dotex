using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.System.Tests;

[TestClass]
public class BcdExTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsBCD_PackedSpan_ReturnsTheNumber()
    {
        TestContext.WriteLine("Stage: bytes 0x12 0x34 0x56");
        ReadOnlySpan<byte> input = [0x12, 0x34, 0x56];

        var value = input.AsBCD();

        TestContext.WriteLine($"Assert: {value}");
        Assert.AreEqual(123456L, value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsBCD_NonDigitNibble_Throws()
    {
        var bytes = new byte[] { 0x1A };
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((ReadOnlySpan<byte>)bytes).AsBCD());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToBCD_IntoSpan_PadsOnTheLeft()
    {
        Span<byte> destination = stackalloc byte[4];

        123456L.ToBCD(destination);

        TestContext.WriteLine(Convert.ToHexString(destination));
        Assert.AreEqual("00123456", Convert.ToHexString(destination));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToBCD_ValueTooLargeForDestination_Throws()
    {
        var destination = new byte[1];
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => 100L.ToBCD(destination));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToBCD_RoundTripsThroughAsBCD()
    {
        Span<byte> destination = stackalloc byte[9];

        987654321012345678L.ToBCD(destination);

        Assert.AreEqual(987654321012345678L, ((ReadOnlySpan<byte>)destination).AsBCD());
    }
}
