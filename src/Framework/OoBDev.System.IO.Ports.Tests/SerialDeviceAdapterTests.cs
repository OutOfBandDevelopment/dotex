using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.System.IO.Ports.Tests;


[TestCategory("Unit")]
[TestClass]
public sealed class SerialTransportTests
{
    private static IOptions<SerialDeviceAdapterOptions> Options(string portName = "COM1") =>
        Microsoft.Extensions.Options.Options.Create(new SerialDeviceAdapterOptions { PortName = portName });

    /// <summary>
    /// A port double whose <see cref="ISerialPort.BaseStream"/> is backed by a real
    /// <see cref="Pipe"/>: the transport's background pump reads from it for real, and the test
    /// plays "bytes arrived on the wire" by writing to <c>DevicePipe.Writer</c> — no need to
    /// simulate the read side by mocking a <see cref="Stream"/> or raising events.
    /// </summary>
    private static (Mock<ISerialPort> Port, Pipe DevicePipe) CreatePort()
    {
        var devicePipe = new Pipe();
        var port = new Mock<ISerialPort>();
        port.SetupGet(p => p.BaseStream).Returns(devicePipe.Reader.AsStream());
        return (port, devicePipe);
    }

    [TestMethod]
    public async Task LineSetters_OnAnOpenTransport_ApplyToThePortAndAreRemembered()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);
        var options = Options();
        var transport = new SerialDeviceAdapter(factory.Object, options);
        await transport.OpenAsync(TestContext.CancellationToken);

        var chained = transport.SetBaudRate(115200).SetDataBits(7).SetParity(ComParity.Even).SetStopBits(ComStopBits.Two).SetDtr(false).SetRts(false);

        Assert.AreSame(transport, chained);
        port.Verify(p => p.SetBaudRate(115200), Times.Once);
        port.Verify(p => p.SetDataBits(7), Times.Once);
        port.Verify(p => p.SetParity(global::System.IO.Ports.Parity.Even), Times.Once);
        port.Verify(p => p.SetStopBits(global::System.IO.Ports.StopBits.Two), Times.Once);
        port.Verify(p => p.SetDtr(false), Times.Once);
        port.Verify(p => p.SetRts(false), Times.Once);
        Assert.AreEqual(115200, options.Value.BaudRate);
        Assert.AreEqual(global::System.IO.Ports.Parity.Even, options.Value.Parity);
        Assert.AreEqual(ConnectionState.Open, transport.State);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public void LineSetters_OnAClosedTransport_OnlyRememberTheValues()
    {
        var options = Options();
        var transport = new SerialDeviceAdapter(Mock.Of<ISerialPortFactory>(), options);

        transport.SetBaudRate(57600).SetDtr(false);

        Assert.AreEqual(57600, options.Value.BaudRate);
        Assert.IsFalse(options.Value.DtrEnable);
    }

    [TestMethod]
    public async Task SetRts_UnderHardwareHandshake_DoesNotTouchThePort()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);
        var options = Options();
        options.Value.Handshake = global::System.IO.Ports.Handshake.RequestToSend;
        var transport = new SerialDeviceAdapter(factory.Object, options);
        await transport.OpenAsync(TestContext.CancellationToken);

        transport.SetRts(false);

        port.Verify(p => p.SetRts(It.IsAny<bool>()), Times.Never);
        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task OpenAsync_CreatesAndOpensPortFromFactory()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options("COM3"));

        await transport.OpenAsync(TestContext.CancellationToken);

        factory.Verify(f => f.Create(It.Is<SerialDeviceAdapterOptions>(o => o.PortName == "COM3")), Times.Once);
        port.Verify(p => p.Open(), Times.Once);
        Assert.AreEqual(ConnectionState.Open, transport.State);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task OpenAsync_RaisesStateChangedThroughOpeningToOpen()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());
        var states = new List<ConnectionState>();
        transport.StateChanged += (_, e) => states.Add(e.Current);

        await transport.OpenAsync(TestContext.CancellationToken);

        Assert.AreSequenceEqual([ConnectionState.Opening, ConnectionState.Open], states);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task OpenAsync_WhenPortThrows_TransitionsToFaultedAndRethrows()
    {
        var port = new Mock<ISerialPort>();
        port.Setup(p => p.Open()).Throws(new UnauthorizedAccessException("port in use"));
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());

        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => transport.OpenAsync(TestContext.CancellationToken));
        Assert.AreEqual(ConnectionState.Faulted, transport.State);
        port.Verify(p => p.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task WriteAsync_WhenNotOpen_Throws()
    {
        var transport = new SerialDeviceAdapter(Mock.Of<ISerialPortFactory>(), Options());

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => transport.WriteAsync(new byte[] { 1 }, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task WriteAsync_WhenOpen_WritesBytesToPort()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());
        await transport.OpenAsync(TestContext.CancellationToken);

        var payload = new byte[] { 0x01, 0x02, 0x03 };
        await transport.WriteAsync(payload, TestContext.CancellationToken);

        port.Verify(p => p.Write(payload, 0, payload.Length), Times.Once);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task WriteAsync_WithZeroByteDelay_WritesOneByteAtATimeThroughThePort()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Microsoft.Extensions.Options.Options.Create(new SerialDeviceAdapterOptions { PortName = "COM1", WriteByteDelayMs = 0 }));
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);

        port.Verify(p => p.Write(new byte[] { 1 }, 0, 1), Times.Once);
        port.Verify(p => p.Write(new byte[] { 2 }, 0, 1), Times.Once);
        port.Verify(p => p.Write(new byte[] { 3 }, 0, 1), Times.Once);
        port.Verify(p => p.Write(It.IsAny<byte[]>(), 0, 3), Times.Never);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task IncomingBytes_AreAvailableThroughInput()
    {
        var (port, devicePipe) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());
        await transport.OpenAsync(TestContext.CancellationToken);

        var payload = "ޭ"u8.ToArray();
        await devicePipe.Writer.WriteAsync(payload, TestContext.CancellationToken);

        var result = await transport.Input.ReadAsync(TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);
        Assert.AreSequenceEqual(payload, result.Buffer.ToArray());
        transport.Input.AdvanceTo(result.Buffer.End);

        await transport.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task DeviceClosesTheConnection_TransportTransitionsToClosed()
    {
        var (port, devicePipe) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());
        var closedTcs = new TaskCompletionSource();
        transport.StateChanged += (_, e) =>
        {
            if (e.Current == ConnectionState.Closed)
            {
                closedTcs.TrySetResult();
            }
        };

        await transport.OpenAsync(TestContext.CancellationToken);
        await devicePipe.Writer.CompleteAsync();

        await closedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);
        Assert.AreEqual(ConnectionState.Closed, transport.State);
    }

    [TestMethod]
    public async Task CloseAsync_ClosesAndDisposesPort()
    {
        var (port, _) = CreatePort();
        var factory = new Mock<ISerialPortFactory>();
        factory.Setup(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>())).Returns(port.Object);

        var transport = new SerialDeviceAdapter(factory.Object, Options());
        await transport.OpenAsync(TestContext.CancellationToken);

        await transport.CloseAsync(TestContext.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        port.Verify(p => p.Close(), Times.Once);
        port.Verify(p => p.Dispose(), Times.Once);
        Assert.AreEqual(ConnectionState.Closed, transport.State);
    }

    [TestMethod]
    public async Task CloseAsync_WhenNeverOpened_DoesNothing()
    {
        var factory = new Mock<ISerialPortFactory>();
        var transport = new SerialDeviceAdapter(factory.Object, Options());

        await transport.CloseAsync(TestContext.CancellationToken);

        factory.Verify(f => f.Create(It.IsAny<SerialDeviceAdapterOptions>()), Times.Never);
    }

    public required TestContext TestContext { get; set; }
}
