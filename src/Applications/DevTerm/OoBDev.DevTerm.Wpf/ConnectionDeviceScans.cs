using System;
using System.Collections.Generic;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.System.IO.Bluetooth;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The scan buttons of the connection editor: LAN instruments, BLE peripherals and the characteristics of the chosen peripheral.</summary>
public sealed class ConnectionDeviceScans
{
    /// <summary>Creates the commands over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor that receives the scan results.</param>
    /// <param name="lxi">Finds LAN instruments; the network scan when null.</param>
    /// <param name="ble">Finds BLE peripherals; the Bluetooth scan when null.</param>
    /// <param name="characteristics">Reads the GATT services of a peripheral; the Bluetooth explorer when null.</param>
    public ConnectionDeviceScans(
        ConnectionEditorViewModel editor,
        Func<IReadOnlyList<LxiDeviceOption>>? lxi = null,
        Func<IReadOnlyList<BleDeviceOption>>? ble = null,
        Func<string, IReadOnlyList<BleGattServiceDescriptor>>? characteristics = null)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ScanLxi = new ScanDevicesCommand(() => editor.SetLxiDeviceOptions((lxi ?? LxiDeviceScanner.ScanNetwork)()));
        ScanBle = new ScanDevicesCommand(() => editor.SetBleDeviceOptions((ble ?? BleDeviceScanner.Scan)()));
        ExploreBle = new ScanDevicesCommand(() => editor.SetBleCharacteristicOptions((characteristics ?? BleDeviceScanner.ExploreCharacteristics)(editor.BleDeviceId)));
    }

    /// <summary>Fills the detected network devices (takes seconds).</summary>
    public ICommand ScanLxi { get; }

    /// <summary>Fills the detected BLE devices.</summary>
    public ICommand ScanBle { get; }

    /// <summary>Fills the detected characteristics of the BLE device in the editor.</summary>
    public ICommand ExploreBle { get; }
}
