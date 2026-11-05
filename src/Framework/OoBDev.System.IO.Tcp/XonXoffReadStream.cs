using OoBDev.System.IO.Pipelines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// Read-only adapter that strips XON (0x11)/XOFF (0x13) bytes out of the inbound stream before they
/// reach <see cref="StreamToPipePump"/> - some serial-to-Ethernet bridges
/// forward the attached serial device's software flow-control bytes over the TCP stream verbatim
/// rather than honoring them locally, and left unstripped they'd corrupt whatever a text presenter
/// decodes. Updates <see cref="XonXoffFlowControlGate"/> as it goes so <see cref="TcpEndpointDeviceAdapter"/>
/// can pause/resume its own outbound writes on the same signal. Checks
/// <see cref="XonXoffFlowControlGate.Enabled"/> on every read rather than only at construction, since
/// <see cref="TcpEndpointDeviceAdapter.SoftwareFlowControl"/> can be toggled live on an already-open connection.
/// </summary>
internal sealed class XonXoffReadStream(Stream inner, XonXoffFlowControlGate gate) : Stream
{
    private const byte _xon = 0x11;
    private const byte _xoff = 0x13;

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var bytesRead = await inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                return 0;
            }

            if (!gate.Enabled)
            {
                return bytesRead;
            }

            var span = buffer.Span[..bytesRead];
            var kept = 0;
            for (var i = 0; i < span.Length; i++)
            {
                var b = span[i];
                switch (b)
                {
                    case _xoff:
                        gate.OnXoffReceived();
                        break;
                    case _xon:
                        gate.OnXonReceived();
                        break;
                    default:
                        span[kept++] = b;
                        break;
                }
            }

            if (kept > 0)
            {
                return kept;
            }

            // The whole read was XON/XOFF control bytes - loop for more rather than returning 0,
            // which StreamToPipePump treats as EOF.
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

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

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
