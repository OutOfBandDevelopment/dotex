using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Scpi.Tests;

/// <summary>A bench instrument on a local serial port whose commands and replies carry no terminator (the Korad supplies), so a reply ends when the line goes quiet.</summary>
internal sealed class BenchSerialInstrument : IDisposable
{
    private readonly SerialPort _port;

    private BenchSerialInstrument(SerialPort port) => _port = port;

    /// <summary>Opens the port named by <paramref name="portParameter"/> at 9600 8-N-1, or ends the test Inconclusive when it is blank or cannot be opened.</summary>
    public static BenchSerialInstrument Require(TestContext testContext, string portParameter)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        var name = testContext.GetPropertyOrDefault(portParameter, string.Empty);
        if (string.IsNullOrWhiteSpace(name))
            Assert.Inconclusive($"No '{portParameter}' run parameter: set it in .runsettings to the serial port of the instrument.");

        var port = new SerialPort(name, 9600, Parity.None, 8, StopBits.One) { ReadTimeout = 200, WriteTimeout = 1000 };
        try
        {
            port.Open();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or global::System.IO.IOException or ArgumentException or InvalidOperationException)
        {
            port.Dispose();
            Assert.Inconclusive($"{name} cannot be opened ({ex.Message}): is the instrument attached and not in use?");
        }

        return new BenchSerialInstrument(port);
    }

    /// <summary>Sends <paramref name="command"/> as is and returns everything received until the line has been quiet for <paramref name="quiet"/>.</summary>
    public async Task<string> QueryAsync(string command, TimeSpan quiet)
    {
        _port.DiscardInBuffer();
        _port.Write(command);

        var text = new StringBuilder();
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
        var lastData = DateTime.UtcNow;
        while (DateTime.UtcNow < deadline)
        {
            if (_port.BytesToRead > 0)
            {
                var buffer = new byte[_port.BytesToRead];
                var read = _port.Read(buffer, 0, buffer.Length);
                text.Append(Encoding.Latin1.GetString(buffer, 0, read));
                lastData = DateTime.UtcNow;
            }
            else if (text.Length > 0 && DateTime.UtcNow - lastData > quiet)
            {
                break;
            }
            else
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        return text.ToString();
    }

    public void Dispose() => _port.Dispose();
}
