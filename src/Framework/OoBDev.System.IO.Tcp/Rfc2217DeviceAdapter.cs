using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.Tcp;

/// <summary>A serial port on a remote terminal server, reached with RFC 2217 (Telnet COM-PORT-OPTION): the line settings are sent after connecting and the stream carries the port's data bytes.</summary>
/// <param name="host">The terminal server host.</param>
/// <param name="port">The terminal server TCP port.</param>
/// <param name="baudRate">The line speed.</param>
/// <param name="dataBits">The data bits, 5 to 8.</param>
/// <param name="parity">The parity.</param>
/// <param name="stopBits">The stop bits.</param>
public sealed class Rfc2217DeviceAdapter(
    string host,
    int port,
    uint baudRate = 9600,
    byte dataBits = 8,
    ComPortParity parity = ComPortParity.None,
    ComPortStopBits stopBits = ComPortStopBits.One) : StreamDeviceAdapter
{
    private const byte SetBaudRate = 1;
    private const byte SetDataSize = 2;
    private const byte SetParity = 3;
    private const byte SetStopSize = 4;


    /// <inheritdoc />
    public override string Type => "Rfc2217";

    /// <inheritdoc />
    public override string Path => $"{host}:{port}";

    /// <summary>The bytes sent after connecting: the option announcement and the four line-setting subnegotiations.</summary>
    /// <returns>The bytes.</returns>
    public byte[] Handshake()
    {
        var bytes = new List<byte> { TelnetStream.Iac, TelnetStream.Will, TelnetStream.ComPortOption };
        var speed = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(speed, baudRate);
        AddSubNegotiation(bytes, SetBaudRate, speed);
        AddSubNegotiation(bytes, SetDataSize, [dataBits]);
        AddSubNegotiation(bytes, SetParity, [(byte)parity]);
        AddSubNegotiation(bytes, SetStopSize, [(byte)stopBits]);
        return [.. bytes];
    }

    /// <inheritdoc />
    protected override async Task<Stream?> ConnectAsync(CancellationToken cancellationToken)
    {
        var connection = await TcpDeviceAdapter.ConnectStreamAsync(host, port, TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
        if (connection is null) return null;
        var telnet = new TelnetStream(connection) { ComPortAnnounced = true };
        await telnet.SendRawAsync(Handshake(), cancellationToken).ConfigureAwait(false);
        return telnet;
    }

    private static void AddSubNegotiation(List<byte> bytes, byte command, byte[] value)
    {
        bytes.AddRange([TelnetStream.Iac, TelnetStream.Sb, TelnetStream.ComPortOption, command]);
        foreach (var b in value)
        {
            bytes.Add(b);
            if (b == TelnetStream.Iac) bytes.Add(TelnetStream.Iac);
        }

        bytes.AddRange([TelnetStream.Iac, TelnetStream.Se]);
    }
}
