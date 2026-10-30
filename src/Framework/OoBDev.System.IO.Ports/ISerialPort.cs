using OoBDev.System.IO.Pipelines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Ports;


/// <summary>
/// Thin abstraction over <see cref="System.IO.Ports.SerialPort"/> so <see cref="SerialDeviceAdapter"/>
/// can be unit tested with a fake/mock instead of a real serial port.
/// </summary>
public interface ISerialPort : IDisposable
{
    bool IsOpen { get; }

    /// <summary>The readable/writable stream <see cref="SerialDeviceAdapter"/> pumps into a pipe (see <see cref="StreamToPipePump"/>).</summary>
    Stream BaseStream { get; }

    void Open();

    void Close();

    // Builder-style line settings: each applies at once (also while open) and returns this port so calls chain.
    // Dtr/Rts throw for RTS under hardware handshake, like SerialPort.RtsEnable.
    ISerialPort SetBaudRate(int baudRate);

    ISerialPort SetDataBits(int dataBits);

    ISerialPort SetParity(global::System.IO.Ports.Parity parity);

    ISerialPort SetStopBits(global::System.IO.Ports.StopBits stopBits);

    ISerialPort SetDtr(bool enabled);

    ISerialPort SetRts(bool enabled);

    void Write(byte[] buffer, int offset, int count);
}
