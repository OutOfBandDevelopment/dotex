using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

internal sealed class FailingStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new IOException("unplugged");
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new IOException("unplugged");
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

/// <summary>Blocks reads until cancelled or disposed, like an idle serial port.</summary>
internal sealed class BlockingStream : Stream
{
    private readonly TaskCompletionSource _disposed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _disposed.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        _disposed.TrySetResult();
        base.Dispose(disposing);
    }
}

/// <summary>Serves bytes fed by the test and records what is written.</summary>
internal sealed class RecordingStream : Stream
{
    private readonly MemoryStream _written = new();
    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();

    public byte[] Written => _written.ToArray();

    public void Feed(params byte[] data) => _incoming.Writer.TryWrite(data);

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            var chunk = await _incoming.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            chunk.CopyTo(buffer);
            return chunk.Length;
        }
        catch (ChannelClosedException)
        {
            return 0;
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => _written.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        _incoming.Writer.TryComplete();
        base.Dispose(disposing);
    }
}
