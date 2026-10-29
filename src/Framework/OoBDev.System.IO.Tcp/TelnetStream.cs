using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>Presents the data bytes of a Telnet connection: <c>IAC</c> bytes are doubled on write, commands are removed on read, and option negotiation is answered (only the COM-PORT option, 44, is accepted).</summary>
/// <param name="inner">The connection.</param>
public sealed class TelnetStream(Stream inner) : Stream
{
    internal const byte Iac = 255;
    internal const byte Dont = 254;
    internal const byte Do = 253;
    internal const byte Wont = 252;
    internal const byte Will = 251;
    internal const byte Sb = 250;
    internal const byte Se = 240;

    /// <summary>The RFC 2217 COM-PORT-OPTION.</summary>
    public const byte ComPortOption = 44;

    private enum State
    {
        Data,
        Iac,
        Option,
        SubOption,
        SubIac,
    }

    private readonly Stream _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private State _state;
    private byte _verb;

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void Flush() => _inner.Flush();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var raw = new byte[global::System.Math.Max(buffer.Length, 1)];
        while (true)
        {
            var read = await _inner.ReadAsync(raw, cancellationToken).ConfigureAwait(false);
            if (read == 0) return 0;

            var produced = 0;
            for (var i = 0; i < read; i++)
            {
                if (await Consume(raw[i], cancellationToken).ConfigureAwait(false) is { } data)
                {
                    buffer.Span[produced++] = data;
                }
            }

            if (produced > 0) return produced;
        }
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

    /// <inheritdoc />
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var escaped = new byte[buffer.Length * 2];
        var length = 0;
        foreach (var value in buffer.Span)
        {
            escaped[length++] = value;
            if (value == Iac) escaped[length++] = Iac;
        }

        await _inner.WriteAsync(escaped.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);

    /// <summary>Sends raw Telnet bytes (commands and subnegotiations) without escaping.</summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="cancellationToken">Cancels the write.</param>
    /// <returns>A task that completes when the bytes are written.</returns>
    public async ValueTask SendRawAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        await _inner.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await _inner.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing) _inner.Dispose();
        base.Dispose(disposing);
    }

    private async ValueTask<byte?> Consume(byte value, CancellationToken cancellationToken)
    {
        switch (_state)
        {
            case State.Data:
                if (value != Iac) return value;
                _state = State.Iac;
                return null;

            case State.Iac:
                switch (value)
                {
                    case Iac:
                        _state = State.Data;
                        return Iac;
                    case Sb:
                        _state = State.SubOption;
                        return null;
                    case Do or Dont or Will or Wont:
                        _verb = value;
                        _state = State.Option;
                        return null;
                    default:
                        _state = State.Data;
                        return null;
                }

            case State.Option:
                _state = State.Data;
                await Negotiate(_verb, value, cancellationToken).ConfigureAwait(false);
                return null;

            case State.SubOption:
                if (value == Iac) _state = State.SubIac;
                return null;

            default:
                _state = value == Se ? State.Data : State.SubOption;
                return null;
        }
    }

    private ValueTask Negotiate(byte verb, byte option, CancellationToken cancellationToken)
    {
        var accept = option == ComPortOption;
        byte? answer = verb switch
        {
            Do => accept ? Will : Wont,
            Will => accept ? Do : Dont,
            _ => null,
        };

        return answer is { } reply && !(accept && Answered(verb))
            ? SendRawAsync(new[] { Iac, reply, option }, cancellationToken)
            : ValueTask.CompletedTask;
    }

    // The client opens with WILL COM-PORT; the server's DO is the agreement, not a new request, so it needs no second WILL.
    private bool Answered(byte verb) => verb == Do && ComPortAnnounced;

    internal bool ComPortAnnounced { get; set; }
}
