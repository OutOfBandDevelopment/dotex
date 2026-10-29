using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveSessionTests
{
    private sealed class FakeStreamDevice : IStreamDevice<ZWaveFrame>
    {
        public bool Accept { get; set; } = true;

        public Exception? Throw { get; set; }

        public List<ZWaveFrame> Sent { get; } = [];

        public Task Runner => Task.CompletedTask;

        public event EventHandler<ZWaveFrame>? MessageReceived;

        public event EventHandler<StreamDeviceStatus>? DeviceStatus
        {
            add { }
            remove { }
        }

        public event EventHandler<DeviceErrorEventArgs>? MessageReceivedError
        {
            add { }
            remove { }
        }

        public event EventHandler<DeviceErrorEventArgs>? MessageTransmitterError
        {
            add { }
            remove { }
        }

        public int Subscribers => MessageReceived?.GetInvocationList().Length ?? 0;

        public void Receive(ZWaveFrame frame) => MessageReceived?.Invoke(this, frame);

        public Task<bool> Transmit(ZWaveFrame message)
        {
            if (Throw is not null) throw Throw;
            Sent.Add(message);
            return Task.FromResult(Accept);
        }

        public void Dispose()
        {
        }
    }

    private static ZWaveFrame VersionReply() => new(ZWaveFrameKind.Data, ZWaveFrameTests.Bytes(ZWaveFrameTests.VersionReply));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Receive_DataFrame_QueuesAckAndRelaysIt()
    {
        // Stage
        var device = new FakeStreamDevice();
        using var session = new ZWaveSession(device);
        var seen = new List<ZWaveFrame>();
        session.FrameReceived += (_, f) => seen.Add(f);
        var frame = VersionReply();

        // Test
        device.Receive(frame);

        // Assert
        CollectionAssert.AreEqual(new[] { ZWaveFrame.Ack }, device.Sent);
        CollectionAssert.AreEqual(new[] { frame }, seen);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Receive_LoneAck_RelaysWithoutAnswering()
    {
        // Stage
        var device = new FakeStreamDevice();
        using var session = new ZWaveSession(device);
        var seen = 0;
        session.FrameReceived += (_, _) => seen++;

        // Test
        device.Receive(ZWaveFrame.Ack);

        // Assert
        Assert.AreEqual(0, device.Sent.Count);
        Assert.AreEqual(1, seen);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Receive_DeviceRefusesAnswer_RaisesAnswerFailed()
    {
        // Stage
        var device = new FakeStreamDevice { Accept = false };
        using var session = new ZWaveSession(device);
        Exception? failure = null;
        session.AnswerFailed += (_, e) => failure = e;

        // Test
        device.Receive(VersionReply());

        // Assert
        Assert.IsInstanceOfType<InvalidOperationException>(failure);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Receive_TransmitThrows_RaisesAnswerFailedAndStillRelays()
    {
        // Stage
        var device = new FakeStreamDevice { Throw = new InvalidOperationException("closed") };
        using var session = new ZWaveSession(device);
        Exception? failure = null;
        var seen = 0;
        session.AnswerFailed += (_, e) => failure = e;
        session.FrameReceived += (_, _) => seen++;

        // Test
        device.Receive(VersionReply());

        // Assert
        Assert.AreEqual("closed", failure?.Message);
        Assert.AreEqual(1, seen);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Request_ForwardsToDeviceAsync()
    {
        // Stage
        var device = new FakeStreamDevice();
        using var session = new ZWaveSession(device);
        var request = ZWaveRequests.GetVersion();

        // Test
        var accepted = await session.SendAsync(request).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(accepted);
        CollectionAssert.AreEqual(new[] { request }, device.Sent);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Dispose_Always_StopsListening()
    {
        // Stage
        var device = new FakeStreamDevice();
        var session = new ZWaveSession(device);

        // Test
        session.Dispose();
        device.Receive(VersionReply());

        // Assert
        Assert.AreEqual(0, device.Subscribers);
        Assert.AreEqual(0, device.Sent.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_NullDevice_Throws()
    {
        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new ZWaveSession(null!));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void SendAsync_NullRequest_Throws()
    {
        // Stage
        using var session = new ZWaveSession(new FakeStreamDevice());

        // Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => session.SendAsync(null!));
    }
}
