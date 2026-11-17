using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OoBDev.ClamAv.Tests;

[TestClass]
public class ClamAvContentScannerTests
{
    private static ClamAvContentScanner Scanner(int port, int chunk = 65_536) =>
        new(Options.Create(new ClamAvOptions { Host = "127.0.0.1", Port = port, ChunkSize = chunk, TimeoutSeconds = 10 }));

    private static ContentReference Content(byte[] bytes) =>
        new() { Content = new MemoryStream(bytes), ContentType = "application/octet-stream", FileName = "file.bin" };

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Scan_OkReply_IsCleanAndSendsFramedStream()
    {
        // Stage
        await using var clamd = new FakeClamd("stream: OK");
        var bytes = Enumerable.Range(0, 5000).Select(i => (byte)(i % 251)).ToArray();

        // Test
        var result = await Scanner(clamd.Port, chunk: 2048).ScanAsync(Content(bytes));

        // Assert
        Assert.IsTrue(result.IsClean);
        Assert.AreEqual("zINSTREAM", clamd.Command);
        Assert.AreEqual(3, clamd.Chunks);
        CollectionAssert.AreEqual(bytes, clamd.Payload);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Scan_FoundReply_IsInfectedWithSignature()
    {
        await using var clamd = new FakeClamd("stream: Eicar-Test-Signature FOUND");

        var result = await Scanner(clamd.Port).ScanAsync(Content([1, 2, 3]));

        Assert.IsFalse(result.IsClean);
        Assert.AreEqual("Eicar-Test-Signature", result.Detail);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Scan_EmptyContent_SendsOnlyTerminator()
    {
        await using var clamd = new FakeClamd("stream: OK");

        var result = await Scanner(clamd.Port).ScanAsync(Content([]));

        Assert.IsTrue(result.IsClean);
        Assert.AreEqual(0, clamd.Chunks);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Scan_ErrorReply_Throws()
    {
        await using var clamd = new FakeClamd("INSTREAM size limit exceeded. ERROR");

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => Scanner(clamd.Port).ScanAsync(Content([1])));

        StringAssert.Contains(ex.Message, "size limit");
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Scan_NothingListening_Throws()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        await Assert.ThrowsAsync<SocketException>(() => Scanner(port).ScanAsync(Content([1])));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Parse_EmptyReply_ThrowsNoReply()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => ClamAvContentScanner.Parse(string.Empty));

        StringAssert.Contains(ex.Message, "no reply");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Parse_FoundWithoutSignature_UsesGenericDetail()
    {
        var result = ClamAvContentScanner.Parse("stream: FOUND");

        Assert.IsFalse(result.IsClean);
        Assert.AreEqual("malware found", result.Detail);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Scan_NullReference_Throws() =>
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Scanner(1).ScanAsync(null!));
}
