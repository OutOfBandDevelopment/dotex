using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Serial)]
public sealed class SerialPortWriteStreamTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task WriteAsync_Memory_WritesThroughToThePort()
    {
        var port = new Mock<ISerialPort>();
        using var stream = new SerialPortWriteStream(port.Object);

        await stream.WriteAsync(new byte[] { 1, 2, 3 });

        port.Verify(p => p.Write(new byte[] { 1, 2, 3 }, 0, 3), Times.Once);
    }

    [TestMethod]
    public void Write_WritesThroughToThePort()
    {
        var port = new Mock<ISerialPort>();
        using var stream = new SerialPortWriteStream(port.Object);
        var buffer = new byte[] { 9, 8, 7 };

        stream.Write(buffer, 0, buffer.Length);

        port.Verify(p => p.Write(buffer, 0, 3), Times.Once);
    }

    [TestMethod]
    public void CanRead_CanSeek_AreFalse_CanWrite_IsTrue()
    {
        var port = new Mock<ISerialPort>();
        using var stream = new SerialPortWriteStream(port.Object);

        Assert.IsFalse(stream.CanRead);
        Assert.IsFalse(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);
    }

    [TestMethod]
    public void Read_Throws()
    {
        var port = new Mock<ISerialPort>();
        using var stream = new SerialPortWriteStream(port.Object);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
    }

    [TestMethod]
    public async Task WriteAsync_Array_RunsOffTheCallerThread()
    {
        // Stage: the write signals when it ran; the caller thread stays blocked until then so the pool cannot reuse it for the write
        var callerThread = Environment.CurrentManagedThreadId;
        int? writeThread = null;
        using var written = new ManualResetEventSlim();
        var port = new Mock<ISerialPort>();
        port.Setup(p => p.Write(It.IsAny<byte[]>(), 0, 2)).Callback(() =>
        {
            writeThread = Environment.CurrentManagedThreadId;
            written.Set();
        });
        using var stream = new SerialPortWriteStream(port.Object);

        // Test
        var task = stream.WriteAsync(new byte[] { 1, 2 }, 0, 2, CancellationToken.None);
        Assert.IsTrue(written.Wait(TimeSpan.FromSeconds(10)), "The write did not run within 10 s.");
        await task;

        // Assert
        TestContext.WriteLine($"Caller thread {callerThread}, write thread {writeThread}");
        Assert.IsNotNull(writeThread);
        Assert.AreNotEqual(callerThread, writeThread);
    }

    [TestMethod]
    public async Task WriteAsync_AlreadyCanceled_DoesNotWrite()
    {
        var port = new Mock<ISerialPort>();
        using var stream = new SerialPortWriteStream(port.Object);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await stream.WriteAsync(new byte[] { 1 }, cts.Token));

        port.Verify(p => p.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
