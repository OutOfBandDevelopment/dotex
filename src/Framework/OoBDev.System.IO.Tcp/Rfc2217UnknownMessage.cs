using System;

namespace OoBDev.System.IO.Tcp;

/// <summary>An unrecognized command code or a payload too short for its command — decoding never throws, this is the fallback.</summary>
internal sealed record Rfc2217UnknownMessage(byte CommandCode, ReadOnlyMemory<byte> Payload) : IRfc2217Message
{
    public bool IsFromServer => CommandCode >= Rfc2217Command.ServerOffset;
}
