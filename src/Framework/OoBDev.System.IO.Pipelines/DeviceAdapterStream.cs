using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines;

internal sealed class DeviceAdapterStream(IDeviceAdapter adapter) : Stream
{
    private readonly Stream _input = adapter.Input.AsStream();
    private readonly Stream _output = adapter.Output.AsStream();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _output.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => _output.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => _input.Read(buffer, offset, count);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _input.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => _output.Write(buffer, offset, count);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _output.WriteAsync(buffer, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing) adapter.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await adapter.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
