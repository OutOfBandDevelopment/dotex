using OoBDev.System.IO.Pipelines;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Write-only adapter over <see cref="ISerialPort.Write"/> so <see cref="SerialDeviceAdapter"/> can
/// wrap it in <see cref="WriteDelayStream"/> when write pacing is enabled.
/// <see cref="ISerialPort.BaseStream"/> itself can't be reused here - its <c>Write</c> throws
/// <see cref="NotSupportedException"/> deliberately, since that stream exists only for the read
/// pump (see <see cref="SerialPortReadStream"/>'s own doc comment on why reads can't go through
/// <see cref="System.IO.Ports.SerialPort.BaseStream"/> directly).
/// </summary>
internal sealed class SerialPortWriteStream(ISerialPort port) : Stream
{
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        Task.Run(() => port.Write(buffer, offset, count), cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var array = buffer.ToArray();
        return new ValueTask(Task.Run(() => port.Write(array, 0, array.Length), cancellationToken));
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => port.Write(buffer, offset, count);
}
