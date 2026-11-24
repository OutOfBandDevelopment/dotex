using System;
using System.IO;
using OoBDev.System.IO.UsbHids;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>An <see cref="IUsbHidDevice"/> whose reports and writes travel through a <see cref="UsbBridgeServer"/>.</summary>
public sealed class RemoteUsbHidDevice : IUsbHidDevice
{
    private readonly string _host;
    private readonly int _port;
    private readonly TimeSpan? _requestTimeout;
    private readonly RemoteHidStream _stream = new();
    private RemoteConnection? _connection;

    /// <summary>Creates a closed device for a bridge at <paramref name="host"/>:<paramref name="port"/>.</summary>
    public RemoteUsbHidDevice(string host, int port, TimeSpan? requestTimeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host;
        _port = port;
        _requestTimeout = requestTimeout;
    }

    /// <inheritdoc />
    public bool IsOpen => _connection is { IsConnected: true };

    /// <inheritdoc />
    public Stream BaseStream => _stream;

    /// <inheritdoc />
    public void Open()
    {
        if (_connection is not null)
            return;
        var connection = new RemoteConnection(UsbBridgeKind.Hid, _requestTimeout);
        connection.ReportReceived += _stream.Push;
        connection.Disconnected += _stream.Complete;
        try
        {
            connection.Connect(_host, _port);
            connection.Call(UsbBridgeOp.Open, []);
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
        catch (Exception ex) when (ex is IOException or TimeoutException)
        {
            // The peer is already gone; closing is still done.
        }

        connection.Dispose();
        _stream.Complete();
    }

    /// <inheritdoc />
    public void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        var connection = _connection ?? throw new InvalidOperationException("The device is not open.");
        connection.Call(UsbBridgeOp.HidWrite, buffer[offset..(offset + count)]);
    }

    /// <inheritdoc />
    public void Dispose() => Close();
}
