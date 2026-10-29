using OoBDev.System.IO.Messages;
using System;
using System.Text;

namespace OoBDev.Scpi;

/// <summary>
/// Encodes a <see cref="ScpiRequest"/> as ASCII.
/// </summary>
public sealed class ScpiMessageEncoder : IMessageEncoder<ScpiRequest>
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref ScpiRequest request) => Encoding.ASCII.GetBytes(request.WireText);
}
