using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.IO.Pipelines;
using Moq;

namespace OoBDev.DevTerm.Tests;

/// <summary><see cref="Session.AddObserver"/>: what a passive tap (the session logger) is told, and when.</summary>
[TestCategory("Unit")]
[TestCategory("Unit")]
[TestClass]
public sealed class SessionObserverTests
{
    private sealed class RecordingObserver : ISessionObserver
    {
        private readonly List<string> _events = [];

        public string[] Events
        {
            get
            {
                lock (_events)
                {
                    return [.. _events];
                }
            }
        }

        public void OnOpened() => Add("open");

        public void OnReceived(ReadOnlySequence<byte> data) => Add($"rx:{data.Length}");

        public void OnSent(ReadOnlyMemory<byte> data) => Add($"tx:{data.Length}");

        public void OnClosed(bool requested, Exception? error) => Add(requested ? "close" : $"fault:{error?.Message ?? "hangup"}");

        private void Add(string e)
        {
            lock (_events)
            {
                _events.Add(e);
            }
        }
    }

    private static (Mock<IDeviceAdapter> Transport, Pipe Pipe) CreateTransport()
    {
        var pipe = new Pipe();
        var transport = new Mock<IDeviceAdapter>();
        transport.SetupGet(t => t.Input).Returns(pipe.Reader);
        transport.SetupGet(t => t.Output).Returns(DeviceAdapterOutput.Create((_, _) => Task.CompletedTask));
        transport.SetupGet(t => t.State).Returns(ConnectionState.Open);
        return (transport, pipe);
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.IsTrue(condition(), "Timed out.");
    }

    [TestMethod]
    public async Task SeesOpenThenTrafficThenClose_WithoutTheRenderedOutputChanging()
    {
        var (transport, pipe) = CreateTransport();
        var presenter = new RawPresenter();
        await using var session = new Session(transport.Object, new Pipeline([presenter]));
        var observer = new RecordingObserver();
        using var registration = session.AddObserver(observer);
        var outputs = 0;
        session.Output += (_, _) => Interlocked.Increment(ref outputs);

        await session.OpenAsync(TestContext.CancellationToken);
        await session.SendAsync(new byte[] { 1, 2 }, TestContext.CancellationToken);
        await pipe.Writer.WriteAsync(new byte[] { 3, 4, 5 }, TestContext.CancellationToken);
        await WaitForAsync(() => Volatile.Read(ref outputs) == 1);
        await session.CloseAsync(TestContext.CancellationToken);

        CollectionAssert.AreEqual(new[] { "open", "tx:2", "rx:3", "close" }, observer.Events);
    }

    [TestMethod]
    public async Task AReadFault_IsReportedAsAnUnrequestedClose()
    {
        var (transport, pipe) = CreateTransport();
        await using var session = new Session(transport.Object, new Pipeline([]));
        var observer = new RecordingObserver();
        using var registration = session.AddObserver(observer);

        await session.OpenAsync(TestContext.CancellationToken);
        await pipe.Writer.CompleteAsync(new IOException("gone"));
        await WaitForAsync(() => observer.Events.Length == 2);

        CollectionAssert.AreEqual(new[] { "open", "fault:gone" }, observer.Events);
    }

    [TestMethod]
    public async Task DisposingTheRegistration_Detaches_AndClosingANeverOpenedSessionReportsNothing()
    {
        var (transport, _) = CreateTransport();
        await using var session = new Session(transport.Object, new Pipeline([]));
        var observer = new RecordingObserver();

        session.AddObserver(observer).Dispose();
        await session.OpenAsync(TestContext.CancellationToken);
        await session.CloseAsync(TestContext.CancellationToken);

        var second = new RecordingObserver();
        await using var unopened = new Session(transport.Object, new Pipeline([]));
        using var registration = unopened.AddObserver(second);
        await unopened.CloseAsync(TestContext.CancellationToken);

        Assert.IsEmpty(observer.Events);
        Assert.IsEmpty(second.Events);
    }

    public required TestContext TestContext { get; set; }
}
