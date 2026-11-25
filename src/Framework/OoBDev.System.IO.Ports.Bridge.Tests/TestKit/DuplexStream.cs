namespace OoBDev.System.IO.Ports.Bridge.Tests.TestKit;

/// <summary>Combines a readable stream and a writable stream into one stream.</summary>
public sealed class DuplexStream : Stream
{
    private readonly Stream _readSide;
    private readonly Stream _writeSide;

    public DuplexStream(Stream readSide, Stream writeSide)
    {
        _readSide = readSide;
        _writeSide = writeSide;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _writeSide.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _readSide.Read(buffer, offset, count);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        _readSide.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => _writeSide.Write(buffer, offset, count);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
        _writeSide.WriteAsync(buffer, cancellationToken);
}
