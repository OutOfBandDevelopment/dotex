using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Devices.TestUtilities.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class RecordedTraceTests
{
    [TestMethod]
    public void Bytes_MixedSeparatorsAndPrefixes_ParsesEveryByte()
    {
        var bytes = RecordedTrace.Bytes("01 0x02,-FF\r\n a");

        CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0xFF, 0x0A }, bytes);
    }

    [TestMethod]
    [DataRow("GG")]
    [DataRow("123")]
    [DataRow("0x")]
    public void Bytes_NotAHexByte_Throws(string text) =>
        Assert.ThrowsExactly<FormatException>(() => RecordedTrace.Bytes(text));

    [TestMethod]
    public void Frames_OnePerLine_SkipsCommentsAndBlankLines()
    {
        var frames = RecordedTrace.Frames("# request from the bench\n01 02\n\n  03  \n");

        Assert.HasCount(2, frames);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, frames[0]);
        CollectionAssert.AreEqual(new byte[] { 3 }, frames[1]);
    }
}
