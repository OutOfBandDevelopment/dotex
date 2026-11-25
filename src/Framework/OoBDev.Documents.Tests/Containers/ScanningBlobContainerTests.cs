using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.Documents.Scanning;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class ScanningBlobContainerTests
{
    private static ContentReference Content(Stream stream) =>
        new() { Content = stream, ContentType = "text/plain", FileName = "a.txt" };

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_Clean_StoresFromTheStart()
    {
        // Stage
        var inner = new Mock<IBlobContainer>(MockBehavior.Strict);
        var scanner = new Mock<IContentScanner>(MockBehavior.Strict);
        long positionAtStore = -1;
        scanner.Setup(s => s.ScanAsync(It.IsAny<ContentReference>()))
            .Returns<ContentReference>(r => { r.Content.Position = r.Content.Length; return Task.FromResult(ContentScanResult.Clean); });
        inner.Setup(s => s.StoreContentAsync(It.IsAny<ContentReference>(), null, false))
            .Callback<ContentReference, IDictionary<string, string>?, bool>((r, _, _) => positionAtStore = r.Content.Position)
            .Returns(Task.CompletedTask);

        // Test
        await new ScanningBlobContainer(inner.Object, scanner.Object).StoreContentAsync(Content(new MemoryStream([1, 2, 3])));

        // Assert
        Assert.AreEqual(0, positionAtStore);
        inner.VerifyAll();
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_Infected_RefusesAndDoesNotStore()
    {
        // Stage
        var inner = new Mock<IBlobContainer>(MockBehavior.Strict);
        var scanner = new Mock<IContentScanner>();
        scanner.Setup(s => s.ScanAsync(It.IsAny<ContentReference>())).ReturnsAsync(ContentScanResult.Infected("Eicar-Test"));

        // Test
        var ex = await Assert.ThrowsAsync<ContentRejectedException>(() =>
            new ScanningBlobContainer(inner.Object, scanner.Object).StoreContentAsync(Content(new MemoryStream([1]))));

        // Assert
        StringAssert.Contains(ex.Message, "Eicar-Test");
        inner.Verify(s => s.StoreContentAsync(It.IsAny<ContentReference>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<bool>()), Times.Never);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_ScannerFails_FailsClosed()
    {
        // Stage
        var inner = new Mock<IBlobContainer>(MockBehavior.Strict);
        var scanner = new Mock<IContentScanner>();
        scanner.Setup(s => s.ScanAsync(It.IsAny<ContentReference>())).ThrowsAsync(new InvalidOperationException("daemon down"));

        // Test
        var ex = await Assert.ThrowsAsync<ContentRejectedException>(() =>
            new ScanningBlobContainer(inner.Object, scanner.Object).StoreContentAsync(Content(new MemoryStream([1]))));

        // Assert
        Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_NonSeekableStream_IsBufferedForBothScanAndStore()
    {
        // Stage
        var inner = new Mock<IBlobContainer>();
        var scanner = new Mock<IContentScanner>();
        long scanned = 0, stored = 0;
        scanner.Setup(s => s.ScanAsync(It.IsAny<ContentReference>()))
            .Returns<ContentReference>(r => { scanned = r.Content.Length; return Task.FromResult(ContentScanResult.Clean); });
        inner.Setup(s => s.StoreContentAsync(It.IsAny<ContentReference>(), null, false))
            .Callback<ContentReference, IDictionary<string, string>?, bool>((r, _, _) => stored = r.Content.Length)
            .Returns(Task.CompletedTask);

        // Test
        await new ScanningBlobContainer(inner.Object, scanner.Object).StoreContentAsync(Content(new ForwardOnlyStream([1, 2, 3, 4])));

        // Assert
        Assert.AreEqual(4, scanned);
        Assert.AreEqual(4, stored);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task GetAndDelete_PassThroughWithoutScanning()
    {
        // Stage
        var inner = new Mock<IBlobContainer>();
        var scanner = new Mock<IContentScanner>(MockBehavior.Strict);
        inner.Setup(s => s.DeleteContentAsync("p")).Returns(Task.CompletedTask);

        // Test
        var container = new ScanningBlobContainer(inner.Object, scanner.Object);
        await container.DeleteContentAsync("p");
        await container.GetContentAsync("p");

        // Verify
        inner.Verify(s => s.DeleteContentAsync("p"), Times.Once);
        inner.Verify(s => s.GetContentAsync("p"), Times.Once);
    }

    private sealed class ForwardOnlyStream(byte[] data) : MemoryStream(data)
    {
        public override bool CanSeek => false;
    }
}
