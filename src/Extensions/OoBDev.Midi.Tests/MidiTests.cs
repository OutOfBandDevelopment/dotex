using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Midi.Tests;

[TestClass]
public sealed class MidiTests
{
    // Format 0, 96 ticks per quarter, one track: tempo 500000, program change, note on, running-status note off (velocity 0), SysEx, end of track.
    private static readonly byte[] _file =
    [
        0x4D, 0x54, 0x68, 0x64, 0, 0, 0, 6, 0, 0, 0, 1, 0, 96,
        0x4D, 0x54, 0x72, 0x6B, 0, 0, 0, 0x1C,
        0x00, 0xFF, 0x51, 0x03, 0x07, 0xA1, 0x20,
        0x00, 0xC0, 0x05,
        0x00, 0x90, 0x3C, 0x40,
        0x81, 0x40, 0x3C, 0x00,
        0x00, 0xF0, 0x03, 0x7E, 0x7F, 0xF7,
        0x00, 0xFF, 0x2F, 0x00,
    ];

    [TestMethod, TestCategory("Unit")]
    public void Parse_StandardFile_ReturnsHeaderAndEvents()
    {
        // Test
        var file = MidiFileReader.Parse(_file);

        // Assert
        Assert.AreEqual(0, file.Format);
        Assert.AreEqual(96, file.TicksPerQuarterNote);
        var events = file.Tracks.Single().Events;
        Assert.HasCount(6, events);
        Assert.AreEqual(500000, events[0].MicrosecondsPerQuarterNote);
        Assert.AreEqual(5, events[1].Data1);
        Assert.AreEqual(0, events[2].Channel);
        Assert.IsTrue(events[3].UsedRunningStatus);
        Assert.AreEqual(192, events[3].DeltaTicks);
        Assert.AreEqual(MidiEventKind.SysEx, events[4].Kind);
        Assert.AreEqual(0x2F, events[5].MetaType);
    }

    [TestMethod, TestCategory("Unit")]
    public void Write_ParsedFile_RoundTripsByteForByte()
    {
        var written = MidiFileWriter.Write(MidiFileReader.Parse(_file));

        CollectionAssert.AreEqual(_file, written);
    }

    [TestMethod, TestCategory("Unit")]
    public void Parse_UnknownChunk_IsSkipped()
    {
        byte[] unknown = [0x58, 0x58, 0x58, 0x58, 0, 0, 0, 2, 9, 9];
        var data = _file[..14].Concat(unknown).Concat(_file[14..]).ToArray();

        var file = MidiFileReader.Parse(data);

        Assert.HasCount(1, file.Tracks);
    }

    [TestMethod, TestCategory("Unit")]
    public void Parse_TruncatedTrack_Throws()
    {
        var data = _file[..^3];

        Assert.ThrowsExactly<InvalidDataException>(() => MidiFileReader.Parse(data));
    }

    [TestMethod, TestCategory("Unit")]
    public void Parse_RunningStatusWithoutStatus_Throws()
    {
        byte[] data = [.. _file[..14], 0x4D, 0x54, 0x72, 0x6B, 0, 0, 0, 3, 0x00, 0x3C, 0x40];

        Assert.ThrowsExactly<InvalidDataException>(() => MidiFileReader.Parse(data));
    }

    [TestMethod, TestCategory("Unit")]
    public void Parse_MoreEventsThanTheBound_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => MidiFileReader.Parse(_file, maxEventsPerTrack: 2));

    [TestMethod, TestCategory("Unit")]
    public void Parse_NotMidi_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => MidiFileReader.Parse([1, 2, 3, 4, 5, 6, 7, 8, 9]));

    [TestMethod, TestCategory("Unit")]
    public void Write_LargeDelta_UsesVariableLength()
    {
        var track = new MidiTrack([new MidiEvent(0x0FFFFFFF, MidiEventKind.Meta, 0xFF, [], 0x2F)]);

        var written = MidiFileWriter.Write(new MidiFile(0, 480, [track]));

        CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F, 0xFF, 0x2F, 0x00 }, written[22..]);
        Assert.AreEqual(0x0FFFFFFF, MidiFileReader.Parse(written).Tracks.Single().Events.Single().DeltaTicks);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_RunningStatus_YieldsEachMessage()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed([0x90, 0x3C, 0x40, 0x3E, 0x41, 0xC1, 0x07]);

        Assert.HasCount(3, messages);
        Assert.AreEqual(new MidiMessage(0x90, 0x3E, 0x41), messages[1]);
        Assert.AreEqual(1, messages[2].Channel);
        Assert.AreEqual(0x07, messages[2].Data1);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_RealTimeByteInsideMessage_DoesNotBreakIt()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed([0x90, 0x3C, 0xF8, 0x40]);

        Assert.HasCount(2, messages);
        Assert.AreEqual(0xF8, messages[0].Status);
        Assert.AreEqual(new MidiMessage(0x90, 0x3C, 0x40), messages[1]);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_ChunkedAcrossCalls_StillDecodes()
    {
        var decoder = new MidiStreamDecoder();

        var first = decoder.Feed([0xE0, 0x00]);
        var second = decoder.Feed([0x40]);

        Assert.IsEmpty(first);
        Assert.AreEqual(new MidiMessage(0xE0, 0x00, 0x40), second.Single());
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_SysEx_ReturnsPayload()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed([0xF0, 0x7E, 0x7F, 0xF7]);

        CollectionAssert.AreEqual(new byte[] { 0x7E, 0x7F }, messages.Single().SysEx);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_StrayDataByte_IsDropped()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed([0x10, 0x20, 0xF6]);

        Assert.AreEqual(0xF6, messages.Single().Status);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_SystemCommonClearsRunningStatus()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed([0x90, 0x3C, 0x40, 0xF6, 0x3C, 0x40]);

        Assert.HasCount(2, messages);
    }

    [TestMethod, TestCategory("Unit")]
    public void Feed_OversizedSysEx_IsAbandoned()
    {
        var decoder = new MidiStreamDecoder();

        var messages = decoder.Feed(new byte[] { 0xF0 }.Concat(Enumerable.Repeat((byte)1, MidiStreamDecoder.MaxSysExLength + 2)).Append((byte)0xF7));

        Assert.IsEmpty(messages);
    }

    [TestMethod, TestCategory("Unit")]
    public void Event_Equality_ComparesData()
    {
        var a = new MidiEvent(0, MidiEventKind.Channel, 0x90, [1, 2]);
        var b = new MidiEvent(0, MidiEventKind.Channel, 0x90, [1, 2]);

        Assert.AreEqual(a, b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.AreNotEqual(a, a with { Data = [1, 3] });
        Assert.IsFalse(a.Equals(null));
        Assert.AreEqual(-1, new MidiEvent(0, MidiEventKind.Meta, 0xFF, []).Channel);
        Assert.IsNull(new MidiEvent(0, MidiEventKind.Meta, 0xFF, [], 0x51).MicrosecondsPerQuarterNote);
        Assert.AreEqual(0, new MidiEvent(0, MidiEventKind.Channel, 0xC0, [3]).Data2);
        Assert.AreEqual(0xF8, new MidiMessage(0xF8, 0, 0).Type);
        Assert.AreEqual(9, new MidiMessage(0x93, 0, 0).Type);
        Assert.ThrowsExactly<ArgumentNullException>(() => MidiFileWriter.Write(null!));
    }
}
