using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.ZWave;

/// <summary>
/// Writes a <see cref="ZWaveFrame"/> as the bytes that go on the wire.
/// </summary>
public sealed class ZWaveMessageEncoder : IMessageEncoder<ZWaveFrame>
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref ZWaveFrame request) => request.Raw;
}
