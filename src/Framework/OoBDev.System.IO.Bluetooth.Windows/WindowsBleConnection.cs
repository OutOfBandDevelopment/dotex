using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace OoBDev.System.IO.Bluetooth.Windows;

/// <summary>
/// <see cref="IBleConnection"/> backed by <see cref="Windows.Devices.Bluetooth"/>'s WinRT GATT client
/// - the first (Windows) backend behind the connection seam. Not yet verified against real hardware.
/// </summary>
public sealed class WindowsBleConnection : IBleConnection
{
    private readonly BleDeviceAdapterOptions _options;
    private BluetoothLEDevice? _device;
    private GattDeviceService? _service;
    private GattCharacteristic? _writeCharacteristic;
    private GattCharacteristic? _notifyCharacteristic;

    public WindowsBleConnection(BleDeviceAdapterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public event EventHandler<ReadOnlyMemory<byte>>? NotificationReceived;

    public event EventHandler? Disconnected;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var device = await BluetoothLEDevice.FromIdAsync(_options.DeviceId).AsTask(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"No BLE device found for id '{_options.DeviceId}'.");

        GattDeviceService? service = null;
        GattCharacteristic? notifyCharacteristic = null;
        try
        {
            var serviceUuid = Guid.Parse(_options.ServiceUuid);
            var writeUuid = Guid.Parse(_options.WriteCharacteristicUuid);
            var notifyUuid = Guid.Parse(_options.NotifyCharacteristicUuid);

            var servicesResult = await device.GetGattServicesForUuidAsync(serviceUuid).AsTask(cancellationToken).ConfigureAwait(false);
            if (servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
            {
                throw new InvalidOperationException($"BLE service '{_options.ServiceUuid}' was not found on device '{_options.DeviceId}'.");
            }

            service = servicesResult.Services[0];

            var writeResult = await service.GetCharacteristicsForUuidAsync(writeUuid).AsTask(cancellationToken).ConfigureAwait(false);
            if (writeResult.Status != GattCommunicationStatus.Success || writeResult.Characteristics.Count == 0)
            {
                throw new InvalidOperationException($"BLE write characteristic '{_options.WriteCharacteristicUuid}' was not found.");
            }

            var notifyResult = await service.GetCharacteristicsForUuidAsync(notifyUuid).AsTask(cancellationToken).ConfigureAwait(false);
            if (notifyResult.Status != GattCommunicationStatus.Success || notifyResult.Characteristics.Count == 0)
            {
                throw new InvalidOperationException($"BLE notify characteristic '{_options.NotifyCharacteristicUuid}' was not found.");
            }

            var writeCharacteristic = writeResult.Characteristics[0];
            notifyCharacteristic = notifyResult.Characteristics[0];

            notifyCharacteristic.ValueChanged += OnValueChanged;
            var notifyStatus = await notifyCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);
            if (notifyStatus != GattCommunicationStatus.Success)
            {
                throw new InvalidOperationException($"Failed to subscribe to the BLE notify characteristic: {notifyStatus}.");
            }

            _writeCharacteristic = writeCharacteristic;
            _notifyCharacteristic = notifyCharacteristic;
            device.ConnectionStatusChanged += OnConnectionStatusChanged;
            _device = device;
            _service = service;
        }
        catch
        {
            // Unsubscribe here too, not just in the explicit notifyStatus-failure branch above - a
            // cancellation or a WinRT exception thrown out of WriteClientCharacteristicConfigurationDescriptorAsync
            // itself skips that branch entirely and used to leave ValueChanged attached to a
            // characteristic whose device is about to be disposed.
            notifyCharacteristic?.ValueChanged -= OnValueChanged;

            service?.Dispose();
            device.Dispose();
            throw;
        }
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (_writeCharacteristic is null)
        {
            throw new InvalidOperationException("The BLE adapter is not connected.");
        }

        // Prefer write-without-response when the characteristic supports it - most BLE Serial
        // (NUS-style) devices expose their RX characteristic this way, and a response round-trip
        // per byte chunk would otherwise throttle throughput for no benefit on those devices.
        var writeOption = _writeCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)
            ? GattWriteOption.WriteWithoutResponse
            : GattWriteOption.WriteWithResponse;

        // A without-response write is limited to one ATT packet - split anything larger instead of
        // letting the stack fail or silently truncate it.
        foreach (var chunk in BleWriteChunker.Chunk(data, _options.MaxWriteChunkSize))
        {
            var status = await _writeCharacteristic
                .WriteValueAsync(chunk.ToArray().AsBuffer(), writeOption)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            if (status != GattCommunicationStatus.Success)
            {
                throw new IOException($"BLE write failed: {status}.");
            }
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await UnsubscribeAsync(cancellationToken).ConfigureAwait(false);
        Cleanup();
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeAsync(CancellationToken.None).ConfigureAwait(false);
        Cleanup();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Turns the notification subscription off before the handles are released. A peripheral that accepts one central keeps its
    /// link up while the subscription is still on, so without this it can stay "connected" for a long time after a disconnect and
    /// refuse the next connection.
    /// </summary>
    private async Task UnsubscribeAsync(CancellationToken cancellationToken)
    {
        if (_notifyCharacteristic is null)
        {
            return;
        }

        try
        {
            await _notifyCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException or IOException or global::System.Runtime.InteropServices.COMException)
        {
            // The link is already gone, which is the state this method is after.
        }
    }

    private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args) =>
        NotificationReceived?.Invoke(this, args.CharacteristicValue.ToArray());

    private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Cleanup()
    {
        _notifyCharacteristic?.ValueChanged -= OnValueChanged;
        _notifyCharacteristic = null;

        _writeCharacteristic = null;
        _service?.Dispose();
        _service = null;

        if (_device is not null)
        {
            _device.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _device.Dispose();
            _device = null;
        }
    }
}
