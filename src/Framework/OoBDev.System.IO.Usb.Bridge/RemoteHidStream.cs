using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>A read-only stream of HID input reports pushed by the bridge; it ends when the connection ends.</summary>
internal sealed class RemoteHidStream : Stream
{
    private readonly Channel<byte[]> _reports = Channel.CreateUnbounded<byte[]>();
    private byte[] _current = [];
    private int _offset;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public void Push(byte[] report) => _reports.Writer.TryWrite(report);

    public void Complete() => _reports.Writer.TryComplete();

    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_offset >= _current.Length)
        {
            try
            {
                _current = await _reports.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                _offset = 0;
            }
            catch (ChannelClosedException)
            {
                return 0;
            }
        }

        var count = global::System.Math.Min(buffer.Length, _current.Length - _offset);
        _current.AsMemory(_offset, count).CopyTo(buffer);
        _offset += count;
        return count;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Complete();
        base.Dispose(disposing);
    }
}
