using System;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Attribute for binary decoder to detail default serial configurations
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SerialPortAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortAttribute"/> class with default settings.
    /// </summary>
    public SerialPortAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortAttribute"/> class with the specified baud rate.
    /// </summary>
    /// <param name="baudRate">The baud rate for the serial port.</param>
    public SerialPortAttribute(int baudRate)
    {
        BaudRate = baudRate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialPortAttribute"/> class with full port configuration.
    /// </summary>
    /// <param name="baudRate">The baud rate for the serial port.</param>
    /// <param name="parity">The parity checking protocol.</param>
    /// <param name="dataBits">The number of data bits per byte.</param>
    /// <param name="stopBits">The number of stop bits per byte.</param>
    public SerialPortAttribute(int baudRate, Parity parity, int dataBits, StopBits stopBits)
        : this(baudRate)
    {
        Parity = parity;
        DataBits = dataBits;
        StopBits = stopBits;
    }

    /// <summary>
    /// Default Baud Rate
    /// </summary>
    public int BaudRate { get; set; } = 9600;
    /// <summary>
    /// Default bitwidth
    /// </summary>
    public int DataBits { get; set; } = 8;
    /// <summary>
    /// Default stop bits
    /// </summary>
    public StopBits StopBits { get; set; } = StopBits.One;
    /// <summary>
    /// Default parity bit
    /// </summary>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// Gets or sets the read timeout in milliseconds. Default is -1 (infinite timeout).
    /// </summary>
    public int ReadTimeout { get; set; } = -1;

    /// <summary>
    /// Gets or sets the write timeout in milliseconds. Default is -1 (infinite timeout).
    /// </summary>
    public int WriteTimeout { get; set; } = -1;
}
