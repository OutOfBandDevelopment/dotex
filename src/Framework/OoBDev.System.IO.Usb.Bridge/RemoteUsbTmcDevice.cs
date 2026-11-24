using System;
using System.Buffers.Binary;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>An <see cref="IUsbTmcDevice"/> whose calls are forwarded to a <see cref="UsbBridgeServer"/>.</summary>
public sealed class RemoteUsbTmcDevice : IUsbTmcDevice
{
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan? _requestTimeout;
    private RemoteConnection? _connection;
    private int _maxTransfer = 1024;
    private int _maxPacket = 64;

    /// <summary>Creates a closed device for a bridge at <paramref name="host"/>:<paramref name="port"/>.</summary>
    public RemoteUsbTmcDevice(string host, int port, TimeSpan? requestTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host;
        _port = port;
        _requestTimeout = requestTimeout;
    }

    /// <inheritdoc />
    public bool IsOpen => _connection is { IsConnected: true };

    /// <inheritdoc />
    public int MaxTransferSize => _maxTransfer;

    /// <inheritdoc />
    public int MaxPacketSize => _maxPacket;

    /// <inheritdoc />
    public void Open()
    {
        if (_connection is not null)
            return;
        var connection = new RemoteConnection(UsbBridgeKind.UsbTmc, _requestTimeout);
        try
        {
            connection.Connect(_host, _port);
            var open = connection.Call(UsbBridgeOp.Open, []);
            _maxTransfer = (int)BinaryPrimitives.ReadUInt32BigEndian(open.Payload.Span);
            _maxPacket = (int)BinaryPrimitives.ReadUInt32BigEndian(open.Payload.Span[4..]);
        }
        catch
        {
            connection.Dispose();
            throw;
        }

        _connection = connection;
    }

    /// <inheritdoc />
    public void Close()
    {
        var connection = _connection;
        _connection = null;
        if (connection is null)
            return;
        try
        {
            connection.Call(UsbBridgeOp.Close, []);
        }
        catch (Exception ex) when (ex is global::System.IO.IOException or TimeoutException)
        {
            // The peer is already gone; closing is still done.
        }

        connection.Dispose();
    }

    /// <inheritdoc />
    public void WriteBulkOut(byte[] transfer) => Connection().Call(UsbBridgeOp.WriteBulkOut, transfer);

    /// <inheritdoc />
    public int ReadBulkIn(byte[] buffer, out bool stalled)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        var size = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(size, (uint)buffer.Length);
        var reply = Connection().Call(UsbBridgeOp.ReadBulkIn, size);
        var span = reply.Payload.Span;
        stalled = span[0] != 0;
        var data = span[1..];
        data.CopyTo(buffer);
        return data.Length;
    }

    /// <inheritdoc />
    public void AbortBulkIn(byte bTag) => Connection().Call(UsbBridgeOp.AbortBulkIn, [bTag]);

    /// <inheritdoc />
    public void AbortBulkOut(byte bTag) => Connection().Call(UsbBridgeOp.AbortBulkOut, [bTag]);

    /// <inheritdoc />
    public void Clear() => Connection().Call(UsbBridgeOp.Clear, []);

    /// <inheritdoc />
    public void SetRemote(bool remote) => Connection().Call(UsbBridgeOp.SetRemote, [remote ? (byte)1 : (byte)0]);

    /// <inheritdoc />
    public void Dispose() => Close();

    private RemoteConnection Connection() => _connection ?? throw new InvalidOperationException("The device is not open.");
}
