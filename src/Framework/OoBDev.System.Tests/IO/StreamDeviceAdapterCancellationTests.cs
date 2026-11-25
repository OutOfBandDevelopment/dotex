using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

/// <summary>
/// A serial port base stream ignores the cancellation token while no bytes arrive (BUG-002 of BinaryDataDecoders).
/// These tests use a stream that behaves the same way and prove the adapter still closes in bounded time.
/// </summary>
[TestClass]
public class StreamDeviceAdapterCancellationTests
{
    private static readonly TimeSpan _bound = TimeSpan.FromSeconds(5);

    /// <summary>A stream whose read blocks and ignores its token until the stream is disposed, like <c>SerialStream</c>.</summary>
    private sealed class DeafStream : Stream
    {
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _released.Task.ConfigureAwait(false);
            throw new ObjectDisposedException(nameof(DeafStream));
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        protected override void Dispose(bool disposing)
        {
            _released.TrySetResult();
            base.Dispose(disposing);
        }
    }

    private sealed class DeafAdapter : StreamDeviceAdapter
    {
        public int Disconnects { get; private set; }
        public override string Type => "Deaf";
        public override string Path => "COMX";
        protected override Task<Stream?> ConnectAsync(CancellationToken cancellationToken) => Task.FromResult<Stream?>(new DeafStream());
        protected override void Disconnect() => Disconnects++;
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public async Task CloseAsync_WhileReadIgnoresCancellation_CompletesAndReleasesThePort()
    {
        var adapter = new DeafAdapter();
        await adapter.OpenAsync();
        await Task.Delay(100); // let the pump block inside the read

        var close = adapter.CloseAsync();
        var finished = await Task.WhenAny(close, Task.Delay(_bound));

        Assert.AreSame(close, finished, "CloseAsync did not complete: the pending read was not released.");
        await close;
        Assert.AreEqual(ConnectionState.Closed, adapter.State);
        Assert.AreEqual(1, adapter.Disconnects);
    }

    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public async Task CloseAsync_ThenOpenAsync_ReopensTheDevice()
    {
        var adapter = new DeafAdapter();
        await adapter.OpenAsync();
        await adapter.CloseAsync().WaitAsync(_bound);

        await adapter.OpenAsync();

        Assert.AreEqual(ConnectionState.Open, adapter.State);
        await adapter.DisposeAsync();
        Assert.AreEqual(2, adapter.Disconnects);
    }
}
