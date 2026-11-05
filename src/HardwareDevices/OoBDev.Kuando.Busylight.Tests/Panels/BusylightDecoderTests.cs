using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Kuando.Busylight.Panels.Tests;

[TestClass]
[TestCategory(TestCategories.Hid)]
[TestCategory(TestCategories.Kuando_Busylight)]
public sealed class BusylightDecoderTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithEmptySequence_ProducesNoLines()
    {
        var decoder = new BusylightDecoder();

        var lines = decoder.Decode(ReadOnlySequence<byte>.Empty);

        Assert.IsEmpty(lines);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithAsciiIdentificationReply_ProducesOnePrefixedLine()
    {
        var decoder = new BusylightDecoder();
        var bytes = Encoding.ASCII.GetBytes("0001PLENOM0000010000000");

        var lines = decoder.Decode(new ReadOnlySequence<byte>(bytes));

        Assert.HasCount(1, lines);
        Assert.AreEqual("BUSYLIGHT: 0001PLENOM0000010000000", lines[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithNonPrintableBytes_ReplacesThemWithADot()
    {
        var decoder = new BusylightDecoder();
        byte[] bytes = [(byte)'O', (byte)'K', 0x00, 0x01, 0x7F];

        var lines = decoder.Decode(new ReadOnlySequence<byte>(bytes));

        Assert.AreEqual("BUSYLIGHT: OK...", lines[0]);
    }
}
