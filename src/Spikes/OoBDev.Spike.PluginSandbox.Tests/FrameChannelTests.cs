using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>The versioned frame header, CRC-32, HMAC, sequence counters and counter-signed replies, over in-memory streams.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class FrameChannelTests
{
    private const FrameFlags _all = FrameFlags.Crc32 | FrameFlags.Hmac | FrameFlags.CounterSign;
    private static readonly byte[] _key = RandomNumberGenerator.GetBytes(32);
    private const string _request = """{"n":1}""";
    private const string _topic = "echo";

    private static (FrameChannel Sender, FrameChannel Receiver, MemoryStream Wire) Pair(FrameFlags sender = _all, FrameFlags receiver = _all, byte[]? receiverKey = null)
    {
        var wire = new MemoryStream();
        var senderChannel = new FrameChannel(Stream.Null, wire, sender, _key);
        var receiverChannel = new FrameChannel(new RewindingStream(wire), Stream.Null, receiver, receiverKey ?? _key);
        return (senderChannel, receiverChannel, wire);
    }

    [TestMethod]
    public void RoundTrip_AllProtections_DeliversBodyAndSequence()
    {
        var (sender, receiver, _) = Pair();

        sender.Send(FrameType.Request, _topic, _request);
        var frame = receiver.Read();

        Assert.AreEqual(1UL, frame.Sequence);
        Assert.AreEqual(FrameType.Request, frame.Type);
        Assert.AreEqual(_topic, frame.Topic);
        Assert.AreEqual(_request, frame.Text);
        Assert.HasCount(32, frame.Mac);
    }

    [TestMethod]
    public void Header_IsPackedWithVersionFlagsTypeFormatLengthSequenceAndTopic()
    {
        var (sender, _, wire) = Pair();

        sender.Send(FrameType.Request, _topic, _request);
        var bytes = wire.ToArray();

        Assert.AreEqual(FrameChannel.Version, bytes[0]);
        Assert.AreEqual((byte)_all, bytes[1]);
        Assert.AreEqual((byte)FrameType.Request, bytes[2]);
        Assert.AreEqual((byte)PayloadFormat.Json, bytes[3]);
        Assert.AreEqual((uint)_request.Length, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4)));
        Assert.AreEqual(1UL, BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8)));
        Assert.AreEqual((ushort)_topic.Length, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(24)));
        Assert.AreEqual(FrameChannel.HeaderSize + _topic.Length + _request.Length + 4 + 32, bytes.Length);
    }

    [TestMethod]
    public void Read_WrongVersion_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        wire.GetBuffer()[0] = 2;

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_UnknownFlagBit_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        wire.GetBuffer()[1] |= 0x80;

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_ReservedBytesNotZero_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        wire.GetBuffer()[27] = 1;

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_SenderDropsRequiredProtection_IsAViolation()
    {
        var (sender, receiver, _) = Pair(sender: FrameFlags.Crc32);

        sender.Send(FrameType.Request, _topic, _request);

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_FlippedBodyBit_FailsTheCrc()
    {
        var (sender, receiver, wire) = Pair(_all & ~FrameFlags.Hmac & ~FrameFlags.CounterSign, FrameFlags.Crc32);
        sender.Send(FrameType.Request, _topic, _request);
        wire.GetBuffer()[FrameChannel.HeaderSize + _topic.Length + 3] ^= 1;

        var ex = Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());

        StringAssert.Contains(ex.Message, "CRC");
    }

    [TestMethod]
    public void Read_CrcRecomputedByAttacker_FailsTheHmac()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        var bytes = wire.GetBuffer();
        var bodyEnd = FrameChannel.HeaderSize + _topic.Length + _request.Length;
        bytes[FrameChannel.HeaderSize + _topic.Length + 3] ^= 1;
        var crc = new System.IO.Hashing.Crc32();
        crc.Append(bytes.AsSpan(0, bodyEnd));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(bodyEnd), crc.GetCurrentHashAsUInt32());

        var ex = Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());

        StringAssert.Contains(ex.Message, "HMAC");
    }

    [TestMethod]
    public void Read_WrongKey_FailsTheHmac()
    {
        var (sender, receiver, _) = Pair(receiverKey: RandomNumberGenerator.GetBytes(32));

        sender.Send(FrameType.Request, _topic, _request);

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_ReplayedFrame_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        var once = wire.ToArray();
        wire.Write(once);

        var first = receiver.Read();

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_DroppedFrame_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        var firstLength = wire.Length;
        sender.Send(FrameType.Request, _topic, _request);
        var all = wire.ToArray();
        wire.SetLength(0);
        wire.Write(all, (int)firstLength, all.Length - (int)firstLength);

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Reply_CounterSigned_VerifiesAgainstTheRequest()
    {
        var toPlugin = new MemoryStream();
        var toHost = new MemoryStream();
        var host = new FrameChannel(new RewindingStream(toHost), toPlugin, _all, _key);
        var plugin = new FrameChannel(new RewindingStream(toPlugin), toHost, _all, _key);

        host.Send(FrameType.Request, _topic, _request);
        var received = plugin.Read();
        plugin.Reply(received, FrameType.Response, """{"ok":true}"""u8);
        var response = host.Read();

        Assert.AreEqual(1UL, response.Ack);
    }

    [TestMethod]
    public void Reply_ToADifferentRequest_FailsTheHmac()
    {
        var toPlugin = new MemoryStream();
        var toHost = new MemoryStream();
        var host = new FrameChannel(new RewindingStream(toHost), toPlugin, _all, _key);
        var plugin = new FrameChannel(new RewindingStream(toPlugin), toHost, _all, _key);
        host.Send(FrameType.Request, _topic, _request);
        host.Send(FrameType.Request, _topic, _request);
        var first = plugin.Read();
        var second = plugin.Read();

        // The plugin answers request 1 but folds in request 2's HMAC: the host must refuse it.
        plugin.Reply(Reshape(first, second.Mac), FrameType.Response, """{"ok":true}"""u8);

        Assert.ThrowsExactly<FrameViolation>(() => host.Read());
    }

    [TestMethod]
    public void Read_AckForARequestNeverSent_IsAViolation()
    {
        var toPlugin = new MemoryStream();
        var toHost = new MemoryStream();
        var host = new FrameChannel(new RewindingStream(toHost), toPlugin, _all, _key);
        var plugin = new FrameChannel(new RewindingStream(toPlugin), toHost, _all, _key);

        plugin.Reply(Reshape(Fake(9), new byte[32]), FrameType.Response, """{"ok":true}"""u8);

        Assert.ThrowsExactly<FrameViolation>(() => host.Read());
    }

    [TestMethod]
    public void Read_UnknownFrameType_IsAViolation()
    {
        var (sender, receiver, wire) = Pair();
        sender.Send(FrameType.Request, _topic, _request);
        wire.GetBuffer()[2] = 99;

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void Read_JsonFormatWithInvalidJson_IsAViolation()
    {
        var (sender, receiver, _) = Pair();

        sender.Send(FrameType.Event, _topic, "not json"u8, PayloadFormat.Json);

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    [TestMethod]
    public void RoundTrip_BinaryPayload_IsDeliveredUntouched()
    {
        var (sender, receiver, _) = Pair();
        byte[] payload = [0, 255, 1, 254, 2];

        sender.Send(FrameType.Event, _topic, payload, PayloadFormat.Binary, ttlMs: 1500, hops: 2);
        var frame = receiver.Read();

        CollectionAssert.AreEqual(payload, frame.Payload);
        Assert.AreEqual(1500U, frame.TtlMs);
        Assert.AreEqual((byte)2, frame.Hops);
    }

    [TestMethod]
    public void Constructor_CounterSignWithoutHmac_IsRefused()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new FrameChannel(Stream.Null, Stream.Null, FrameFlags.CounterSign, null));
    }

    [TestMethod]
    public void Read_NoProtectionsRequired_StillChecksVersionAndLength()
    {
        var (sender, receiver, wire) = Pair(FrameFlags.None, FrameFlags.None);
        sender.Send(FrameType.Request, _topic, _request);
        BinaryPrimitives.WriteUInt32LittleEndian(wire.GetBuffer().AsSpan(4), FrameChannel.MaxPayload + 1);

        Assert.ThrowsExactly<FrameViolation>(() => receiver.Read());
    }

    private static ReceivedFrame Fake(ulong sequence) => new() { Type = FrameType.Request, Format = PayloadFormat.Json, Sequence = sequence, Ack = 0, Topic = _topic, Hops = 0, TtlMs = 0, Payload = [], Mac = new byte[32] };

    private static ReceivedFrame Reshape(ReceivedFrame request, byte[] mac) => new() { Type = request.Type, Format = request.Format, Sequence = request.Sequence, Ack = 0, Topic = request.Topic, Hops = 0, TtlMs = 0, Payload = request.Payload, Mac = mac };

    // Reads from the start of a stream that another channel keeps appending to.
    private sealed class RewindingStream(MemoryStream source) : Stream
    {
        private long _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => source.Length;

        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytes = source.GetBuffer();
            var available = (int)Math.Min(count, source.Length - _position);
            Array.Copy(bytes, _position, buffer, offset, available);
            _position += available;
            return available;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
