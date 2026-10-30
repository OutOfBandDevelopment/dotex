using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.Velleman.K8055.Panels.Tests;

[TestClass]
public sealed class K8055DecoderTests
{
    // [00, 00, 03, AnalogIn1=0x4C(76), AnalogIn2=0x4C(76), CounterLo1=0x2A, CounterHi1=0x01, CounterLo2=0x00, CounterHi2=0x00]
    // -> counter1 = 0x012A = 298, counter2 = 0.
    private static readonly byte[] _sampleFrame = [0x00, 0x00, 0x03, 0x4C, 0x4C, 0x2A, 0x01, 0x00, 0x00];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithConfirmedFrameShape_ProducesOneReadableLine()
    {
        var decoder = new K8055Decoder();

        var lines = decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame));

        Assert.HasCount(1, lines);
        Assert.Contains("A1=76", lines[0]);
        Assert.Contains("A2=76", lines[0]);
        Assert.Contains("C1=298", lines[0]);
        Assert.Contains("C2=0", lines[0]);
        Assert.Contains("D=0x00", lines[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_RaisesValuesChangedWithExpectedDictionary()
    {
        var decoder = new K8055Decoder();
        IReadOnlyDictionary<string, string>? received = null;
        decoder.ValuesChanged += (_, values) => received = values;

        decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame));

        Assert.IsNotNull(received);
        Assert.AreEqual("0x00", received["digitalInRaw"]);
        Assert.AreEqual("76", received["analogIn1"]);
        Assert.AreEqual("76", received["analogIn2"]);
        Assert.AreEqual("298", received["counter1"]);
        Assert.AreEqual("0", received["counter2"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithNonZeroDigitalInByte_ReportsItRawInHex()
    {
        var decoder = new K8055Decoder();
        var frame = (byte[])_sampleFrame.Clone();
        frame[1] = 0x05;

        var lines = decoder.Decode(new ReadOnlySequence<byte>(frame));

        Assert.Contains("D=0x05", lines[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithAPartialFrameAcrossTwoCalls_EmitsNothingUntilComplete()
    {
        var decoder = new K8055Decoder();

        var firstCallLines = decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame.AsMemory(0, 5)));
        Assert.IsEmpty(firstCallLines);

        var secondCallLines = decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame.AsMemory(5)));
        Assert.HasCount(1, secondCallLines);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithTwoFramesInOneCall_EmitsTwoLines()
    {
        var decoder = new K8055Decoder();
        var twoFrames = new byte[_sampleFrame.Length * 2];
        _sampleFrame.CopyTo(twoFrames, 0);
        _sampleFrame.CopyTo(twoFrames, _sampleFrame.Length);

        var lines = decoder.Decode(new ReadOnlySequence<byte>(twoFrames));

        Assert.HasCount(2, lines);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithARepeatedIdenticalFrame_DoesNotRaiseValuesChangedAgain()
    {
        var decoder = new K8055Decoder();
        var raiseCount = 0;
        decoder.ValuesChanged += (_, _) => raiseCount++;

        decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame));
        decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame));

        Assert.AreEqual(1, raiseCount);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WithOneChangedField_RaisesValuesChangedWithOnlyThatField()
    {
        var decoder = new K8055Decoder();
        var receivedDictionaries = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => receivedDictionaries.Add(values);

        decoder.Decode(new ReadOnlySequence<byte>(_sampleFrame));
        var changedFrame = (byte[])_sampleFrame.Clone();
        changedFrame[3] = 0x4D; // AnalogIn1: 76 -> 77, everything else identical.
        decoder.Decode(new ReadOnlySequence<byte>(changedFrame));

        Assert.HasCount(2, receivedDictionaries);
        var secondUpdate = receivedDictionaries[1];
        Assert.HasCount(1, secondUpdate);
        Assert.AreEqual("77", secondUpdate["analogIn1"]);
    }
}
