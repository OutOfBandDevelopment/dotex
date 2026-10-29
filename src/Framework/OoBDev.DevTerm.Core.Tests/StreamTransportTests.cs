using System.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Core.Tests;

/// <summary>The transport lifecycle over an adapter backed by in-memory streams.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class StreamTransportTests
{
    private sealed class FakeAdapter(Func<Stream?> open) : IDeviceAdapter
    {
        private Stream? _stream;
        public string Type => "fake";
        public string Path => "fake0";
        public Stream Stream => _stream ?? throw new InvalidOperationException();

        public bool TryOpen(out Stream? stream)
        {
            _stream = open();
            stream = _stream;
            return stream is not null;
        }
    }

    [TestMethod]
    public async Task Open_ThenFeed_DeliversBytesThroughInput()
    {
        var stream = new RecordingStream();
        await using var transport = new StreamTransport(new FakeAdapter(() => stream));

        await transport.OpenAsync();
        stream.Feed(9, 8, 7);

        var read = await transport.Input.ReadAsync();
        CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, read.Buffer.ToArray());
        Assert.AreEqual(ConnectionState.Open, transport.State);
    }

    [TestMethod]
    public async Task Write_SendsBytesToTheStream()
    {
        var stream = new RecordingStream();
        await using var transport = new StreamTransport(new FakeAdapter(() => stream));
        await transport.OpenAsync();

        await transport.WriteAsync(new byte[] { 1, 2 });

        CollectionAssert.AreEqual(new byte[] { 1, 2 }, stream.Written);
    }

    [TestMethod]
    public async Task StateChanged_ReportsEachTransition()
    {
        var changes = new List<(ConnectionState, ConnectionState)>();
        await using var transport = new StreamTransport(new FakeAdapter(() => new RecordingStream()));
        transport.StateChanged += (_, e) => changes.Add((e.Previous, e.Current));

        await transport.OpenAsync();
        await transport.CloseAsync();

        CollectionAssert.AreEqual(
            new[]
            {
                (ConnectionState.Closed, ConnectionState.Opening),
                (ConnectionState.Opening, ConnectionState.Open),
                (ConnectionState.Open, ConnectionState.Closing),
                (ConnectionState.Closing, ConnectionState.Closed),
            },
            changes);
    }

    [TestMethod]
    public async Task Close_ThenOpen_ReusesTheTransport()
    {
        var streams = new Queue<RecordingStream>([new RecordingStream(), new RecordingStream()]);
        await using var transport = new StreamTransport(new FakeAdapter(streams.Dequeue));

        await transport.OpenAsync();
        await transport.CloseAsync();
        await transport.OpenAsync();

        Assert.AreEqual(ConnectionState.Open, transport.State);
    }

    [TestMethod]
    public async Task Open_WhenTheAdapterFails_FaultsAndThrows()
    {
        await using var transport = new StreamTransport(new FakeAdapter(() => null));

        await Assert.ThrowsExactlyAsync<IOException>(() => transport.OpenAsync());

        Assert.AreEqual(ConnectionState.Faulted, transport.State);
    }

    [TestMethod]
    public async Task Write_BeforeOpen_Throws()
    {
        await using var transport = new StreamTransport(new FakeAdapter(() => new RecordingStream()));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transport.WriteAsync(new byte[] { 1 }));
    }

    [TestMethod]
    public async Task Close_WhenAlreadyClosed_DoesNothing()
    {
        await using var transport = new StreamTransport(new FakeAdapter(() => new RecordingStream()));

        await transport.CloseAsync();

        Assert.AreEqual(ConnectionState.Closed, transport.State);
    }

    [TestMethod]
    public void Constructor_NullAdapter_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new StreamTransport(null!));
}
