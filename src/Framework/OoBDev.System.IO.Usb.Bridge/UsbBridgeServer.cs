using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>
/// Shares one USB device on one TCP port. A client connects, says Hello, opens the device and then sends requests
/// one at a time; the frames are described in <c>docs/design/Ser2NetWin/usb-bridge.md</c>.
/// </summary>
public sealed class UsbBridgeServer : IDisposable
{
    private readonly UsbBridgeOptions _options;
    private readonly IUsbTmcDeviceFactory? _tmcFactory;
    private readonly IUsbHidDeviceFactory? _hidFactory;
    private readonly ILogger _logger;
    private TcpListener? _listener;
    private Session? _current;

    /// <summary>Creates the server; call <see cref="Start"/> and then <see cref="RunAsync"/>.</summary>
    public UsbBridgeServer(UsbBridgeOptions options, IUsbTmcDeviceFactory? tmcFactory, IUsbHidDeviceFactory? hidFactory, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _options = options;
        _tmcFactory = tmcFactory;
        _hidFactory = hidFactory;
        _logger = logger;
    }

    /// <summary>The port the listener is bound to (useful when <see cref="UsbBridgeOptions.TcpPort"/> is 0).</summary>
    public int BoundPort => ((IPEndPoint)(_listener ?? throw new InvalidOperationException("Not started.")).LocalEndpoint).Port;

    /// <summary>True while a client is connected.</summary>
    public bool IsClientConnected => _current is { IsDone: false };

    /// <summary>Binds the listener.</summary>
    public void Start()
    {
        var errors = _options.TcpPort == 0 ? [] : _options.Validate();
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(" ", errors));
        _listener = new TcpListener(IPAddress.Parse(_options.BindAddress), _options.TcpPort);
        _listener.Start();
        _logger.Listening(_options.BindAddress, BoundPort, _options.DisplayName);
    }

    /// <summary>Accepts clients until cancelled.</summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (_listener is null)
            Start();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener!.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                var name = client.Client.RemoteEndPoint?.ToString() ?? "client";
                if (_current is { IsDone: false } running)
                {
                    if (!_options.ReplaceExistingClient)
                    {
                        _logger.ClientRefused(name);
                        client.Dispose();
                        continue;
                    }

                    await running.StopAsync().ConfigureAwait(false);
                }

                _current = new Session(this, client, name, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown.
        }
        finally
        {
            if (_current is not null)
                await _current.StopAsync().ConfigureAwait(false);
            _listener?.Stop();
        }
    }

    /// <inheritdoc />
    public void Dispose() => _listener?.Stop();

    private sealed class Session : IDisposable
    {
        private readonly UsbBridgeServer _server;
        private readonly TcpClient _client;
        private readonly string _name;
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly Task _task;
        private IUsbTmcDevice? _tmc;
        private IUsbHidDevice? _hid;

        public Session(UsbBridgeServer server, TcpClient client, string name, CancellationToken shutdown)
        {
            _server = server;
            _client = client;
            _name = name;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(shutdown);
            client.NoDelay = true;
            _task = Task.Run(RunAsync, CancellationToken.None);
        }

        public bool IsDone => _task.IsCompleted;

        public void Dispose()
        {
            _cts.Dispose();
            _writeLock.Dispose();
        }

        public async Task StopAsync()
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _client.Dispose();
            try
            {
                await _task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // The session logs its own failure.
            }
        }

        private async Task RunAsync()
        {
            _server._logger.ClientConnected(_name);
            try
            {
                var stream = _client.GetStream();
                while (!_cts.IsCancellationRequested)
                {
                    var request = await UsbFrameCodec.ReadAsync(stream, _cts.Token).ConfigureAwait(false);
                    if (request is null)
                        break;
                    await HandleAsync(stream, request).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidDataException or OperationCanceledException)
            {
                if (!_cts.IsCancellationRequested)
                    _server._logger.SessionFailed(ex.Message);
            }
            finally
            {
                CloseDevice();
                _client.Dispose();
                _server._logger.ClientDisconnected(_name);
            }
        }

        private async Task HandleAsync(Stream stream, UsbFrame request)
        {
            UsbFrame reply;
            try
            {
                reply = Dispatch(stream, request);
            }
            catch (TimeoutException ex)
            {
                reply = Error(request, UsbBridgeErrorCode.Timeout, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                reply = Error(request, UsbBridgeErrorCode.BadRequest, ex.Message);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                reply = Error(request, UsbBridgeErrorCode.DeviceGone, ex.Message);
            }

            await SendAsync(stream, reply).ConfigureAwait(false);
        }

        private UsbFrame Dispatch(Stream stream, UsbFrame request)
        {
            var payload = request.Payload.Span;
            switch (request.Op)
            {
                case UsbBridgeOp.Hello:
                    if (payload.Length < 2 || payload[0] != UsbFrameCodec.Version || payload[1] != (byte)_server._options.Kind)
                        return Error(request, UsbBridgeErrorCode.BadRequest, $"This bridge shares a {_server._options.Kind} device (protocol version {UsbFrameCodec.Version}).");
                    return Reply(request, UsbBridgeOp.Hello, [UsbFrameCodec.Version, (byte)_server._options.Kind, .. Encoding.UTF8.GetBytes(_server._options.DisplayName)]);

                case UsbBridgeOp.Open:
                    return OpenDevice(stream, request);

                case UsbBridgeOp.Close:
                    CloseDevice();
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.WriteBulkOut:
                    Tmc().WriteBulkOut(payload.ToArray());
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.ReadBulkIn:
                    {
                        if (payload.Length < 4)
                            return Error(request, UsbBridgeErrorCode.BadRequest, "ReadBulkIn needs a buffer size.");
                        var size = BinaryPrimitives.ReadUInt32BigEndian(payload);
                        if (size is 0 or > UsbFrameCodec.MaxFrameBytes - 16)
                            return Error(request, UsbBridgeErrorCode.BadRequest, "The buffer size is out of range.");
                        var buffer = new byte[size];
                        var count = Tmc().ReadBulkIn(buffer, out var stalled);
                        var data = new byte[1 + count];
                        data[0] = stalled ? (byte)1 : (byte)0;
                        Array.Copy(buffer, 0, data, 1, count);
                        return Reply(request, UsbBridgeOp.Data, data);
                    }

                case UsbBridgeOp.AbortBulkIn:
                    Tmc().AbortBulkIn(Tag(request));
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.AbortBulkOut:
                    Tmc().AbortBulkOut(Tag(request));
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.Clear:
                    Tmc().Clear();
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.SetRemote:
                    Tmc().SetRemote(Tag(request) != 0);
                    return Reply(request, UsbBridgeOp.Ok, []);

                case UsbBridgeOp.HidWrite:
                    {
                        var hid = _hid ?? throw new InvalidOperationException("No HID device is open.");
                        var report = payload.ToArray();
                        hid.Write(report, 0, report.Length);
                        return Reply(request, UsbBridgeOp.Ok, []);
                    }

                default:
                    return Error(request, UsbBridgeErrorCode.BadRequest, $"Operation {request.Op} is not a request.");
            }
        }

        private UsbFrame OpenDevice(Stream stream, UsbFrame request)
        {
            if (_tmc is not null || _hid is not null)
                return Error(request, UsbBridgeErrorCode.BadRequest, "The device is already open.");
            var options = _server._options;
            try
            {
                if (options.Kind == UsbBridgeKind.UsbTmc)
                {
                    var device = (_server._tmcFactory ?? throw new InvalidOperationException("No USBTMC factory is configured.")).Create(new UsbTmcDeviceAdapterOptions
                    {
                        VendorId = options.VendorId ?? 0,
                        ProductId = options.ProductId ?? 0,
                        SerialNumber = options.UsbSerialNumber,
                        ReadTimeoutMs = options.ReadTimeoutMs,
                        WriteTimeoutMs = options.WriteTimeoutMs,
                    });
                    device.Open();
                    _tmc = device;
                    var sizes = new byte[8];
                    BinaryPrimitives.WriteUInt32BigEndian(sizes, (uint)device.MaxTransferSize);
                    BinaryPrimitives.WriteUInt32BigEndian(sizes.AsSpan(4), (uint)device.MaxPacketSize);
                    return Reply(request, UsbBridgeOp.Ok, sizes);
                }

                var hid = (_server._hidFactory ?? throw new InvalidOperationException("No HID factory is configured.")).Create(new UsbHidEndpointOptions
                {
                    VendorId = options.VendorId ?? 0,
                    ProductId = options.ProductId ?? 0,
                    SerialNumber = options.UsbSerialNumber,
                    ReadTimeoutMs = options.ReadTimeoutMs,
                    WriteTimeoutMs = options.WriteTimeoutMs,
                });
                hid.Open();
                _hid = hid;
                _ = Task.Run(() => PumpReportsAsync(stream, hid));
                return Reply(request, UsbBridgeOp.Ok, []);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                CloseDevice();
                _server._logger.OpenFailed(ex.Message);
                return Error(request, UsbBridgeErrorCode.DeviceGone, ex.Message);
            }
        }

        private async Task PumpReportsAsync(Stream stream, IUsbHidDevice hid)
        {
            var buffer = new byte[4096];
            try
            {
                int count;
                while (!_cts.IsCancellationRequested && (count = await hid.BaseStream.ReadAsync(buffer, _cts.Token).ConfigureAwait(false)) > 0)
                    await SendAsync(stream, new UsbFrame(UsbBridgeOp.HidReport, 0, buffer.AsMemory(0, count).ToArray())).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or OperationCanceledException or InvalidOperationException)
            {
                // The device closed or the client left; the session ends on its own.
            }
        }

        private async Task SendAsync(Stream stream, UsbFrame frame)
        {
            await _writeLock.WaitAsync(_cts.Token).ConfigureAwait(false);
            try
            {
                await UsbFrameCodec.WriteAsync(stream, frame, _cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private void CloseDevice()
        {
            var tmc = _tmc;
            var hid = _hid;
            _tmc = null;
            _hid = null;
            try
            {
                tmc?.Dispose();
                hid?.Dispose();
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or ObjectDisposedException)
            {
                // Closing a device that is already gone is not an error.
            }
        }

        private IUsbTmcDevice Tmc() => _tmc ?? throw new InvalidOperationException("No USBTMC device is open.");

        private static byte Tag(UsbFrame request) =>
            request.Payload.Length >= 1 ? request.Payload.Span[0] : throw new InvalidOperationException("The request needs one byte.");

        private static UsbFrame Reply(UsbFrame request, UsbBridgeOp op, byte[] payload) => new(op, request.Id, payload);

        private static UsbFrame Error(UsbFrame request, UsbBridgeErrorCode code, string text)
        {
            byte[] payload = [(byte)code, .. Encoding.UTF8.GetBytes(text)];
            return new UsbFrame(UsbBridgeOp.Error, request.Id, payload);
        }
    }
}
